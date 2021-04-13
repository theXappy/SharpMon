using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace SharpMon
{
    public class NetMonSniffer
    {
        private NetMonEngine _engine;
        private bool _netApiInited = false;

        public delegate void FrameEventHandle(object sender, uint macType, byte[] frameData);
        public event FrameEventHandle FrameAvailable;

        private CaptureCallbackDelegate captureCallback;

        public NetMonSniffer()
        {
            // Holding this delegate as a member otherwise it get's GC'd
            captureCallback = new CaptureCallbackDelegate(FrameCapturedCallback);
        }

        private uint InitIfRequired()
        {
            if (_netApiInited)
                return 0;

            // Changing the configuration is crucial otherwise NmOpenCaptureEngine fails with error 2147549446 (Not documented? :/ )
            // The issue has something to do with the 'threading mode' there are some non helpful mentions of such issue here:
            // https://social.microsoft.com/Forums/azure/en-US/a0388a28-dc14-47a3-af21-a68380dcd4ab/network-monitor-fails-running-under-quotlocal-system-accountquot?forum=netmon
            NM_API_CONFIGURATION apiConfig = new NM_API_CONFIGURATION();
            apiConfig.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NM_API_CONFIGURATION));
            uint errno = NetmonAPI.NmGetApiConfiguration(ref apiConfig);
            if (errno != 0)
            {
                Console.WriteLine("Failed Initiating NetMon API.NmGetApiConfiguration returned: " + errno);
                return errno;
            }

            // Edit current configuration and send the new version to the API.
            apiConfig.ThreadingMode = 0;
            errno = NetmonAPI.NmApiInitialize(ref apiConfig);

            if (errno != 0)
            {
                Console.WriteLine("Failed Initiating NetMon API. NmApiInitialize returned: " + errno);
                return errno;
            }

            _netApiInited = true;
            return 0; // success
        }

        /// <returns>A dictionary mapping Adapter IDs to their NetMon/.Net information structures</returns>
        public Dictionary<uint, AdapterInformation> GetAdapters()
        {
            var output =
                new Dictionary<uint, AdapterInformation>();

            uint errno = InitIfRequired();
            if (errno != 0)
            {
                Console.WriteLine("Failed to get adapters. Initialization of NetAPI returned: " + errno);
                return null;
            }

            using (NetMonEngine eng = NetmonAPI.NmOpenCaptureEngineManaged())
            {
                NM_NIC_ADAPTER_INFO[] netmonAdapters = NetmonAPI.NmGetAdaptersManaged(eng);

                NetworkInterface[] dotnetAdapters = NetworkInterface.GetAllNetworkInterfaces();

                // Iterating the NetMon adapters we found and matching 'NetworkInterface' objects to them.
                for (uint i = 0; i < netmonAdapters.Length; i++)
                {
                    NM_NIC_ADAPTER_INFO adapterInfo = netmonAdapters[i];
                    String nameStr = new String(adapterInfo.ConnectionName);
                    NetworkInterface ni = dotnetAdapters.Single(iface => nameStr.Contains(iface.Id));

                    // Add the adapter to output
                    output[i] = new AdapterInformation(adapterInfo, ni);
                }
            }

            return output;
        }

        public void Start(uint adapterId)
        {
            // TODO: Support multiple adapters
            uint errno;

            errno = InitIfRequired();
            if (errno != 0)
            {
                throw new Exception("Failed to initialize capture. NmApiInitalize returned: " + errno);
            }

            _engine = NetmonAPI.NmOpenCaptureEngineManaged();

            errno = NetmonAPI.NmConfigAdapter(_engine, adapterId, captureCallback, IntPtr.Zero,
                NmCaptureCallbackExitMode.DiscardRemainFrames);
            if (errno != 0)
            {
                throw new Exception($"Error configuring adapter #{adapterId}. Errno: " + errno);
            }

            uint ret = NetmonAPI.NmStartCapture(_engine, adapterId, NmCaptureMode.Promiscuous);
            if (ret != 0)
            {
                throw new Exception($"Failed to start capturing adapter #{adapterId}. NmStartCapture returned: {ret}");
            }
        }

        public void Stop()
        {
            uint errno = NetmonAPI.NmStopCapture(_engine, 1);
            if (errno != 0)
            {
                throw new Exception($"Error stopping Capture. NmStopCapture returned: {errno}");
            }

            _engine.Dispose();
            _engine = null;
        }

        /// <summary>
        /// The callback that is called from the unmanaged code whenever a new packet arrives.
        /// It parses the frame and forwards it to anyone listening on the <see cref="FrameAvailable"/> event
        /// </summary>
        /// <param name="hCaptureEngine">The engine who captured the packet</param>
        /// <param name="ulAdapterIndex">The index of the adapter</param>
        /// <param name="pCallerContext">Context - a way to transfer state from the initializer thread to the callback thread (null in our case)</param>
        /// <param name="hFrame">The handle for the frame</param>
        private void FrameCapturedCallback(IntPtr hCaptureEngine, UInt32 ulAdapterIndex, IntPtr pCallerContext, IntPtr hFrame)
        {
            // Basic parsing: Get MAC (link layer) type & frame's bytes
            uint res = NetmonAPI.NmGetFrameMacType(hFrame, out var macType);
            if (res != 0) return;

            // Seems like the MAC value we are getting is not zero-based and it should be
            macType -= 1;

            byte[] rawFrameData = NetmonAPI.NmGetRawFrameManaged(hFrame);

            // Dispose of unmanaged frame handle
            NetmonAPI.NmCloseHandle(hFrame);

            // Forward to any listeners
            FrameAvailable?.Invoke(this, macType, rawFrameData);
        }
    }
}
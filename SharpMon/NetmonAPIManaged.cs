using System;
using System.Collections.Generic;

namespace SharpMon
{
    // Additions to Microsoft's 'NetmonAPI.cs' that have a more managed approach
    public partial class NetmonAPI
    {
        /// <summary>
        /// Managed wrapper around <see cref="NmGetRawFrame"/>
        /// </summary>
        public static byte[] NmGetRawFrameManaged(IntPtr hFrame)
        {
            uint errno = NmGetRawFrameLength(hFrame, out uint reportedLength);
            if (errno != 0)
            {
                throw new Exception($"Failed to get raw frame's length. NmGetRawFrameLength returned: {errno}");
            }

            uint actualLength;
            byte[] strFrameBuffer = new byte[reportedLength];
            unsafe
            {
                fixed (byte* pFrameBuffer = strFrameBuffer)
                {
                    errno = NetmonAPI.NmGetRawFrame(hFrame, reportedLength, pFrameBuffer, out actualLength);
                }
            }
            if (errno != 0)
            {
                throw new Exception($"Failed to get raw frame's data. NmGetRawFrame returned: {errno}");
            }

            if (actualLength != reportedLength)
            {
                throw new Exception($"Failed to get raw frame's data. NmGetRawFrame indicates wrong number of bytes copied. Requested: {reportedLength}, Got: {actualLength}");
            }

            return strFrameBuffer;
        }

        /// <summary>
        /// Managed wrapper around <see cref="NmOpenCaptureEngine"/> and <see cref="NmCloseHandle"/>
        /// The returned object is disposable, calling Dispose closes the engine handle.
        /// </summary>
        /// <returns>The opened NetMon engine</returns>
        public static NetMonEngine NmOpenCaptureEngineManaged()
        {
            return new NetMonEngine();
        }

        /// <summary>
        /// Managed wrapper around <see cref="NmGetAdapter"/> and <see cref="NmGetAdapterCount"/>
        /// </summary>
        public static NM_NIC_ADAPTER_INFO[] NmGetAdaptersManaged(IntPtr hCaptureEngine)
        {
            List<NM_NIC_ADAPTER_INFO> workList = new List<NM_NIC_ADAPTER_INFO>();

            uint dwAdapterCount = 0;
            uint errno = NetmonAPI.NmGetAdapterCount(hCaptureEngine, out dwAdapterCount);
            if (errno != 0)
            {
                throw new Exception($"Failed to get adapters count. NmGetAdapterCount returned: {errno}");
            }

            for (uint i = 0; i < dwAdapterCount; i++)
            {
                NM_NIC_ADAPTER_INFO adapterInfo = new NM_NIC_ADAPTER_INFO();
                adapterInfo.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(adapterInfo);
                var status = NetmonAPI.NmGetAdapter(hCaptureEngine, i, ref adapterInfo);
                if (status != 0)
                {
                    throw new Exception($"Failed to get adapter #{i}. NmGetAdapter returned: {errno}");
                }
                workList.Add(adapterInfo);
            }

            return workList.ToArray();
        }
    }
}
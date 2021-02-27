using System;

namespace SharpMon
{
    /// <summary>
    /// A disposable approach for possessing NetMon engine handle.
    /// </summary>
    public class NetMonEngine : IDisposable
    {
        private IntPtr _ptr;
        public IntPtr Ptr
        {
            get => _ptr;
            set => _ptr = value;
        }

        public NetMonEngine()
        {
            uint errno = NetmonAPI.NmOpenCaptureEngine(out this._ptr);
            if (errno != 0)
            {
                throw new Exception($"Failed to open Netmon Engine. NmOpenCaptureEngine returned: {errno}");
            }
        }

        // Implicit cast to IntPtr so we can just call the netmon api functions with the engine object
        // e.g.  NetmonAPI.NmGetAdapterCount(engine, out uint adpCount);
        public static implicit operator IntPtr(NetMonEngine eng) => eng.Ptr;

        public void Dispose()
        {
            NetmonAPI.NmCloseHandle(_ptr);
            _ptr = IntPtr.Zero;
        }
    }
}
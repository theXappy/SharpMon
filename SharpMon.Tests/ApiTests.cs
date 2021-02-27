using System;
using System.Linq;
using NUnit.Framework;

namespace SharpMon.Tests
{
    public class ApiTests
    {
        [SetUp]
        public void Setup()
        {
            NM_API_CONFIGURATION apiConfig = new NM_API_CONFIGURATION();
            apiConfig.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NM_API_CONFIGURATION));
            uint status = NetmonAPI.NmGetApiConfiguration(ref apiConfig);
            apiConfig.ThreadingMode = 0;
            uint errno = NetmonAPI.NmApiInitialize(ref apiConfig);

            if (errno != 0)
            {
                throw new Exception("Error NmApiInitalize. Errno: " + errno);
            }
        }

        [Test]
        public void NmOpenCaptureEngineManaged_ValidState_EngineReturned()
        {
            NetMonEngine eng = NetmonAPI.NmOpenCaptureEngineManaged();

            Assert.IsNotNull(eng, "NmOpenCaptureEngineManaged returned a null engine");
            eng.Dispose();
        }

        [Test]
        public void NmGetAdaptersManaged_ValidEngine_AtLeastOneFound()
        {
            NM_NIC_ADAPTER_INFO[] adapters;
            using (var engine = NetmonAPI.NmOpenCaptureEngineManaged())
            {
                adapters = NetmonAPI.NmGetAdaptersManaged(engine);
            }

            Assert.IsTrue(adapters.Any(), "NmGetAdaptersManaged returned an empty array of adapters");
        }
    }
}
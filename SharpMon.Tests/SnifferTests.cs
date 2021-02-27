using System.Linq;
using NUnit.Framework;

namespace SharpMon.Tests
{
    public class SnifferTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetAdapters_ValidState_AtLeastOneReturned()
        {
            NetMonSniffer sniffer = new NetMonSniffer();

            var adapters = sniffer.GetAdapters();

            Assert.IsTrue(adapters.Any(), "NetMonSniffer.GetAdapters returned an empty dictionary of adapters");
        }

    }
}
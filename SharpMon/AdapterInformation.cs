using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace SharpMon
{
    /// <summary>
    /// Information about a capture-enabled adapter
    /// </summary>
    public class AdapterInformation
    {
        public NM_NIC_ADAPTER_INFO NetmonInfo { get; private set; }
        public NetworkInterface DotNetInfo { get; private set; }

        public AdapterInformation(NM_NIC_ADAPTER_INFO netmonInfo, NetworkInterface dotNetInfo)
        {
            NetmonInfo = netmonInfo;
            DotNetInfo = dotNetInfo;
        }
    }
}

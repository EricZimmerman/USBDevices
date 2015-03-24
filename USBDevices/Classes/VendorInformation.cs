using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if DEBUG

[assembly: InternalsVisibleTo("USBDevices.Test")]
#endif

namespace USBDevices.Classes
{
    internal class VendorInformation
    {
        public VendorInformation(int vendorId, string vendorName)
        {
            VendorId = vendorId;
            VendorName = vendorName;
        }

        public string VendorName { get; }
        public int VendorId { get; }
        public Dictionary<int, string> ProductInformation { get; } = new Dictionary<int, string>();
    }
}
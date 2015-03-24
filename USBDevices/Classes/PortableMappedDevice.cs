using System;
using System.Text;

namespace USBDevices
{
    public class PortableMappedDevice
    {
        public string Vendor { get; set; } = String.Empty;
        public string Product { get; set; } = String.Empty;
        public string Revision { get; set; } = String.Empty;
        public string SerialNumber { get; set; } = String.Empty;
        public string GUID { get; set; }  = String.Empty;
        public string FriendlyName { get; set; } = String.Empty;


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Vendor: {Vendor}");
            sb.AppendLine($"Product: {Product}");
            sb.AppendLine($"Revision: {Revision}");
            sb.AppendLine($"SerialNumber: {SerialNumber}");
            sb.AppendLine($"GUID: {GUID}");
            sb.AppendLine($"FriendlyName: {FriendlyName}");

            return sb.ToString();
        }
    }
}
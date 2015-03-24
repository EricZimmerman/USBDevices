using System.Runtime.CompilerServices;
using System.Text;

namespace USBDevices.Classes
{
    public class ProductInformation
    {
        public string VendorName { get; set; } = "Unknown";
        public string ProductDescription { get; set; } = "Unknown";

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Vendor name: {VendorName}");
            sb.AppendLine($"Product description: {ProductDescription}");

            return sb.ToString();
        }
    }
}
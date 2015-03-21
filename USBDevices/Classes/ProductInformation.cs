using System.Runtime.CompilerServices;

#if DEBUG

[assembly: InternalsVisibleTo("USBDevices.Test")]
#endif

namespace USBDevices.Classes
{
	internal class ProductInformation
	{
		public string VendorName { get; set; } = "Unknown";
		public string ProductDescription { get; set; } = "Unknown";
	}
}
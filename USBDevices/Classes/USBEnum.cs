using System;
using USBDevices.Classes;

namespace USBDevices
{
	internal class USBEnum
	{
		public int VID_ID { get; set; }
	//	public string VendorName { get; set; }
		public int PID_ID { get; set; }
	//	public string ProductName { get; set; }

		public ProductInformation ProductInfo { get; set; } = new ProductInformation();
		public string SerialNumber { get; set; }
		public bool WindowsGeneratedSerial { get; set; }
		public DateTimeOffset LastDateTimeConnected { get; set; }
	}
}
using System;
using System.Text;
using USBDevices.Classes;

namespace USBDevices
{
    public class USBEnum
    {
        public int VID_ID { get; set; }
        public int PID_ID { get; set; }
        public ProductInformation ProductInfo { get; set; } = new ProductInformation();
        public string SerialNumber { get; set; } = string.Empty;
        public bool WindowsGeneratedSerial { get; set; }
        public DateTimeOffset LastDateTimeConnected { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Vid_Id: {VID_ID}");
            sb.AppendLine($"Pid_Id: {PID_ID}");
            sb.AppendLine($"Product info: {ProductInfo}");
            sb.AppendLine($"Serial #: {SerialNumber}");
            sb.AppendLine($"Windows generated Serial: {WindowsGeneratedSerial}");
            sb.AppendLine($"Last time connected: {LastDateTimeConnected}");

            return sb.ToString();
        }
    }
}
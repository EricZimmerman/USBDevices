using System;

namespace USBDevices
{
    internal class SetupAPIDevRecord
    {
        public DateTimeOffset FirstConnectedDatetime { get; set; }
        public string SerialNumber { get; set; }
    }
}
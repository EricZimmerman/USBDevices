using System;
using System.Text;

namespace USBDevices
{
    public class MountPoint2Device
    {
        public string Guid { get; set; }
        public DateTimeOffset LastWriteTime { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Guid: {Guid}");
            sb.AppendLine($"LastWriteTime: {LastWriteTime}");

            return sb.ToString();
        }
    }
}
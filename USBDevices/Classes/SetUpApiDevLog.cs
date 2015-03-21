using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if DEBUG

[assembly: InternalsVisibleTo("USBDevices.Test")]
#endif

namespace USBDevices.Classes
{
	internal class SetUpApiDevLog
	{

		public List<SetupAPIDevRecord> SetUpApiRecords { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logPath"></param>
		/// <param name="timeZoneInfo">The time zone of the machine where this log came from. This will be used to convert all timestamps to UTC</param>
		public SetUpApiDevLog(string logPath, TimeZoneInfo timeZoneInfo)
		{
			SetUpApiRecords = new List<SetupAPIDevRecord>();

			if (File.Exists(logPath) == false)
			{
				return ;
			}

			using (var sr = new StreamReader(logPath))
			{
				string line = null;

				line = sr.ReadLine();
				while (line != null)
				{
					if (line.Contains("USBSTOR#Disk"))
					{
						var parentLine = line;
						//>>>  [Device Install (Hardware initiated) - SWD\WPDBUSENUM\_??_USBSTOR#Disk&Ven_USB2.0&Prod_Flash_Disk&Rev_2.10#1000000000001C37&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}]
						//>>>  Section start 2013/10/17 17:06:15.883

						var segs = parentLine.Split('&');
						var segs2 = segs[3].Split('#');

						var serial = segs2[1];

						var timeStampLine = sr.ReadLine();
						var tsDate = DateTime.Parse(timeStampLine.Replace(">>>  Section start ", ""));

						var ts = new DateTimeOffset(tsDate, timeZoneInfo.GetUtcOffset(tsDate));

						var sar = new SetupAPIDevRecord
						{
							FirstConnectedDatetime = ts.ToUniversalTime(),
							SerialNumber = serial
						};

						SetUpApiRecords.Add(sar);
					}

					line = sr.ReadLine();
				}
			}
		}
	}
}

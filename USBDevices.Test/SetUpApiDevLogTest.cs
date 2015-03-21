using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using USBDevices.Classes;

namespace USBDevices.Test
{
	[TestFixture]
	class SetUpApiDevLogTest
	{
		private SetUpApiDevLog _devLog;

		[TestFixtureSetUp]
		public void Initialize()
		{
			const string logPath = @"..\..\Hives\setupapi.dev.log";
            _devLog = new SetUpApiDevLog(logPath,TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
		}

		[Test]
		public void ShouldFindOneEntryAndValidateIt()
		{
			//>>>  [Device Install (Hardware initiated) - SWD\WPDBUSENUM\_??_USBSTOR#Disk&Ven_&Prod_USB_Flash_Memory&Rev_1.00#001D92DC4AEDC8B0F37201C1&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}]
			//>>>  Section start 2015/03/20 21:29:41.950
			//     dvi: {Build Driver List} 21:29:41.966

			Check.That(_devLog.SetUpApiRecords.Count).IsEqualTo(1);

			var setupRecord = _devLog.SetUpApiRecords.First();

			Check.That(setupRecord).IsNotNull();

			Check.That(setupRecord.SerialNumber).IsEqualTo("001D92DC4AEDC8B0F37201C1");

			Check.That(setupRecord.FirstConnectedDatetime.ToString()).IsEqualTo("3/21/2015 3:29:41 AM +00:00");
		}


		[Test]
		public void ShouldReturnEmptyListWhenLogDoesntExist()
		{
			var devLog = new SetUpApiDevLog(@"ThisIsABadPath", TimeZoneInfo.Utc);

			Check.That(devLog.SetUpApiRecords.Count).IsEqualTo(0);
		}
	}
}

using System.Configuration;
using NUnit.Framework;
using Sando.Configuration;

namespace Sando.Configuration.UnitTests
{
	[TestFixture]
	public class ConfigurationTest
	{
		[Test]
		public void GetValueTest()
		{
			string settingName = "testSetting";
			string settingValue = "testSettingValue";
			ConfigurationManager.AppSettings[settingName] = settingValue;
			Assert.True(Configuration.GetValue(settingName) == settingValue);
		}
	}
}

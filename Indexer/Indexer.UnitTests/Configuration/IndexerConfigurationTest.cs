using System.Configuration;
using NUnit.Framework;
using Sando.Indexer.Configuration;

namespace Sando.Indexer.UnitTests.Configuration
{
	[TestFixture]
	public class IndexerConfigurationTest
	{
		[Test]
		public void GetValueTest()
		{
			string settingName = "testSetting";
			string settingValue = "testSettingValue";
			ConfigurationManager.AppSettings[settingName] = settingValue;
			Assert.True(IndexerConfiguration.GetValue(settingName) == settingValue);
		}
	}
}

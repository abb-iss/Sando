using System;
using System.Diagnostics.Contracts;
using System.IO;
using NUnit.Framework;
using Sando.Indexer.IndexState;
using System.Threading;

namespace Sando.Indexer.UnitTests.IndexState
{
    [TestFixture]
	public class PhysicalFilesStatesManagerTest
	{
    	[Test]
		public void PhysicalFilesStatesManager_GetPhysicalFileStateThrowsWhenFullFilePathIsNull()
		{
				PhysicalFilesStatesManager physicalFilesStatesManager = new PhysicalFilesStatesManager();
				Assert.True(physicalFilesStatesManager.GetPhysicalFileState(null)==null);
		}

		[Test]
		public void PhysicalFilesStatesManager_GetPhysicalFileStateThrowsWhenFullFilePathIsAnEmptyString()
		{
				PhysicalFilesStatesManager physicalFilesStatesManager = new PhysicalFilesStatesManager();
				Assert.True(physicalFilesStatesManager.GetPhysicalFileState(String.Empty)==null);
		}

		[Test]
		public void PhysicalFilesStatesManager_GetPhysicalFileStateThrowsWhenFileDoesNotExists()
		{
				PhysicalFilesStatesManager physicalFilesStatesManager = new PhysicalFilesStatesManager();
                Assert.True(physicalFilesStatesManager.GetPhysicalFileState("Fake path")==null);
		}

		[Test]
		public void PhysicalFilesStatesManager_GetPhysicalFileStateReturnsObjectForValidData()
		{
			try
			{
				PhysicalFilesStatesManager physicalFilesStatesManager = new PhysicalFilesStatesManager();
				PhysicalFileState physicalFileState = physicalFilesStatesManager.GetPhysicalFileState(filePath);
				Assert.IsNotNull(physicalFileState, "GetPhysicalFileState should return valid object for the valid file path!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void PhysicalFilesStatesManager_GetPhysicalFileStateReturnsDifferentObjectWhenFileHasChanged()
		{
			try
			{
				PhysicalFilesStatesManager physicalFilesStatesManager = new PhysicalFilesStatesManager();
				PhysicalFileState physicalFileStateBeforeChange = physicalFilesStatesManager.GetPhysicalFileState(filePath);
				Thread.Sleep(2000);
				WriteToTemporaryFile();
				PhysicalFileState physicalFileStateAfterChange = physicalFilesStatesManager.GetPhysicalFileState(filePath);
				Assert.True(physicalFileStateBeforeChange.LastModificationDate < physicalFileStateAfterChange.LastModificationDate, "GetPhysicalFileState should return object with greated LastModificationDate when file has changed than it returns before the change!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		private void CreateTemporaryFile()
		{
			FileStream fileStream = null;
			try
			{
				fileStream = File.Create(filePath);
			}
			finally
			{
				if(fileStream != null)
					fileStream.Close();
			}
		}

		private void DeleteTemporaryFile()
		{
			File.Delete(filePath);
		}

		private void WriteToTemporaryFile()
		{
			StreamWriter streamWriter = null;
			try
			{
				streamWriter = new StreamWriter(filePath);
				streamWriter.Write("anything");
			}
			finally
			{
				if(streamWriter != null)
					streamWriter.Close();
			}
		}

		[SetUp]
		public void ResetContract()
		{
			contractFailed = false;
			Contract.ContractFailed += (sender, e) =>
			{
				e.SetHandled();
				e.SetUnwind();
				contractFailed = true;
			};
			CreateTemporaryFile();
		}

		[TearDown]
		public void ClearTemporaryFile()
		{
			DeleteTemporaryFile();
		}

		private string filePath = "C:/Windows/Temp/fileName.cs";
		private bool contractFailed;
	}
}

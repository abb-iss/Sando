using System;
using System.Diagnostics.Contracts;
using System.IO;
using NUnit.Framework;
using Sando.Indexer.IndexState;

namespace Sando.Indexer.UnitTests.IndexState
{
    [TestFixture]
	public class IndexFilesStatesManagerTest
	{
    	[Test]
		public void IndexFilesStatesManager_ConstructorThrowsWhenIndexDirectoryPathIsNull()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(null);
			}
			catch 
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_ConstructorThrowsWhenIndexDirectoryPathIsAnEmptyString()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(String.Empty);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_ConstructorThrowsWhenIndexDirectoryDoesNotExists()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager("Fake path");
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_GetIndexFileStateThrowsWhenFullFilePathIsNull()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				indexFilesStatesManager.GetIndexFileState(null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}
		

		[Test]
		public void IndexFilesStatesManager_GetIndexFileStateThrowsWhenFullFilePathIsAnEmptyString()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				indexFilesStatesManager.GetIndexFileState(String.Empty);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_GetIndexFileStateReturnsNullForNonIndexedFile()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
				IndexFileState indexFileState = indexFilesStatesManager.GetIndexFileState("fake file");
				Assert.IsNull(indexFileState, "GetIndexFileState should return null for the non indexed file!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void IndexFilesStatesManager_GetIndexFileStateReturnsObjectForUpdatedIndexFile()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
				IndexFileState indexFileState = new IndexFileState("file path", DateTime.UtcNow);
				CreateTemporaryFile();
				indexFilesStatesManager.UpdateIndexFileState(temporaryFilePath, indexFileState);
				indexFilesStatesManager.GetIndexFileState(temporaryFilePath);
				Assert.IsNotNull(indexFileState, "GetIndexFileState should return valid object for the indexed file!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			finally
			{
				DeleteTemporaryFile();
			}
		}

		[Test]
		public void IndexFilesStatesManager_UpdateIndexFileStateThrowsWhenFullFilePathIsNull()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				indexFilesStatesManager.UpdateIndexFileState(null, new IndexFileState("file path", DateTime.UtcNow));
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_UpdateIndexFileStateThrowsWhenFullFilePathIsAnEmptyString()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				indexFilesStatesManager.UpdateIndexFileState(String.Empty, new IndexFileState("file path", DateTime.UtcNow));
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_UpdateIndexFileStateThrowsWhenFileDoesNotExists()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				indexFilesStatesManager.UpdateIndexFileState("fake path", new IndexFileState("fake path", DateTime.UtcNow));
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_UpdateIndexFileStateThrowsWhenIndexFileStateIsNull()
		{
			IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
			try
			{
				CreateTemporaryFile();
				indexFilesStatesManager.UpdateIndexFileState(temporaryFilePath, null);
			}
			catch
			{
				//contract exception catched here
			}
			finally
			{
				DeleteTemporaryFile();
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void IndexFilesStatesManager_SaveIndexFilesStatesSerializesDataToIndexFilesStatesFile()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
				CreateTemporaryFile();
				DateTime modificationDate = DateTime.UtcNow;
				indexFilesStatesManager.UpdateIndexFileState(temporaryFilePath, new IndexFileState(temporaryFilePath, modificationDate));
				Assert.True(!File.Exists(filePath), "Index files states file should not exists here!");
				indexFilesStatesManager.SaveIndexFilesStates();
				Assert.True(File.Exists(filePath), "SaveIndexFilesStates should create index files states file!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			finally
			{
				DeleteTemporaryFile();
				DeleteTemporaryIndexFilesStatesFile();
			}
		}

		[Test]
		public void IndexFilesStatesManager_ReadIndexFilesStatesDeserializesDataFromIndexFilesStatesFile()
		{
			try
			{
				IndexFilesStatesManager indexFilesStatesManager = new IndexFilesStatesManager(fileDirectory);
				CreateTemporaryIndexFilesStatesFile();
				Assert.True(File.Exists(filePath), "Index files states file should exists here!");
				indexFilesStatesManager.ReadIndexFilesStates();
				IndexFileState indexFileState = indexFilesStatesManager.GetIndexFileState(temporaryFilePath);
				Assert.IsNotNull(indexFileState, "GetIndexFileState should return object read from the index files states file!");
				Assert.True(indexFileState.FilePath == temporaryFilePath, "GetIndexFileState should return file path read from the index files states file!");
				Assert.True(indexFileState.LastIndexingDate == new DateTime(2012, 2, 24, 0, 0, 0), "GetIndexFileState should return date read from the index files states file!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			finally
			{
				DeleteTemporaryIndexFilesStatesFile();
			}
		}

		private void CreateTemporaryIndexFilesStatesFile()
		{
			FileStream fileStream = null;
			StreamWriter streamWriter = null;
			try
			{
				fileStream = File.Create(filePath);
				streamWriter = new StreamWriter(fileStream);
				string fileContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
										"<ArrayOfIndexFileState xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
											"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
											"<IndexFileState>" +
												"<FilePath>C:/Windows/Temp/fileName.cs</FilePath>" +
												"<LastIndexingDate>2012-02-24T00:00:00</LastIndexingDate>" +
											"</IndexFileState>" +
										"</ArrayOfIndexFileState>";
				streamWriter.Write(fileContent);
			}
			finally
			{
				if(streamWriter != null)
					streamWriter.Close();
			}
		}

		private void DeleteTemporaryIndexFilesStatesFile()
		{
			File.Delete(filePath);
		}

		private void CreateTemporaryFile()
		{
			FileStream fileStream = null;
			try
			{
				fileStream = File.Create(temporaryFilePath);
			}
			finally
			{
				if(fileStream != null)
					fileStream.Close();
			}
		}

		private void DeleteTemporaryFile()
		{
			File.Delete(temporaryFilePath);
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

			filePath = Path.Combine(fileDirectory, fileName);
		}

		private string fileDirectory = "C:/Windows/Temp";
		private string fileName = "sandoindexfilesstates.xml";
		private string filePath;
		private string temporaryFilePath = "C:/Windows/Temp/fileName.cs";
		private bool contractFailed;
	}
}

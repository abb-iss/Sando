using System;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Indexer.IndexState;

namespace Sando.Indexer.UnitTests.IndexState
{
    [TestFixture]
	public class FileOperationResolverTest
	{
    	[Test]
		public void FileOperationResolver_ResolveRequiredOperationThrowsWhenPhysicalFileStateIsNull()
		{
			try
			{
				FileOperationResolver fileOperationResolver = new FileOperationResolver();
				fileOperationResolver.ResolveRequiredOperation(null, null);
			}
			catch 
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void FileOperationResolver_ResolveRequiredOperationReturnsOperationAddWhenIndexFileStateIsNull()
		{
			try
			{
				FileOperationResolver fileOperationResolver = new FileOperationResolver();
				PhysicalFileState physicalFileState = new PhysicalFileState("file path", DateTime.UtcNow);
				IndexOperation indexOperation = fileOperationResolver.ResolveRequiredOperation(physicalFileState, null);
				Assert.True(indexOperation == IndexOperation.Add, "ResolveRequiredOperation should return IndexOperation.Add when index file state is null!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void FileOperationResolver_ResolveRequiredOperationReturnsOperationUpdateWhenPhysicalAndIndexDatesAreDifferent()
		{
			try
			{
				FileOperationResolver fileOperationResolver = new FileOperationResolver();
				DateTime currentDate = DateTime.UtcNow;
				PhysicalFileState physicalFileState = new PhysicalFileState("file path", currentDate.AddHours(3));
				IndexFileState indexFileState = new IndexFileState("file path", currentDate);
				IndexOperation indexOperation = fileOperationResolver.ResolveRequiredOperation(physicalFileState, indexFileState);
				Assert.True(indexOperation == IndexOperation.Update, "ResolveRequiredOperation should return IndexOperation.Update when physical and index dates are different!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void FileOperationResolver_ResolveRequiredOperationReturnsOperationDoNothingWhenPhysicalAndIndexDatesAreTheSame()
		{
			try
			{
				FileOperationResolver fileOperationResolver = new FileOperationResolver();
				DateTime currentDate = DateTime.UtcNow;
				PhysicalFileState physicalFileState = new PhysicalFileState("file path", currentDate);
				IndexFileState indexFileState = new IndexFileState("file path", currentDate);
				IndexOperation indexOperation = fileOperationResolver.ResolveRequiredOperation(physicalFileState, indexFileState);
				Assert.True(indexOperation == IndexOperation.DoNothing, "ResolveRequiredOperation should return IndexOperation.DoNothing when physical and index dates are the same!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
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
		}

		private bool contractFailed;
	}
}

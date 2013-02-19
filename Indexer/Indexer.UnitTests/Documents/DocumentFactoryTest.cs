using System;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.UnitTestHelpers;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
	public class DocumentFactoryTest
	{
    	[Test]
		public void DocumentFactory_CreateReturnsClassDocumentForValidClassElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSampleClassElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is ClassDocument, "ClassDocument must be returned for ClassElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

        [Test]
        public void DocumentFactory_CreateReturnsStructDocumentForValidStructElement()
        {
            try
            {
                ProgramElement programElement = SampleProgramElementFactory.GetSampleStructElement();
                SandoDocument sandoDocument = DocumentFactory.Create(programElement);
                Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
                Assert.True(sandoDocument is StructDocument, "StructDocument must be returned for StructElement object!");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + ". " + ex.StackTrace);
            }
        }

		[Test]
		public void DocumentFactory_CreateReturnsCommentDocumentForValidCommentElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSampleCommentElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is CommentDocument, "CommentDocument must be returned for CommentElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentFactory_CreateReturnsEnumDocumentForValidEnumElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSampleEnumElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is EnumDocument, "EnumDocument must be returned for EnumElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentFactory_CreateReturnsFieldDocumentForValidFieldElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSampleFieldElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is FieldDocument, "FieldDocument must be returned for FieldElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentFactory_CreateReturnsMethodDocumentForValidMethodElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSampleMethodElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is MethodDocument, "MethodDocument must be returned for MethodElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentFactory_CreateReturnsPropertyDocumentForValidPropertyElement()
		{
			try
			{
				ProgramElement programElement = SampleProgramElementFactory.GetSamplePropertyElement();
				SandoDocument sandoDocument = DocumentFactory.Create(programElement);
				Assert.True(sandoDocument != null, "Null returned from DocumentFactory!");
				Assert.True(sandoDocument is PropertyDocument, "PropertyDocument must be returned for PropertyElement object!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentFactory_CreateThrowsContractExceptionIfProgramElementIsNull()
		{
			try
			{
				SandoDocument sandoDocument = DocumentFactory.Create(null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void DocumentFactory_CreateThrowsContractExceptionIfUnsportedProgramElementSubclassObjectPassed()
		{
			try
			{
				SandoDocument sandoDocument = DocumentFactory.Create(new TestElement("name", 12, "full path", "snippet"));
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
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

		private class TestElement : ProgramElement
		{
			public TestElement(string name, int definitionLineNumber, string fullFilePath, string snippet)
				: base(name, definitionLineNumber, fullFilePath, snippet)
			{
			}

			public override ProgramElementType ProgramElementType
			{
				get { throw new NotImplementedException(); }
			}
		}

		private bool contractFailed;
	}
}

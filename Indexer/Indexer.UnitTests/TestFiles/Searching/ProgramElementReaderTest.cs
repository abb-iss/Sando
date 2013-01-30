using Lucene.Net.Documents;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.UnitTestHelpers;
using UnitTestHelpers;

namespace Sando.Indexer.UnitTests.Searching
{
	[TestFixture]
	public class ProgramElementReaderTest
	{
		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidClassElementForValidDocument()
		{
			ClassElement element = SampleProgramElementFactory.GetSampleClassElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			ClassElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as ClassElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
			Assert.True(element.AccessLevel == returnedElement.AccessLevel, "AccessLevel is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(element.ExtendedClasses == returnedElement.ExtendedClasses, "ExtendedClasses is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.ImplementedInterfaces == returnedElement.ImplementedInterfaces, "ImplementedInterfaces is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.Namespace == returnedElement.Namespace, "Namespace is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}

		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidCommentElementForValidDocument()
		{
			CommentElement element = SampleProgramElementFactory.GetSampleCommentElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			CommentElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as CommentElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
            Assert.True("not stored in index" == returnedElement.Body, "AccessLevel is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}
		
		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidDocCommentElementForValidDocument()
		{
			DocCommentElement element = SampleProgramElementFactory.GetSampleDocCommentElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			DocCommentElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as DocCommentElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
            Assert.True("not stored in index" == returnedElement.Body, "Body is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(element.DocumentedElementId == returnedElement.DocumentedElementId, "DocumentedElementId is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}

		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidEnumElementForValidDocument()
		{
			EnumElement element = SampleProgramElementFactory.GetSampleEnumElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			EnumElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as EnumElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
			Assert.True(element.AccessLevel == returnedElement.AccessLevel, "AccessLevel is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.Namespace == returnedElement.Namespace, "Namespace is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
            Assert.True("not stored in index" == returnedElement.Body, "Body is different");
		}

		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidFieldElementForValidDocument()
		{
			FieldElement element = SampleProgramElementFactory.GetSampleFieldElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			FieldElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as FieldElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
			Assert.True(element.AccessLevel == returnedElement.AccessLevel, "AccessLevel is different!");
			Assert.True(element.ClassId == returnedElement.ClassId, "ClassId is different!");
			Assert.True(element.ClassName == returnedElement.ClassName, "ClassName is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(element.FieldType == returnedElement.FieldType, "FieldType is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}

		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidMethodElementForValidDocument()
		{
			MethodElement element = SampleProgramElementFactory.GetSampleMethodElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			MethodElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as MethodElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
			Assert.True(element.AccessLevel == returnedElement.AccessLevel, "AccessLevel is different!");
			Assert.True(element.Arguments == returnedElement.Arguments, "Arguments is different!");
            Assert.True("not stored in index" == returnedElement.Body, "Body is different!");
			Assert.True(element.ClassId == returnedElement.ClassId, "ClassId is different!");
			Assert.True(element.ClassName == returnedElement.ClassName.ToSandoDisplayable(), "ClassName is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.ReturnType == returnedElement.ReturnType, "ReturnType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}

		[Test]
		public void ProgramElementReader_ReadProgramElementFromDocumentReturnValidPropertyElementForValidDocument()
		{
			PropertyElement element = SampleProgramElementFactory.GetSamplePropertyElement();
			Document document = DocumentFactory.Create(element).GetDocument();

			PropertyElement returnedElement = ProgramElementReader.ReadProgramElementFromDocument(document) as PropertyElement;

			Assert.IsNotNull(returnedElement, "returned class element is null!");
			Assert.True(element.AccessLevel == returnedElement.AccessLevel, "AccessLevel is different!");
            Assert.True("not stored in index" == returnedElement.Body, "Body is different!");
			Assert.True(element.ClassId == returnedElement.ClassId, "ClassId is different!");
			Assert.True(element.ClassName == returnedElement.ClassName, "ClassName is different!");
			Assert.True(element.DefinitionLineNumber == returnedElement.DefinitionLineNumber, "DefinitionLineNumber is different!");
			Assert.True(StandardizeFilePath(element.FullFilePath) == returnedElement.FullFilePath, "FullFilePath is different!");
			Assert.True(element.Name == returnedElement.Name, "Name is different!");
			Assert.True(element.ProgramElementType == returnedElement.ProgramElementType, "ProgramElementType is different!");
			Assert.True(element.PropertyType == returnedElement.PropertyType, "PropertyType is different!");
			Assert.True(element.RawSource == returnedElement.RawSource, "Snippet is different!");
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		}

		private static string StandardizeFilePath(string fullFilePath)
		{
			if(fullFilePath.Contains("/"))
			{
				string old = "/";
				string rep = "\\";
				var path = fullFilePath.Replace(old, rep);
				return path;
			}
			return fullFilePath;
		}
	}
}

using System;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Documents;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
	public class DocumentIndexerTest
	{
		[Test]
		public void DocumentIndexer_ConstructorDoesNotThrowWhenValidData()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer documentIndexer = null;
			Assert.DoesNotThrow(() => documentIndexer = new DocumentIndexer("C:/Windows/Temp", analyzer));
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenInvalidDirectoryPath()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer documentIndexer = null;
			Assert.Throws(typeof(ArgumentException), () => documentIndexer = new DocumentIndexer(null, analyzer));
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenAnalyzerIsNull()
		{
			DocumentIndexer documentIndexer = null;
			Assert.Throws(typeof(ArgumentException), () => documentIndexer = new DocumentIndexer("C:/Windows/Temp", null));
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_AddDocumentDoesNotThrowWhenValidData()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer target = new DocumentIndexer("C:/Windows/Temp", analyzer);
			ClassElement classElement = new ClassElement()
										{
											AccessLevel = Core.AccessLevel.Public,
											DefinitionLineNumber = 11,
											ExtendedClasses = "SimpleClassBase",
											FileName = "SimpleClass.cs",
											FullFilePath = "C:/Projects/SimpleClass.cs",
											Id = Guid.NewGuid(),
											ImplementedInterfaces = "IDisposable",
											Name = "SimpleClassName",
											Namespace = "Sanod.Indexer.UnitTests"
										};
			SandoDocument sandoDocument = ClassDocument.Create(classElement);
			Assert.NotNull(sandoDocument);
			Assert.NotNull(sandoDocument.GetDocument());
			Assert.DoesNotThrow(() => target.AddDocument(sandoDocument));
			Assert.DoesNotThrow(() => target.CommitChanges());
			target.Dispose();
		}

		[Test]
		public void DocumentIndexer_AddDocumentThrowsWhenProgramElementIsNull()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer target = new DocumentIndexer("C:/Windows/Temp", analyzer);
			Assert.Throws(typeof(ArgumentException), () => target.AddDocument(null));
			target.Dispose();
		}
	}
}

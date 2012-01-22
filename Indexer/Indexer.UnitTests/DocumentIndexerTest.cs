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
		public void DocumentIndexerConstructorTest()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			Assert.DoesNotThrow(() => new DocumentIndexer("C:/Windows/Temp", analyzer).Dispose());
		}

		[Test]
		public void AddDocumentTest()
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
	}
}

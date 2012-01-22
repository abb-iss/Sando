using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Indexer.Documents;
using Sando.Core;

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
			ClassElement classElement = new ClassElement() { Name = "SimpleClassName" };
			SandoDocument sandoDocument = ClassDocument.Create(classElement);
			Assert.DoesNotThrow(() => target.AddDocument(sandoDocument));
			Assert.DoesNotThrow(() => target.CommitChanges());
			target.Dispose();
		}
	}
}

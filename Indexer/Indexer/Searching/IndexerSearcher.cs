using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Sando.Core;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	class IndexerSearcher: IIndexerSearcher
	{
	

		private IndexSearcher MySearcher
		{
			get; set; 
		}

		public IndexerSearcher(String directoryPath)
		{					
			//TODO - Change to Open(DirectoryInfo)
			Directory dir = FSDirectory.GetDirectory(directoryPath);									
			MySearcher = new IndexSearcher(dir,true);
		}

	

		#region Implementation of IIndexerSearcher

		public List<Tuple<ProgramElement, float>> Search(SearchCriteria searchCriteria)
		{
			//TODO - this doesn't really search, just returns a list of 10 methods so I can debug the UI
			var simple = searchCriteria as SimpleSearchCriteria;
			if(simple !=null)
			{
				List<Tuple<ProgramElement, float>> searchResults = new List<Tuple<ProgramElement, float>>();
				var query = new TermQuery(new Term("Body", ((SimpleSearchCriteria) simple).SearchTerms.First()));
				var query2 = new TermQuery(new Term("ProgramElementType", "Method"));
				var both = new BooleanQuery();
				both.Add(query,BooleanClause.Occur.MUST);
				both.Add(query2,BooleanClause.Occur.MUST);
				var methodDocuments = MySearcher.Search(both, 10);
				for (int i = 0; i < methodDocuments.ScoreDocs.Length; i++)
				{
					
					//TODO - need to return the real information, only thing being returned now is the "Name"
					var hitDocument = MySearcher.Doc(methodDocuments.ScoreDocs[i].doc);
					var score = methodDocuments.ScoreDocs[i].score;
					MethodElement methodElement = new MethodElement();
					methodElement.AccessLevel = AccessLevel.Public;
					methodElement.Arguments = "";
					methodElement.Body = "the body";
					methodElement.ClassId = Guid.Empty;
					methodElement.DefinitionLineNumber = 0;
					methodElement.FullFilePath = "full path";
					methodElement.Id = Guid.Empty;
					methodElement.Name = hitDocument.GetField("Name").StringValue();
					methodElement.ReturnType = "Object";
					searchResults.Add(Tuple.Create(methodElement as ProgramElement, score));
						
				}
				return searchResults;
			}
			return new List<Tuple<ProgramElement, float>>();
		}

		#endregion

	
	}

	public class IndexerSearcherFactory
	{
		public static IIndexerSearcher CreateSearcher(String directoryPath)
		{
			return new IndexerSearcher(directoryPath);	
		}
	}
}

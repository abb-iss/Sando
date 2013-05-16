using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Sando.Indexer.Documents;
using Lucene.Net.Analysis;
using System.IO;
using Lucene.Net.Analysis.Tokenattributes;


namespace Sando.Indexer.Searching.Metrics
{
	public class PreRetrievalMetrics
	{
		public PreRetrievalMetrics(IndexReader indexReader, Analyzer analyzer)
		{
			Reader = indexReader;
            Stemmer = analyzer;
		}

		#region specificity

		public double AvgIdf(string query)
		{
			double SumIdf = 0.0;
            query = StemText(query);
            string[] terms = query.Split(' ');
			foreach(var term in terms)
			{
				SumIdf += Idf(term);
			}
			return (SumIdf / terms.Length);
		}

		public double MaxIdf(string query)
		{
			double MaxIdf = Double.MinValue;
			query = StemText(query);
			string[] terms = query.Split(' ');
			foreach(var term in terms)
			{
				if(MaxIdf < Idf(term)) MaxIdf = Idf(term);
			}
			return MaxIdf;
		}


		public double DevIdf(string query)
		{
			double diff = 0.0;
            query = StemText(query);
			double avgIdf = AvgIdf(query);
			string[] terms = query.Split(' ');
			foreach(var term in terms)
			{
				diff += Math.Pow(Idf(term) - avgIdf, 2.0);
			}
			return Math.Sqrt(diff / terms.Length);
		}

		#endregion

		#region similarity

		public double AvgSqc(string query)
		{
			double SumSqc = 0.0;
            query = StemText(query);
			string[] terms = query.Split(' ');
			foreach(var term in terms)
			{
				double tfCorp = TfOfCorpus(term);
				double idf = Idf(term);
				if(tfCorp > 0.0)
				{
					SumSqc += ((1 + Math.Log(tfCorp)) * idf);
				}
				else
				{
					SumSqc += 1 * idf;
				}
			}
			return (SumSqc / terms.Length);
		}

		#endregion

		#region coherency

		public double AvgVar(string query)
		{
			double AvgVar = 0.0;
            query = StemText(query);
			string[] terms = query.Split(' ');
			foreach(var term in terms)
			{
				AvgVar += Var(term);
			}
			return (AvgVar / terms.Length);
		}

        private double Var(string term)
        {
            int num_docs = 0;
            List<double> freqs = new List<double>();
            TermDocs termDocs = Reader.TermDocs(new Term(SandoField.Name.ToString(), term));
            if (termDocs != null)
            {
                while (termDocs.Next())
                {
                    num_docs++;
                    freqs.Add(termDocs.Freq());
                }
            }

            double var = 0.0;
            if (freqs.Count > 0)
            {
                List<double> weights = new List<double>();
                foreach (var freq in freqs)
                {
                    weights.Add((Math.Log(1 + freq) * Idf(term)) / num_docs);
                }

                double avg_w = 0.0;
                foreach (var w in weights)
                {
                    avg_w += w;
                }
                avg_w = avg_w / num_docs;

                foreach (var w in weights)
                {
                    var += Math.Abs(w - avg_w);
                }
                var = var / num_docs;
            }
            return var;
        }

		#endregion

		private int TfOfCorpus(string term)
		{
			int tf = 0;
			TermDocs termDocs = Reader.TermDocs(new Term(SandoField.Name.ToString(), term));
			if(termDocs != null)
			{
				//tf += termDocs.Freq();
				while(termDocs.Next())
				{
					tf += termDocs.Freq();
				}
			}
			return tf;
		}

		private double Idf(string term)
		{
			double idf = 0.0;
			int nameDocFreq = Reader.DocFreq(new Term(SandoField.Name.ToString(), term));
			int bodyDocFreq = Reader.DocFreq(new Term(SandoField.Body.ToString(), term));
			int docFreq = nameDocFreq + bodyDocFreq;
			if(docFreq > 0)
			{
				idf = Math.Log(Reader.NumDocs() / docFreq);
			}
			return idf;
		}

        public string StemText(string text) 
        {
            string result = "";
            TokenStream stream  = Stemmer.TokenStream(null, new StringReader(text));

            while(stream.IncrementToken()) {
                TermAttribute termAttr = (TermAttribute) stream.GetAttribute(typeof(TermAttribute));
                result = result + termAttr.Term() + " "; 
            }
            
            return result.Trim();
        }  

		private IndexReader Reader;
        private Analyzer Stemmer;
	}
}

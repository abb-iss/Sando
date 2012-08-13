using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Core.Extensions.PairedInterleaving
{
	public class PairedInterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public PairedInterleavingManager()
		{
			LogCount = 0;
            ClickIdx = new List<int>();
            IsLogEntryReady = false;
		}

		public string RewriteQuery(string query)
		{
			//write a log entry for the previous query (now that the clicking has completed for it)
            if (IsLogEntryReady)
            {
                LogCount++;
                int scoreA, scoreB;
                DetermineWinner(SandoResults, SecondaryResults, InterleavedResults, ClickIdx, out scoreA, out scoreB);
                //write to file...
            }

			//capture the query and reissue it to the secondary FLT getting the secondary results

			//check the number of log entires collected and decide whether to push the log to S3
            if (LogCount >= LOG_ENTRIES_PER_FILE)
            {
                //...
            }

            return query;
		}

		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
            SandoResults = searchResults.ToList();
            InterleavedResults = BalancedInterleave(searchResults.ToList(), SecondaryResults);
            return InterleavedResults.AsQueryable(); 
        }

		//called from UI.FileOpener
		public void NotifyClicked(CodeSearchResult clickedElement)
		{
            if (InterleavedResults != null && InterleavedResults.Count > 0)
            {
                ClickIdx.Add(InterleavedResults.IndexOf(clickedElement));
                IsLogEntryReady = true;
            }
		}

        public List<CodeSearchResult> BalancedInterleave(List<CodeSearchResult> A, List<CodeSearchResult> B)
        {
            List<CodeSearchResult> I = new List<CodeSearchResult>();
            int Ka = 0, Kb = 0;

            Random random = new Random();
            bool AFirst = (random.NextDouble() >= 0.5);

            while (Ka < A.Count && Kb < B.Count)
            {
                if (Ka < Kb || (Ka == Kb && AFirst == true))
                {
                    if (!I.Contains(A[Ka]))
                    {
                        I.Add(A[Ka]);
                    }
                    Ka++;
                }
                else
                {
                    if (!I.Contains(B[Kb]))
                    {
                        I.Add(B[Kb]);
                    }
                    Kb++;
                }
            }

            return I;
        }

        public void DetermineWinner(List<CodeSearchResult> A, List<CodeSearchResult> B, List<CodeSearchResult> I, 
                                    List<int> C, out int scoreA, out int scoreB)
        {
            scoreA = 0; 
            scoreB = 0;

            if (C.Count <= 0) return;

            int Cmax = C.Max();
            int Pa = A.IndexOf(I[Cmax]);
            int Pb = B.IndexOf(I[Cmax]);
            if (Pa == -1) Pa = Int32.MaxValue;
            if (Pb == -1) Pb = Int32.MaxValue;
            int K = Math.Min(Pa, Pb);

            foreach (int c in C)
            {
                int Ha = A.IndexOf(I[c]);
                int Hb = B.IndexOf(I[c]);
                if (Ha == -1) Ha = Int32.MaxValue;
                if (Hb == -1) Hb = Int32.MaxValue;

                if (Ha < Hb)
                {
                    scoreA++;
                }
                else if (Hb < Ha)
                {
                    scoreB++;
                }
            }
        }

        private readonly int LOG_ENTRIES_PER_FILE = 50;
        private readonly string FLT_A_NAME = "Sando";
        private readonly string FLT_B_NAME = "Lex";

        private List<CodeSearchResult> SecondaryResults;
        private List<CodeSearchResult> SandoResults;
        private bool IsLogEntryReady;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}

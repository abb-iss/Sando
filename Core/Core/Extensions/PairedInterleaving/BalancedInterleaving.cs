using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Core.Extensions.PairedInterleaving
{
	public static class BalancedInterleaving
	{
		public static List<CodeSearchResult> Interleave(List<CodeSearchResult> A, List<CodeSearchResult> B)
		{
			List<CodeSearchResult> I = new List<CodeSearchResult>();
			int Ka = 0, Kb = 0;

			Random random = new Random();
			bool AFirst = (random.NextDouble() >= 0.5);

			while(Ka < A.Count && Kb < B.Count)
			{
				if(Ka < Kb || (Ka == Kb && AFirst == true))
				{
					if(!I.Contains(A[Ka]))
					{
						I.Add(A[Ka]);
					}
					Ka++;
				}
				else
				{
					if(!I.Contains(B[Kb]))
					{
						I.Add(B[Kb]);
					}
					Kb++;
				}
			}

			return I;
		}


		public static void DetermineWinner(List<CodeSearchResult> A, List<CodeSearchResult> B, List<CodeSearchResult> I,
											List<int> C, out int scoreA, out int scoreB)
		{
			scoreA = 0;
			scoreB = 0;

			if(C.Count <= 0) return;

			int Cmax = C.Max();
			int Pa = A.IndexOf(I[Cmax]);
			int Pb = B.IndexOf(I[Cmax]);
			if(Pa == -1) Pa = Int32.MaxValue;
			if(Pb == -1) Pb = Int32.MaxValue;
			int K = Math.Min(Pa, Pb);

			foreach(int c in C)
			{
				int Ha = A.IndexOf(I[c]);
				int Hb = B.IndexOf(I[c]);
				if(Ha == -1) Ha = Int32.MaxValue;
				if(Hb == -1) Hb = Int32.MaxValue;

				if(Ha < Hb)
				{
					scoreA++;
				}
				else if(Hb < Ha)
				{
					scoreB++;
				}
			}
		}
	}
}

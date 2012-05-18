using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Sando.ExperimentalExtensions.RelevanceFeedbackExtension
{
	public class RFRankGenerator
	{
		private const string SvmRankLearnExe = "\\svm_rank_learn.exe";
		private const string SvmRankClassifyExe = "\\svm_rank_classify.exe";
    	private string SvmRankFolderPath;

		public int C { get; set; }

		public RFRankGenerator(string location)
		{
			Contract.Ensures(System.IO.File.Exists(SvmRankFolderPath + SvmRankLearnExe), "svm_rank_train.exe cannot be found");
			Contract.Ensures(System.IO.File.Exists(SvmRankFolderPath + SvmRankClassifyExe), "svm_rank_classify.exe cannot be found");

			SvmRankFolderPath = location;
			C = 20; //TODO: rethink this default?
		}

		public void GenerateModel(string trainFile, string modelFile)
		{
			Contract.Requires(System.IO.File.Exists(trainFile), "RFExtension:RFRankGenerator - train file does not exist");
			Contract.Ensures(System.IO.File.Exists(modelFile), "RFExtension:RFRankGenerator - didn't successfully generate the model file");

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = false;
			startInfo.RedirectStandardInput = false;
			startInfo.FileName = SvmRankFolderPath + SvmRankLearnExe;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = "-c " + C + " \"" + trainFile + "\" \"" + modelFile + "\"";

			try
			{
				using(Process exeProcess = Process.Start(startInfo))
				{
					exeProcess.WaitForExit();
				}
			}
			catch
			{
			}
		}

		public void GenerateRanking(string inputFile, string modelFile, string outputFile)
		{
			Contract.Requires(System.IO.File.Exists(inputFile), "RFExtension:RFRankGenerator - input file does not exist");
			Contract.Requires(System.IO.File.Exists(modelFile), "RFExtension:RFRankGenerator - model file does not exist");
			Contract.Ensures(System.IO.File.Exists(outputFile), "RFExtension:RFRankGenerator - didn't successfully generate the output file");

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = false;
			startInfo.RedirectStandardInput = false;
			startInfo.FileName = SvmRankFolderPath + SvmRankClassifyExe;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = "\"" + inputFile + "\" \"" + modelFile + "\" \"" + outputFile + "\"";

			try
			{
				using(Process exeProcess = Process.Start(startInfo))
				{
					exeProcess.WaitForExit();
				}
			}
			catch
			{
			}
		}

	}
}

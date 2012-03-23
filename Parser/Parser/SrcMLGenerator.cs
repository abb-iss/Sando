using System;
using System.Diagnostics;
using Sando.Translation;
using System.Text.RegularExpressions;

namespace Sando.Parser
{
	public enum LanguageEnum
	{
		Java,
		C,
		CPP,
		CSharp
	};

    public class SrcMLGenerator
    {
		
    	private const string Src2SrcmlExe = "\\src2srcml.exe";
    	private string SrcMLFolderPath;

		public LanguageEnum Language { get; set; }

		public SrcMLGenerator()
		{
			Language = LanguageEnum.CSharp;
		}

		public void SetSrcMLLocation(string location)
		{
			SrcMLFolderPath = location;

			if(!System.IO.File.Exists(SrcMLFolderPath+Src2SrcmlExe))
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "sr2srcml.exe cannot be found. looking in: " + SrcMLFolderPath);
			}
		}
		

		//
		// run srcML and return the generated sourceXML as a string
		//
		public string GenerateSrcML(string filename)
		{
			//check whether filename exists
			if(!System.IO.File.Exists(filename))
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "parser input file name does not exist: " + filename);
			}

			return LaunchSrcML(filename);
		}

		
		private string LaunchSrcML(string filename)
		{
			string srcML = "";

			string inputCode = System.IO.File.ReadAllText(filename);
			string langText = Language.ToString();

			if(Language == LanguageEnum.CSharp)
			{
				//temporary, otherwise very ugly
				inputCode = AdaptCSharpToJavaParsing(inputCode);
				langText = "Java";
			}
			else if(Language == LanguageEnum.CPP)
			{
				langText = "C++";
			}


			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardInput = true;
			startInfo.FileName = SrcMLFolderPath + Src2SrcmlExe;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = "--position -l "+langText;

			try
			{
				using (Process exeProcess = Process.Start(startInfo))
				{
					System.IO.StreamReader sOut = exeProcess.StandardOutput;
					System.IO.StreamWriter sIn = exeProcess.StandardInput;
					sIn.Write(inputCode);
					sIn.Close();
					srcML = sOut.ReadToEnd();
					exeProcess.WaitForExit();
					sOut.Close();
				}
			}
			catch
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "sr2srcml.exe execution error, check parameters");
			}

			return srcML;
		}

		private string AdaptCSharpToJavaParsing(string inputCode)
		{
			//replace ':' with extends in class definitions 
			inputCode = Regex.Replace(inputCode, @"class (\w+) : (\w+)", "class $1 implements $2");

			//erase #region #endregion lines
			inputCode = Regex.Replace(inputCode, @"#region(\w| )*", "");
			inputCode = Regex.Replace(inputCode, @"#endregion", "");

			return inputCode;
		}


    }
}

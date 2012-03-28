using System.Diagnostics;
using System.Text.RegularExpressions;
using Sando.Translation;

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
			inputCode = Regex.Replace(inputCode, @"class\s*(\w+)\s*:\s*(\w+)", "class $1 implements $2");

			//erase #region #endregion lines
			inputCode = Regex.Replace(inputCode, @"#region[\w\. ]*", "");
			inputCode = Regex.Replace(inputCode, @"#endregion", "");

			//place semicolons after set and get in C# properties
			inputCode = Regex.Replace(inputCode, @"set\s*{", "set; {");
			inputCode = Regex.Replace(inputCode, @"get\s*{", "get; {");

			//converting foreach C# loops into regular fors (java 5 capability not yet in srcml)
			inputCode = Regex.Replace(inputCode, @"foreach\s*\((\w+)\s*(\w+)\s*in\s*(\w+)\)", "for ($1 $2; $3; )");

			//removing attributes
			//note: this is probably too aggresive, so array indices etc. would also be clobbered
			inputCode = Regex.Replace(inputCode, @"\[[^\[]*\]", "");

			return inputCode;
		}


    }
}

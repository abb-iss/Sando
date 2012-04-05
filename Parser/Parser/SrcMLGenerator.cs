using System.Diagnostics;
using System.Text.RegularExpressions;
using Sando.Translation;
using System.ComponentModel;
using System;

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

        public static SrcMLGenerator Generator(LanguageEnum language)
        {
            return new SrcMLGenerator(language);
        }

        public SrcMLGenerator(LanguageEnum language)
        {
            Language = language;
        }

		public SrcMLGenerator()
		{
			Language = LanguageEnum.CSharp;
		}

		public SrcMLGenerator SetSrcMLLocation(string location)
		{
			SrcMLFolderPath = location;

			if(!System.IO.File.Exists(SrcMLFolderPath+Src2SrcmlExe))
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "sr2srcml.exe cannot be found. looking in: " + SrcMLFolderPath);
			}
		    return this;
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
			string langText = Language.ToString();

			if(Language == LanguageEnum.CSharp)
			{
				//temporary, otherwise very ugly
				string allCode = System.IO.File.ReadAllText(filename);
				allCode = AdaptCSharpToJavaParsing(allCode);
				filename = filename + ".tmp";
				System.IO.File.WriteAllText(filename, allCode);
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
			startInfo.Arguments = "--position -l " + langText + " " + filename;

			try
			{
				using (Process exeProcess = Process.Start(startInfo))
				{
					System.IO.StreamReader sOut = exeProcess.StandardOutput;
					System.IO.StreamWriter sIn = exeProcess.StandardInput;
					sIn.Close();

					var _readInputInBackground = new System.ComponentModel.BackgroundWorker();
					_readInputInBackground.DoWork += (s, e) => _readInputInBackground_DoWork(sOut, out srcML);
					_readInputInBackground.RunWorkerAsync();

					exeProcess.WaitForExit();

					_readInputInBackground.Dispose();
					sOut.Close();
				}
			}
			catch
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "sr2srcml.exe execution error, check parameters");
			}


			//erase the temp file we generate for csharp parsing
			if(Language == LanguageEnum.CSharp)
			{
				System.IO.File.Delete(filename);
			}

			return srcML;
		}

		private void _readInputInBackground_DoWork(System.IO.StreamReader sOut, out string srcML)
		{
			srcML = "";
			try
			{
				while(true)
				{
					srcML += sOut.ReadToEnd();
				}
			}
			catch(Exception e)
			{
				//this will happen, so do nothing
			}
		}

		private string AdaptCSharpToJavaParsing(string inputCode)
		{
			//replace ':' with extends in class definitions 
			inputCode = Regex.Replace(inputCode, @"class(\s*\w+\s*):(\s*\w+)", "class$1implements$2");

			//erase #region #endregion lines
			inputCode = Regex.Replace(inputCode, @"#region[\w\. ]*", "");
			inputCode = Regex.Replace(inputCode, @"#endregion", "");

			//place semicolons after set and get in C# properties
			inputCode = Regex.Replace(inputCode, @"set(\s*){", "set;$1{");
			inputCode = Regex.Replace(inputCode, @"get(\s*){", "get;$1{");

			//converting foreach C# loops into regular fors (java 5 capability not yet in srcml)
			inputCode = Regex.Replace(inputCode, @"foreach[ ]*\((\w+\s*)(\w+\s*)in(\s*\w+)\)", "for ($1 $2; $3; )");

			//removing attributes (hack!!!)
			//---entire attribute is on one line
			inputCode = Regex.Replace(inputCode, @"^[ \t]*\[[^\[\r\n]*\][ \t\r\n]*$", "", RegexOptions.Multiline);
			//---attribute is comma delimited on multiple lines (preserve spaces)
			inputCode = Regex.Replace(inputCode, @"^[ \t]*\[[^,\r\n]*,[ \t\r\n]*$", "", RegexOptions.Multiline);
			inputCode = Regex.Replace(inputCode, @"^[ \t]*[^\]\r\n]*\][ \t\r\n]*$", "", RegexOptions.Multiline);

			  return inputCode;
		}


    }
}

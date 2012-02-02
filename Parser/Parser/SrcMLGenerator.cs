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
		CSharp,
		FakeJava
	};

    public class SrcMLGenerator
    {
		
    	private const string Src2SrcmlExe = "\\src2srcml.exe";
    	private String SrcMLFolderPath;
    	private LanguageEnum Language;

		public SrcMLGenerator()
		{
			Language = LanguageEnum.FakeJava;
		}

		public void SetLanguage(LanguageEnum language)
		{
			Language = language;
			//temporary
			if(language==LanguageEnum.CSharp)
				Language = LanguageEnum.FakeJava;
		}

		public void SetSrcMLLocation(String location)
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
		public String GenerateSrcML(String filename)
		{
			//check whether filename exists
			if(!System.IO.File.Exists(filename))
			{
				throw new ParserException(TranslationCode.Exception_General_IOException, "parser input file name does not exist: " + filename);
			}

			return LaunchSrcML(filename);
		}

		
		private String LaunchSrcML(String filename)
		{
			string srcML = "";

			String inputCode = System.IO.File.ReadAllText(filename);
			//temporary, otherwise very ugly
			if(Language == LanguageEnum.FakeJava)
			{
				inputCode = AdaptCSharpToJavaParsing(inputCode);
				Language = LanguageEnum.Java;
			}


			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardInput = true;
			startInfo.FileName = SrcMLFolderPath + Src2SrcmlExe;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = "--position -l "+Language.ToString();

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

		private String AdaptCSharpToJavaParsing(String inputCode)
		{
			//replace ':' with extends in class definitions 
			return Regex.Replace(inputCode, @"class (\w+) : (\w+)", "class $1 implements $2");
		}


    }
}

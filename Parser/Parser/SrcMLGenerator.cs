using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Sando.Core.Extensions.Logging;
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
		private volatile bool _srcMLExecComplete = false;

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

        [MethodImpl(MethodImplOptions.Synchronized)]
        private string LaunchSrcML(string filename)
        { 
            string srcML = "";
            string langText = Language.ToString();
            var tmpFilename = filename;

            if (Language == LanguageEnum.CSharp)
            {
                tmpFilename = filename + ".tmp";

                string allCode = System.IO.File.ReadAllText(filename);
                allCode = AdaptCSharpToJavaParsing(allCode);
                tmpFilename = filename + ".tmp";
                System.IO.File.WriteAllText(tmpFilename, allCode);

                //StreamReader reader = (new FileInfo(filename)).OpenText();
                //string line;
                //var writer = new StreamWriter((new FileInfo(tmpFilename)).OpenWrite());
                //while ((line = reader.ReadLine()) != null)
                //{
                //    string adaptCSharpToJavaParsing = AdaptCSharpToJavaParsing(line);
                //    writer.Write(adaptCSharpToJavaParsing + "\r\n");
                //}
                //writer.Flush();
                //writer.Close();
                langText = "Java";
            }
            else if (Language == LanguageEnum.CPP)
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
            startInfo.Arguments = "--position -l " + langText + " \"" + tmpFilename + "\"";

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    System.IO.StreamReader sOut = exeProcess.StandardOutput;
                    System.IO.StreamWriter sIn = exeProcess.StandardInput;
                    sIn.Close();

                    var readInputThread = new Thread(new ThreadStart(() => _readInput_DoWork(sOut, out srcML)));
                    readInputThread.Name = "SrcML";
                    readInputThread.Start();

                    exeProcess.WaitForExit();

                    _srcMLExecComplete = true;
                    readInputThread.Join();
                    if (!sOut.EndOfStream) srcML += sOut.ReadToEnd();
                    sOut.Close();
                }
            }
            catch (Exception ex)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(ex));
                throw new ParserException(TranslationCode.Exception_General_IOException, "sr2srcml.exe execution error, check parameters");
            }

            //erase the temp file we generate for csharp parsing
            if (Language == LanguageEnum.CSharp)
            {
                System.IO.File.Delete(tmpFilename);
            }

            return srcML;
        }

		private void _readInput_DoWork(System.IO.StreamReader sOut, out string srcML)
		{
			bool keepGoing = true;

			srcML = "";
			while(keepGoing)
			{
				if(_srcMLExecComplete) keepGoing = false;
				srcML += sOut.ReadToEnd();
			}
			
		}

		private string AdaptCSharpToJavaParsing(string inputCode)
		{
			//replace ':' with extends in class definitions 
			inputCode = Regex.Replace(inputCode, @"([class|struct])(\s*\w+\s*):(\s*\w+)", "$1$2 implements $3");
            
            //remove generics
            inputCode = Regex.Replace(inputCode, "<([\\w_]*)>", "$1");            

			//erase #region #endregion lines
            inputCode = Regex.Replace(inputCode, @"#[ \t]*region([\w\. _])*", "");
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

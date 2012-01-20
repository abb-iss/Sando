using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sando.Parser
{
    public class SrcMLGenerator
    {
		//
		// run srcML and return the generated sourceXML as a string
		//
		public static String generateSrcML(String filename)
		{
			//check whether filename exists
			if(!System.IO.File.Exists(filename))
			{
				throw new ParserException("parser input file name does not exist: " + filename);
			}

			return launchSrcML(filename);
		}

		
		private static String launchSrcML(String filename)
		{
			string srcML = "";

			string srcMLExecutable = Environment.CurrentDirectory;
			int idx = srcMLExecutable.IndexOf("sando");
			srcMLExecutable = srcMLExecutable.Remove(idx + "sando".Length);
			srcMLExecutable = srcMLExecutable + "\\LIBS\\srcML-Win\\src2srcml.exe";
			if(!System.IO.File.Exists(srcMLExecutable))
			{
				throw new ParserException("sr2srcml.exe cannot be found. looking in: " + srcMLExecutable);
			}

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.FileName = srcMLExecutable;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = "-l Java " + filename ;

			try
			{
				using (Process exeProcess = Process.Start(startInfo))
				{
					System.IO.StreamReader sOut = exeProcess.StandardOutput;
					srcML = sOut.ReadToEnd();
					exeProcess.WaitForExit();
				}
			}
			catch
			{
				throw new ParserException("sr2srcml.exe execution error, check parameters");
			}

			return srcML;
		}


    }
}

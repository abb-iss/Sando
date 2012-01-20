using System;
using System.Diagnostics;

namespace Sando.Parser
{
    public class SrcMLGenerator
    {
    	private const string Src2SrcmlExe = "\\src2srcml.exe";

    	private String SrcMLFolderPath;

		public void SetSrcMLLocation(String location)
		{
			SrcMLFolderPath = location;

			if(!System.IO.File.Exists(SrcMLFolderPath+Src2SrcmlExe))
			{
				throw new ParserException("sr2srcml.exe cannot be found. looking in: " + SrcMLFolderPath);
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
				throw new ParserException("parser input file name does not exist: " + filename);
			}

			return LaunchSrcML(filename);
		}

		
		private String LaunchSrcML(String filename)
		{
			string srcML = "";

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.FileName = SrcMLFolderPath + Src2SrcmlExe;
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

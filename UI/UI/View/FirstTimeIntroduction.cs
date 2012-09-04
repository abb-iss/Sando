using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sando.Core.Extensions.Logging;

namespace Sando.UI.View
{
    public class FirstTimeIntroduction
    {
        private string DirectoryPath;
        private const string INTRO = "\\.intro";
        private bool UpdatedDuringThisRun = false;

        public FirstTimeIntroduction(string path)
        {
            DirectoryPath = path;
        }

        public bool ShouldIntroduce()
        {
            if(File.Exists(DirectoryPath+INTRO))
            {
                var lastWrite = File.GetLastWriteTime(DirectoryPath + INTRO);
                var now = DateTime.Now;
                if (TooLongSinceLastSandoUsage(lastWrite, now))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;            
        }

        public void Introduced()
        {
            try
            {
                File.Create(DirectoryPath + INTRO);
            }catch(Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e));
            }
            Update();
        }

        public void Update()
        {
            if (!UpdatedDuringThisRun)
            {
                string path = DirectoryPath + INTRO;
                if (File.Exists(path))
                {
                    try
                    {
                        File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                    }
                    catch (Exception e)
                    {
                        FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e));
                    }
                    UpdatedDuringThisRun = true;
                }
            }
        }

        private static bool TooLongSinceLastSandoUsage(DateTime lastWrite, DateTime now)
        {
            return now.Subtract(lastWrite).TotalDays>4;
        }
    }
}

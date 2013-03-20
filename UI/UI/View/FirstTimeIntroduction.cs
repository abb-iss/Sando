using System;
using System.IO;
using Sando.Core.Logging;
using Sando.Core.Logging.Persistence;

namespace Sando.UI.View
{
    public class FirstTimeIntroduction
    {
        private readonly string _directoryPath;
        private const string Intro = "\\.intro";
        private bool _updatedDuringThisRun;

        public FirstTimeIntroduction(string path)
        {
            _directoryPath = path;
        }

        public bool ShouldIntroduce()
        {
            if(File.Exists(_directoryPath+Intro))
            {
                var lastWrite = File.GetLastWriteTime(_directoryPath + Intro);
                var now = DateTime.Now;
                return TooLongSinceLastSandoUsage(lastWrite, now);
            }
            return true;            
        }

        public void Introduced()
        {
            try
            {
                File.Create(_directoryPath + Intro);
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e));
            }
            Update();
        }

        public void Update()
        {
            if (!_updatedDuringThisRun)
            {
                string path = _directoryPath + Intro;
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
                    _updatedDuringThisRun = true;
                }
            }
        }

        private static bool TooLongSinceLastSandoUsage(DateTime lastWrite, DateTime now)
        {
            return now.Subtract(lastWrite).TotalDays>4;
        }
    }
}

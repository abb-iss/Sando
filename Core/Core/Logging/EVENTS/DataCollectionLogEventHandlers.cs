using log4net;
using Sando.Core.Logging.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
    public static class DataCollectionLogEventHandlers
    {
        public static void InitializeLogFile(ILog logger)
        {
            Logger = logger;
        }

        public static void RegisterLogEventHandlers()
        {
            DataCollectionLogEvents.Event_SolutionOpened += new EventHandler<EventArgs<string>>(Handler_SolutionOpened);
        }

        public static void UnregisterLogEventHandlers()
        {
            DataCollectionLogEvents.Event_SolutionOpened -= new EventHandler<EventArgs<string>>(Handler_SolutionOpened);
        }

        private static void Handler_SolutionOpened(Object sender, EventArgs<string> solutionNameArg)
        {
            WriteInfoLogMessage(sender.GetType().ToString(), solutionNameArg.Value);
        }

        private static void WriteInfoLogMessage(string sendingType, string message)
        {
            Logger.Info(sendingType + ": " + message);
        }

        private static ILog Logger;
    }
}

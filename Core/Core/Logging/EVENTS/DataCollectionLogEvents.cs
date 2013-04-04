using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
    public class DataCollectionLogEvents
    {
        public static void SolutionOpened(Object sender, string solutionName)
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Message from the logger");
        }
    }
}

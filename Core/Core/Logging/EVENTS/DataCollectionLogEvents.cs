using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
    public class DataCollectionLogEvents
    {
        public static event EventHandler<EventArgs<string>> Event_SolutionOpened;

        public static void SolutionOpened(Object sender, string solutionName)
        {
            if (Event_SolutionOpened != null) Event_SolutionOpened(sender, new EventArgs<string>(solutionName));
        }
    }
}

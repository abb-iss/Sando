using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public abstract class ProgramElement
    {
        public abstract String Name { get; set; }
        public abstract String SummaryText { get; set; }
    }
}

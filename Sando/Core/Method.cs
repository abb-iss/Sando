using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Method: ProgramElement
    {
        public override String Name
        {
            get { return "methodname"; }
            set { }
        }

        public override String SummaryText
        {
            get { return "private void GenNextGraphs(object sender, EventArgs e){\n   this.Dispatcher.BeginInvoke(//System.Windows.Threading.DispatcherPriority.Background,\n     new UpdateGraphDelegate(UpdateGraph), new Object[] { _searchTerm });\n  this.Dispatcher.BeginInvoke(//System.Windows.Threading.DispatcherPriority.Background,\n     new ZoomerDelegate(UpdateZoom));   \n}"; }
            set { }
        }
    }
}

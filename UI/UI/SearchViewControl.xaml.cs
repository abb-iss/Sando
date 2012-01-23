using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sando.Core;

namespace Sando.UI
{
    /// <summary>
    /// Interaction logic for SearchViewControl.xaml
    /// </summary>
    public partial class SearchViewControl : UserControl
    {
    	public SearchViewControl()
        {
            this.DataContext = this; //so we can show results
            InitializeFakeData();
            InitializeComponent();            
        }


		private void InitializeFakeData()
        {
			string code = "private void GenNextGraphs(object sender, EventArgs e){\n   this.Dispatcher.BeginInvoke(//System.Windows.Threading.DispatcherPriority.Background,\n     new UpdateGraphDelegate(UpdateGraph), new Object[] { _searchTerm });\n  this.Dispatcher.BeginInvoke(//System.Windows.Threading.DispatcherPriority.Background,\n     new ZoomerDelegate(UpdateZoom));   \n}"; 

            SearchResults = new List<ProgramElement>();
        	var MethodElement = new MethodElement();
        	MethodElement.Name = "Add";
			//MethodElement.ContainerName = "List";
			//MethodElement.FileName = "List.cs";
			//MethodElement.SummaryText = code;
			SearchResults.Add(MethodElement);

        	var item = new MethodElement();
			item.Name = "Add";
			//item.ContainerName = "List";
			//item.FileName = "List.cs";
			//item.SummaryText = code;
			SearchResults.Add(item);

        	var MethodElement1 = new MethodElement();
			MethodElement1.Name = "Add";
			//MethodElement1.ContainerName = "List";
			//MethodElement1.FileName = "List.cs";
			//MethodElement1.SummaryText = code;
			SearchResults.Add(MethodElement1);

        	var item1 = new MethodElement();
			item1.Name = "Add";
			//item1.ContainerName = "List";
			//item1.FileName = "List.cs";
			//item1.SummaryText = code;
			SearchResults.Add(item1);

        	var MethodElement2 = new MethodElement();
			MethodElement2.Name = "Add";
			//MethodElement2.ContainerName = "List";
			//MethodElement2.FileName = "List.cs";
			//MethodElement2.SummaryText = code;
			SearchResults.Add(MethodElement2);  
  
			
         
        }

    	public List<ProgramElement> SearchResults { get; private set; }

    

		

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "Sando Search");

        }
    }
}
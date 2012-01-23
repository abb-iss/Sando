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
			var method = new MethodElement()
						{
							Name = "Add",
							Arguments = "object sender",
							ReturnType = "void",
							Id = Guid.NewGuid(),
							ClassId = Guid.NewGuid(),
							AccessLevel = AccessLevel.Public,
							Body = code
						};
			SearchResults.Add(method);

			method = new MethodElement()
						{
							Name = "Add",
							Arguments = "object sender",
							ReturnType = "void",
							Id = Guid.NewGuid(),
							ClassId = Guid.NewGuid(),
							AccessLevel = AccessLevel.Public,
							Body = code
						};
			SearchResults.Add(method);


			method = new MethodElement()
						{
							Name = "Add",
							Arguments = "object sender",
							ReturnType = "void",
							Id = Guid.NewGuid(),
							ClassId = Guid.NewGuid(),
							AccessLevel = AccessLevel.Public,
							Body = code
						};
			SearchResults.Add(method);


			method = new MethodElement()
						{
							Name = "Add",
							Arguments = "object sender",
							ReturnType = "void",
							Id = Guid.NewGuid(),
							ClassId = Guid.NewGuid(),
							AccessLevel = AccessLevel.Public,
							Body = code
						};
			SearchResults.Add(method);
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
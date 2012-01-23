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

    	public ProgramElement Result1
        {
            get { return SearchResults.ToArray()[0]; }
        }

        public ProgramElement Result2
        {
            get { return SearchResults.ToArray()[1]; }
        }

		public ProgramElement Result3
		{
			get
			{
				return SearchResults.ToArray()[2];
			}
		}

		public ProgramElement Result4
		{
			get
			{
				return SearchResults.ToArray()[3];
			}
		}
		public ProgramElement Result5
		{
			get
			{
				return SearchResults.ToArray()[4];
			}
		}

		

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "Sando Search");

        }
    }
}
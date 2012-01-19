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
            SearchResults = new List<ProgramElement>();
            SearchResults.Add(new Method());
            SearchResults.Add(new Method());
            SearchResults.Add(new Method());
            SearchResults.Add(new Method());
            SearchResults.Add(new Method());                         
        }

        public List<ProgramElement> SearchResults { get; set; }

        public ProgramElement Result1
        {
            get { return SearchResults.ToArray()[0]; }
        }

        public ProgramElement Result2
        {
            get { return SearchResults.ToArray()[1]; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "Sando Search");

        }
    }
}
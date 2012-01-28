using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
            InitializeComponent();            
        }



    	public List<ProgramElement> SearchResults { get; private set; }

		public String SearchString
		{
			get; set;
		}
		

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
			MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}", "Sando Search: " + SearchString),
                            "Sando Search");

        }
    }
}
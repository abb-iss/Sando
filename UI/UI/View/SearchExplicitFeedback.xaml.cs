using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sando.UI.View
{
	/// <summary>
	/// Interaction logic for SearchExplicitFeedback.xaml
	/// </summary>
	public partial class SearchExplicitFeedback : Window
	{
		public SearchExplicitFeedback(string previousQuery)
		{
			PreviousQueryDisp = "'" + previousQuery + "'";
			this.DataContext = this;
			InitializeComponent();
			this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
			this.Top = 0;
		}

		public string PreviousQueryDisp { get; private set; }
	}
}

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
using Sando.Core.Logging.Events;

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
			RecodedFeedback = false;
		}

		private void SATButton_Click(object sender, RoutedEventArgs e)
		{
			LogEvents.Search_SAT(sender, PreviousQueryDisp);
			RecodedFeedback = true;
			this.Close();
		}

		private void SomeSATButton_Click(object sender, RoutedEventArgs e)
		{
			LogEvents.Search_SomeSAT(sender, PreviousQueryDisp);
			RecodedFeedback = true;
			this.Close();
		}

		private void FewSATButton_Click(object sender, RoutedEventArgs e)
		{
			LogEvents.Search_FewSAT(sender, PreviousQueryDisp);
			RecodedFeedback = true;
			this.Close();
		}

		private void NotSATButton_Click(object sender, RoutedEventArgs e)
		{
			LogEvents.Search_NotSAT(sender, PreviousQueryDisp);
			RecodedFeedback = true;
			this.Close();
		}

		private void NoFeedbackButton_Click(object sender, RoutedEventArgs e)
		{
			LogEvents.Search_NoFeedback(sender, PreviousQueryDisp);
			RecodedFeedback = true;
			this.Close();
		}

		private void FeedbackWindow_Closed(object sender, EventArgs e)
		{
			if(!RecodedFeedback)
			{
				LogEvents.Search_NoFeedback(sender, PreviousQueryDisp);
			}
		}

		public string PreviousQueryDisp { get; private set; }
		private bool RecodedFeedback;
	}
}

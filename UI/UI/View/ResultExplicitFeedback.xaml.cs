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
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.View
{
	/// <summary>
	/// Interaction logic for ResultExplicitFeedback.xaml
	/// </summary>
	public partial class ResultExplicitFeedback : Window
	{
		public ResultExplicitFeedback(CodeSearchResult result)
		{
			if(result.ProgramElementType == ProgramElementType.Class)
			{
				ResultTypeDisp = "class ";
			}
			else if(result.ProgramElementType == ProgramElementType.Method)
			{
				ResultTypeDisp = "method ";
			}
			else if(result.ProgramElementType == ProgramElementType.Field)
			{
				ResultTypeDisp = "field ";
			}
			else if(result.ProgramElementType == ProgramElementType.Property)
			{
				ResultTypeDisp = "property ";
			}
			else
			{
				ResultTypeDisp = "";
			}

			ResultNameDisp = result.Name;
			ResultFileNameDisp = result.FileName;
			ResultCodeDisp = result.Snippet;
			this.DataContext = this;
			InitializeComponent();
			this.Left = Screen.PrimaryScreen.WorkingArea.Width - (this.Width * 2);
			this.Top = 0;
			RecodedFeedback = false;
		}

		private void SATButton_Click(object sender, RoutedEventArgs e)
		{
			var wholeResult = ResultTypeDisp + ResultNameDisp + " in " + ResultFileNameDisp;
			LogEvents.Result_SAT(sender, wholeResult);
			RecodedFeedback = true;
			this.Close();
		}

		private void NotSATButton_Click(object sender, RoutedEventArgs e)
		{
			var wholeResult = ResultTypeDisp + ResultNameDisp + " in " + ResultFileNameDisp;
			LogEvents.Result_NotSAT(sender, wholeResult);
			RecodedFeedback = true;
			this.Close();
		}

		private void NoFeedbackButton_Click(object sender, RoutedEventArgs e)
		{
			var wholeResult = ResultTypeDisp + ResultNameDisp + " in " + ResultFileNameDisp;
			LogEvents.Result_NoFeedback(sender, wholeResult);
			RecodedFeedback = true;
			this.Close();
		}

		private void FeedbackWindow_Closed(object sender, EventArgs e)
		{
			if(!RecodedFeedback)
			{
				var wholeResult = ResultTypeDisp + ResultNameDisp + " in " + ResultFileNameDisp;
				LogEvents.Result_NoFeedback(sender, wholeResult);
			}
		}

		public string ResultNameDisp { get; private set; }
		public string ResultFileNameDisp { get; private set; }
		public string ResultTypeDisp { get; private set; }
		public string ResultCodeDisp { get; private set; }
		private bool RecodedFeedback;

	}
}

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
			ResultNameDisp = result.Name + " in " + result.FileName;
			ResultCodeDisp = result.Snippet;
			this.DataContext = this;
			InitializeComponent();
			this.Left = (Screen.PrimaryScreen.WorkingArea.Width - this.Width) + 25;
			this.Top = 25;
		}

		public string ResultNameDisp { get; private set; }
		public string ResultCodeDisp { get; private set; }

	}
}

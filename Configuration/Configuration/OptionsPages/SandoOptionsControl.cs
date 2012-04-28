using System;
using System.Windows.Forms;

namespace Configuration.OptionsPages
{
	public class SandoOptionsControl : System.Windows.Forms.UserControl
	{
		#region Fields

		private SandoDialogPage customOptionsPage;
		private FolderBrowserDialog ExtensionPointsPluginDirectoryPathFolderBrowserDialog;
		private GroupBox ExtensionPoinsConfigurationGroupBox;
		private TextBox ExtensionPointsPluginDirectoryPathValueTextBox;
		private Button ExtensionPointsPluginDirectoryPathButton;
		private Label ExtensionPointsPluginDirectoryPathLabel;

		/// <summary>  
		/// Required designer variable. 
		/// </summary> 
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructors

		public SandoOptionsControl()
		{
			// This call is required by the Windows.Forms Form Designer. 
			InitializeComponent();
		}

		#endregion

		#region Methods

		#region IDisposable implementation
		/// <summary>  
		/// Clean up any resources being used. 
		/// </summary> 
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(ExtensionPointsPluginDirectoryPathFolderBrowserDialog != null)
				{
					ExtensionPointsPluginDirectoryPathFolderBrowserDialog.Dispose();
					ExtensionPointsPluginDirectoryPathFolderBrowserDialog = null;
				}
				if(components != null)
				{
					components.Dispose();
				}
				GC.SuppressFinalize(this);
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Component Designer generated code
		/// <summary>  
		/// Required method for Designer support - do not modify  
		/// the contents of this method with the code editor. 
		/// </summary> 
		private void InitializeComponent()
		{
			this.ExtensionPointsPluginDirectoryPathFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.ExtensionPoinsConfigurationGroupBox = new System.Windows.Forms.GroupBox();
			this.ExtensionPointsPluginDirectoryPathValueTextBox = new System.Windows.Forms.TextBox();
			this.ExtensionPointsPluginDirectoryPathButton = new System.Windows.Forms.Button();
			this.ExtensionPointsPluginDirectoryPathLabel = new System.Windows.Forms.Label();
			this.ExtensionPoinsConfigurationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExtensionPointsPluginDirectoryPathFolderBrowserDialog
			// 
			this.ExtensionPointsPluginDirectoryPathFolderBrowserDialog.ShowNewFolderButton = false;
			// 
			// ExtensionPoinsConfigurationGroupBox
			// 
			this.ExtensionPoinsConfigurationGroupBox.Controls.Add(this.ExtensionPointsPluginDirectoryPathValueTextBox);
			this.ExtensionPoinsConfigurationGroupBox.Controls.Add(this.ExtensionPointsPluginDirectoryPathButton);
			this.ExtensionPoinsConfigurationGroupBox.Controls.Add(this.ExtensionPointsPluginDirectoryPathLabel);
			this.ExtensionPoinsConfigurationGroupBox.Location = new System.Drawing.Point(5, 10);
			this.ExtensionPoinsConfigurationGroupBox.Name = "ExtensionPoinsConfigurationGroupBox";
			this.ExtensionPoinsConfigurationGroupBox.Size = new System.Drawing.Size(445, 62);
			this.ExtensionPoinsConfigurationGroupBox.TabIndex = 4;
			this.ExtensionPoinsConfigurationGroupBox.TabStop = false;
			this.ExtensionPoinsConfigurationGroupBox.Text = "Extension poins configuration";
			// 
			// ExtensionPointsPluginDirectoryPathValueTextBox
			// 
			this.ExtensionPointsPluginDirectoryPathValueTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.ExtensionPointsPluginDirectoryPathValueTextBox.Location = new System.Drawing.Point(155, 29);
			this.ExtensionPointsPluginDirectoryPathValueTextBox.Name = "ExtensionPointsPluginDirectoryPathValueTextBox";
			this.ExtensionPointsPluginDirectoryPathValueTextBox.ReadOnly = true;
			this.ExtensionPointsPluginDirectoryPathValueTextBox.Size = new System.Drawing.Size(245, 20);
			this.ExtensionPointsPluginDirectoryPathValueTextBox.TabIndex = 6;
			// 
			// ExtensionPointsPluginDirectoryPathButton
			// 
			this.ExtensionPointsPluginDirectoryPathButton.AutoSize = true;
			this.ExtensionPointsPluginDirectoryPathButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ExtensionPointsPluginDirectoryPathButton.Location = new System.Drawing.Point(405, 27);
			this.ExtensionPointsPluginDirectoryPathButton.Name = "ExtensionPointsPluginDirectoryPathButton";
			this.ExtensionPointsPluginDirectoryPathButton.Size = new System.Drawing.Size(26, 23);
			this.ExtensionPointsPluginDirectoryPathButton.TabIndex = 5;
			this.ExtensionPointsPluginDirectoryPathButton.Text = "...";
			this.ExtensionPointsPluginDirectoryPathButton.UseVisualStyleBackColor = true;
			this.ExtensionPointsPluginDirectoryPathButton.Click += new System.EventHandler(this.ExtensionPointsPluginDirectoryPathButton_Click);
			// 
			// ExtensionPointsPluginDirectoryPathLabel
			// 
			this.ExtensionPointsPluginDirectoryPathLabel.AutoSize = true;
			this.ExtensionPointsPluginDirectoryPathLabel.Location = new System.Drawing.Point(10, 30);
			this.ExtensionPointsPluginDirectoryPathLabel.Name = "ExtensionPointsPluginDirectoryPathLabel";
			this.ExtensionPointsPluginDirectoryPathLabel.Size = new System.Drawing.Size(130, 13);
			this.ExtensionPointsPluginDirectoryPathLabel.TabIndex = 4;
			this.ExtensionPointsPluginDirectoryPathLabel.Text = "Extension points directory:";
			// 
			// SandoOptionsControl
			// 
			this.AllowDrop = true;
			this.Controls.Add(this.ExtensionPoinsConfigurationGroupBox);
			this.Name = "SandoOptionsControl";
			this.Size = new System.Drawing.Size(465, 195);
			this.ExtensionPoinsConfigurationGroupBox.ResumeLayout(false);
			this.ExtensionPoinsConfigurationGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
		/// <summary> 
		/// Gets or Sets the reference to the underlying OptionsPage object. 
		/// </summary> 
		public SandoDialogPage OptionsPage
		{
			get
			{
				return customOptionsPage;
			}
			set
			{
				customOptionsPage = value;
			}
		}

		public string ExtensionPointsPluginDirectoryPath
		{
			get
			{
				return ExtensionPointsPluginDirectoryPathValueTextBox.Text;
			}
			set
			{
				ExtensionPointsPluginDirectoryPathValueTextBox.Text = value;
			}
		}

		#endregion

		private void ExtensionPointsPluginDirectoryPathButton_Click(object sender, EventArgs e)
		{
			ExtensionPointsPluginDirectoryPathFolderBrowserDialog = new FolderBrowserDialog();
			if(ExtensionPointsPluginDirectoryPathFolderBrowserDialog != null &&
				DialogResult.OK == ExtensionPointsPluginDirectoryPathFolderBrowserDialog.ShowDialog())
			{
				if(customOptionsPage != null)
				{
					customOptionsPage.ExtensionPointsPluginDirectoryPath = ExtensionPointsPluginDirectoryPathFolderBrowserDialog.SelectedPath;
				}
				ExtensionPointsPluginDirectoryPathValueTextBox.Text = ExtensionPointsPluginDirectoryPathFolderBrowserDialog.SelectedPath;
			}
		}
	} 
}

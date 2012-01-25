using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
//need to try and remove this
using Lucene.Net.Analysis.Snowball;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Sando.Indexer;

namespace Sando.UI
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(SearchToolWindow))]
    [Guid(GuidList.guidUIPkgString)]
    public sealed class UIPackage : Package
    {
    	private const string Lucene = "\\lucene";
    	private DocumentIndexer CurrentIndexer;
		private SolutionMonitor CurrentMonitor;

    	/// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public UIPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(SearchToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidUICmdSet, (int)PkgCmdIDList.sandoSearch);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

			DTE2 dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
			var solutionEvents = dte.Events.SolutionEvents;
			solutionEvents.Opened += SolutionHasBeenOpened;
			solutionEvents.AfterClosing += SolutionHasBeenClosed;
        }

		private void SolutionHasBeenClosed()
		{
			//shut down the current indexer
			if(CurrentIndexer!=null)
			{
				CurrentIndexer.Dispose();
				CurrentIndexer = null;
			}
			if(CurrentMonitor != null)
			{
				CurrentMonitor.Dispose();
				CurrentMonitor = null;
			}
		}

		private void SolutionHasBeenOpened()
		{			
			if(CurrentIndexer!=null)
				throw new NullReferenceException("Indexer must be null when opening a new solution.");

			//create a new indexer to search this solution
			//or reuse the existing index
			string luceneFolder = CreateLuceneFolder();
			DTE2 dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
			var openSolution = dte.Solution;
			
			//note: will remove the reference to snowballanalyzer to eliminate lucene reference
			CurrentIndexer = new DocumentIndexer(luceneFolder+"\\"+GetName(openSolution), new SnowballAnalyzer("English"));
			CurrentMonitor = new SolutionMonitor(openSolution, CurrentIndexer);
			CurrentMonitor.StartMonitoring();
		}

		private string GetName(Solution openSolution)
		{
			var fullName = openSolution.FullName;
			var split = fullName.Split('\\');
			return split[split.Length - 1];
		}

		private string CreateLuceneFolder()
		{			
			var current = Directory.GetCurrentDirectory();
			if(!File.Exists(current+Lucene))
			{
				var directoryInfo = Directory.CreateDirectory(current + Lucene);
				return directoryInfo.FullName;
			}else
			{
				return current + Lucene;
			}
		}
    
        #endregion

	
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using Configuration.OptionsPages;
using EnvDTE;
using EnvDTE80;
using log4net;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Configuration;
using Sando.Core.Extensions.Logging;
using Sando.Core.Tools;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching;
using Sando.Parser;
using Sando.SearchEngine;
using Sando.Translation;
using Sando.UI.Monitoring;
using Sando.UI.View;
using Sando.Indexer.IndexState;

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
    [ProvideToolWindow(typeof(SearchToolWindow), Transient = true, MultiInstances = false, Style = VsDockStyle.Tabbed)]
    
    [Guid(GuidList.guidUIPkgString)]
	// This attribute starts up our extension early so that it can listen to solution events    
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    // Start when solution exists
    //[ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]    
	[ProvideOptionPage(typeof(SandoDialogPage), "Sando", "General", 1000, 1001, true)]
	[ProvideProfile(typeof(SandoDialogPage), "Sando", "General", 1002, 1003, true)]
    public sealed class UIPackage : Package, IVsPackageDynamicToolOwnerEx
    {    	
    	
		private SolutionMonitor _currentMonitor;
		
		//For classloading... //TODO- eliminate the need for these
    	private List<ProgramElement> _list = new List<ProgramElement>();
		private List<CodeSearchResult> _stuff = new List<CodeSearchResult>();
		private string _other = Configuration.Configuration.GetValue("stuff");
    	private TranslationCode _mycode = TranslationCode.Exception_General_IOException;



    	private SolutionEvents _solutionEvents;
        private ILog logger;
        private string pluginDirectory;        
        private ExtensionPointsConfiguration extensionPointsConfiguration;
        private DTEEvents _dteEvents;

        private static UIPackage MyPackage
		{
			get;
			set;
		}

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
            ShowSando();
        }

        /// <summary>
        /// Side affect is creating the tool window if it doesn't exist yet
        /// </summary>
        /// <returns></returns>
        private IVsWindowFrame GetWindowFrame()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof (SearchToolWindow), 0, true);            
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            return windowFrame;
        }

        private bool isRunning = false;
        private int ShouldShow = 0;

        public void EnsureViewExists()
		{
            if (!isRunning)
            {
                var windowFrame = GetWindowFrame();
                isRunning = true;
            }		    
		}

   

        public static SandoOptions GetSandoOptions(UIPackage package)
        {
            return GetSandoOptions(null, 20,package);
        }

        public static SandoOptions GetSandoOptions(string defaultPluginDirectory, int defaultToReturn, UIPackage package)
		{
			SandoDialogPage sandoDialogPage = package.GetDialogPage(typeof(SandoDialogPage)) as SandoDialogPage;
            if(sandoDialogPage.ExtensionPointsPluginDirectoryPath==null&& defaultPluginDirectory!=null)
            {
                sandoDialogPage.ExtensionPointsPluginDirectoryPath = defaultPluginDirectory;
            }
            if(sandoDialogPage.NumberOfSearchResultsReturned==null)
            {
                sandoDialogPage.NumberOfSearchResultsReturned = defaultToReturn+"";
            }
            if (Directory.Exists(sandoDialogPage.ExtensionPointsPluginDirectoryPath) == false && defaultPluginDirectory != null)
		    {
		        sandoDialogPage.ExtensionPointsPluginDirectoryPath = defaultPluginDirectory;
		    }
			SandoOptions sandoOptions = new SandoOptions(sandoDialogPage.ExtensionPointsPluginDirectoryPath, sandoDialogPage.NumberOfSearchResultsReturned);
			return sandoOptions;
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
            try
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}",
                                              this.ToString()));
                FileLogger.DefaultLogger.Info("Sando initialization started.");
                base.Initialize();
                SetUpLogger();
                AddCommand();                
                RegisterExtensionPoints();
                SetUpLifeCycleEvents();
                MyPackage = this;                
            }catch(Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e));
            }
        }

        private void SetUpLifeCycleEvents()
        {
            var dte = Package.GetGlobalService(typeof (DTE)) as DTE2;
            _dteEvents = dte.Events.DTEEvents;
            _dteEvents.OnBeginShutdown += DteEventsOnOnBeginShutdown;
            _dteEvents.OnStartupComplete += StartupCompleted;
        }

        private void AddCommand()
        {
// Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                var toolwndCommandID = new CommandID(GuidList.guidUICmdSet, (int) PkgCmdIDList.sandoSearch);
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
        }

        private void StartupCompleted()
        {
            ShouldShow = 1;
            ShowSando();
            RegisterSolutionEvents();
            if(GetOpenSolution()!=null && _currentMonitor==null)
            {
                SolutionHasBeenOpened();
            }
        }

        private void RegisterSolutionEvents()
        {
            var dte = Package.GetGlobalService(typeof (DTE)) as DTE2;
            if (dte != null)
            {
                _solutionEvents = dte.Events.SolutionEvents;
                _solutionEvents.Opened += SolutionHasBeenOpened;
                _solutionEvents.BeforeClosing += SolutionAboutToClose;
            }
        }

        private void ShowSando()
        {
            var windowFrame = GetWindowFrame();
            // Dock Sando to the bottom of Visual Studio.
            windowFrame.SetFramePos(VSSETFRAMEPOS.SFP_fDockRight, Guid.Empty, 0, 0, 0, 0);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        private void DteEventsOnOnBeginShutdown()
        {
            if (extensionPointsConfiguration != null)
            {                                
                ExtensionPointsConfigurationFileReader.WriteConfiguration(GetExtensionPointsConfigurationFilePath(GetExtensionPointsConfigurationDirectory()), extensionPointsConfiguration);
            }
            //TODO - kill file processing threads
        }


        private void SetUpLogger()
        {
            IVsExtensionManager extensionManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsExtensionManager)) as IVsExtensionManager;
            var directoryProvider = new ExtensionDirectoryProvider(extensionManager);
            pluginDirectory = directoryProvider.GetExtensionDirectory();
            var logFilePath = Path.Combine(pluginDirectory, "UIPackage.log");
            logger = FileLogger.CreateCustomLogger(logFilePath);
            FileLogger.DefaultLogger.Info("pluginDir: "+pluginDirectory);
        }

        private void RegisterExtensionPoints()
        {
            var extensionPointsRepository = ExtensionPointsRepository.Instance;

            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".cs" }, new SrcMLCSharpParser(GetSrcMLDirectory()));
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".h", ".cpp", ".cxx" },
                                                                   new SrcMLCppParser(GetSrcMLDirectory()));
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".xaml", ".htm", ".html", ".xml", ".resx", ".aspx"},
                                                                   new XMLFileParser());
			extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".txt" },
																   new TextFileParser());

            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter()); 	
            extensionPointsRepository.RegisterResultsReordererImplementation(new SortByScoreResultsReorderer());
 	        extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
 	        extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());


            
            var extensionPointsConfigurationDirectoryPath = GetExtensionPointsConfigurationDirectory();
            string extensionPointsConfigurationFilePath = GetExtensionPointsConfigurationFilePath(extensionPointsConfigurationDirectoryPath);

            extensionPointsConfiguration = ExtensionPointsConfigurationFileReader.ReadAndValidate(extensionPointsConfigurationFilePath, logger);

            if(extensionPointsConfiguration != null)
			{
                extensionPointsConfiguration.PluginDirectoryPath = extensionPointsConfigurationDirectoryPath;
				ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			}

            var csParser = extensionPointsRepository.GetParserImplementation(".cs") as SrcMLCSharpParser;
            if(csParser!=null)
            {
                csParser.SetSrcMLPath(GetSrcMLDirectory());
            }
            var cppParser = extensionPointsRepository.GetParserImplementation(".cpp") as SrcMLCppParser;
            if (cppParser != null)
            {
                cppParser.SetSrcMLPath(GetSrcMLDirectory());
            }



        }

        private static string GetExtensionPointsConfigurationFilePath(string extensionPointsConfigurationDirectoryPath)
        {
            return Path.Combine(extensionPointsConfigurationDirectoryPath,
                                "ExtensionPointsConfiguration.xml");
        }

        private string GetExtensionPointsConfigurationDirectory()
        {
            string extensionPointsConfigurationDirectory =
                GetSandoOptions(pluginDirectory, 20, this).ExtensionPointsPluginDirectoryPath;
            if (extensionPointsConfigurationDirectory == null)
            {
                extensionPointsConfigurationDirectory = pluginDirectory;
            }
            return extensionPointsConfigurationDirectory;
        }

       




        private string GetSrcMLDirectory()
        {
            return pluginDirectory + "\\LIBS";
        }



        private void SolutionAboutToClose()
		{
		
			if(_currentMonitor != null)
			{
                try
                {
                    _currentMonitor.RemoveUpdateListener(SearchViewControl.GetInstance());
                }
                finally
                {
                    _currentMonitor.Dispose();
                    _currentMonitor = null;
                }
			}
		}

        public static Solution GetOpenSolution()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte != null)
            {
                var openSolution = dte.Solution;
                return openSolution;
            }
            else
            {
                return null;
            }
        }

		private void SolutionHasBeenOpened()
		{
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(RespondToSolutionOpened);
		    bw.RunWorkerAsync();
		}

        private void RespondToSolutionOpened(object sender, DoWorkEventArgs ee)
        {
            try
            {
                SolutionMonitorFactory.LuceneDirectory = pluginDirectory;
                string extensionPointsConfigurationDirectory =
                    GetSandoOptions(pluginDirectory, 20, this).ExtensionPointsPluginDirectoryPath;
                if (extensionPointsConfigurationDirectory == null || Directory.Exists(extensionPointsConfigurationDirectory) == false)
                {
                    extensionPointsConfigurationDirectory = pluginDirectory;
                }

                FileLogger.DefaultLogger.Info("extensionPointsDirectory: " + extensionPointsConfigurationDirectory);
                bool isIndexRecreationRequired =
                    IndexStateManager.IsIndexRecreationRequired(extensionPointsConfigurationDirectory);
                _currentMonitor = SolutionMonitorFactory.CreateMonitor(isIndexRecreationRequired);
                _currentMonitor.StartMonitoring();
                _currentMonitor.AddUpdateListener(SearchViewControl.GetInstance());
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e, "Problem responding to Solution Opened."));
            }    
        }


        public void AddNewParser(string qualifiedClassName, string dllName, List<string> supportedFileExtensions)
        {            
            extensionPointsConfiguration.ParsersConfiguration.Add(new ParserExtensionPointsConfiguration()
            {
                FullClassName = qualifiedClassName,
                LibraryFileRelativePath = dllName,
                SupportedFileExtensions = supportedFileExtensions,
                ProgramElementsConfiguration = new List<BaseExtensionPointConfiguration>()
							{
								new BaseExtensionPointConfiguration()
								{
									FullClassName = qualifiedClassName,
									LibraryFileRelativePath = dllName
								}
							}
            });
        }


        public static UIPackage GetInstance()
		{
			return MyPackage;
		}


        private class ExtensionDirectoryProvider
        {
            //TODO - there must be a better way to do this?

            private IVsExtensionManager _myMan;

            public ExtensionDirectoryProvider(IVsExtensionManager vsExtensionManager)
            {
                _myMan = vsExtensionManager;
            }

            private IInstalledExtension GetExtension(string identifier)
            {
                return _myMan.GetInstalledExtension(identifier);
            }

            internal string GetExtensionDirectory()
            {                
                return GetExtension("7e03caf3-06ed-4ff5-962a-effa1fb2f383").InstallPath;
            }
        }


    	#endregion

    	public string GetCurrentDirectory()
    	{
			if(_currentMonitor != null)
				return _currentMonitor.GetCurrentDirectory();
			else
				return null;
    	}


		public SolutionKey GetCurrentSolutionKey()
		{
			if(_currentMonitor != null)
				return _currentMonitor.GetSolutionKey();
			else
				return null;
		}

    	#region Implementation of IIndexUpdateListener

    	public void NotifyAboutIndexUpdate()
    	{
    		throw new NotImplementedException();
    	}

    	#endregion

        public bool IsPerformingInitialIndexing()
        {
            if(_currentMonitor!=null)
            {
                return _currentMonitor.PerformingInitialIndexing();
            }else
            {
                return false;
            }
        }

        public int QueryShowTool(ref Guid rguidPersistenceSlot, uint dwId, out int pfShowTool)
        {
            pfShowTool =  ShouldShow;
            return pfShowTool;
        }
    }
}

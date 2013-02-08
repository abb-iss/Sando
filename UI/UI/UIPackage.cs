using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Configuration.OptionsPages;
using EnvDTE;
using EnvDTE80;
using Sando.DependencyInjection;
using Sando.Indexer.IndexFiltering;
using log4net; 
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Configuration;
using Sando.Core.Extensions.Logging;
using Sando.Core.Tools;
using Sando.Indexer.Searching;
using Sando.Parser;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using Sando.UI.View;
using Sando.Indexer.IndexState;
using Sando.Recommender;

// Code changed by JZ: solution monitor integration
using System.Xml.Linq;
// TODO: clarify where SolutionMonitorFactory (now in Sando), SolutionKey (now in Sando), ISolution (now in SrcML.NET) should be.
//using ABB.SrcML.VisualStudio.SolutionMonitor;
// End of code changes

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
    [ProvideToolWindow(typeof(SearchToolWindow), Transient = false, MultiInstances = false, Style = VsDockStyle.Tabbed)]
    
    [Guid(GuidList.guidUIPkgString)]
	// This attribute starts up our extension early so that it can listen to solution events    
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    // Start when solution exists
    //[ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]    
	[ProvideOptionPage(typeof(SandoDialogPage), "Sando", "General", 1000, 1001, true)]
	[ProvideProfile(typeof(SandoDialogPage), "Sando", "General", 1002, 1003, true)]
    public sealed class UIPackage : Package, IToolWindowFinder
    {
        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// Use SrcML.NET's SolutionMonitor, instead of Sando's SolutionMonitor
        /// </summary>
        private ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor _currentMonitor;
        private ABB.SrcML.SrcMLArchive _srcMLArchive;
        ////private SolutionMonitor _currentMonitor;
        // End of code changes

    	private SolutionEvents _solutionEvents;
		private ILog logger;
        private ExtensionPointsConfiguration extensionPointsConfiguration;
        private DTEEvents _dteEvents;
        private ViewManager _viewManager;
		private SolutionReloadEventListener listener;
		private IVsUIShellDocumentWindowMgr winmgr;
        private WindowEvents _windowEvents;

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

                SetupDependencyInjectionObjects();

                SetUpLogger();
                _viewManager = ServiceLocator.Resolve<ViewManager>();
                AddCommand();                
                SetUpLifeCycleEvents();
                MyPackage = this;                
            }catch(Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e));
            }
        }

        private void SetUpLifeCycleEvents()
        {
            var dte = ServiceLocator.Resolve<DTE2>();
            _dteEvents = dte.Events.DTEEvents;
            _dteEvents.OnBeginShutdown += DteEventsOnOnBeginShutdown;
            _dteEvents.OnStartupComplete += StartupCompleted;
            _windowEvents = dte.Events.WindowEvents;
            _windowEvents.WindowActivated += SandoWindowActivated;
            
        }

        private void SandoWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus.ObjectKind.Equals("{AC71D0B7-7613-4EDD-95CC-9BE31C0A993A}"))
                {
                    var window = this.FindToolWindow(typeof(SearchToolWindow), 0, true);
                    if ((null == window) || (null == window.Frame))
                    {
                        throw new NotSupportedException(Resources.CanNotCreateWindow);
                    }
                    var stw = window as SearchToolWindow;
                    if (stw != null)
                    {
                        stw.GetSearchViewControl().FocusOnText();
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(e);
            }
            
        }

        private void AddCommand()
        {
            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                var toolwndCommandID = new CommandID(GuidList.guidUICmdSet, (int) PkgCmdIDList.sandoSearch);
                var menuToolWin = new MenuCommand(_viewManager.ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
        }
        
        private void StartupCompleted()
        {
        	if (_viewManager.ShouldShow())
        	{
        		_viewManager.ShowSando();
        		_viewManager.ShowToolbar();
        	}

            if (ServiceLocator.Resolve<DTE2>().Version.StartsWith("10"))
        	{
				//only need to do this in VS2010, and it breaks things in VS2012
        		Solution openSolution = ServiceLocator.Resolve<DTE2>().Solution;
        		if (openSolution != null && !String.IsNullOrWhiteSpace(openSolution.FullName) && _currentMonitor == null)
        		{
        			SolutionHasBeenOpened();
        		}
        	}

        	RegisterSolutionEvents();
        }  

        private void RegisterSolutionEvents()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte != null)
            {
                _solutionEvents = dte.Events.SolutionEvents;                
                _solutionEvents.Opened += SolutionHasBeenOpened;
                _solutionEvents.BeforeClosing += SolutionAboutToClose;
            }

			listener = new SolutionReloadEventListener();
			winmgr = Package.GetGlobalService(typeof(IVsUIShellDocumentWindowMgr)) as IVsUIShellDocumentWindowMgr;
			listener.OnQueryUnloadProject += () =>
			{
				SolutionAboutToClose();
				SolutionHasBeenOpened();
			};
        }

         

        private void DteEventsOnOnBeginShutdown()
        {
            if (extensionPointsConfiguration != null)
            {                                
                ExtensionPointsConfigurationFileReader.WriteConfiguration(GetExtensionPointsConfigurationFilePath(GetExtensionPointsConfigurationDirectory()), extensionPointsConfiguration);
                //After writing the extension points configuration file, the index state file on disk is out of date; so it needs to be rewritten
                IndexStateManager.SaveCurrentIndexState(GetExtensionPointsConfigurationDirectory());
            }
            //TODO - kill file processing threads
        }


        private void SetUpLogger()
        {
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            var logFilePath = Path.Combine(solutionKey.SandoAssemblyDirectoryPath, "UIPackage.log");
            logger = FileLogger.CreateFileLogger("UIPackageLogger", logFilePath);
            FileLogger.DefaultLogger.Info("pluginDir: " + solutionKey.SandoAssemblyDirectoryPath);
        }

        private void RegisterExtensionPoints()
        {
            var extensionPointsRepository = ExtensionPointsRepository.Instance;

            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".cs" }, new SrcMLCSharpParser(_srcMLArchive));
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".h", ".cpp", ".cxx", ".c" }, new SrcMLCppParser(_srcMLArchive));
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".xaml", ".htm", ".html", ".xml", ".resx", ".aspx"},
                                                                   new XMLFileParser());
			extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".txt" },
																   new TextFileParser());

            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter()); 	
            extensionPointsRepository.RegisterResultsReordererImplementation(new SortByScoreResultsReorderer());
 	        extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
 	        extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            extensionPointsRepository.RegisterIndexFilterManagerImplementation(new IndexFilterManager(solutionKey.IndexPath));

            
            var extensionPointsConfigurationDirectoryPath = GetExtensionPointsConfigurationDirectory();
            string extensionPointsConfigurationFilePath = GetExtensionPointsConfigurationFilePath(extensionPointsConfigurationDirectoryPath);

            extensionPointsConfiguration = ExtensionPointsConfigurationFileReader.ReadAndValidate(extensionPointsConfigurationFilePath, logger);

            if(extensionPointsConfiguration != null)
			{
                extensionPointsConfiguration.PluginDirectoryPath = extensionPointsConfigurationDirectoryPath;
				ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			}

            var csParser = extensionPointsRepository.GetParserImplementation(".cs") as SrcMLCSharpParser;
            if(csParser != null) {
                csParser.Archive = _srcMLArchive;
            }
            var cppParser = extensionPointsRepository.GetParserImplementation(".cpp") as SrcMLCppParser;
            if(cppParser != null) {
                cppParser.Archive = _srcMLArchive;
            }

        }

        private static string GetExtensionPointsConfigurationFilePath(string extensionPointsConfigurationDirectoryPath)
        {
            return Path.Combine(extensionPointsConfigurationDirectoryPath, "ExtensionPointsConfiguration.xml");
        }

        private string GetExtensionPointsConfigurationDirectory()
        {
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            string extensionPointsConfigurationDirectory = GetSandoOptions(solutionKey.SandoAssemblyDirectoryPath, 20, this).ExtensionPointsPluginDirectoryPath;
            if (extensionPointsConfigurationDirectory == null)
            {
                extensionPointsConfigurationDirectory = solutionKey.SandoAssemblyDirectoryPath;
            }
            return extensionPointsConfigurationDirectory;
        }

        private void SolutionAboutToClose()
		{
		
			if(_currentMonitor != null)
			{
                try
                {
                    // Code changed by JZ: solution monitor integration
                    // Don't know if the update listener is still useful. 
                    // The following statement would cause an exception in ViewManager.cs (Line 42).
                    //SolutionMonitorFactory.RemoveUpdateListener(SearchViewControl.GetInstance());
                    ////_currentMonitor.RemoveUpdateListener(SearchViewControl.GetInstance());
                    // End of code changes
                }
                finally
                {
                    try
                    {
                        // Code changed by JZ: solution monitor integration
                        // Use SrcML.NET's StopMonitoring()
                        if (_srcMLArchive != null)
                        {
                            // SolutionMonitor.StopWatching() is called in SrcMLArchive.StopWatching()
                            _srcMLArchive.StopWatching();
                            _srcMLArchive = null;
                        }
                        ////_currentMonitor.Dispose();
                        ////_currentMonitor = null;
                        // End of code changes
                    }
                    catch (Exception e)
                    {
                        FileLogger.DefaultLogger.Error(e);
                    }
                }
			}
            RegisterEmptySolutionKey();
		}

        private void RegisterEmptySolutionKey()
        {
            ServiceLocator.RegisterInstance(new SolutionKey(Guid.NewGuid(),"x","x",Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)));            
        }

		private void SolutionHasBeenOpened()
		{
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(RespondToSolutionOpened);
		    bw.RunWorkerAsync();
		}

        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// Respond to solution opening.
        /// Still use Sando's SolutionMonitorFactory because Sando's SolutionMonitorFactory has too much indexer code which is specific with Sando.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ee"></param>
        private void RespondToSolutionOpened(object sender, DoWorkEventArgs ee)
        {
            try
            {
                var solutionKey = ServiceLocator.Resolve<SolutionKey>();
                ServiceLocator.RegisterInstance(solutionKey);
                SolutionMonitorFactory.LuceneDirectory = solutionKey.SandoAssemblyDirectoryPath;
                string extensionPointsConfigurationDirectory =
                    GetSandoOptions(solutionKey.SandoAssemblyDirectoryPath, 20, this).ExtensionPointsPluginDirectoryPath;
                if (extensionPointsConfigurationDirectory == null || Directory.Exists(extensionPointsConfigurationDirectory) == false)
                {
                    extensionPointsConfigurationDirectory = solutionKey.SandoAssemblyDirectoryPath;
                }

                FileLogger.DefaultLogger.Info("extensionPointsDirectory: " + extensionPointsConfigurationDirectory);
                bool isIndexRecreationRequired =
                    IndexStateManager.IsIndexRecreationRequired(extensionPointsConfigurationDirectory);

                // Create a new instance of SrcML.NET's solution monitor
                _currentMonitor = SolutionMonitorFactory.CreateMonitor(isIndexRecreationRequired);
                // Subscribe events from SrcML.NET's solution monitor
                _currentMonitor.FileEventRaised += RespondToSolutionMonitorEvent;

                // Create a new instance of SrcML.NET's SrcMLArchive
                string src2srcmlDir = Path.Combine(solutionKey.SandoAssemblyDirectoryPath, "LIBS", "SrcML");
                var generator = new ABB.SrcML.SrcMLGenerator(src2srcmlDir);
                var openSolution = ServiceLocator.Resolve<DTE2>().Solution;
                _srcMLArchive = new ABB.SrcML.SrcMLArchive(_currentMonitor, SolutionMonitorFactory.GetSrcMlArchiveFolder(openSolution), !isIndexRecreationRequired, generator);
                // Subscribe events from SrcML.NET's SrcMLArchive
                _srcMLArchive.SourceFileChanged += RespondToSourceFileChangedEvent;
                _srcMLArchive.StartupCompleted += RespondToStartupCompletedEvent;
                _srcMLArchive.MonitoringStopped += RespondToMonitoringStoppedEvent;

                //This is done here because some extension points require data that isn't set until the solution is opened, e.g. the solution key or the srcml archive
                //However, registration must happen before file monitoring begins below.
                RegisterExtensionPoints();

                //Set up the SwumManager
                SwumManager.Instance.Initialize(solutionKey.IndexPath, !isIndexRecreationRequired);
                SwumManager.Instance.Archive = _srcMLArchive;

                // SolutionMonitor.StartWatching() is called in SrcMLArchive.StartWatching()
                _srcMLArchive.StartWatching();

                // Don't know if AddUpdateListener() is still useful.
                SolutionMonitorFactory.AddUpdateListener(SearchViewControl.GetInstance());
                ////_currentMonitor.AddUpdateListener(SearchViewControl.GetInstance());
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e, "Problem responding to Solution Opened."));
            }    
        }

        /// <summary>
        /// Respond to the SourceFileChanged event from SrcML.NET's Solution Monitor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void RespondToSolutionMonitorEvent(object sender, ABB.SrcML.FileEventRaisedArgs eventArgs)
        {
            writeLog("Sando: RespondToSolutionMonitorEvent(), File = " + eventArgs.SourceFilePath + ", EventType = " + eventArgs.EventType);
            // Current design decision: 
            // Ignore files that can be parsed by SrcML.NET. Those files are processed by RespondToSourceFileChangedEvent().
            if (!_srcMLArchive.IsValidFileExtension(eventArgs.SourceFilePath))
            {
                HandleSrcMLDOTNETEvents(eventArgs);
            }
        }

        /// <summary>
        /// Respond to the SourceFileChanged event from SrcMLArchive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void RespondToSourceFileChangedEvent(object sender, ABB.SrcML.FileEventRaisedArgs eventArgs)
        {
            writeLog( "Sando: RespondToSourceFileChangedEvent(), File = " + eventArgs.SourceFilePath + ", EventType = " + eventArgs.EventType);
            HandleSrcMLDOTNETEvents(eventArgs);
        }

        /// <summary>
        /// Handle SrcML.NET events, either from SrcMLArchive or from SolutionMonitor.
        /// TODO: UpdateIndex(), DeleteIndex(), and CommitIndexChanges() might be refactored to another class.
        /// </summary>
        /// <param name="eventArgs"></param>
        private void HandleSrcMLDOTNETEvents(ABB.SrcML.FileEventRaisedArgs eventArgs)
        {
            // Ignore files that can not be indexed by Sando.
		    string fileExtension = Path.GetExtension(eventArgs.SourceFilePath);
            if (fileExtension != null && !fileExtension.Equals(String.Empty))
            {
                if (ExtensionPointsRepository.Instance.GetParserImplementation(fileExtension) != null)
                {
                    string sourceFilePath = eventArgs.SourceFilePath;
                    string oldSourceFilePath = eventArgs.OldSourceFilePath;
                    XElement xelement = eventArgs.SrcMLXElement;

                    switch (eventArgs.EventType)
                    {
                        case ABB.SrcML.FileEventType.FileAdded:
                            SolutionMonitorFactory.DeleteIndex(sourceFilePath); //"just to be safe!" from IndexUpdateManager.UpdateFile()
                            SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                            SwumManager.Instance.AddSourceFile(sourceFilePath); 
                            break;
                        case ABB.SrcML.FileEventType.FileChanged:
                            SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                            SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                            SwumManager.Instance.UpdateSourceFile(sourceFilePath); 
                            break;
                        case ABB.SrcML.FileEventType.FileDeleted:
                            SolutionMonitorFactory.DeleteIndex(sourceFilePath);
                            SwumManager.Instance.RemoveSourceFile(sourceFilePath);
                            break;
                        case ABB.SrcML.FileEventType.FileRenamed: // FileRenamed is actually never raised.
                            SolutionMonitorFactory.DeleteIndex(oldSourceFilePath);
                            SolutionMonitorFactory.UpdateIndex(sourceFilePath, xelement);
                            SwumManager.Instance.UpdateSourceFile(sourceFilePath);
                            break;
                    }
                    SolutionMonitorFactory.CommitIndexChanges();
                }
            }
        }

        /// <summary>
        /// Respond to the StartupCompleted event from SrcMLArchive.
        /// TODO: StartupCompleted() might be refactored to another class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void RespondToStartupCompletedEvent(object sender, EventArgs eventArgs)
        {
            SolutionMonitorFactory.StartupCompleted();
            SwumManager.Instance.PrintSwumCache();
        }

        /// <summary>
        /// Respond to the MonitorStopped event from SrcMLArchive.
        /// TODO: MonitoringStopped() might be refactored to another class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void RespondToMonitoringStoppedEvent(object sender, EventArgs eventArgs)
        {
            SolutionMonitorFactory.MonitoringStopped();
            if(SwumManager.Instance != null) {
                SwumManager.Instance.PrintSwumCache();
            }
        }
        
        /* //// Original implementation
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
                _currentMonitor = SolutionMonitorFactory.CreateMonitor(isIndexRecreationRequired, GetOpenSolution());
                //SwumManager needs to be initialized after the current solution key is set, but before monitoring/indexing begins
                Recommender.SwumManager.Instance.Initialize(PluginDirectory(), this.GetCurrentSolutionKey().GetIndexPath(), !isIndexRecreationRequired);
                _currentMonitor.StartMonitoring();
                _currentMonitor.AddUpdateListener(SearchViewControl.GetInstance());
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e, "Problem responding to Solution Opened."));
            }
        }
        */
        // End of code changes

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

    	#endregion

        public bool IsPerformingInitialIndexing()
        {
            return SolutionMonitorFactory.PerformingInitialIndexing();
        }


        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private static void writeLog(string str)
        {
            FileLogger.DefaultLogger.Info(str);
        }
        // End of code changes




        private void SetupDependencyInjectionObjects()
        {
            ServiceLocator.RegisterInstance(GetService(typeof (DTE)) as DTE2);
            ServiceLocator.RegisterInstance(this);
            ServiceLocator.RegisterInstance(new ViewManager(this));
            RegisterEmptySolutionKey();
        }
    }
}

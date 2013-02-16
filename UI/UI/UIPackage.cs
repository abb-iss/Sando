using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using ABB.SrcML.VisualStudio.SolutionMonitor;
using Configuration.OptionsPages;
using EnvDTE;
using EnvDTE80;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Sando.DependencyInjection;
using Sando.Indexer;
using Sando.Indexer.IndexFiltering;
using Sando.UI.Options;
using Microsoft.VisualStudio.Shell;
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
        private ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor _currentMonitor;
        private ABB.SrcML.SrcMLArchive _srcMLArchive;

    	private SolutionEvents _solutionEvents;
		private ExtensionPointsConfiguration _extensionPointsConfiguration;
        private DTEEvents _dteEvents;
        private ViewManager _viewManager;
		private SolutionReloadEventListener _listener;
        private WindowEvents _windowEvents;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public UIPackage()
        {
            PathManagerFactory.Setup();
            FileLogger.SetupDefautlFileLogger(PathManager.Instance.GetExtensionRoot());
            FileLogger.DefaultLogger.Info(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
        }



        public SandoDialogPage GetSandoDialogPage()
        {
            return GetDialogPage(typeof (SandoDialogPage)) as SandoDialogPage;
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
                FileLogger.DefaultLogger.Info("Sando initialization started.");
                base.Initialize();

                SetupDependencyInjectionObjects();

                _viewManager = ServiceLocator.Resolve<ViewManager>();
                AddCommand();                
                SetUpLifeCycleEvents();
            }
            catch(Exception e)
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

        private void SandoWindowActivated(Window gotFocus, Window lostFocus)
        {
            try
            {
                if (gotFocus.ObjectKind.Equals("{AC71D0B7-7613-4EDD-95CC-9BE31C0A993A}"))
                {
                    var window = FindToolWindow(typeof(SearchToolWindow), 0, true);
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
            var dte = ServiceLocator.Resolve<DTE2>();
            if (dte != null)
            {
                _solutionEvents = dte.Events.SolutionEvents;                
                _solutionEvents.Opened += SolutionHasBeenOpened;
                _solutionEvents.BeforeClosing += SolutionAboutToClose;
            }

			_listener = new SolutionReloadEventListener();
			_listener.OnQueryUnloadProject += () =>
			{
				SolutionAboutToClose();
				SolutionHasBeenOpened();
			};
        }

         

        private void DteEventsOnOnBeginShutdown()
        {
            if (_extensionPointsConfiguration != null)
            {                                
                ExtensionPointsConfigurationFileReader.WriteConfiguration(_extensionPointsConfiguration);
                //After writing the extension points configuration file, the index state file on disk is out of date; so it needs to be rewritten
                IndexStateManager.SaveCurrentIndexState();
            }
            //TODO - kill file processing threads
        }

        private void RegisterExtensionPoints()
        {
            var extensionPointsRepository = ExtensionPointsRepository.Instance;

            extensionPointsRepository.RegisterParserImplementation(new List<string> { ".cs" }, new SrcMLCSharpParser(_srcMLArchive));
            extensionPointsRepository.RegisterParserImplementation(new List<string> { ".h", ".cpp", ".cxx", ".c" }, new SrcMLCppParser(_srcMLArchive));
            extensionPointsRepository.RegisterParserImplementation(new List<string> { ".xaml", ".htm", ".html", ".xml", ".resx", ".aspx"},
                                                                   new XMLFileParser());
			extensionPointsRepository.RegisterParserImplementation(new List<string> { ".txt" },
																   new TextFileParser());

            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter()); 	
            extensionPointsRepository.RegisterResultsReordererImplementation(new SortByScoreResultsReorderer());
 	        extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
 	        extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());
            extensionPointsRepository.RegisterIndexFilterManagerImplementation(new IndexFilterManager());


            var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();

            var loggerPath = Path.Combine(sandoOptions.ExtensionPointsPluginDirectoryPath, "ExtensionPointsLogger.log");
            var logger = FileLogger.CreateFileLogger("ExtensionPointsLogger", loggerPath);
            _extensionPointsConfiguration = ExtensionPointsConfigurationFileReader.ReadAndValidate(logger);

            if(_extensionPointsConfiguration != null)
			{
                _extensionPointsConfiguration.PluginDirectoryPath = sandoOptions.ExtensionPointsPluginDirectoryPath;
				ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(_extensionPointsConfiguration, logger);
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

        private void SolutionAboutToClose()
		{
			try
            {
                if (_srcMLArchive != null)
                {
                    _srcMLArchive.Dispose();
                    _srcMLArchive = null;
                    ServiceLocator.Resolve<IndexFilterManager>().Dispose();
                    ServiceLocator.Resolve<DocumentIndexer>().Dispose();
                }
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(e);
            }
		}

		private void SolutionHasBeenOpened()
		{
            var bw = new BackgroundWorker {WorkerReportsProgress = false, WorkerSupportsCancellation = false};
		    bw.DoWork += RespondToSolutionOpened;
		    bw.RunWorkerAsync();
		}

        /// <summary>
        /// Respond to solution opening.
        /// Still use Sando's SolutionMonitorFactory because Sando's SolutionMonitorFactory has too much indexer code which is specific with Sando.
        /// </summary>
        private void RespondToSolutionOpened(object sender, DoWorkEventArgs ee)
        {
            try
            {
                //TODO if solution is reopen - the guid should be read from file - future change
                var solutionId = Guid.NewGuid();
                var openSolution = ServiceLocator.Resolve<DTE2>().Solution;
                var solutionPath = openSolution.FileName;
                var key = new SolutionKey(solutionId, solutionPath);              
                ServiceLocator.RegisterInstance(key);
                ServiceLocator.RegisterInstance(new IndexFilterManager());                

                var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();
                FileLogger.DefaultLogger.Info("extensionPointsDirectory: " + sandoOptions.ExtensionPointsPluginDirectoryPath);
                bool isIndexRecreationRequired = IndexStateManager.IsIndexRecreationRequired();

                ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));

                var currentIndexer = new DocumentIndexer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(4));
                ServiceLocator.RegisterInstance(currentIndexer);

                ServiceLocator.RegisterInstance(new IndexUpdateManager());

                if (isIndexRecreationRequired)
                {
                    currentIndexer.ClearIndex();
                }

                ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingStarted();

                _currentMonitor = SolutionMonitorFactory.CreateMonitor();
                _currentMonitor.FileEventRaised += RespondToSolutionMonitorEvent;

                string src2SrcmlDir = Path.Combine(PathManager.Instance.GetExtensionRoot(), "LIBS", "SrcML");
                var generator = new ABB.SrcML.SrcMLGenerator(src2SrcmlDir);
                var srcMlArchiveFolder = LuceneDirectoryHelper.GetOrCreateSrcMlArchivesDirectoryForSolution(openSolution.FullName, PathManager.Instance.GetExtensionRoot());
                _srcMLArchive = new ABB.SrcML.SrcMLArchive(_currentMonitor, srcMlArchiveFolder, !isIndexRecreationRequired, generator);
                
                var srcMLArchiveEventsHandlers = ServiceLocator.Resolve<SrcMLArchiveEventsHandlers>();
                _srcMLArchive.SourceFileChanged += srcMLArchiveEventsHandlers.SourceFileChanged;
                _srcMLArchive.StartupCompleted += srcMLArchiveEventsHandlers.StartupCompleted;
                _srcMLArchive.MonitoringStopped += srcMLArchiveEventsHandlers.MonitoringStopped;

                //This is done here because some extension points require data that isn't set until the solution is opened, e.g. the solution key or the srcml archive
                //However, registration must happen before file monitoring begins below.
                RegisterExtensionPoints();

                SwumManager.Instance.Initialize(PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<SolutionKey>()), !isIndexRecreationRequired);
                SwumManager.Instance.Archive = _srcMLArchive;
                
                // SolutionMonitor.StartWatching() is called in SrcMLArchive.StartWatching()
                _srcMLArchive.StartWatching();
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(e, "Problem responding to Solution Opened."));
            }    
        }

        private void RespondToSolutionMonitorEvent(object sender, ABB.SrcML.FileEventRaisedArgs eventArgs)
        {
            FileLogger.DefaultLogger.Info("Sando: RespondToSolutionMonitorEvent(), File = " + eventArgs.SourceFilePath + ", EventType = " + eventArgs.EventType);
            // Ignore files that can be parsed by SrcML.NET. Those files are processed by _srcMLArchive.SourceFileChanged event handler.
            if (!_srcMLArchive.IsValidFileExtension(eventArgs.SourceFilePath))
            {
                var srcMLArchiveEventsHandlers = ServiceLocator.Resolve<SrcMLArchiveEventsHandlers>();
                srcMLArchiveEventsHandlers.SourceFileChanged(null, eventArgs);
            }
        }

    	#endregion




        private void SetupDependencyInjectionObjects()
        {
            ServiceLocator.RegisterInstance(GetService(typeof (DTE)) as DTE2);
            ServiceLocator.RegisterInstance(this);
            ServiceLocator.RegisterInstance(new ViewManager(this));
            ServiceLocator.RegisterInstance<ISandoOptionsProvider>(new SandoOptionsProvider());
            ServiceLocator.RegisterInstance(new SrcMLArchiveEventsHandlers());
            ServiceLocator.RegisterInstance(new InitialIndexingWatcher());
            ServiceLocator.RegisterType<IIndexerSearcher, IndexerSearcher>();
        }
    }
}

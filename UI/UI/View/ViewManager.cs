using System;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sando.Core;
using Sando.Core.Logging;
using Sando.DependencyInjection;
using Sando.Core.Tools;
using Sando.Core.Logging.Persistence;

namespace Sando.UI.View
{
    public class ViewManager
    {

        private readonly IToolWindowFinder _toolWindowFinder;
        private const string Introducesandoflag = "IntroduceSandoFlag";



        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        public void ShowToolWindow(object sender, EventArgs e)
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
            ToolWindowPane window = _toolWindowFinder.FindToolWindow(typeof(SearchToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            _toolWindowFinder.UpdateIndexingFilesList();
            return windowFrame;
        }

        private bool _isRunning;

        public ViewManager(IToolWindowFinder finder)
        {
            _toolWindowFinder = finder;
        }

        public void EnsureViewExists()
        {
            if (!_isRunning)
            {
                GetWindowFrame();
                _isRunning = true;
            }
        }


        public void ShowSando()
        {
            var windowFrame = GetWindowFrame();
            // Dock Sando to the bottom of Visual Studio.
            windowFrame.SetFramePos(VSSETFRAMEPOS.SFP_fDockRight, Guid.Empty, 0, 0, 0, 0);            
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            File.Create(GetFullIntroduceSandoFlagPath());                       
        }

        public bool ShouldShow()
        {
            return !File.Exists(GetFullIntroduceSandoFlagPath());
        }

        public void ShowToolbar()
        {
            var dte = ServiceLocator.Resolve<DTE2>();
            var cbs = ((CommandBars) dte.CommandBars);
            CommandBar cb = cbs["Sando Toolbar"];
            cb.Visible = true;
        }

        private string GetFullIntroduceSandoFlagPath()
        {            
            return Path.Combine(PathManager.Instance.GetExtensionRoot(), Introducesandoflag);
        }
    }

    public  interface IToolWindowFinder
    {
        ToolWindowPane FindToolWindow(Type type, int i, bool b);

        void UpdateIndexingFilesList();
    }
}

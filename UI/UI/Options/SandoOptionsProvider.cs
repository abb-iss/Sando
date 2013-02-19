using System;
using System.IO;
using Configuration.OptionsPages;
using Sando.Core;
using Sando.DependencyInjection;
using ABB.SrcML.VisualStudio.SolutionMonitor;
using Sando.Core.Tools;

namespace Sando.UI.Options
{
    public class SandoOptionsProvider : ISandoOptionsProvider
    {
        public SandoOptions GetSandoOptions()
        {
            var uiPackage = ServiceLocator.Resolve<UIPackage>();
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();

            var sandoDialogPage = uiPackage.GetSandoDialogPage();

            var extensionPointsPluginDirectoryPath = PathManager.Instance.GetExtensionRoot();
            if(!String.IsNullOrWhiteSpace(sandoDialogPage.ExtensionPointsPluginDirectoryPath) && Directory.Exists(sandoDialogPage.ExtensionPointsPluginDirectoryPath))
                extensionPointsPluginDirectoryPath = sandoDialogPage.ExtensionPointsPluginDirectoryPath;

            var numberOfSearchResultsReturned = 20;
            if (!String.IsNullOrWhiteSpace(sandoDialogPage.NumberOfSearchResultsReturned))
            {
                if (!int.TryParse(sandoDialogPage.NumberOfSearchResultsReturned, out numberOfSearchResultsReturned) || numberOfSearchResultsReturned < 0)
                    numberOfSearchResultsReturned = 20;
            }
            
            var sandoOptions = new SandoOptions(extensionPointsPluginDirectoryPath, numberOfSearchResultsReturned);
            return sandoOptions;
        }
    }
}
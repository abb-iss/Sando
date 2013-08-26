using System;
using System.IO;
using Configuration.OptionsPages;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.Core.Tools;
using Sando.Indexer.Searching.Criteria;
using Sando.UI.View;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell;

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
            sandoDialogPage.ExtensionPointsPluginDirectoryPath = extensionPointsPluginDirectoryPath;

            var numberOfSearchResultsReturned = SearchCriteria.DefaultNumberOfSearchResultsReturned;
            if (!String.IsNullOrWhiteSpace(sandoDialogPage.NumberOfSearchResultsReturned))
            {
                if (!int.TryParse(sandoDialogPage.NumberOfSearchResultsReturned, out numberOfSearchResultsReturned) || numberOfSearchResultsReturned < 0)
                    numberOfSearchResultsReturned = SearchCriteria.DefaultNumberOfSearchResultsReturned;
            }

            var allowDataCollectionLogging = true;
            if (!bool.TryParse(sandoDialogPage.AllowDataCollectionLogging, out allowDataCollectionLogging))
            {
                bool usersAnswer = false;
                usersAnswer = ThreadHelper.Generic.Invoke(() => ShowWelcomePopup());                                
                allowDataCollectionLogging = usersAnswer;
                SaveNewSettings(sandoDialogPage, extensionPointsPluginDirectoryPath, numberOfSearchResultsReturned, allowDataCollectionLogging);
            }

            var sandoOptions = new SandoOptions(extensionPointsPluginDirectoryPath, numberOfSearchResultsReturned, allowDataCollectionLogging);
            return sandoOptions;
        }

        private void SaveNewSettings(SandoDialogPage sandoDialogPage, string extensionPointsPluginDirectoryPath, int numberOfSearchResultsReturned, bool allowDataCollectionLogging)
        {
            if (allowDataCollectionLogging)
                sandoDialogPage.AllowDataCollectionLogging = Boolean.TrueString;
            else
                sandoDialogPage.AllowDataCollectionLogging = Boolean.FalseString;
            sandoDialogPage.ExtensionPointsPluginDirectoryPath = extensionPointsPluginDirectoryPath;
            sandoDialogPage.NumberOfSearchResultsReturned = numberOfSearchResultsReturned+String.Empty;
            sandoDialogPage.SaveSettingsToStorage();
        }

 

        private bool ShowWelcomePopup()
        {
            IntroToSando intro = new IntroToSando();
            intro.ShowDialog();
            return intro.UploadAllowed;
        }
    }
}
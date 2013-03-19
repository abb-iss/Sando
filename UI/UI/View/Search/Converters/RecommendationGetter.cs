using Sando.Core.Tools;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.LocalSearch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Sando.UI.View.Search.Converters
{    
    public class RecommendationGetter 
    {
        
        public string Convert(CodeSearchResult original, int stepsOut)
        {         

            var context = new Context();
            context.Intialize(original.ProgramElement.FullFilePath, Path.Combine(PathManager.Instance.GetExtensionRoot(), "LIBS\\SrcML\\CSharp"));
            ObservableCollection<CodeSearchResult> originalResults = null;
            Application.Current.Dispatcher.Invoke((Action)(() => originalResults = GetSearchResults()));            
            foreach (var result in originalResults)
            {
                int number = System.Convert.ToInt32(result.ProgramElement.DefinitionLineNumber);
                context.InitialSearchResults.Add(Tuple.Create(result, number));
            }
            
            context.CurrentPath.Add(original);

            var relatedmembers = context.GetRecommendations(original);
            var enumerator = relatedmembers.GetEnumerator();
            enumerator.MoveNext();
            if (stepsOut > 0)
            {
                context.CurrentPath.Add(enumerator.Current);
                relatedmembers = context.GetRecommendations(enumerator.Current);
                enumerator = relatedmembers.GetEnumerator();
                enumerator.MoveNext();
            }
            return enumerator.Current.Name;
        }

        private ObservableCollection<CodeSearchResult> GetSearchResults()
        {
            return ServiceLocator.Resolve<SearchViewControl>().SearchResults;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}

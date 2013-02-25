using LocalSearch;
using Sando.Core.Tools;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View;
using Sando.UI.View.Navigator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sando.UI.Actions
{
    public class RecommendationShower
    {
        private ListView listBox;
        private string fileName;
        private Dispatcher dispatcher;
        private Context context;
        private double Y=0;
        private RelatedItemsWindow myWindow;
        private RelatedItems itemsView;

        public RecommendationShower(ListView listBox, string fileName, Dispatcher dispatcher)
        {
            // TODO: Complete member initialization
            this.listBox = listBox;
            this.fileName = fileName;
            this.dispatcher = dispatcher;
        }

        public void Show()
        {
            if (context == null)
            {
                context = new Context();
                context.Intialize(fileName, Path.Combine(PathManager.Instance.GetExtensionRoot(), "LIBS\\SrcML\\CSharp"));
            }
            else
            {
                context = context.Copy();
            }

            var selected = listBox.SelectedItem as CodeSearchResult;                                    
            myWindow = new RelatedItemsWindow();
            itemsView = myWindow.Content as RelatedItems;
            RecommendAsync(context, selected, itemsView);
            itemsView.Context = context.Copy();                        
            var point = (listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex) as ListViewItem).PointToScreen(new Point(0, 0));
            myWindow.Left = point.X - myWindow.Width -10;
            if(Y!=0)
                myWindow.Top = Y;
            else
                myWindow.Top = point.Y;
            myWindow.Show();
        }

        private void RecommendAsync(Context context, CodeSearchResult selected, RelatedItems related)
        {
            var sandoWorker = new BackgroundWorker();
            sandoWorker.DoWork += sandoWorker_DoRecommend;
            var workerSearchParams = new RecommendParameters { CurrentContext = context, Item = selected, MyView = related };
            sandoWorker.RunWorkerAsync(workerSearchParams);
        }

        private void sandoWorker_DoRecommend(object sender, DoWorkEventArgs e)
        {
            var context = (e.Argument as RecommendParameters).CurrentContext;
            var selected = (e.Argument as RecommendParameters).Item;
            var itemsView = (e.Argument as RecommendParameters).MyView;
            context.CurrentPath.Add(selected);
            var relatedmembers = context.GetRecommendations(selected);            
            
            if (Thread.CurrentThread == dispatcher.Thread)
            {
                UpdateRecommender(relatedmembers, itemsView);
            }
            else
            {
                dispatcher.Invoke((Action)(() => UpdateRecommender(relatedmembers, itemsView)));
            }
        }

        private void UpdateRecommender(List<CodeNavigationResult> relatedmembers, RelatedItems itemsView)
        {
            foreach (var item in relatedmembers)
                itemsView.relatedItems.Add(item);
        }

        private class RecommendParameters
        {
            public Context CurrentContext { get; set; }
            public CodeSearchResult Item { get; set; }
            public RelatedItems MyView { get; set; }
        }

        public static RecommendationShower Create(ListView listBox, string fileName, Dispatcher dispatcher)
        {
            return new RecommendationShower(listBox, fileName, dispatcher);
        }

        public RecommendationShower SetContext(Context Context)
        {
            this.context = Context;
            return this;
        }

        public RecommendationShower SetY(double Y)
        {
            this.Y = Y;
            return this;
        }

        public void Dispose()
        {
            if (myWindow != null)
            {
                itemsView.Dispose();
                myWindow.Close();
            }
        }
    }
}

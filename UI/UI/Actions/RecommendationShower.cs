using Sando.LocalSearch;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Sando.UI.Actions
{
    public class RecommendationShower
    {
        private string fileName;
        private Dispatcher dispatcher;
        private Context context;
        private RelatedItemsWindow myWindow;
        private RelatedItems itemsView;
        private CodeSearchResult starter;
        private Point point;
        private Window ParentWindow;

   

        public RecommendationShower(CodeSearchResult starter, string fileName, Dispatcher dispatcher, Point point)
        {
            // TODO: Complete member initialization
            this.starter = starter;
            this.fileName = fileName;
            this.dispatcher = dispatcher;
            this.point = point;
        }

        public void Show()
        {
            if (context == null)
            {
                context = new Context();           
            }
            else
            {
                context = context.Copy();
            }

            
            myWindow = new RelatedItemsWindow();
            if (ParentWindow != null)
                myWindow.Owner = ParentWindow;
            itemsView = myWindow.Content as RelatedItems;
            itemsView.FileName = fileName;            
            RecommendAsync(context, starter, itemsView);
            itemsView.Context = context.Copy();

            myWindow.Left = point.X - 170;
            myWindow.Top = point.Y;

            myWindow.ShowDialog();
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
            if (!context.IsInitialized())
            {
                try
                {
                    context.Intialize(fileName, Path.Combine(PathManager.Instance.GetExtensionRoot(), "LIBS\\SrcML\\CSharp"));
                }
                catch (Exception ee)
                {
                    context.Intialize(fileName, Path.Combine(PathManager.Instance.GetExtensionRoot(), "LIBS\\srcML-Win-cSharp"));
                }
            }
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
            var first = relatedmembers.First();          
            itemsView.CurrentSearchResult = first;                                       
       }

  


        private class RecommendParameters
        {
            public Context CurrentContext { get; set; }
            public CodeSearchResult Item { get; set; }
            public RelatedItems MyView { get; set; }
        }

        public static RecommendationShower Create(ListView listBox, string fileName, Dispatcher dispatcher)
        {
            var point = (listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex) as ListViewItem).PointToScreen(new Point(-10, 0)); ;
            return new RecommendationShower(listBox.SelectedItem as CodeSearchResult, fileName, dispatcher,point).SetParentWindow(Window.GetWindow(listBox));
        }

        public static RecommendationShower Create(CodeSearchResult selected, string fileName, Dispatcher dispatcher, Point point)
        {            
            return new RecommendationShower(selected, fileName, dispatcher, point);
        }

        public RecommendationShower SetContext(Context Context)
        {
            this.context = Context;
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

        internal RecommendationShower SetParentWindow(Window parentWindow)
        {
            ParentWindow = parentWindow;
            return this;
        }
    }
}

using LocalSearch.View;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace LocalSearch.UnitTests
{
    [TestFixture]
    public class NavigationBoxesTest
    {
        [Test]
        [STAThread] 
        public void ShowUI()
        {            
            GraphBuilder gbuilder = new GraphBuilder(@"C:\Users\USDASHE1\Documents\Visual Studio 2012\Projects\Sando\Indexer\Indexer\DocumentIndexer.cs",@"..\..\..\..\LIBS\srcML-Win-cSharp");
            var elements = gbuilder.GetFieldsAsFieldElements();
            var boxes = new NavigationBoxes();
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                boxes.FirstProgramElements.Add(element);
            }
            Window window = new Window
            {
                Title = "My User Control Dialog",
                Content = boxes 
            };
            window.ShowDialog();
            window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

    }
}

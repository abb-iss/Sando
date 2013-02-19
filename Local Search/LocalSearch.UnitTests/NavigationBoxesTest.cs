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
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\ConfigManip.cs");
            var elements = gbuilder.GetRecommendations();
            var boxes = new NavigationBoxes();
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                ProgramElementWithRelation element2 = new ProgramElementWithRelation(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
                boxes.FirstProgramElements.Add(element2);
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

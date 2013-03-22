using Sando.LocalSearch.View;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;


namespace Sando.LocalSearch.UnitTests
{
    [TestFixture]
    public class NavigationBoxesTest
    {
        [Test]
        [STAThread] 
        public void ShowUI()
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\MindMapMapModel.java");
            var elements = gbuilder.GetRecommendations();
            var boxes = new NavigationBoxes();
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

        [Test]
        [STAThread]
        public void ShowUI_AmongInitialSearchResultsHeuristicTest()
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\SrcMLCSharpParser.cs");
            var elements = gbuilder.GetRecommendations();

            CodeSearchResult initialSearchRes = elements.ElementAt(4) as CodeSearchResult;
            gbuilder.InitialSearchResults.Add(Tuple.Create(initialSearchRes, 1)); //ParseProperties

            var boxes = new NavigationBoxes(6);
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

        [Test]
        [STAThread]
        public void ShowUI_ShowBeforeHeuristicTest()
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\CreatureManager.cs");
            var elements = gbuilder.GetRecommendations();
            
            var boxes = new NavigationBoxes(5);
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

        [Test]
        [STAThread]
        public void ShowUI_EditDistanceHeuristicInPathTest()
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\CreatureManager.cs");
            var elements = gbuilder.GetRecommendations();

            var boxes = new NavigationBoxes(7);
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

        [Test]
        [STAThread]
        public void ShowUI_EditDistanceAndShowBeforeTest()
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\CreatureManager.cs");
            var elements = gbuilder.GetRecommendations();

            var boxes = new NavigationBoxes(3);
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

        [Test]
        [STAThread]
        public void ShowUI_TopologyHeuristicTest() //ParseMethod
        {
            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\SrcMLCSharpParser.cs");
            var elements = gbuilder.GetRecommendations();

            var boxes = new NavigationBoxes(8);
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                int number = Convert.ToInt32(element.ProgramElement.DefinitionLineNumber);
                CodeNavigationResult element2 = new CodeNavigationResult(element.ProgramElement, element.Score, gbuilder.GetXElementFromLineNum(number));
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

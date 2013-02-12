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
        public void ShowUIOfField()
        {
            Context gbuilder = new Context(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\ConfigManip.cs"); 
            var elements = gbuilder.GetFieldsAsFieldElements();
            var boxes = new NavigationBoxes();
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                ProgramElementWithRelation element2 = new ProgramElementWithRelation(element.Element, element.Score);
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
        public void ShowUIOfMethod()
        {
            Context gbuilder = new Context(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\TreeViewHelper.cs");
            var elements = gbuilder.GetMethodsAsMethodElements();
            var boxes = new NavigationBoxes();
            boxes.InformationSource = gbuilder;
            foreach (var element in elements)
            {
                ProgramElementWithRelation element2 = new ProgramElementWithRelation(element.Element, element.Score);
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

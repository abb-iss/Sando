using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View;
using Sando.UI.View.Navigator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace Sando.UI.UnitTests
{

    [TestFixture]
    public class RelatedItemsTest
    {

        [Test]
        [STAThread]
        public void TestUi(){
                ProgramElement pe1 = new MethodElement("DatabaseMenuCommands", 7, "", "", AccessLevel.Public, "", "", "", System.Guid.NewGuid(), "", "", true);
                ProgramElement pe2 = new FieldElement("DatabaseCommand", 12, "", "", AccessLevel.Public, "", System.Guid.NewGuid(), "", "", "");
                CodeSearchResult cs1 = new CodeSearchResult(pe1, 1.0);
                CodeSearchResult cs2 = new CodeSearchResult(pe2, 1.0);

                RelatedItemsWindow w = new RelatedItemsWindow();
              w.SizeToContent = SizeToContent.Height;
   
              var items = w.Content as RelatedItems;
              items.relatedItems.Add(cs1);
              items.relatedItems.Add(cs2);
              items.SetCurrent(cs1);
              w.WindowStyle = WindowStyle.None;
              w.ShowDialog();
        }


    }
}

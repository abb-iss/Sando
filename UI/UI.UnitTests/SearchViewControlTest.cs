using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sando.UI.View;

namespace Sando.UI.UnitTests
{
    [TestFixture]
    public class SearchViewControlTest
    {

        [Test]
        public void RemoveEscapesFromQuery()
        {
            SearchViewControl.GetKeys("\"\"Graph\"\"").First().Equals("\"Graph\"");            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    class SandoLogTests
    {
        [Test]
        public void Analyzer()
        {
            var manager = new SandoAnalysisManager(@"C:\study data\Sando");
            manager.Analyze();
        }
    }
}

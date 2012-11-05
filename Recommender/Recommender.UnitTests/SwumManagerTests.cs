using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Recommender;
using ABB.SrcML;

namespace Sando.Recommender.UnitTests {
    [TestFixture]
    class SwumManagerTests {
        [Test]
        public void TestCacheRoundTrip() {
            var sm = SwumManager.Instance;
            sm.Clear();
            sm.AddSrcMLFile(new SrcMLFile(@"TestFiles\json_reader.cpp.xml"));
            //string tempFile = Path.GetTempFileName();
            string tempFile = "swum_cache.txt";
            sm.PrintSwumCache(tempFile);
            sm.ReadSwumCache(tempFile);
        }
    }
}

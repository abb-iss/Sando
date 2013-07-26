using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.Parser.UnitTests
{
    [TestFixture]
    public class XAMLFileParserTest
    {
        private const string filePath = @"TestFiles\SampleXAML.txt";

        [Test]
        public void Method1()
        {
            var parser = new XAMLFileParser();
            var elements = parser.Parse(filePath);
            var sb = new StringBuilder();
            foreach (var element in elements)
            {
                sb.AppendLine(element.RawSource);
                sb.AppendLine("====================================================");
                Assert.IsTrue(element.RawSource.Split('\n').Count() <= XAMLFileParser.LengthLimit);
            }
        }

        [Test]
        public void PerformanceTest()
        {
            var parser = new XAMLFileParser();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100; i++)
            {
                parser.Parse(filePath);
            }
            sw.Stop();
            Assert.IsTrue(sw.Elapsed.TotalSeconds < 10);
        }
    }
}
    
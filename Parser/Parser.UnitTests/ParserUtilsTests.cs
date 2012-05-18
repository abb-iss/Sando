using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser.UnitTests
{
    [TestFixture]
    public class ParserUtilsTests
    {

        [Test]
        public void ParseStringLiterals()
        {
            MethodElement method = ParserTestingUtils.GetMethod("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt",
                                         "LaunchSrcML");
            Assert.IsTrue(method.Body.Contains("Testingphraseola"));
        }

        [Test]
        public void ParseVariableDeclarations()
        {
            MethodElement method = ParserTestingUtils.GetMethod("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt",
                                         "LaunchSrcML");
            Assert.IsTrue(method.Body.Contains("waddow"));
        }

        [Test]
        public void ParseParameters()
        {
            MethodElement method = ParserTestingUtils.GetMethod("..\\..\\Parser\\Parser.UnitTests\\TestFiles\\ShortCSharpFile.txt",
                                         "GenerateSrcML");
            Assert.IsTrue(method.Arguments.Contains("parameterFilename"));
            Assert.IsTrue(method.Arguments.Contains("String"));
        }

    }
}

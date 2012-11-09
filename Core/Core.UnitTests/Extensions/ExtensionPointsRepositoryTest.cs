using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ParserContracts;

namespace Sando.Core.UnitTests.Extensions
{
    [TestFixture]
    public class ExtensionPointsRepositoryTest
    {
        [Test]
        public void GetParserImplementation_ReturnsParserWhenCalledWithLowerOrUpperCasedExtensionName()
        {
            var parserMock = new Mock<IParser>();
            ExtensionPointsRepository.Instance.RegisterParserImplementation(new List<string> {".H", ".cpp"}, parserMock.Object);
            Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".h"));
            Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".H"));
            Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cpp"));
            Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".CPP"));
        }
    }
}
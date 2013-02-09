using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests
{
    [TestFixture]
    class LuceneDirectoryHelperTest
    {
        [Test]
        public void GIVEN_SolutionFullNameIsNul_WHEN_GetOrCreateLuceneDirectoryForSolutionMethodIsCalled_THEN_EmptyStringIsReturned()
        {
            var dir = LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution(null, "C:\\lucene");
            Assert.AreEqual(String.Empty, dir);
        }
        
        [Test]
        public void GIVEN_SolutionFullNameIsInvalidPath_WHEN_GetOrCreateLuceneDirectoryForSolutionMethodIsCalled_THEN_EmptyStringIsReturned()
        {
            var dir = LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution("C:\\InvalidDir", "C:\\lucene");
            Assert.AreEqual(String.Empty, dir);
        }

        [Test]
        public void GIVEN_LuceneDirectoryIsNull_WHEN_GetOrCreateLuceneDirectoryForSolutionMethodIsCalled_THEN_EmptyStringIsReturned()
        {
            var dir = LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution("C:\\InvalidDir", null);
            Assert.AreEqual(String.Empty, dir);
        }

        [Test]
        public void GIVEN_LuceneDirectoryParentPathIsInvalidPath_WHEN_GetOrCreateLuceneDirectoryForSolutionMethodIsCalled_THEN_EmptyStringIsReturned()
        {
            var dir = LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution(_solPath, "C:\\invalidPath");
            Assert.AreEqual(String.Empty, dir);
        }

        [Test]
        public void GIVEN_ValidPathAndLuceneDirectory_WHEN_GetOrCreateLuceneDirectoryForSolutionMethodIsCalled_THEN_ValidPathIsReturned()
        {
            var dir = LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution(_solPath, Path.GetTempPath());
            Assert.IsFalse(String.IsNullOrWhiteSpace(dir));
            var luceneDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetTempPath());
            Assert.IsTrue(Directory.Exists(luceneDirectoryPath));
            var solutionDirectoryPath = Path.Combine(Path.GetTempPath(), "lucene", _hash.ToString(CultureInfo.InvariantCulture));
            Assert.IsTrue(Directory.Exists(solutionDirectoryPath));
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            const string solutionName = "sol.sln";
            _solPath = Path.Combine(Path.GetTempPath(), solutionName);
            _hash = solutionName.GetHashCode();
            if(!File.Exists(_solPath))
                File.WriteAllText(_solPath, "content");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (File.Exists(_solPath))
                File.Delete(_solPath);
        }

        private string _solPath;
        private int _hash;
    }
}

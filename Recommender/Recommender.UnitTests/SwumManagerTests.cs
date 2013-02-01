using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Recommender;
using ABB.SrcML;

namespace Sando.Recommender.UnitTests {
    [TestFixture]
    class SwumManagerTests {
        readonly SwumManager manager = SwumManager.Instance;

        [TestFixtureSetUp]
        public void FixtureSetup() {
            manager.Generator = new SrcMLGenerator(@"LIBS\SrcML");
        }

        [TearDown]
        public void Teardown() {
            manager.Clear();
        }

        [Test]
        public void TestCacheRoundTrip() {
            manager.AddSrcMLFile(new SrcMLFile(@"TestFiles\json_reader.cpp.xml"));
            int beforeCount = manager.GetSwumData().Count;
            //string tempFile = Path.GetTempFileName();
            string tempFile = "swum_cache.txt";
            manager.PrintSwumCache(tempFile);
            manager.ReadSwumCache(tempFile);
            Assert.AreEqual(beforeCount, manager.GetSwumData().Count);
            //TODO: add assertions that verify contents of SWUM
        }

        [Test]
        public void TestAddSourceFile() {
            Assert.IsFalse(manager.GetSwumData().Any());
            manager.AddSourceFile(@"TestFiles\small_json_reader.cpp");
            Assert.AreEqual(5, manager.GetSwumData().Keys.Count);
            Assert.IsNotNull(manager.GetSwumForSignature("static bool containsNewLine( Reader::Location begin, Reader::Location end )"));
            Assert.IsNotNull(manager.GetSwumForSignature("static std::string codePointToUTF8(unsigned int cp)"));
            Assert.IsNotNull(manager.GetSwumForSignature("Reader::Reader()"));
            Assert.IsNotNull(manager.GetSwumForSignature("bool Reader::parse( const std::string &document, Value &root, bool collectComments )"));
            Assert.IsNotNull(manager.GetSwumForSignature("bool Reader::parse( std::istream& sin, Value &root, bool collectComments )"));
        }

        [Test]
        public void TestAddSourceFile_DoubleExtension() {
            Assert.IsFalse(manager.GetSwumData().Any());
            manager.AddSourceFile(@"TestFiles\small_json.reader.cpp");
            Assert.AreEqual(5, manager.GetSwumData().Keys.Count);
            Assert.IsNotNull(manager.GetSwumForSignature("static bool containsNewLine( Reader::Location begin, Reader::Location end )"));
            Assert.IsNotNull(manager.GetSwumForSignature("static std::string codePointToUTF8(unsigned int cp)"));
            Assert.IsNotNull(manager.GetSwumForSignature("Reader::Reader()"));
            Assert.IsNotNull(manager.GetSwumForSignature("bool Reader::parse( const std::string &document, Value &root, bool collectComments )"));
            Assert.IsNotNull(manager.GetSwumForSignature("bool Reader::parse( std::istream& sin, Value &root, bool collectComments )"));
        }

        [Test]
        public void TestAddSourceFile_CSharp_Property() {
            Assert.IsFalse(manager.GetSwumData().Any());
            manager.AddSourceFile(@"TestFiles\CSharp_with_property.cs");
            Assert.AreEqual(3, manager.GetSwumData().Keys.Count);
            Assert.IsNotNull(manager.GetSwumForSignature("public TestClass()"));
            Assert.IsNotNull(manager.GetSwumForSignature("public void DoStuff(string theStuff, int count)"));
            Assert.IsNotNull(manager.GetSwumForSignature("private int PrivateStuff(int count)"));
        }

        [Test]
        public void TestRemoveSourceFile() {
            manager.AddSourceFile(@"TestFiles\small_json_reader.cpp");
            manager.AddSourceFile(@"TestFiles\function_def.cpp");
            Assert.IsNotNull(manager.GetSwumForSignature("char* MyFunction(int foo)"));
            manager.RemoveSourceFile(@"TestFiles\function_def.cpp");
            Assert.IsNull(manager.GetSwumForSignature("char* MyFunction(int foo)"));
        }

        [Test]
        public void TestUpdateSourceFile() {
            File.Copy(@"TestFiles\function_def.cpp", @"TestFiles\SwumUpdateTest.cpp", true);
            manager.AddSourceFile(@"TestFiles\SwumUpdateTest.cpp");
            Assert.AreEqual(2, manager.GetSwumData().Count);
            Assert.IsNotNull(manager.GetSwumForSignature("char* MyFunction(int foo)"));

            File.Copy(@"TestFiles\function_def2.cpp", @"TestFiles\SwumUpdateTest.cpp", true);
            manager.UpdateSourceFile(@"TestFiles\SwumUpdateTest.cpp");
            Assert.AreEqual(2, manager.GetSwumData().Count);
            Assert.IsNull(manager.GetSwumForSignature("char* MyFunction(int foo)"));
            Assert.IsNotNull(manager.GetSwumForSignature("char* UpdatedMyFunction(int foo)"));

            File.Delete(@"TestFiles\SwumUpdateTest.cpp");
        }

        [Test]
        public void TestConcurrentReadWrite() {
            manager.AddSourceFile(@"TestFiles\json_reader.cpp");
            Thread addThread = new Thread(AddSourceFiles);
            addThread.Start();
            foreach(var sig in manager.GetSwumData()) {
                Console.WriteLine("From file {0}, found sig: {1}", sig.Value.FileNames.FirstOrDefault(), sig.Key);
            }
            addThread.Join(5000);
        }

        private void AddSourceFiles() {
            Console.WriteLine("Thread 2: adding file: " + @"TestFiles\small_json_reader.cpp.xml");
            manager.AddSrcMLFile(new SrcMLFile(@"TestFiles\small_json_reader.cpp.xml"));
            Console.WriteLine("Thread 2: adding file: " + @"TestFiles\function_def.cpp.xml");
            manager.AddSrcMLFile(new SrcMLFile(@"TestFiles\function_def.cpp.xml"));
            Console.WriteLine("Thread 2: adding file: " + @"TestFiles\function_def2.cpp");
            manager.AddSourceFile(@"TestFiles\function_def2.cpp");
        }
    }
}

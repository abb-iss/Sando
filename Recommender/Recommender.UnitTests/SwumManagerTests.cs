using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Recommender;
using ABB.SrcML;
using System.Threading.Tasks;

namespace Sando.Recommender.UnitTests {
    [TestFixture]
    class SwumManagerTests {
        readonly SwumManager manager = SwumManager.Instance;

        [TestFixtureSetUp]
        public void FixtureSetup() {
            manager.Generator = new SrcMLGenerator(@"SrcML");
        }

        [TearDown]
        public void Teardown() {
            manager.Clear();
        }


        [Test]
        public void TestReadingMultiple()
        {
            Task[] tasks = new Task[1000];
            string path = Environment.CurrentDirectory + @"\..\..\Recommender\Recommender.UnitTests\TestFiles\swum-cache.txt";
            for (int i = 0; i < 1000; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    manager.ReadSwumCache(path);
                });
            }            
            System.Threading.Tasks.Task.WaitAll(tasks, 1000, System.Threading.CancellationToken.None);
        }

        [Test]
        public void TestWritingMultiple()
        {
            manager.ReadSwumCache(@"..\..\Recommender\Recommender.UnitTests\TestFiles\swum-cache.txt");
            Task[] tasks = new Task[50];
            for (int i = 0; i < 50; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    manager.PrintSwumCache();
                });
            }
            System.Threading.Tasks.Task.WaitAll(tasks, 1000, System.Threading.CancellationToken.None);
        }

        [Test]
        public void TestReadingWritingMultiple()
        {
            Task[] read = new Task[1000];
            Task[] write = new Task[1000];
            string path = Environment.CurrentDirectory+ @"\..\..\Recommender\Recommender.UnitTests\TestFiles\swum-cache.txt";
            for (int i = 0; i < 1000; i++)
            {
                read[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    manager.ReadSwumCache(path);
                });
                write[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    manager.PrintSwumCache();
                });

            }
            
            System.Threading.Tasks.Task.WaitAll(write, 1000, System.Threading.CancellationToken.None);
            System.Threading.Tasks.Task.WaitAll(read, 1000, System.Threading.CancellationToken.None);
        }


        [Test]
        public void TestReadingRealCache()
        {
            manager.ReadSwumCache(@"..\..\Recommender\Recommender.UnitTests\TestFiles\swum-cache.txt");
            var datas = manager.GetSwumData();
            Assert.IsTrue(datas.Count > 400);
            foreach (var data in datas)
                Assert.IsTrue(data.Value.SwumNode != null);
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

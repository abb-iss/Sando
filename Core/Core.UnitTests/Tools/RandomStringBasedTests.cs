using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public abstract class RandomStringBasedTests
    {
        private List<string> _createdDirectory = new List<string>();
        private static Random random = new Random((int)DateTime.Now.Ticks);

        protected string GenerateRandomString(int size)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.
                    Floor(26 * random.NextDouble() + 97)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        protected List<string> GenerateRandomWordList(int length)
        {
            var words = new List<String>();
            for (int i = 0; i < length; i++)
            {
                words.Add(GenerateRandomString(15));
            }
            return words;
        }

        protected void CreateDirectory(String path)
        {
            Directory.CreateDirectory(path);
            if (!_createdDirectory.Contains(path))
                _createdDirectory.Add(path);
        }

        protected string CombiningWords(IEnumerable<String> words, int wordCount)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wordCount; i++)
            {
                int index = random.Next() % words.Count();
                sb.Append(words.ElementAt(index));
            }
            return sb.ToString();
        }

        protected void DeleteCreatedFile()
        {
            foreach (string directory in _createdDirectory)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            _createdDirectory.Clear();
        }
    }
}

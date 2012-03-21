using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTestHelpers
{
    public class TestUtils
    {
        public static void ClearDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }
    }
}

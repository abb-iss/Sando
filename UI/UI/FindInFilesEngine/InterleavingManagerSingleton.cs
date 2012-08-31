using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Extensions.PairedInterleaving
{
    public static class InterleavingManagerSingleton
    {
        private static InterleavingManager instance = new InterleavingManager();
 
        public static InterleavingManager GetInstance() {
            return instance;
        }
    }
}

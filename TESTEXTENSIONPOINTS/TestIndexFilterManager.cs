using Sando.ExtensionContracts.IndexerContracts;

namespace Sando.TestExtensionPoints
{
    public class TestIndexFilterManager : IIndexFilterManager
    {
        public bool ShouldFileBeIndexed(string fullFilePath)
        {
            return true;
        }
    }
}

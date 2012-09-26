

using Sando.Core.Extensions;


namespace Sando.UI.InterleavingExperiment
{
    public static class InterleavingManagerSingleton
    {
    	private static InterleavingManager instance = null;
 
		public static void CreateInstance(string pluginDir, ExtensionPointsRepository extensionPointsRepository)
		{
			instance = new InterleavingManager(pluginDir, extensionPointsRepository);
		}

        public static InterleavingManager GetInstance()
        {
			return instance;
        }
    }
}



namespace Sando.UI.InterleavingExperiment
{
    public static class InterleavingManagerSingleton
    {
    	private static InterleavingManager instance = null;
 
		public static void CreateInstance(string pluginDir)
		{
			instance = new InterleavingManager(pluginDir);
		}

        public static InterleavingManager GetInstance()
        {
			return instance;
        }
    }
}

namespace Sando.UI.InterleavingExperiment
{
    public static class InterleavingManagerSingleton
    {
        private static InterleavingManager instance = new InterleavingManager();
 
        public static InterleavingManager GetInstance() {
            return instance;
        }
    }
}

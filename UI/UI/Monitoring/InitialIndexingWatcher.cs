namespace Sando.UI.Monitoring
{
    public class InitialIndexingWatcher
    {
        public InitialIndexingWatcher()
        {
            _isInitialIndexingInProgress = true;
        }

        public void InitialIndexingStarted()
        {
            _isInitialIndexingInProgress = true;
        }

        public void InitialIndexingCompleted()
        {
            _isInitialIndexingInProgress = false;
        }

        public bool IsInitialIndexingInProgress()
        {
            return _isInitialIndexingInProgress;
        }

        private bool _isInitialIndexingInProgress;
    }
}
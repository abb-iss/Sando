namespace Sando.ExtensionContracts.IndexerContracts
{
    public interface IIndexFilterManager
    {
        /// <summary>
        /// Checks if file should be indexed, based on the index filter settings.
        /// </summary>
        /// <param name="fullFilePath">Full path of the file</param>
        /// <returns>true if no ignore rule matches the file path</returns>
        bool ShouldFileBeIndexed(string fullFilePath);
    }
}
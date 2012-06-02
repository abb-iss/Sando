using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Sando.Indexer.IndexState
{
	public class IndexStateManager
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static bool IsIndexRecreationRequired(string extensionPointsConfigurationPath)
		{
			string indexDirectoryPath = Assembly.GetCallingAssembly().Location;
			indexDirectoryPath = Directory.GetParent(indexDirectoryPath).FullName;
			string indexStatePath = Path.Combine(indexDirectoryPath, IndexStateFileName);
			IndexState previousIndexState = readPreviousIndexState(indexStatePath);
			IndexState currentIndexState = readCurrentIndexState(indexDirectoryPath, extensionPointsConfigurationPath);
			bool result = previousIndexState == null || !previousIndexState.Equals(currentIndexState);
			saveIndexState(currentIndexState, indexStatePath);
			return result;
		}

		private static IndexState readPreviousIndexState(string indexStatePath)
		{
			IndexState previousIndexState = new IndexState();
			if(!File.Exists(indexStatePath))
			{
				return null; //no file - solution indexed for the first time
			}

			XmlSerializer xmlSerializer = null;
			TextReader textReader = null;
			try
			{
				xmlSerializer = new XmlSerializer(typeof(IndexState));
				textReader = new StreamReader(indexStatePath);
				previousIndexState = (IndexState)xmlSerializer.Deserialize(textReader);
			}
			finally
			{
				if(textReader != null)
					textReader.Close();
			}
			return previousIndexState;
		}

		private static IndexState readCurrentIndexState(string indexDirectoryPath, string extensionPointsConfigurationPath)
		{
			IndexState currentIndexState = new IndexState();
			findAndAddRelevantFilesToIndexState(indexDirectoryPath, currentIndexState);
			findAndAddRelevantFilesToIndexState(extensionPointsConfigurationPath, currentIndexState);
			string extensionPointsConfigurationFilePath = Path.Combine(extensionPointsConfigurationPath, "ExtensionPointsConfiguration.xml");
			FileInfo configFileInfo = new FileInfo(extensionPointsConfigurationFilePath);
			currentIndexState.RelevantFilesInfo.Add(new RelevantFileInfo() { FullName = configFileInfo.FullName, LastWriteTime = configFileInfo.LastWriteTime });
			return currentIndexState;
		}

		private static void findAndAddRelevantFilesToIndexState(string directoryPath, IndexState currentIndexState)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			IEnumerable<RelevantFileInfo> relevantFilesInfo = from fileInfo in directoryInfo.EnumerateFiles("*.dll", SearchOption.AllDirectories)
															  select new RelevantFileInfo
															  {
																  FullName = fileInfo.FullName,
																  LastWriteTime = fileInfo.LastWriteTime
															  };
			currentIndexState.RelevantFilesInfo.AddRange(relevantFilesInfo);
		}

		private static void saveIndexState(IndexState currentIndexState, string indexStatePath)
		{
			if(currentIndexState == null)
				return;

			XmlSerializer xmlSerializer = null;
			TextWriter textWriter = null;
			try
			{
				xmlSerializer = new XmlSerializer(typeof(IndexState));
				textWriter = new StreamWriter(indexStatePath);
				xmlSerializer.Serialize(textWriter, currentIndexState);
			}
			finally
			{
				if(textWriter != null)
					textWriter.Close();
			}
		}

		private static string IndexStateFileName = "sandoindexstate.xml";
	}
}

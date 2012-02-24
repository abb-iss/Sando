using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Serialization;

namespace Sando.Indexer.IndexState
{
	public class IndexFilesStatesManager
	{
		public IndexFilesStatesManager(string indexDirectoryPath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(indexDirectoryPath), "IndexFilesStatesManager:Constructor - index directory path cannot be null or an empty string!");
			Contract.Requires(Directory.Exists(indexDirectoryPath), "IndexFilesStatesManager:Constructor - index directory path does not point to a valid directory!");

			this.indexDirectoryPath = indexDirectoryPath;
			this.indexFilesStatesPath = Path.Combine(indexDirectoryPath, IndexFilesStatesFileName);
		}

		public void ReadIndexFilesStates()
		{
			indexFilesStates = new Dictionary<string, IndexFileState>();
			if(!File.Exists(indexFilesStatesPath))
			{
				return; //no file - solution indexed for the first time
			}

			XmlSerializer xmlSerializer = null;
			TextReader textReader = null;
			try
			{
				xmlSerializer = new XmlSerializer(typeof(List<IndexFileState>));
				textReader = new StreamReader(indexFilesStatesPath);
				indexFilesStates = ConvertDeserializedListToIndexFilesStates((List<IndexFileState>)xmlSerializer.Deserialize(textReader));
			}
			finally
			{
				if(textReader != null)
					textReader.Close();
			}
		}

		private Dictionary<string, IndexFileState> ConvertDeserializedListToIndexFilesStates(List<IndexFileState> deserializedData)
		{
			Dictionary<string, IndexFileState> convertedDeserializedData = new Dictionary<string, IndexFileState>();
			foreach(IndexFileState indexFileState in deserializedData)
			{
				convertedDeserializedData.Add(indexFileState.FilePath, indexFileState);
			}
			return convertedDeserializedData;
		}

		public void SaveIndexFilesStates()
		{
			if(indexFilesStates == null)
				return;

			XmlSerializer xmlSerializer = null;
			TextWriter textWriter = null;
			try
			{
				List<IndexFileState> serializationData = ConvertIndexFilesStatesToSerializableList();
				xmlSerializer = new XmlSerializer(serializationData.GetType());
				textWriter = new StreamWriter(indexFilesStatesPath);
				xmlSerializer.Serialize(textWriter, serializationData);
			}
			finally
			{
				if(textWriter != null)
					textWriter.Close();
			}
		}

		private List<IndexFileState> ConvertIndexFilesStatesToSerializableList()
		{
			List<IndexFileState> serializationData = new List<IndexFileState>();
			foreach(IndexFileState indexFileState in indexFilesStates.Values)
			{
				serializationData.Add(indexFileState);
			}
			return serializationData;
		}

		public IndexFileState GetIndexFileState(string fullFilePath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "IndexFilesStatesManager:GetIndexFileState - full file path cannot be null or an empty string!");

			if(indexFilesStates == null)
				ReadIndexFilesStates();

			return indexFilesStates.ContainsKey(fullFilePath) ? indexFilesStates[fullFilePath] : null;
		}

		public void UpdateIndexFileState(string fullFilePath, IndexFileState indexFileState)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "IndexFilesStatesManager:UpdateIndexFileState - full file path cannot be null or an empty string!");
			Contract.Requires(File.Exists(fullFilePath), "IndexFilesStatesManager:UpdateIndexFileState - full file path does not point to a valid file!");
			Contract.Requires(indexFileState != null, "IndexFilesStatesManager:UpdateIndexFileState - index file state cannot be null!");

			if(indexFilesStates == null)
				ReadIndexFilesStates();

			indexFilesStates[fullFilePath] = indexFileState;
		}

		private const string IndexFilesStatesFileName = "sandoindexfilesstates.xml";

		private string indexDirectoryPath;
		private string indexFilesStatesPath;
		private Dictionary<string, IndexFileState> indexFilesStates;
	}
}

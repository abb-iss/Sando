using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Reflection;

namespace Sando.Indexer.IndexState
{
	public class IndexStateManager
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SaveIndexState()
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

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SetCurrentIndexState(IndexState currentIndexState)
		{
			this.currentIndexState = currentIndexState;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool IsIndexRecreationRequired()
		{
			return previousIndexState.Equals(currentIndexState);
		}

		public static IndexStateManager Instance
		{
			get
			{
				if(indexStateManager == null)
					indexStateManager = new IndexStateManager();
				
				return indexStateManager;
			}
		}

		private IndexStateManager()
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(indexDirectoryPath), "IndexStateManager:Constructor - index directory path cannot be null or an empty string!");
			Contract.Requires(Directory.Exists(indexDirectoryPath), "IndexStateManager:Constructor - index directory path does not point to a valid directory!");

			this.indexDirectoryPath = Assembly.GetCallingAssembly().Location;
			this.indexStatePath = Path.Combine(indexDirectoryPath, IndexStateFileName);

			readPreviousIndexState();
		}

		private void readPreviousIndexState()
		{
			previousIndexState = new IndexState();
			if(!File.Exists(indexStatePath))
			{
				return; //no file - solution indexed for the first time
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
		}

		private const string IndexStateFileName = "sandoindexstate.xml";

		private string indexDirectoryPath;
		private string indexStatePath;
		private IndexState previousIndexState;
		private IndexState currentIndexState;

		private static IndexStateManager indexStateManager;
	}
}

using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    public abstract class ProgramElement
    {
		protected ProgramElement(string name, int definitionLineNumber, string fullFilePath, string snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(name), "ProgramElement:Constructor - name cannot be null or an empty string!");
			Contract.Requires(definitionLineNumber >= 0, "ProgramElement:Constructor - definition line number must be greater or equal 0!");
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "ProgramElement:Constructor - full file path cannot be null or an empty string!"); 
			//this should probably stay commented as we will have to check it every time we read the file
			//Contract.Requires(File.Exists(fullFilePath), "ProgramElement:Constructor - file must exist for the specified path! (path = \"" + fullFilePath + "\")");
			Contract.Requires(!String.IsNullOrWhiteSpace(snippet), "ProgramElement:Constructor - snippet cannot be null!");

			Id = Guid.NewGuid();
			DefinitionLineNumber = definitionLineNumber;
			FullFilePath = fullFilePath;
			Snippet = snippet;
			Name = name;
		}

		public static readonly String UndefinedName = "__undefined__";


		private string _name;
		public virtual string Name
		{
			get
			{
				if(_name == UndefinedName) return "";
				return _name;
			}
			private set { _name = value; }
		}

		public virtual Guid Id { get; private set; }
		public virtual int DefinitionLineNumber { get; private set; }
		public virtual string FullFilePath { get; private set; }
		public virtual string Snippet { get; private set; }
		public abstract ProgramElementType ProgramElementType { get; }
    }
}

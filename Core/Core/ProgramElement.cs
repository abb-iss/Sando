using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Sando.Core
{
    public abstract class ProgramElement
    {
		protected ProgramElement(string name, int definitionLineNumber, string fullFilePath, string snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(name), "ProgramElement:Constructor - name cannot be null or an empty string!");
			Contract.Requires(definitionLineNumber > 0, "ProgramElement:Constructor - definition line number must be greater than 0! (value = " + definitionLineNumber + ")");
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "ProgramElement:Constructor - full file path cannot be null or an empty string!"); 
			//this should probably stay commented as we will have to check it every time we read the file
			//Contract.Requires(File.Exists(fullFilePath), "ProgramElement:Constructor - file must exist for the specified path! (path = \"" + fullFilePath + "\")");
			Contract.Requires(!String.IsNullOrWhiteSpace(snippet), "ProgramElement:Constructor - snippet cannot be null!");

			Name = name;
			Id = Guid.NewGuid();
			DefinitionLineNumber = definitionLineNumber;
			FullFilePath = fullFilePath;
			Snippet = snippet;
		}

		public virtual string Name{ get; set; }
		public virtual Guid Id { get; set; }
		public virtual int DefinitionLineNumber { get; set; }
		public virtual string FullFilePath { get; set; }
		public virtual string Snippet { get; set; }
		public abstract ProgramElementType ProgramElementType { get; }
    }
}

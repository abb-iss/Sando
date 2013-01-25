using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class EnumElement : ProgramElement
	{
		public EnumElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string namespaceName, string body) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(namespaceName != null, "EnumElement:Constructor - namespace cannot be null!");
			
			AccessLevel = accessLevel;
			Namespace = namespaceName;
			Body = body;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Namespace { get; private set; }
		public virtual string Body { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Enum; } }
	}
}

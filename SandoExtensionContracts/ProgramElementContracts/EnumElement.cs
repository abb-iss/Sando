using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class EnumElement : ProgramElement
	{
		public EnumElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string namespaceName, string values) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(namespaceName != null, "EnumElement:Constructor - namespace cannot be null!");
			Contract.Requires(values != null, "EnumElement:Constructor - values cannot be null!");
			
			AccessLevel = accessLevel;
			Namespace = namespaceName;
			Values = values;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Namespace { get; private set; }
		public virtual string Values { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Enum; } }
	}
}

using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class StructElement : ProgramElement
	{
		public StructElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string namespaceName, string extendedClasses, string implementedInterfaces, string modifiers) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(namespaceName != null, "StructElement:Constructor - namespace cannot be null!");
			Contract.Requires(extendedClasses != null, "StructElement:Constructor - extended classes cannot be null!");
			Contract.Requires(implementedInterfaces != null, "StructElement:Constructor - implemented interfaces cannot be null!");
			
			AccessLevel = accessLevel;
			Namespace = namespaceName;
			ExtendedClasses = extendedClasses;
			ImplementedInterfaces = implementedInterfaces;
			Modifiers = modifiers;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Namespace { get; private set; }
		public virtual string ExtendedClasses { get; private set; }
		public virtual string ImplementedInterfaces { get; private set; }
		public virtual string Modifiers { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Struct; } }
	}
}

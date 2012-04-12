using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class MethodPrototypeElement : ProgramElement
	{
		public MethodPrototypeElement(string name, int definitionLineNumber, string returnType, AccessLevel accessLevel, 
			string arguments, string fullFilePath, string snippet) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{ 
			Contract.Requires(!String.IsNullOrWhiteSpace(returnType), "CppMethodPrototypeElement:Constructor - return type cannot be null or an empty string!");
			Contract.Requires(arguments != null, "CppMethodPrototypeElement:Constructor - arguments cannot be null!");

			AccessLevel = accessLevel;
			Arguments = arguments;
			ReturnType = returnType;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Arguments { get; private set; }
		public virtual string ReturnType { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.MethodPrototype; } }
	}
}

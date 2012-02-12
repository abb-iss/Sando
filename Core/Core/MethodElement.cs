using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class MethodElement : ProgramElement
	{
		public MethodElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string arguments, string returnType, string body, Guid classId) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(arguments != null, "MethodElement:Constructor - arguments cannot be null!");
			Contract.Requires(!String.IsNullOrWhiteSpace(returnType), "MethodElement:Constructor - return type cannot be null or an empty string!");
			Contract.Requires(body != null, "MethodElement:Constructor - body cannot be null!");
			Contract.Requires(classId != null, "MethodElement:Constructor - class id cannot be null!");
			Contract.Requires(classId != Guid.Empty, "MethodElement:Constructor - class id cannot be an empty Guid!");

			AccessLevel = accessLevel;
			Arguments = arguments;
			ReturnType = returnType;
			Body = body;
			ClassId = classId;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Arguments { get; private set; }
		public virtual string ReturnType { get; private set; }
		public virtual string Body { get; private set; }
		public virtual Guid ClassId { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Method; } }
	}
}

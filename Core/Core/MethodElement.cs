using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class MethodElement : ProgramElement
	{
		[Obsolete]
		public MethodElement()
			: base("temp", 1, "path", "snippet")
		{
		}

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

		public virtual AccessLevel AccessLevel { get; set; }
		public virtual string Arguments { get; set; }
		public virtual string ReturnType { get; set; }
		public virtual string Body { get; set; }
		public virtual Guid ClassId { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Method; } }
	}
}

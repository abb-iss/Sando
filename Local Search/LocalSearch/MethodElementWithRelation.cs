using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace LocalSearch
{
    class MethodElementWithRelation : ProgramElementWithRelation
    {
        public MethodElementWithRelation(string name, int definitionLineNumber, string fullFilePath, 
            string snippet, ProgramElementRelation elementrelation, AccessLevel accessLevel,
			string arguments, string returnType, string body, Guid classId, string className, 
            string modifiers, bool isConstructor) 
			: base(name, definitionLineNumber, fullFilePath, snippet, elementrelation)
		{
			Contract.Requires(arguments != null, "MethodElement:Constructor - arguments cannot be null!");
			Contract.Requires(isConstructor || !String.IsNullOrWhiteSpace(returnType), "MethodElement:Constructor - return type cannot be null or an empty string!");
			Contract.Requires(body != null, "MethodElement:Constructor - body cannot be null!");
			Contract.Requires(classId != null, "MethodElement:Constructor - class id cannot be null!");
			//Contract.Requires(classId != Guid.Empty, "MethodElement:Constructor - class id cannot be an empty Guid!");
			Contract.Requires(className != null, "MethodElement:Constructor - class name cannot be null!");

			AccessLevel = accessLevel;
			Arguments = arguments;
			ReturnType = returnType;
			Body = body;
			ClassId = classId;
			ClassName = className;
			Modifiers = modifiers;
			IsConstructor = isConstructor;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string Arguments { get; private set; }
		public virtual string ReturnType { get; private set; }
		public virtual string Body { get;  set; }
		public virtual Guid ClassId { get; private set; }
		public virtual string ClassName { get; private set; }
		public virtual string Modifiers { get; private set; }
		public virtual bool IsConstructor { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Method; } }
	
    }
}

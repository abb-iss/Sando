using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class PropertyElement : ProgramElement
	{
		public PropertyElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string propertyType, string body, Guid classId, string className, string modifiers) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(propertyType), "PropertyElement:Constructor - property type cannot be null!");
			Contract.Requires(body != null, "FieldElement:Constructor - body cannot be null!");
			Contract.Requires(classId != null, "PropertyElement:Constructor - class id cannot be null!");
			//Contract.Requires(classId != Guid.Empty, "PropertyElement:Constructor - class id cannot be an empty Guid!");
			Contract.Requires(className != null, "PropertyElement:Constructor - class name cannot be null!");

			AccessLevel = accessLevel;
			PropertyType = propertyType;
			Body = body;
			ClassId = classId;
			ClassName = className;
			Modifiers = modifiers;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string PropertyType { get; private set; }
		public virtual string Body { get; private set; }
		public virtual Guid ClassId { get; private set; }
		public virtual string ClassName { get; private set; }
		public virtual string Modifiers { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Property; } }
	}
}

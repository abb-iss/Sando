using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class PropertyElement : ProgramElement
	{
		[Obsolete]
		public PropertyElement()
			: base("temp", 1, "path", "snippet")
		{
		}

		public PropertyElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string propertyType, string body, Guid classId) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(propertyType), "PropertyElement:Constructor - property type cannot be null!");
			Contract.Requires(body != null, "FieldElement:Constructor - body cannot be null!");
			Contract.Requires(classId != null, "PropertyElement:Constructor - class id cannot be null!");
			Contract.Requires(classId != Guid.Empty, "PropertyElement:Constructor - class id cannot be an empty Guid!");

			AccessLevel = accessLevel;
			PropertyType = propertyType;
			Body = body;
			ClassId = classId;
		}

		public virtual AccessLevel AccessLevel { get; set; }
		public virtual string PropertyType { get; set; }
		public virtual string Body { get; set; }
		public virtual Guid ClassId { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Property; } }
	}
}

using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class FieldElement : ProgramElement
	{
		public FieldElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel,
			string fieldType, Guid classId) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fieldType), "FieldElement:Constructor - field type cannot be null!");
			Contract.Requires(classId != null, "FieldElement:Constructor - class id cannot be null!");
			Contract.Requires(classId != Guid.Empty, "FieldElement:Constructor - class id cannot be an empty Guid!");

			AccessLevel = accessLevel;
			FieldType = fieldType;
			ClassId = classId;
		}

		public virtual AccessLevel AccessLevel { get; private set; }
		public virtual string FieldType { get; private set; }
		public virtual Guid ClassId { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Field; } }
	}
}

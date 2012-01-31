using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class FieldElement : ProgramElement
	{
		public virtual AccessLevel AccessLevel { get; set; }
		public virtual string FieldType { get; set; }
		public virtual Guid ClassId { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Field; } }
	}
}

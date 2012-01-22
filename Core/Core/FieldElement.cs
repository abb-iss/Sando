using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class FieldElement : ProgramElement
	{
		public virtual string FieldType { get; set; }
		public virtual Guid ClassId { get; set; }
	}
}

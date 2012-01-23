using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class PropertyElement : ProgramElement
	{
		public virtual string Name { get; set; }
		public virtual AccessLevel AccessLevel { get; set; }
		public virtual int DefinitionLineNumber { get; set; }
		public virtual string PropertyType { get; set; }
		public virtual string Body { get; set; }
		public virtual Guid ClassId { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class MethodElement : ProgramElement
	{
		public virtual string Arguments { get; set; }
		public virtual string ReturnType { get; set; }
		public virtual string Body { get; set; }
		public virtual Guid ClassId { get; set; }
	}
}

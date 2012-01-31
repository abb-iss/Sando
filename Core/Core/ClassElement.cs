using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class ClassElement : ProgramElement
	{		
		public virtual AccessLevel AccessLevel { get; set; }
		public virtual string Namespace { get; set; }
		public virtual string ExtendedClasses { get; set; }
		public virtual string ImplementedInterfaces { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.Class; } }
	}
}

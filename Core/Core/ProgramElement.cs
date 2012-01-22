using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
    public abstract class ProgramElement
    {
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual AccessLevel AccessLevel { get; set; }
		public virtual int DefinitionLineNumber { get; set; }
    }
}

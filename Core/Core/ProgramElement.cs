using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
    public abstract class ProgramElement
    {
		public virtual string Name{ get;set; }
		public virtual Guid Id { get; set; }
		public virtual int DefinitionLineNumber { get; set; }
		public virtual string FullFilePath { get; set; }
		public virtual string Snippet { get; set; }
		public abstract ProgramElementType ProgramElementType { get; }
    }
}

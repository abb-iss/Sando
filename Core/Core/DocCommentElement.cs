using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class DocCommentElement : ProgramElement
	{
		public virtual int DefinitionLineNumber { get; set; }
		public virtual string Body { get; set; }
		public virtual string FileName { get; set; }
		public virtual string FullFilePath { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.DocComment; } }
	}
}

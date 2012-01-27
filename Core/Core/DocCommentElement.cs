using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public class DocCommentElement : ProgramElement
	{
		public virtual string Body { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.DocComment; } }
	}
}

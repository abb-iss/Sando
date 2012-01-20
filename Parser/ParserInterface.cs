using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sando.Core;

namespace Sando.Parser
{
	interface ParserInterface
	{
		ProgramElement[] parse(String filename);
	}
}

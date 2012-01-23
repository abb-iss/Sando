using System;
using Sando.Core;

namespace Sando.Parser
{
	interface ParserInterface
	{
		ProgramElement[] Parse(String filename);
	}
}

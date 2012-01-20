using System;
using Sando.Core;

namespace Sando.Parser
{
	interface ParserInterface
	{
		ProgramElement[] parse(String filename);
	}
}

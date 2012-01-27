using System;
using Sando.Core;

namespace Sando.Parser
{
	public interface ParserInterface
	{
		ProgramElement[] Parse(String filename);
	}
}

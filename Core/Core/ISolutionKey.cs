using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
	public interface ISolutionKey
	{
		string GetKeyRepresentation();
		string GetSolutionPath();
	}
}

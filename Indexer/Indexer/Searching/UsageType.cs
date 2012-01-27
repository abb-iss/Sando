using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Searching
{
	public enum UsageType
	{
		Definition,
		MethodArgument,
		MethodReturnType,
		PropertyOrFieldType,
		MethodBody //most commmon usage
	}
}

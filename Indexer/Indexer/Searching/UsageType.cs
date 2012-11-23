using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Searching
{
	public enum UsageType
	{
		Bodies, //comment, doc comment, method, property
		Definitions,
		EnumValues,
		ExtendedClasses,
		ImplementedInterfaces,
		MethodArguments,
		MethodReturnTypes,
		NamespaceNames,
		PropertyOrFieldTypes,
        UnsplitTerms
	}
}

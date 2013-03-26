namespace Sando.Indexer.Searching
{
	public enum UsageType
	{
		Bodies, //comment, doc comment, method, property, enum, struct
		Definitions,
		ExtendedClasses,
		ImplementedInterfaces,
		MethodArguments,
		MethodReturnTypes,
		NamespaceNames,
		PropertyOrFieldTypes,
		RawSourceCode,
        ClassName
	}
}

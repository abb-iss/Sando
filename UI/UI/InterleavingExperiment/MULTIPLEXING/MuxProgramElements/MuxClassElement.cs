﻿using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.InterleavingExperiment.Multiplexing.MuxProgramElements
{
	public class MuxClassElement : ClassElement
	{
		public MuxClassElement(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel, 
									string namespaceName, string extendedClasses, string implementedInterfaces, string modifiers, int samuraiTag = 1) 
			: base(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, implementedInterfaces, modifiers)
		{
			SamuraiTag = samuraiTag;
		}

		public MuxClassElement(ClassElement copyFromElement) 
			: base(copyFromElement.Name, copyFromElement.DefinitionLineNumber, copyFromElement.FullFilePath, copyFromElement.Snippet, copyFromElement.AccessLevel,
			copyFromElement.Namespace, copyFromElement.ExtendedClasses, copyFromElement.ImplementedInterfaces, copyFromElement.Modifiers)
		{
			SamuraiTag = 1;
		}

		[CustomIndexField] private int SamuraiTag;
	}
}

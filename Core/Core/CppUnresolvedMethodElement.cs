using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class CppUnresolvedMethodElement : MethodElement
	{
		public CppUnresolvedMethodElement(string name, int definitionLineNumber, string fullFilePath, string snippet, string arguments, 
			string returnType, string body, string className, string [] headerFiles) 
			: base(name, definitionLineNumber, fullFilePath, snippet, AccessLevel.Protected, arguments, returnType, body, Guid.NewGuid())
		{
			Contract.Requires(className != null, "CppSplitMethodElement:Constructor - class name cannot be null!");
			Contract.Requires(headerFiles.Length > 0, "CppSplitMethodElement:Constructor - there have to be some header files defined here");

			ClassName = className;
			DefinitionFileNames = headerFiles;
			IsResolved = false;
		}

		public MethodElement Resolve(ProgramElement[] headerElements) 
		{
			AccessLevel accessLevel = ResolveAccessType(Name, headerElements);
			Guid classId = ResolveClassId(ClassName, headerElements);		
			IsResolved = true;

			return new MethodElement(Name, DefinitionLineNumber, FullFilePath, Snippet, accessLevel, Arguments, ReturnType, Body, classId);
		}

		private Guid ResolveClassId(string className, ProgramElement[] includeElements)
		{
			foreach(ProgramElement element in includeElements)
			{
				if(element is ClassElement && element.Name == ClassName)
				{
					return ((ClassElement)element).Id;
				}
			}

			return Guid.Empty;
		}

		private AccessLevel ResolveAccessType(string funcName, ProgramElement[] includeElements)
		{
			foreach(ProgramElement element in includeElements)
			{
				if(element is MethodPrototypeElement && element.Name == funcName) 
				{
					return ((MethodPrototypeElement)element).AccessLevel;
				}
			}

			return AccessLevel.Protected;
		}
		

		public virtual string ClassName { get; private set; }
		public virtual string[] DefinitionFileNames { get; private set; }
		public virtual bool IsResolved { get; set; }
	}
}

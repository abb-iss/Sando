using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class CppUnresolvedMethodElement : MethodElement
	{
		public CppUnresolvedMethodElement(string name, int definitionLineNumber, string fullFilePath, string snippet, string arguments, 
			string returnType, string body, string className, bool isConstructor, string [] headerFiles)
			: base(name, definitionLineNumber, fullFilePath, snippet, AccessLevel.Protected, arguments, returnType, body, 
					Guid.NewGuid(), className, String.Empty, isConstructor)
		{
			Contract.Requires(className != null, "CppSplitMethodElement:Constructor - class name cannot be null!");
			Contract.Requires(headerFiles.Length > 0, "CppSplitMethodElement:Constructor - there have to be some header files defined here");

			IncludeFileNames = headerFiles;
			IsResolved = false;
		}

		public bool TryResolve(ProgramElement[] headerElements, out MethodElement outMethodElement) 
		{
			AccessLevel accessLevel; 
			Guid classId;

			outMethodElement = null;
			if(ResolveClassId(ClassName, headerElements, out classId) == false) return false;
			if(ResolveAccessType(Name, headerElements, out accessLevel) == false) return false;

			IsResolved = true;
			outMethodElement = new MethodElement(Name, DefinitionLineNumber, FullFilePath, Snippet, accessLevel, Arguments, ReturnType, Body, 
													classId, ClassName, String.Empty, IsConstructor);
			return true;
		}

		private bool ResolveClassId(string className, ProgramElement[] includeElements, out Guid outGuid)
		{
			foreach(ProgramElement element in includeElements)
			{
				if(element is ClassElement && element.Name == ClassName)
				{
					outGuid = ((ClassElement)element).Id;
					return true;
				}
			}

			outGuid = Guid.Empty;
			return false;
		}

		private bool ResolveAccessType(string funcName, ProgramElement[] includeElements, out AccessLevel outAccessLevel)
		{
			foreach(ProgramElement element in includeElements)
			{
				if(element is MethodPrototypeElement && element.Name == funcName) 
				{
					outAccessLevel = ((MethodPrototypeElement)element).AccessLevel;
					return true;
				}
			}

			outAccessLevel = AccessLevel.Protected;
			return false;
		}
		
		public virtual string[] IncludeFileNames { get; private set; }
		public virtual bool IsResolved { get; set; }
	}
}

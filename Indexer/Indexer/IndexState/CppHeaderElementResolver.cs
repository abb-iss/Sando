using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.IO;
using System.Diagnostics;
using Sando.Core.Extensions;
using Sando.Indexer.Documents;

namespace Sando.Indexer.IndexState
{
	public static class CppHeaderElementResolver
	{

		public static List<ProgramElement> GenerateCppHeaderElements(string filePath, List<ProgramElement> unresolvedElements)
		{
			List<ProgramElement> headerElements = new List<ProgramElement>();

			//first parse all the included header files. they are the same in all the unresolved elements
			CppUnresolvedMethodElement firstUnresolved = (CppUnresolvedMethodElement)unresolvedElements[0];
			foreach(String headerFile in firstUnresolved.IncludeFileNames)
			{
				//it's reasonable to assume that the header file path is relative from the cpp file,
				//as other included files are unlikely to be part of the same project and therefore 
				//should not need to be parsed
				string headerPath = System.IO.Path.GetDirectoryName(filePath) + "\\" + headerFile;
				if(!System.IO.File.Exists(headerPath))
				{
					Debug.WriteLine("????? header file -" + headerFile + "- was not found.. this can lead to issues");					
					continue;
				}
				Debug.WriteLine("*** parsing header = " + headerPath);
				var headerInfo = new FileInfo(headerPath);
				headerElements.AddRange(ExtensionPointsRepository.Instance.GetParserImplementation(headerInfo.Extension).Parse(headerPath));
			}
			return headerElements;
		}

		public static SandoDocument GetDocumentForUnresolvedCppMethod(CppUnresolvedMethodElement unresolvedMethod, List<ProgramElement> headerElements)
		{
			bool isResolved = false;
			MethodElement methodElement = null;

			isResolved = unresolvedMethod.TryResolve(unresolvedMethod, headerElements, out methodElement);
			if(isResolved == true)
			{
				return DocumentFactory.Create(methodElement);
			}
			else
			{
				Debug.WriteLine("????? " + unresolvedMethod.Name + " is not resolved, this is bad!!");
				methodElement = unresolvedMethod.Copy();
				return DocumentFactory.Create(methodElement);
			}
		}
	}
}

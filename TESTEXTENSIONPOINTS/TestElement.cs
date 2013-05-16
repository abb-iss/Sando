using System;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.TestExtensionPoints
{
	public class TestElement : ProgramElement
	{
		public TestElement(string name, int definitionLineNumber, int definitionColumnNumber, string fullFilePath, string snippet)
            : base(name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet)
		{

		}

		public override ProgramElementType ProgramElementType
		{
			get 
			{
				return ProgramElementType.Custom;
			}
		}
	}
}

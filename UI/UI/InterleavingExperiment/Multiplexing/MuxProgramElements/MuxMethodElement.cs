using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.InterleavingExperiment.Multiplexing.MuxProgramElements
{
	public class MuxMethodElement : MethodElement
	{
		public MuxMethodElement(MethodElement copy)
			:base(copy.Name, copy.DefinitionLineNumber, copy.FullFilePath, copy.Snippet, copy.AccessLevel, copy.Arguments, 
					copy.ReturnType, copy.Body, copy.ClassId, copy.ClassName, copy.Modifiers, copy.IsConstructor)
		{
			MuxedTag = 1;
		}

		[CustomIndexField]
		private int MuxedTag;
	}
}

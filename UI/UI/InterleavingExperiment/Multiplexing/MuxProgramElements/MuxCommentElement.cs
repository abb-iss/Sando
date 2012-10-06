using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.InterleavingExperiment.Multiplexing.MuxProgramElements
{
	public class MuxCommentElement : CommentElement
	{
		public MuxCommentElement(CommentElement copy)
			:base(copy.Name, copy.DefinitionLineNumber, copy.FullFilePath, copy.Snippet, copy.Body)
		{
			MuxedTag = 1;
		}

		[CustomIndexField]
		private int MuxedTag;
	}
}

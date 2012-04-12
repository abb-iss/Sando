using System;
using System.Diagnostics.Contracts;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
	public class DocCommentElement : ProgramElement
	{
		public DocCommentElement(string name, int definitionLineNumber, string fullFilePath, string snippet, string body, Guid documentedElementId) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(body), "DocCommentElement:Constructor - body cannot be null or an empty string!");
			Contract.Requires(documentedElementId != null, "DocCommentElement:Constructor - documented element id cannot be null!");
			Contract.Requires(documentedElementId != Guid.Empty, "DocCommentElement:Constructor - documented element id cannot be an empty Guid!");
			
			Body = body;
			DocumentedElementId = documentedElementId;
		}

		public virtual string Body { get; private set; }
		public virtual Guid DocumentedElementId { get; private set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.DocComment; } }
	}
}

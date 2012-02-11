﻿using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class DocCommentElement : ProgramElement
	{
		[Obsolete]
		public DocCommentElement()
			: base("temp", 1, "path", "snippet")
		{
		}

		public DocCommentElement(string name, int definitionLineNumber, string fullFilePath, string snippet, string body, Guid documentedElementId) 
			: base(name, definitionLineNumber, fullFilePath, snippet)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(body), "DocCommentElement:Constructor - body cannot be null or an empty string!");
			Contract.Requires(documentedElementId != null, "DocCommentElement:Constructor - documented element id cannot be null!");
			Contract.Requires(documentedElementId != Guid.Empty, "DocCommentElement:Constructor - documented element id cannot be an empty Guid!");
			
			Body = body;
			DocumentedElementId = documentedElementId;
		}

		public virtual string Body { get; set; }
		public virtual Guid DocumentedElementId { get; set; }
		public override ProgramElementType ProgramElementType { get { return ProgramElementType.DocComment; } }
	}
}

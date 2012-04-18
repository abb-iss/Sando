using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
	public static class SrcMLParsingUtils
	{
		private static readonly XNamespace SourceNamespace = "http://www.sdml.info/srcML/src";
		private static readonly XNamespace PositionNamespace = "http://www.sdml.info/srcML/position";

		public static void ParseFields(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
		{
			IEnumerable<XElement> fields =
				from el in elements.Descendants(SourceNamespace + "class")
				select el.Element(SourceNamespace + "block");

			fields =
				from el in fields.Elements(SourceNamespace + "decl_stmt").Elements(SourceNamespace + "decl")
				where el.Element(SourceNamespace + "name") != null &&
						el.Element(SourceNamespace + "type") != null &&
						(
							(el.Element(SourceNamespace + "init") != null && el.Elements().Count() == 3) ||
							el.Elements().Count() == 2
						)
				select el;

			foreach(XElement field in fields)
			{
				string name;
				int definitionLineNumber;
				SrcMLParsingUtils.ParseNameAndLineNumber(field, out name, out definitionLineNumber);

				ClassElement classElement = RetrieveClassElement(field, programElements);
				Guid classId = classElement != null ? classElement.Id : Guid.Empty;
				string className = classElement != null ? classElement.Name : String.Empty;

				//parse access level and type
				XElement accessElement = field.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
				AccessLevel accessLevel = accessElement != null ? StrToAccessLevel(accessElement.Value) : AccessLevel.Internal;

				IEnumerable<XElement> types = field.Element(SourceNamespace + "type").Elements(SourceNamespace + "name");
				string fieldType = String.Empty;
				foreach(XElement type in types)
				{
					fieldType += type.Value + " ";
				}
				fieldType = fieldType.TrimEnd();

				string initialValue = String.Empty;
				XElement initialValueElement = field.Element(SourceNamespace + "init");
				if(initialValueElement != null)
					initialValue = initialValueElement.Element(SourceNamespace + "expr").Value;

				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = RetrieveSnippet(fileName, definitionLineNumber, snippetSize);

				programElements.Add(new FieldElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, fieldType, classId, className, String.Empty, initialValue));
			}
		}

		public static DocCommentElement ParseFunctionComments(XElement function, MethodElement funcElement)
		{
			string commentBody = "";

			//retrieve comments inside function
			IEnumerable<XElement> inComments =
				from el in function.Descendants(SourceNamespace + "comment")
				select el;
			foreach(XElement inComment in inComments)
			{
				commentBody += inComment.Value + " ";
			}
			commentBody = commentBody.TrimEnd();

			//retrieve comments above function
			XElement aboveComment = (function.PreviousNode is XElement) ? (XElement)function.PreviousNode : null;
			while(aboveComment != null &&
				  aboveComment.Name == (SourceNamespace + "comment"))
			{
				string commentText = aboveComment.Value;
				if(commentText.StartsWith("/")) commentText = commentText.TrimStart('/');
				commentBody = " " + commentText + commentBody;
				aboveComment = (aboveComment.PreviousNode is XElement) ? (XElement)aboveComment.PreviousNode : null;
			}
			commentBody = commentBody.TrimStart();

			if(commentBody != "")
			{
				return new DocCommentElement(funcElement.Name, funcElement.DefinitionLineNumber, funcElement.FullFilePath,
												funcElement.Snippet, commentBody, funcElement.Id);
			}
			else
			{
				return null;
			}
		}

		public static string ParseBody(XElement function)
		{
			XElement block = function.Element(SourceNamespace + "block");
			IEnumerable<XElement> bodyNames =
				from el in block.Descendants(SourceNamespace + "name")
				select el;
			string body = String.Empty;
			foreach(XElement elem in bodyNames)
			{
				body += String.Join(" ", ExtensionPointsRepository.Instance.GetWordSplitterImplementation().ExtractWords(elem.Value)) + " ";
			}
			body = body.TrimEnd();
			return body;
		}

		public static void ParseNameAndLineNumber(XElement target, out string name, out int definitionLineNumber)
		{
			XElement nameElement = target.Element(SourceNamespace + "name");
			name = nameElement.Value;

			if(nameElement.Attribute(PositionNamespace + "line") != null)
			{
				definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);
			}
			else
			{
				//i can't find the line number
				definitionLineNumber = 0;
			}
		}

		public static ClassElement RetrieveClassElement(XElement field, List<ProgramElement> programElements)
		{
			IEnumerable<XElement> ownerClasses =
				from el in field.Ancestors(SourceNamespace + "class")
				select el;
			if(ownerClasses.Count() > 0)
			{
				//this ignores the possibility that a field may be part of an inner class
				XElement name = ownerClasses.First().Element(SourceNamespace + "name");
				string ownerClassName = name.Value;
				//now find the ClassElement object corresponding to ownerClassName, since those should have been gen'd by now
				ProgramElement ownerClass = programElements.Find(element => element is ClassElement && ((ClassElement)element).Name == ownerClassName);
				return ownerClass as ClassElement;
			}
			else
			{
				//field is not contained by a class
				return null;
			}
		}

		public static string RetrieveSnippet(string filename, int line, int snippetNumLines)
		{
			string[] lines = System.IO.File.ReadAllLines(System.IO.Path.GetFullPath(filename));

			//start at a number of lines above the definition of the program element
			int linesAbove = 0;

			int startLine = line - linesAbove - 1;
			IEnumerable<string> snipLines = lines.Skip(startLine).Take(snippetNumLines - linesAbove);
			return snipLines.Aggregate((snip, nextLine) => snip + Environment.NewLine + nextLine);
		}

		public static AccessLevel StrToAccessLevel(string level)
		{
			return (AccessLevel)Enum.Parse(typeof(AccessLevel), level, true);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

		public static void ParseComments(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
		{
			IEnumerable<XElement> comments =
				from el in elements.Descendants(SourceNamespace + "comment")
				select el;

			foreach(XElement comment in comments)
			{
				string commentText = comment.Value;
				if(commentText.StartsWith("/")) commentText = commentText.TrimStart('/');
				commentText = commentText.TrimStart();
				int commentLine = Int32.Parse(comment.Attribute(PositionNamespace + "line").Value);
				

				if(commentText == String.Empty) continue;

				//comment above method or class
				XElement belowComment = (comment.NextNode is XElement) ? (XElement)comment.NextNode : null;
				while(belowComment != null &&
						belowComment.Name == (SourceNamespace + "comment"))
				{
					//proceed down the list of comments until I get to some other program element
					belowComment = (belowComment.NextNode is XElement) ? (XElement)belowComment.NextNode : null;
				}

				if(belowComment != null &&
					(belowComment.Name == (SourceNamespace + "function") ||
						belowComment.Name == (SourceNamespace + "constructor")))
				{
					XElement name = belowComment.Element(SourceNamespace + "name");
					if(name != null)
					{
						string methodName = name.Value;
						MethodElement methodElement = (MethodElement)programElements.Find(element => element is MethodElement && ((MethodElement)element).Name == methodName);
						if(methodElement != null)
						{
							programElements.Add(new DocCommentElement(methodElement.Name, methodElement.DefinitionLineNumber, methodElement.FullFilePath,
																		RetrieveSnippet(methodElement.FullFilePath, commentLine, snippetSize),
																		commentText, methodElement.Id));
							continue;
						}
					}
				}
				else if(belowComment != null &&
							belowComment.Name == (SourceNamespace + "class"))
				{
					XElement name = belowComment.Element(SourceNamespace + "name");
					if(name != null)
					{
						string className = name.Value;
						ClassElement classElement = (ClassElement)programElements.Find(element => element is ClassElement && ((ClassElement)element).Name == className);
						if(classElement != null)
						{
							programElements.Add(new DocCommentElement(classElement.Name, classElement.DefinitionLineNumber, classElement.FullFilePath,
																		RetrieveSnippet(classElement.FullFilePath, commentLine, snippetSize),
																		commentText, classElement.Id));
							continue;
						}
					}
				}

				//comment inside a method or class
				MethodElement methodEl = RetrieveMethodElement(comment, programElements);
				if(methodEl != null)
				{
					programElements.Add(new DocCommentElement(methodEl.Name, methodEl.DefinitionLineNumber, methodEl.FullFilePath,
																RetrieveSnippet(methodEl.FullFilePath, commentLine, snippetSize),
																commentText, methodEl.Id));
					continue;
				}
				ClassElement classEl = RetrieveClassElement(comment, programElements);
				if(classEl != null)
				{
					programElements.Add(new DocCommentElement(classEl.Name, classEl.DefinitionLineNumber, classEl.FullFilePath,
																RetrieveSnippet(classEl.FullFilePath, commentLine, snippetSize),
																commentText, classEl.Id));
					continue;
				}

				//comment is not associated with another element, so it's a plain CommentElement
				programElements.Add(new CommentElement(commentText, commentLine, fileName, RetrieveSnippet(fileName, commentLine, snippetSize), commentText));
			}
		}

		public static string ParseBody(XElement function)
		{
			string body = String.Empty;
			XElement block = function.Element(SourceNamespace + "block");
			if(block != null)
			{

				IEnumerable<XElement> comments =
					from el in block.Descendants(SourceNamespace + "comment")
					select el;
				foreach(XElement elem in comments)
				{
					body += String.Join(" ", elem.Value) + " ";
				}

				//Expressions should also include all names, but we need to verify this...
				IEnumerable<XElement> expressions =
						from el in block.Descendants(SourceNamespace + "expr")
						select el;
				foreach(XElement elem in expressions)
				{
					body += String.Join(" ", elem.Value) + " ";
				}
                //need to also add a names from declarations
                IEnumerable<XElement> declarations =
                    from el in block.Descendants(SourceNamespace + "decl")
                    select el;
			    foreach (var declaration in declarations)
			    {
                    var declNames = from el in declaration.Descendants(SourceNamespace + "name")
                                    where (el.Parent.Name.LocalName.Equals("type")||
                                    el.Parent.Name.LocalName.Equals("decl"))
                                    select el;
                    foreach (XElement elem in declNames)
                    {
                        body += String.Join(" ", elem.Value) + " ";
                    }			        
			    }
				body = body.TrimEnd();
			}
			body = Regex.Replace(body, "\\W", " ");
			return body;
		}

		public static void ParseNameAndLineNumber(XElement target, out string name, out int definitionLineNumber)
		{
			XElement nameElement;
			nameElement = target.Element(SourceNamespace + "name");
			if(nameElement == null && 
				target.Element(SourceNamespace + "super") != null) 
			{
				//case of anonymous inner class, should have a super
				nameElement = target.Element(SourceNamespace + "super");
				nameElement = nameElement.Element(SourceNamespace + "name");
				name = nameElement.Value;
			}
			else if(nameElement == null)
			{
				//case of there is no resemblance of a name available
				name = ProgramElement.UndefinedName;
				nameElement = target; //still try to parse line number
			}
			else
			{
				//normal case
				name = nameElement.Value;
			}

			////try to get line number
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

		public static MethodElement RetrieveMethodElement(XElement field, List<ProgramElement> programElements)
		{
			IEnumerable<XElement> ownerMethods =
				from el in field.Ancestors(SourceNamespace + "function")
				select el;
			ownerMethods.Union(from el in field.Ancestors(SourceNamespace + "constructor") select el);

			if(ownerMethods.Count() > 0)
			{
				XElement name = ownerMethods.First().Element(SourceNamespace + "name");
				string ownerMethodName = name.Value;
				//now find the MethodElement object corresponding to ownerMethodName, since those should have been gen'd by now
				ProgramElement ownerMethod = programElements.Find(element => element is ClassElement && ((ClassElement)element).Name == ownerMethodName);
				return ownerMethod as MethodElement;
			}
			else
			{
				//field is not contained by a method
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

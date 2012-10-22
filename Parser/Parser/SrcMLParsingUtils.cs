using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
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
                string snippet = RetrieveSnippet(field, snippetSize);

				programElements.Add(new FieldElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, fieldType, classId, className, String.Empty, initialValue));
			}
		}

		public static void ParseComments(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
		{
			IEnumerable<XElement> comments =
				from el in elements.Descendants(SourceNamespace + "comment")
				select el;

            List<List<XElement>> commentGroups = new List<List<XElement>>();
            List<XElement> lastGroup = null;
		    foreach (var aComment in comments)
		    {
                  if(lastGroup!=null)
                  {
                      int line = Int32.Parse(lastGroup.Last().Attribute(PositionNamespace + "line").Value);
                      int linenext = Int32.Parse(aComment.Attribute(PositionNamespace + "line").Value);
                      if(line+1==linenext)
                      {
                          lastGroup.Add(aComment);
                      }else
                      {
                          var xElements = new List<XElement>();
                          xElements.Add(aComment);
                          commentGroups.Add(xElements);
                          lastGroup = xElements;
                      }
                  }
                  else
                  {
                      var xElements = new List<XElement>();
                      xElements.Add(aComment);
                      commentGroups.Add(xElements);
                      lastGroup = xElements;
                  }
		    }


            foreach (var oneGroup in commentGroups)
            {
                var comment = oneGroup.First();
                var commentText = GetCommentText(oneGroup);
                int commentLine = Int32.Parse(comment.Attribute(PositionNamespace + "line").Value);				
				if(String.IsNullOrWhiteSpace(commentText)) continue;

				//comment name doesn't contain non-word characters and is compact-er than its body
                var commentName = "";
                commentName = GetCommentSummary(GetCommentText(oneGroup,true));
				if(commentName == "") continue;

                //comments above method or class
                var lastComment = oneGroup.Last() as XElement;
                ProgramElement programElement=null;
                if (lastComment !=null && lastComment.Attribute(PositionNamespace + "line") != null)
                {
                    var definitionLineNumber = Int32.Parse(lastComment.Attribute(PositionNamespace + "line").Value);
                    programElement =
                        programElements.Find(element => element.DefinitionLineNumber == definitionLineNumber + 1);
                }
                if(programElement!=null)
                {
                    programElements.Add(new DocCommentElement(commentName, commentLine, programElement.FullFilePath,
                                                                        RetrieveSnippet(commentText, snippetSize),
                                                                        commentText, programElement.Id));                    
                    continue;                    
                }

				

				//comments inside a method or class
				MethodElement methodEl = RetrieveMethodElement(comment, programElements);
				if(methodEl != null)
				{
					programElements.Add(new DocCommentElement(commentName, commentLine, methodEl.FullFilePath,
																RetrieveSnippet(commentText, snippetSize),
																commentText, methodEl.Id));
					continue;
				}
				ClassElement classEl = RetrieveClassElement(comment, programElements);
				if(classEl != null)
				{
					programElements.Add(new DocCommentElement(commentName, commentLine, classEl.FullFilePath,
																RetrieveSnippet(commentText,  snippetSize),
																commentText, classEl.Id));
					continue;
				}

				//comments is not associated with another element, so it's a plain CommentElement
				programElements.Add(new CommentElement(commentName, commentLine, fileName, RetrieveSnippet(commentText, snippetSize), commentText));
			}
		}

	 

	    public static string GetCommentSummary(string commentText)
	    {
	        string commentName="";
	        char[] splits = {'\n', '\r'};
	        var splitString = commentText.Split(splits);
            foreach (var line in splitString)
            {
                char[] arr = line.ToCharArray();

                arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) )));
                var textLine = new string(arr);
                if (textLine.Trim().Length > 10)
                {
                    if (!Regex.Match(line, @"\s*/+\s*<\w*>\s*[\r\n]*").Success)
                    {
                        commentName = line.Trim();
                        break;
                    }
                }
            }	        
	        if (String.IsNullOrWhiteSpace(commentName)) commentName = commentText;
            if (commentName.StartsWith("/")) commentName = commentName.TrimStart('/');
	        return commentName.Trim();
	    }

	    private static string GetCommentText(List<XElement> comments, bool preserveSlashes=true)
	    {
            StringBuilder builder = new StringBuilder();
	        Boolean first = true;
	        foreach (var comment in comments)
	        {
                if (first)
                {
                    first = false;
                }
                else
                {                    
                    builder.Append(Environment.NewLine + " ");
                }
	            var commentText = comment.Value;
                if(!preserveSlashes)
                    if (commentText.StartsWith("/")) commentText = commentText.TrimStart('/');
                builder.Append(commentText.TrimStart());

	        }	        
	        return builder.ToString();
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
			    string ownerClassName="";
                if(name==null)
                    ownerClassName = "anonymous";
                else
				    ownerClassName = name.Value;
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
                ProgramElement ownerMethod = programElements.Find(element => element is MethodElement && ((MethodElement)element).Name == ownerMethodName);
				return ownerMethod as MethodElement;
			}
			else
			{
				//field is not contained by a method
				return null;
			}
		}

        public static string RetrieveSnippet(string retrieveSnippet, int numLines)
        {
            try
            {
                string snip = "";
                string[] lines = retrieveSnippet.Split('\n');
                if (lines.Length > 0)
                {
                    int count = 0;
                    foreach (var line in lines)
                    {
                        if (count >= numLines)
                            break;
                        char[] trim = {'\n', '\r'};
                        var myLine = line.TrimEnd(trim);
                        myLine.TrimStart(trim);
                        snip += myLine + Environment.NewLine;
                    }
                }
                return snip;
            }
            catch (Exception e)
            {
                return retrieveSnippet;
            }
        }

		public static string RetrieveSnippet(XElement theThang, int numLines)
		{
		    string retrieveSnippet = theThang.Value;
            return RetrieveSnippet(retrieveSnippet, numLines);

		}

	    public static AccessLevel StrToAccessLevel(string level)
		{
            try
            {
                return (AccessLevel) Enum.Parse(typeof (AccessLevel), level, true);
            }catch(Exception e)
            {
                FileLogger.DefaultLogger.Error("Problem parsing access level.  Input was: "+level);
                return AccessLevel.Internal;
            }
		}
	}
}

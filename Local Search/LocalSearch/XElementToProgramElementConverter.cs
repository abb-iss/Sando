using ABB.SrcML;
using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LocalSearch
{
    public class XElementToProgramElementConverter
    {

        public static CodeNavigationResult GetMethodElementWRelationFromXElement(XElement fullmethod, string fileName)
        {
            var definitionLineNumber = fullmethod.Element(SRC.Name).GetSrcLineNumber();            
            var block = fullmethod.Element(SRC.Block);
            var snippet = "";
            if (block != null)
                snippet = fullmethod.Element(SRC.Block).ToSource(); //todo: only show related lines  
            //var relation = ProgramElementRelation.Other; //by default

            AccessLevel accessLevel = AccessLevel.Internal; //by default
            var specifier = fullmethod.Elements(SRC.Specifier); //for constructor (no return type/value)
            try
            {
                if (specifier.Count() == 0)
                    specifier = fullmethod.Element(SRC.Type).Elements(SRC.Specifier); //for other functions
                if (specifier.Count() != 0)
                {
                    foreach (var temp in specifier)
                    {
                        try
                        {
                            accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), temp.Value, true);
                        }
                        catch (Exception e)
                        {
                            //do nothing
                        }
                    }
                }
            }
            catch (NullReferenceException nre)
            {
                //TODO: handle properties, add, get, etc.
            }

            var returnType = String.Empty;
            bool isconstructor = true;
            var type = fullmethod.Element(SRC.Type);
            if (type != null)
            {
                returnType = type.Element(SRC.Name).Value;
                isconstructor = false;
            }

            var classId = Guid.Empty;
            var className = String.Empty;
            var myParams = fullmethod.Element(SRC.ParameterList);
            var args = "";
            if (myParams != null)
                args = myParams.ToSource();

            var body = "";
            if (fullmethod.Element(SRC.Block) != null)
                body = fullmethod.Element(SRC.Block).ToSource();

            var element = new MethodElement(fullmethod.Element(SRC.Name).Value,
                definitionLineNumber, fileName, snippet,
                accessLevel, args, returnType, body, classId, className, String.Empty, isconstructor);
            var elementwrelation = new CodeNavigationResult(element, 1.0, fullmethod);

            return elementwrelation;

            //var element = new MethodElementWithRelation(fullmethod.Element(SRC.Name).Value, 
            //    definitionLineNumber, fullFilePath, snippet, relation,
            //    accessLevel, args, returnType, body, classId, className, String.Empty, isconstructor);
            //return element;
        }

        public static CodeNavigationResult GetFieldElementWRelationFromDecl(XElement fielddecl, string fileName)
        {
            var definitionLineNumber = fielddecl.Element(SRC.Name).GetSrcLineNumber();
            var snippet = fielddecl.ToSource();
            //var relation = ProgramElementRelation.Other; //by default

            AccessLevel accessLevel = AccessLevel.Internal; //by default
            var specifier = fielddecl.Element(SRC.Type).Elements(SRC.Specifier);
            if (specifier.Count() != 0)
            {
                foreach (var temp in specifier)
                {
                    try
                    {
                        accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), temp.Value, true);
                    }
                    catch (Exception e)
                    {
                        //do nothing, it reaches here becasue it's not a specifier of accesslevel, such as static
                    }
                }
            }

            var fieldType = fielddecl.Element(SRC.Type).Element(SRC.Name);
            var classId = Guid.Empty;
            var className = String.Empty;
            var initialValue = String.Empty;

            var element = new FieldElement(fielddecl.Element(SRC.Name).Value,
                definitionLineNumber, fileName, snippet,
                accessLevel, fieldType.Value, classId, className, String.Empty, initialValue);

            var elementwrelation = new CodeNavigationResult(element, 1.0, fielddecl);

            return elementwrelation;

        }


    }
}

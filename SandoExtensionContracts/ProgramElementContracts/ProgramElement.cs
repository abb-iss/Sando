using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    public class ProgramElement
    {
        public const string CustomTypeTag = "CustomType";

        public String CustomType1534213765
        {
            get { return GetType().AssemblyQualifiedName; }
        }

        public ProgramElement(object[] parameters)
            : this(parameters[0] as string, (int)parameters[0], parameters[0] as string, parameters[0] as string)
        {

        }

        public ProgramElement(string name, int definitionLineNumber, string fullFilePath, string snippet)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name),
                              "ProgramElement:Constructor - name cannot be null or an empty string!");
            Contract.Requires(definitionLineNumber >= 0,
                              "ProgramElement:Constructor - definition line number must be greater or equal 0!");
            Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath),
                              "ProgramElement:Constructor - full file path cannot be null or an empty string!");
            //this should probably stay commented as we will have to check it every time we read the file
            //Contract.Requires(File.Exists(fullFilePath), "ProgramElement:Constructor - file must exist for the specified path! (path = \"" + fullFilePath + "\")");
            Contract.Requires(!String.IsNullOrWhiteSpace(snippet),
                              "ProgramElement:Constructor - snippet cannot be null!");

            Id = Guid.NewGuid();
            DefinitionLineNumber = definitionLineNumber;
            FullFilePath = fullFilePath;
            RawSource = snippet;
            Name = name;
        }

        public static readonly String UndefinedName = "__undefined__";


        private string _name;

        public virtual string Name
        {
            get
            {
                if (_name == UndefinedName) return "";
                return _name;
            }
            private set { _name = value; }
        }

        public virtual Guid Id { get; private set; }
        public virtual int DefinitionLineNumber { get; private set; }
        public virtual string FullFilePath { get; private set; }
        public virtual string RawSource { get; private set; }
        public virtual ProgramElementType ProgramElementType
        {
            get
            {
                return ProgramElementType.Custom;
            }
        }

        public string FileExtension
        {
            get { return Path.GetExtension(FullFilePath); }
        }


        public List<PropertyInfo> GetCustomProperties()
        {
            var propertyInfos = new List<PropertyInfo>();

            foreach (var property in this.GetType().GetProperties())
            {
                var attribs = property.GetCustomAttributes(typeof(CustomIndexFieldAttribute), true);
                foreach (var attrib in attribs)
                {
                    if (attrib.GetType().Equals(typeof(CustomIndexFieldAttribute)))
                    {
                        propertyInfos.Add(property);
                    }
                }
                if (property.Name.Equals(CustomTypeTag))
                {
                    propertyInfos.Add(property);
                }
            }
            return propertyInfos;
        }

        public virtual string GetName()
        {
            return ProgramElementType.ToString();
        }
    }
}

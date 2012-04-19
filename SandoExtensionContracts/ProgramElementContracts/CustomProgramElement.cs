using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    public abstract class CustomProgramElement: ProgramElement
    {
        public const string CustomTypeTag = "CustomType";

        public String CustomType { get { return GetType().AssemblyQualifiedName; }  }

        protected CustomProgramElement(string name, int definitionLineNumber, string fullFilePath, string snippet) : base(name, definitionLineNumber, fullFilePath, snippet)
        {
        }

        public override ProgramElementType ProgramElementType
        {
            get { return ProgramElementType.Custom; }
        }

        public IEnumerable<PropertyInfo> GetCustomProperties()
        {
            var propertyInfos = new List<PropertyInfo>();
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.DeclaringType != typeof(ProgramElement) && property.DeclaringType != typeof(CustomProgramElement))
                {
                    propertyInfos.Add(property);
                }
                else if (property.Name.Equals(CustomTypeTag))
                {
                    propertyInfos.Add(property);
                }
            }
            return propertyInfos;
        }

        public abstract string GetName();
    }
}

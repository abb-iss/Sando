using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    [System.AttributeUsage(System.AttributeTargets.Field |
                       System.AttributeTargets.Property)
    ]
    public class CustomIndexFieldAttribute: System.Attribute
    {
        private string name;

        public CustomIndexFieldAttribute(string name)
        {
            this.name = name;
        }

    }
}

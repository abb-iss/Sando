using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public enum ProgramElementRelation
    {
        Use, //method uses field
        UseBy, //field used by method
        Call, //method calls method
        CallBy, //method called by method
        UseAsPara, //field used as method paramter
        Other
    }
}

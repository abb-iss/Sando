using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public enum ProgramElementRelation
    {
        [Description("used by")]
        Use, //method uses field
        [Description("uses")]
        UseBy, //field used by method
        [Description("called by")]
        Call, //method calls method
        [Description("calls")]
        CallBy, //method called by method
        [Description("uses as parameter")]
        UseAsPara, //field used as method paramter
        [Description("")]
        Other   
    }
    
}

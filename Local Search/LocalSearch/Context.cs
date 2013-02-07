using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace LocalSearch
{
    public class Context : GraphBuilder
    {
        private String query { set;  public get; }
       
        private List<ProgramElementWithRelation> path
        {
            public get
            {
                return null;
            }
        }
         
        private IEnumerator<ProgramElementWithRelation> curPos
        {
            public get
            {
                return null;
            }

        }
        
        //private relationDB;

        public Context(String srcPath, string SrcMLForCSharp = null):base(srcPath, SrcMLForCSharp) 
        {

        }

        public Context(String srcPath, String search, string SrcMLForCSharp = null)
            : base(srcPath, SrcMLForCSharp) 
        {
            query = search;
        }


    }
}

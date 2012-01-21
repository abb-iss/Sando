using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
    public abstract class ProgramElement
    {
    	public String Name
    	{
    		get; set;
    	}
		//could be namespace, package, folder, class, etc. depending on language and element
		public abstract String ContainerName
		{
			get;
			set;
		} 
		public String FileName
		{
			get;
			set;
		}
    	public String SummaryText
    	{
    		get; set;
    	}
    }
}

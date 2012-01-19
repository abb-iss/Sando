using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core
{
    public class Method: ProgramElement
    {
    	public override String Name
    	{
    		get;
			set;
		}

    	public override string ContainerName
    	{
			get;
			set;
		}

    	public override string FileName
    	{
			get;
			set;
		}

    	public override String SummaryText
		{
			get;
			set;
        }
    }
}

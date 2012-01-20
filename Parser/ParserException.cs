using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Parser { 

	class ParserException : System.Exception
	{
		public ParserException(string message)
        : base(message)
		{
		}
	}

}

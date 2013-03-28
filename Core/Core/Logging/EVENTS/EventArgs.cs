using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
	public class EventArgs<T> : System.EventArgs
	{
		public EventArgs(T value)
		{
			m_value = value;
		}

		private T m_value;

		public T Value
		{
			get { return m_value; }
		}
	}

    public class EventArgs<T1, T2> : System.EventArgs
    {
        public EventArgs(T1 value1, T2 value2)
        {
            m_value1 = value1;
            m_value2 = value2;
        }

        private T1 m_value1;
        private T2 m_value2;

        public T1 FirstValue
        {
            get { return m_value1; }
        }

        public T2 SecondValue
        {
            get { return m_value2; }
        }

    }
}

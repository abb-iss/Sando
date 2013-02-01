using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;

namespace DependencyInjection
{
    public class PerProcessLifetimeManager : SynchronizedLifetimeManager
    {
        public PerProcessLifetimeManager()
        {
            _key = Process.GetCurrentProcess().Id;
            _obj = new Int32();
        }

        protected override object SynchronizedGetValue()
        {
            lock (_obj)
            {
                InitializeValues();
                if (_values.ContainsKey(_key))
                {
                    return _values[_key];
                }
                return null;
            }
        }

        protected override void SynchronizedSetValue(object newValue)
        {
            lock (_obj)
            {
                InitializeValues();
                _values[_key] = newValue;
            }
        }

        private void InitializeValues()
        {
            if (_values == null)
            {
                _values = new Dictionary<int, object>();
            }
        }

        private readonly int _key;
        private static Dictionary<int, object> _values;
        private readonly object _obj;
    }
}

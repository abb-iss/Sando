using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Searching.Metrics
{
    [Flags]
    public enum QueryTermType
    {
        None = 0x00,
        Minus = 0x01,
        Camelcase = 0x02,
        Underscore = 0x04,
        Filetype = 0x08,
        Quoted = 0x10,
        Acronym = 0x20
    }

    public class QueryTermTypeList
    {
        public QueryTermTypeList()
        {
            _termTypes = new List<QueryTermType>();
        }

        public QueryTermType this[int i]
        {
            get 
            {
                if (i < _termTypes.Count)
                {
                    return _termTypes[i];
                }
                else
                {
                    return QueryTermType.None;
                }
            }
            set 
            {
                if (i >= _termTypes.Count)
                {
                    _termTypes.AddRange(Enumerable.Repeat(QueryTermType.None, i - _termTypes.Count + 1));
                }
                _termTypes[i] = value;
            }
        }

        public string ToString()
        {
            string result = "";
            foreach (var tt in _termTypes)
            {
                if (tt == QueryTermType.None)
                {
                    result += "Plain";
                }
                else
                {
                    if ((tt & QueryTermType.Filetype) == QueryTermType.Filetype) result += "Filetype";
                    if ((tt & QueryTermType.Minus) == QueryTermType.Minus) result += "Minus";
                    if ((tt & QueryTermType.Quoted) == QueryTermType.Quoted) result += "Quoted";
                    if ((tt & QueryTermType.Camelcase) == QueryTermType.Camelcase) result += "Camelcase";
                    if ((tt & QueryTermType.Acronym) == QueryTermType.Acronym) result += "Acronym";
                    if ((tt & QueryTermType.Underscore) == QueryTermType.Underscore) result += "Underscore";
                }
                result += ",";
            }
            return result.TrimEnd(',');
        }

        private List<QueryTermType> _termTypes; 
    }
}

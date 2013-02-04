using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesProcessing
{
    class StringPointer
    {
        private readonly string _data;
        public StringPointer(string data)
        {
            Index = 0;
            _data = data;
        }

        public static StringPointer operator ++(StringPointer o)
        {
            if(o.Index < o._data.Length)
                o.Index++;

            return o;
        }

        public int Index { get; private set; }


        public char this[int index]
        {
            get 
            {
                if (index <= _data.Length)
                    return _data[index];

                return (char) 0; 
            }
        }
        
        public static implicit operator StringPointer(string s)
        {
            return new StringPointer(s);
        }

        public static explicit operator string(StringPointer o)
        {
            if (string.IsNullOrEmpty(o._data))
                throw new InvalidOperationException();

            return o.Index == o._data.Length ? "" : o._data.Substring(o.Index);
        }

        public static explicit operator char(StringPointer o)
        {
            if (o._data == null)
                throw new InvalidOperationException();

            if (o.Index == o._data.Length)
                return (char) (0);

            return o._data[o.Index];
        }

        public StringPointer Increment(int i)
        {
            Index += i;
            if (Index > _data.Length)
                Index = _data.Length;

            return this;
        }
    }
}

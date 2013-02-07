namespace RuleSystem
{
    class StringPointer
    {
        private readonly string _data;
        public StringPointer(string data)
        {
            Index = 0;
            _data = data;
        }

        public StringPointer(StringPointer p)
        {
            Index = p.Index;
            _data = p._data;
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
                return null;

            return o.Index == o._data.Length ? null : o._data.Substring(o.Index);
        }

        public static explicit operator char(StringPointer o)
        {
            if (o._data == null || o.Index == o._data.Length)
                return (char) (0);

            return o._data[o.Index];
        }

        public StringPointer Increment(int i, bool withUpdate = true)
        {
            if (withUpdate)
            {
                Index += i;
                if (Index > _data.Length)
                    Index = _data.Length;

                return this;
            }


            var sp = new StringPointer(this);
            return sp.Increment(i);
        }
    }
}

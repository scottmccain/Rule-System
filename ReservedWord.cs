using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectQuery.StatementParser
{
    public class ReservedWord
    {
        string tokenString;
        Enums.TokenCode tokenCode;

        public ReservedWord(string tokenString, Enums.TokenCode tokenCode)
        {
            this.tokenString = tokenString;
            this.tokenCode = tokenCode;
        }

        public string TokenString
        {
            get { return tokenString; }
            set { tokenString = value; }
        }

        public Enums.TokenCode Code
        {
            get { return tokenCode; }
            set { tokenCode = value; }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectQuery.StatementParser
{
    static class ReservedWordDictionary
    {
        static Dictionary<string, ReservedWord> reservedWords = new Dictionary<string, ReservedWord>();

        static ReservedWordDictionary()
        {
            InitializeReservedWords();
        }

        public static ReservedWord FindReservedWord(string key)
        {
            ReservedWord reservedWord = null;
            if (reservedWords.ContainsKey(key.ToLower()))
            {
                reservedWord = reservedWords[key.ToLower()];
            }

            return reservedWord;
        }

        private static void InitializeReservedWords()
        {
            AddReservedWord(new ReservedWord("as", Enums.TokenCode.As));
            AddReservedWord(new ReservedWord("from", Enums.TokenCode.From));
            AddReservedWord(new ReservedWord("where", Enums.TokenCode.Where));
            AddReservedWord(new ReservedWord("like", Enums.TokenCode.Like));
            AddReservedWord(new ReservedWord("in", Enums.TokenCode.In));
        }

        private static void AddReservedWord(ReservedWord reservedWord)
        {
            reservedWords.Add(reservedWord.TokenString, reservedWord);
        }
    }
}

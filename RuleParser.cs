using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RulesProcessing
{
    public class RuleParser : IEnumerable<Rule>
    {
        private readonly IList<Rule> _rules = new List<Rule>();

        public RuleParser(string data)
            : this(new MemoryStream(Encoding.ASCII.GetBytes(data)))
        {
        }

        public RuleParser(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        public RuleParser(Stream stream)
        {
            if( stream == null )
                throw new ArgumentNullException("stream");

            BuildList(stream);
        }

        private void BuildList(Stream stream)
        {
            var reader = new StreamReader(stream);

            var fileData = reader.ReadToEnd();

            var pointer = new StringPointer(fileData);

            while((char)pointer != 0)
            {
                var index = ((string)pointer).IndexOf("(defrule", StringComparison.Ordinal);
                if( index == -1)
                {
                    throw new InvalidDataException("defrule is missing");
                }

                pointer.Increment(9 + index); // skip defule declaration

                // get rule name
                var ruleNameBuilder = new StringBuilder();
                while((char)pointer != 0 && (char)pointer != '\r')
                {
                    ruleNameBuilder.Append((char)pointer);
                    pointer++;
                }

                var r = new Rule {RuleName = ruleNameBuilder.ToString()};
                _rules.Add(r);

                pointer = SkipWhiteSpace((string)pointer);
                pointer = ParseAntecedant((string)pointer, r);

                if (string.IsNullOrEmpty((string)pointer))
                {
                    throw new InvalidDataException("failed to parse antecedant");
                }

                if (((string) pointer).Substring(0, 2) != "=>")
                {
                    throw new InvalidDataException("missing '=>'");
                }
                
                pointer = SkipWhiteSpace((string) pointer.Increment(2));

                pointer = ParseConsequent((string) pointer, r);
                if (string.IsNullOrEmpty((string) pointer))
                {
                    throw new InvalidDataException("failed to parse consequent");
                }

                // ensure we are closing out the current rule
                if ((char) pointer == ')')
                {
                    pointer = SkipWhiteSpace((string) pointer.Increment(1));
                }
                else
                {
                    throw new InvalidDataException("failed to close out current rule");
                }

                r.IsActive = true;
            }
        }

        private static StringPointer ParseConsequent(string fileData, Rule rule)
        {
            var block = new StringPointer(fileData);

            while (true)
            {
                block = SkipWhiteSpace((string)block);
                if ((char) block != '(')
                    break;
                
                var me = rule.Consequent;
                block = ParseElement((string) block, ref me);
                rule.Consequent = me;
            }

            return block;
        }

        private static StringPointer ParseAntecedant(string fileData, Rule rule)
        {
            var block = new StringPointer(fileData);

            while(true)
            {
                block = SkipWhiteSpace((string)block);
                if ((char) block != '(')
                    break;
                
                var me = rule.Antecedent;
                block = ParseElement((string) block, ref me);
                rule.Antecedent = me;
            }

            return block;
        }

        private static StringPointer ParseElement(string fileData, ref MemoryElement me)
        {
            var block = new StringPointer(fileData);
            var memoryElement = new MemoryElement();

            var elementBuilder = new StringBuilder();

            elementBuilder.Append((char)block);
            block++;

            var balance = 1;
            while (true)
            {
                if (string.IsNullOrEmpty((string) block))
                {
                    break;
                }

                if ((char)block == ')') balance--;
                if ((char) block == '(') balance++;

                elementBuilder.Append((char)block);
                block++; 
                
                if (balance == 0) break;
            }

            //block++;

            memoryElement.Element = elementBuilder.ToString();
            memoryElement.Next = null;

            if (me == null)
                me = memoryElement;
            else
            {
                var chain = me;
                while (chain.Next != null) chain = chain.Next;
                chain.Next = memoryElement;
            }

            return block;
        }

        private static StringPointer SkipWhiteSpace(string fileData)
        {
            var block = new StringPointer(fileData);
            while(true)
            {
                var ch = (char) block;
                while((ch != '(') && (ch != ')') && (ch != '=') && (ch != 0) && (ch != ';'))
                {
                    block++;
                    ch = (char) block;
                }

                if (ch != ';')
                    break;
                
                while ((char) (block++) != '\r')
                {
                }
            }

            return block;
        }

        public IEnumerator<Rule> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

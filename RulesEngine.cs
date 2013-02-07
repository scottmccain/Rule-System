using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleSystem
{
    public class RulesEngine
    {
        //private const int MaxRules = 40;
        //private const int MaxTimers = 10;

        private readonly List<Rule> _ruleSet = new List<Rule>();
        private readonly List<Timer> _timers = new List<Timer>();

        //private readonly int _ruleIndex;

        private readonly IMemoryStore _memoryStore;

        public RulesEngine(IMemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        public void AddRuleCollection(IEnumerable<Rule> rules)
        {
            _ruleSet.AddRange(rules);
        }

        public void AddRule(Rule rule)
        {
            _ruleSet.Add(rule);    
        }

        public void Interpret()
        {
#pragma warning disable 168
            foreach (var t in _ruleSet.Where(t => t.IsActive).Where(CheckRule))
#pragma warning restore 168
            {
                break;
            }
        }

        private bool CheckRule(Rule rule)
        {
            var arg = string.Empty;
            return CheckPattern(rule, ref arg) && FireRule(rule, arg);
        }

        private bool FireRule(Rule rule, string arg)
        {
            var retval = false;

            var newCons = string.Empty;
            foreach(var walker in rule.Consequents)
            {
                var terms = GetTerms(walker.Element);
                switch (terms[0])
                {
                    case "(add":
                        ConstructElement(ref newCons, terms[1], arg);
                        retval = PerformAddCommand(newCons);
                        break;
                    case "(delete":
                        ConstructElement(ref newCons, terms[1], arg);
                        retval = PerformDeleteCommand(newCons);
                        break;
                    case "(disable-rule":
                        rule.IsActive = false;
                        retval = true;
                        break;
                    case "(print":
                        retval = PerformPrintCommand(terms[1]);
                        break;
                    case "(enable-timer":
                        retval = PerformEnableCommand(walker.Element);
                        break;
                    case "(quit":
                        EndRun = true;
                        break;
                }

            }

            return retval;
        }

        private bool CheckPattern(Rule rule, ref string arg)
        {
            var found = false;

            var chain = new Stack<MemoryElement>();

            foreach (var terms in rule.Antecedents.Select(antecedent => GetTerms(antecedent.Element)))
            {
                if (string.IsNullOrEmpty(terms[1]))
                {
                    throw new InvalidOperationException("Argument can't be null!");
                }

                /* If the antecedent element is variable, find the matches
                 * in the working memory and store the matched terms.
                */
                if (terms[1][0] != '?') continue;
                for (var i = 0; i < _memoryStore.Length; i++)
                {
                    if (!_memoryStore[i].IsActive) continue;
                    var wmTerms = GetTerms(_memoryStore[i].Element);
                    if (!terms[0].Equals(wmTerms[0])) continue;
                    var newElement = new MemoryElement { Element = wmTerms[1] };
                    chain.Push(newElement);
                }
            }

            /* Now that we have the replacement strings, walk through the rules trying
             * the replacement string when necessary.
             */
            do
            {
                //var curRule = rule.Antecedent;

                string[] terms = null;
                foreach(var curRule in rule.Antecedents)
                {
                    terms = GetTerms(curRule.Element);
                    if (terms[0].IndexOf("(true", StringComparison.Ordinal) == 0)
                    {
                        found = true;
                        break;
                    }

                    if ((terms[1][0] == '?') && (chain.Count > 0))
                    {
                        terms[1] = chain.Peek().Element;
                    }

                    found = SearchWorkingMemory(terms[0], terms[1]);

                    if (!found) break;
                }

                if (found)
                {
                    /* Cleanup the replacement string chain */
                    arg = terms[1];
                    chain.Clear();
                    break;
                }

                if(chain.Count > 0 )
                {
                    chain.Pop();
                }

            } while (chain.Count > 0);

            return found;
        }

        public void ProcessTimers()
        {
            foreach (var timer in _timers.Where(timer => timer.IsActive && --timer.Expiration == 0))
            {
                FireTimer(timer);
            }
        }

        private void FireTimer(Timer timer)
        {
            var element = string.Format("(timer-triggered {0})", timer.Index);
            PerformAddCommand(element);
            timer.IsActive = false;
        }

        public string PrintStructure()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("Number of Rules: {0}\n", _ruleSet.Count);
            foreach (var rule in _ruleSet.Where(rule => rule.IsActive))
            {
                builder.AppendFormat("Rule {0} :\n", rule.RuleName);
                builder.Append("Antecedents : \n");

                foreach(var mem in rule.Antecedents)
                {
                    builder.AppendFormat("  {0}\n", mem.Element);
                }

                builder.Append("Consequents : \n");

                foreach(var mem in rule.Consequents)
                {
                    builder.AppendFormat("  {0}\n", mem.Element);
                }
                builder.Append("\n");
            }

            return builder.ToString();
        }

        public string PrintWorkingMemory()
        {
            var builder = new StringBuilder();

            builder.Append("\tWorking Memory:\n");

            var query = from mem in _memoryStore.Elements
                        where mem.IsActive
                        select mem;

            foreach (var m in query.ToList())
            {
                builder.AppendFormat("\t\t{0}\n", m.Element);
            }

            builder.Append("\n");

            return builder.ToString();
        }

        public bool EndRun { get; private set; }

        private bool PerformEnableCommand(string element)
        {
            var parts = element.Split(' ');
            if( parts.Length == 3)
            {
                var timer = parts[1];

                if (timer[0] == '(')
                    timer = timer.Substring(1);

                int expiration;

                var builder = new StringBuilder();

                var index = 0;
                while (index < parts[2].Length && parts[2][index] != ')')
                {
                    builder.Append(parts[2][index]);
                    index++;
                }

                int.TryParse(builder.ToString(), out expiration);
                StartTimer(timer, expiration);
            }

            return true;
        }

        private void StartTimer(string index, int expiration)
        {
            var timer = _timers.FirstOrDefault(t => t.Index == index);
            if (timer == null)
            {
                timer = new Timer {Index = index};
                _timers.Add(timer);
            }

            timer.Expiration = expiration;
            timer.IsActive = true;
        }

        private static bool PerformPrintCommand(string element)
        {
            var i = 0;

            // Find initial '"'
            while (i < element.Length && element[i] != '"') i++;
            i++;

            var builder = new StringBuilder();
            var ignore = false;
            while (i < element.Length && (element[i] != '"' || ignore))
            {
                if (element[i] == '\\')
                {
                    ignore = true;
                }
                else
                {
                    builder.Append(element[i]);
                    ignore = false;
                }

                i++;
            }

            Console.WriteLine(builder.ToString().Trim());

            return true;
        }

        private bool PerformDeleteCommand(string element)
        {
            _memoryStore.Remove(element);
            return _memoryStore.ModifiedByLastAction;
        }

        private bool PerformAddCommand(string element)
        {
            _memoryStore.Add(element);
            return _memoryStore.ModifiedByLastAction;
        }

        private static void ConstructElement(ref string newElement, string oldElement, string arg)
        {
            if (newElement == null) throw new ArgumentNullException("newElement");

            var oldp = new StringPointer(oldElement);

            while((char)oldp != '(') oldp++;

            var builder = new StringBuilder();
            while(!string.IsNullOrEmpty((string)oldp) && ((char)oldp != '?'))
            {
                builder.Append((char) oldp);
                oldp++;
            }
            
            if(string.IsNullOrEmpty((string)oldp))
            {
                builder.Remove(builder.Length - 1, 1);
                newElement = builder.ToString();
                return;
            }

            builder.Append(arg);
            newElement = builder.ToString();

        }

        private bool SearchWorkingMemory(string term1, string term2)
        {
            return (from t in _memoryStore.Elements where t.IsActive select GetTerms(t.Element)).Any(terms => (terms[0] == term1) && (terms[1] == term2));
        }

        private static string[] GetTerms(string element)
        {
            var terms = new string[2];

            // get index of first space
            var index = element.IndexOf(' ');
            if (index != -1)
            {
                terms[0] = element.Substring(0, index);

                if( index + 1 < element.Length )
                    terms[1] = element.Substring(index + 1);
            }

            return terms;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesProcessing
{
    public class RulesEngine
    {
        //private const int MaxRules = 40;
        //private const int MaxTimers = 10;

        private readonly List<Rule> _ruleSet = new List<Rule>();
        private readonly List<Timer> _timers = new List<Timer>();

        //private readonly int _ruleIndex;

        private readonly MemoryStore _memoryStore;

        public RulesEngine(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
            //_ruleIndex = ruleIndex;

            //for (var i = 0; i < MaxTimers; i++)
            //{
            //    _timers.Add(new Timer { IsActive = false });
            //}

            //for (int i = 0; i < MaxRules; i++)
            //{
            //    _ruleSet[i] = new Rule { IsActive = false };
            //}
        }

        public void AddRule(Rule rule)
        {
            _ruleSet.Add(rule);    
        }

        public void Interpret()
        {
            for (int ruleIndex = 0; ruleIndex < _ruleSet.Count; ruleIndex++ )
            {
                if (!_ruleSet[ruleIndex].IsActive) continue;

                // if rule modified memory we exit loop
                if (CheckRule(_ruleSet[ruleIndex])) break;
            }

            //for(var rule = 0; rule < MaxRules; rule++)
            //{
            //    if (!_ruleSet[rule].IsActive) continue;
                
            //    // if rule modified memory we exit the loop
            //    if (CheckRule(rule))
            //    {
            //        break;
            //    }
            //}
        }

        private bool CheckRule(Rule rule)
        {
            var arg = string.Empty;
            return CheckPattern(rule, ref arg) && FireRule(rule, arg);
        }

        private bool FireRule(Rule rule, string arg)
        {
            var retval = false;
            var walker = rule.Consequent;

            var newCons = string.Empty;
            while (walker != null)
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

                walker = walker.Next;
            }

            return retval;
        }

        private bool CheckPattern(Rule rule, ref string arg)
        {
            var ret = false;
            var antecedent = rule.Antecedent;
            MemoryElement chain = null;

            while (antecedent != null)
            {
                var terms = GetTerms(antecedent.Element);

                if (string.IsNullOrEmpty(terms[1]))
                {
                    throw new InvalidOperationException("Argument can't be null!");
                }

                /* If the antecedent element is variable, find the matches
                 * in the working memory and store the matched terms.
                */
                if (terms[1][0] == '?')
                {
                    for (var i = 0; i < _memoryStore.Length; i++)
                    {
                        if (!_memoryStore[i].IsActive) continue;
                        var wmTerms = GetTerms(_memoryStore[i].Element);
                        if (terms[0].Equals(wmTerms[0]))
                        {
                            //AddToChain(wmTerms[1]);

                            var newElement = new MemoryElement { Element = wmTerms[1] };
                            if (chain == null)
                            {
                                chain = newElement;
                            }
                            else
                            {
                                // add to tail
                                var walker = chain;
                                while (walker.Next != null) walker = walker.Next;
                                walker.Next = newElement;
                            }

                            newElement.Next = null;
                        }
                    }
                }


                antecedent = antecedent.Next;

            }

            /* Now that we have the replacement strings, walk through the rules trying
             * the replacement string when necessary.
             */
            do
            {
                var curRule = rule.Antecedent;

                string[] terms = null;
                while (curRule != null)
                {
                    terms = GetTerms(curRule.Element);
                    if (terms[0].IndexOf("(true", StringComparison.Ordinal) == 0)
                    {
                        ret = true;
                        break;
                    }

                    if ((terms[1][0] == '?') && (chain != null))
                    {
                        terms[1] = chain.Element;
                    }

                    //lastTerm = terms[0] + " " + terms[1];

                    ret = SearchWorkingMemory(terms[0], terms[1]);

                    if (!ret) break;

                    curRule = curRule.Next;
                }

                if (ret)
                {
                    /* Cleanup the replacement string chain */
                    arg = terms[1];
                    chain = null;
                }
                else
                {
                    if (chain != null)
                    {
                        chain = chain.Next;
                    }
                }

            } while (chain != null);

            return ret;
        }

        //public void Parse(Stream stream)
        //{
        //    var reader = new StreamReader(stream);

        //    var textBuffer = new TextBuffer(reader.ReadToEnd());
        //    var scanner = new RuleScanner(textBuffer);

        //    var next = scanner.Get();

        //    while (next.Code != Enums.TokenCode.EndOfFile && next.Code != Enums.TokenCode.Error)
        //    {
        //        if (next.Code != Enums.TokenCode.LParen) continue;

        //        // get next token
        //        next = scanner.Get();
        //        if (next.TokenString ==  StringConstants.DefaultRule)
        //        {
        //            next = scanner.Get();
        //            _ruleSet[_ruleIndex].RuleName = next.TokenString;

        //            next = ParseAntecedent(scanner, _ruleSet[_ruleIndex]);
        //            if (next.Code == Enums.TokenCode.Equal)
        //            {
        //                next = scanner.Get();
        //                if (next.Code == Enums.TokenCode.Gt)
        //                {
        //                    next = ParseConsequent(scanner, _ruleSet[_ruleIndex]);

        //                    if (next.Code == Enums.TokenCode.RParen)
        //                    {
        //                        _ruleSet[_ruleIndex].IsActive = true;
        //                        _ruleIndex++;

        //                        next = scanner.Get();
        //                    }
        //                    else
        //                    {
        //                        next.Code = Enums.TokenCode.Error;
        //                    }
        //                }
        //                else
        //                {
        //                    next.Code = Enums.TokenCode.Error;
        //                }
        //            }
        //            else
        //            {
        //                next.Code = Enums.TokenCode.Error;
        //            }
        //        }
        //        else
        //        {
        //            next.Code = Enums.TokenCode.Error;
        //        }
        //    }
        //}

        //private IToken ParseConsequent(RuleScanner scanner, Rule rule)
        //{
        //    var token = scanner.Get();
        //    while (token.Code == Enums.TokenCode.LParen)
        //    {
        //        var element = rule.Consequent;
        //        ParseElement(scanner, ref element);
        //        rule.Consequent = element;

        //        token = scanner.Get();
        //    }

        //    return token;
        //}

        //private IToken ParseAntecedent(RuleScanner scanner, Rule rule)
        //{
        //    var token = scanner.Get();
        //    while (token.Code == Enums.TokenCode.LParen)
        //    {
        //        MemoryElement element = rule.Antecedent;
        //        ParseElement(scanner, ref element);
        //        rule.Antecedent = element;

        //        token = scanner.Get();
        //    }

        //    return token;
        //}

        //private static void ParseElement(RuleScanner scanner, ref MemoryElement met)
        //{
        //    var balance = 1;

        //    var element = new MemoryElement();

        //    var elementBuilder = new StringBuilder();

        //    while (true)
        //    {
        //        var token = scanner.Get();
        //        if (token.Code == Enums.TokenCode.EndOfFile) break;

        //        switch (token.Code)
        //        {
        //            case Enums.TokenCode.RParen:
        //                balance--;
        //                break;
        //            case Enums.TokenCode.LParen:
        //                balance++;
        //                break;
        //            default:
        //                elementBuilder.Append(token.TokenString);
        //                elementBuilder.Append(" ");
        //                break;
        //        }

        //        //if( token.Code == Enums.TokenCode.Word )

        //        if (balance == 0) break;
        //    }

        //    element.Element = elementBuilder.ToString().Trim();
        //    element.Next = null;
        //    if (met == null)
        //    {
        //        met = element;
        //    }
        //    else
        //    {
        //        var chain = met;
        //        while (chain.Next != null) chain = chain.Next;
        //        chain.Next = element;
        //    }

        //}
    
        public void ProcessTimers()
        {
            foreach (var timer in _timers.Where(timer => timer.IsActive && --timer.Expiration == 0))
            {
                FireTimer(timer);
            }

            //for (int i = 0; i < MaxTimers; i++)
            //{
            //    if (_timers[i].IsActive)
            //    {
            //        if (--_timers[i].Expiration == 0)
            //        {
            //            FireTimer(i);
            //        }
            //    }
            //}
        }

        private void FireTimer(Timer timer)
        {
            var element = string.Format("(timer-triggered {0})", timer.Index);
            PerformAddCommand(element);
            timer.IsActive = false;
            //_timers[timerIndex].IsActive = false;
        }

        public string PrintStructure()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("Number of Rules: {0}\n", _ruleSet.Count);
            foreach (var rule in _ruleSet.Where(rule => rule.IsActive))
            {
                builder.AppendFormat("Rule {0} :\n", rule.RuleName);
                builder.Append("Antecedents : \n");

                var mem = rule.Antecedent;
                while (mem != null)
                {
                    builder.AppendFormat("  {0}\n", mem.Element);
                    mem = mem.Next;
                }

                builder.Append("Consequents : \n");

                mem = rule.Consequent;
                while (mem != null)
                {
                    builder.AppendFormat("  {0}\n", mem.Element);
                    mem = mem.Next;
                }
                builder.Append("\n");
            }

            //for (int i = 0; i < MaxRules; i++)
            //{

            //    if (_ruleSet[i].IsActive)
            //    {

            //        builder.AppendFormat("Rule {0} :\n", i);
            //        builder.Append("Antecedents : \n");

            //        var mem = _ruleSet[i].Antecedent;
            //        while (mem != null)
            //        {
            //            builder.AppendFormat("  {0}\n", mem.Element);
            //            mem = mem.Next;
            //        }

            //        builder.Append("Consequents : \n");
                    
            //        mem = _ruleSet[i].Consequent;
            //        while (mem != null)
            //        {
            //            builder.AppendFormat("  {0}\n", mem.Element);
            //            mem = mem.Next;
            //        }
            //        builder.Append("\n");
            //    }
            //}

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

        //private bool CheckRule(int rule)
        //{
        //    var arg = string.Empty;
        //    return CheckPattern(rule, ref arg) && FireRule(rule, arg);
        //}

        //private void FireTimer(int timerIndex)
        //{
        //    var element = string.Format("timer-triggered {0}", timerIndex);

        //    PerformAddCommand(element);
        //    _timers[timerIndex].IsActive = false;
        //}

        //private bool FireRule(int rule, string arg)
        //{
        //    var retval = false;
        //    var walker = _ruleSet[rule].Consequent;

        //    string newCons = string.Empty;
        //    while (walker != null)
        //    {
        //        var terms = GetTerms(walker.Element);
        //        switch (terms[0])
        //        {
        //            case "add":
        //                ConstructElement(ref newCons, terms[1], arg);
        //                retval = PerformAddCommand(newCons);
        //                break;
        //            case "delete":
        //                ConstructElement(ref newCons, terms[1], arg);
        //                retval = PerformDeleteCommand(newCons);
        //                break;
        //            case "disable":
        //                _ruleSet[rule].IsActive = false;
        //                retval = true;
        //                break;
        //            case "print":
        //                retval = PerformPrintCommand(terms[1]);
        //                break;
        //            case "enable":
        //                retval = PerformEnableCommand(walker.Element);
        //                break;
        //            case "quit":
        //                EndRun = true;
        //                break;
        //        }

        //        walker = walker.Next;
        //    }

        //    return retval;
        //}

        private bool PerformEnableCommand(string element)
        {
            var parts = element.Split(' ');
            if( parts.Length == 3)
            {
                string timer = parts[1];

                if (timer[0] == '(')
                    timer = timer.Substring(1);

                int expiration;
                //int.TryParse(parts[], out timer);

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

            //// Copy until we reach the end
            //while ( element[i] != '"') string[j++] = element[i++];
            //string[j] = 0;

            //printf("%s\n", string);

            //return 1;        

            return true;
        }

        private bool PerformDeleteCommand(string element)
        {
            _memoryStore.Remove(element);
            return _memoryStore.ModifiedByLastAction;
            //var query = from m in _workingMemory
            //            where m.IsActive && m.Element.Equals(element)
            //            select m;

            //var retval = false;
            //foreach(var mem in query.ToList())
            //{
            //    _workingMemory.Remove(mem);

            //    retval = true;
            //}

            //return retval;
        }

        private bool PerformAddCommand(string element)
        {
            _memoryStore.Add(element);
            return _memoryStore.ModifiedByLastAction;
            //var query = from mem in _workingMemory
            //            where mem.IsActive && mem.Element.Equals(element)
            //            select mem;

            //// Check to ensure that this element isn't already in working memory
            //if (query.FirstOrDefault() != null)
            //{
            //    return false;
            //}

            //var memoryElement = _workingMemory.FirstOrDefault(p => p.Element.Equals(element));
            //if( memoryElement != null )
            //{
            //    memoryElement.IsActive = true;
            //}
            //else
            //{
            //    _workingMemory.Add(new MemoryElement { IsActive = true, Element = element });
            //}

            //return false;
        }

        //private int findEmptyMemSlot()
        //{
        //    int i;

        //    for (i = 0; i < MAX_MEMORY_ELEMENTS; i++)
        //    {
        //        if (!workingMemory[i].IsActive) break;
        //    }

        //    return i;
        //}

        private static void ConstructElement(ref string newElement, string oldElement, string arg)
        {
            if (newElement == null) throw new ArgumentNullException("newElement");

            //var newp = new StringPointer(newElement);
            var oldp = new StringPointer(oldElement);

            //oldp++;
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


            //var index = oldElement.IndexOf('?');

            //if (index == -1)
            //{
            //    // complete rule (i.e., no ? element)
            //    newElement = oldElement;
            //}
            //else
            //{
            //    newElement = oldElement.Substring(0, index);
            //    //newElement += " ";
            //    newElement += arg;
            //}
        }

        //private bool CheckPattern(int rule, ref string arg)
        //{
        //    var ret = false; 
        //    var antecedent = _ruleSet[rule].Antecedent;
        //    MemoryElement chain = null;

        //    while (antecedent != null)
        //    {
        //        var terms = GetTerms(antecedent.Element);

        //        if( string.IsNullOrEmpty(terms[1]))
        //        {
        //            throw new InvalidOperationException("Argument can't be null!");
        //        }

        //        /* If the antecedent element is variable, find the matches
        //         * in the working memory and store the matched terms.
        //        */
        //        if (terms[1][0] == '?')
        //        {
        //            for (var i = 0; i < _memoryStore.Length; i++)
        //            {
        //                if (!_memoryStore[i].IsActive) continue;
        //                var wmTerms = GetTerms(_memoryStore[i].Element);
        //                if (terms[0].Equals(wmTerms[0]))
        //                {
        //                    //AddToChain(wmTerms[1]);

        //                    var newElement = new MemoryElement { Element = wmTerms[1] };
        //                    if (chain == null)
        //                    {
        //                        chain = newElement;
        //                    }
        //                    else
        //                    {
        //                        // add to tail
        //                        var walker = chain;
        //                        while (walker.Next != null) walker = walker.Next;
        //                        walker.Next = newElement;
        //                    }

        //                    newElement.Next = null;
        //                }
        //            }
        //        }


        //        antecedent = antecedent.Next;

        //    }

        //    /* Now that we have the replacement strings, walk through the rules trying
        //     * the replacement string when necessary.
        //     */
        //    do
        //    {
        //        var curRule = _ruleSet[rule].Antecedent;

        //        string[] terms = null;
        //        while (curRule != null)
        //        {
        //            terms = GetTerms(curRule.Element);
        //            if (terms[0].IndexOf("true", StringComparison.Ordinal) == 0)
        //            {
        //                ret = true;
        //                break;
        //            }

        //            if ((terms[1][0] == '?') && (chain != null))
        //            {
        //                terms[1] =  chain.Element;
        //            }

        //            //lastTerm = terms[0] + " " + terms[1];

        //            ret = SearchWorkingMemory(terms[0], terms[1]);

        //            if (!ret) break;

        //            curRule = curRule.Next;
        //        }

        //        if (ret)
        //        {
        //            /* Cleanup the replacement string chain */
        //            arg = terms[1];
        //            chain = null;
        //        }
        //        else
        //        {
        //            if (chain != null)
        //            {
        //                chain = chain.Next;
        //            }
        //        }

        //    } while (chain != null);

        //    return ret;
        //}

        private bool SearchWorkingMemory(string term1, string term2)
        {
            return (from t in _memoryStore.Elements where t.IsActive select GetTerms(t.Element)).Any(terms => (terms[0] == term1) && (terms[1] == term2));
        }

        //private void AddToChain(string element)
        //{
        //    var newElement = new MemoryElement {Element = element};
        //    if (_chain == null)
        //    {
        //        _chain = newElement;
        //    }
        //    else
        //    {
        //        // add to tail
        //        var walker = _chain;
        //        while (walker.Next != null) walker = walker.Next;
        //        walker.Next = newElement;
        //    }

        //    newElement.Next = null;
        //}

        private static string[] GetTerms(string element)
        {
            var terms = new string[2];

            // get index of first space
            int index = element.IndexOf(' ');
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

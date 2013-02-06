using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RulesProcessing;
using RuleSystem;

namespace RuleSystemDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var ms = new ListMemoryStore();

            var re = new RulesEngine(ms);

            string rule = @"(defrule blah
                                ; malformed comment here
                                (true null)
                        =>
                    (add (i-started true))
                    (disable (self))
                )";

            var parser = new RuleParser(rule);
            re.AddRuleCollection(parser);

            // now we can operate on the rules

        }
    }
}

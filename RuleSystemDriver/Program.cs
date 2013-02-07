using System;
using RuleSystem;

namespace RuleSystemDriver
{
    class Program
    {
        static void Main()
        {
            var ms = new ListMemoryStore();

            var re = new RulesEngine(ms);

            const string rule = @"
;
;  Animal Identification
;

(defrule init
	(true null)
=>
    (add (is-flier animal))
    (add (is-egg-layer animal))
    (add (can-talk animal))
    (enable-timer (timera 5))
	(disable-rule (self))
)

(defrule mammal-1
	(has-hair ?)
=>
	(add (is-animal ?))
)

(defrule mammal-2
	(is-milk-giver ?)
=>
	(add (is-mammal ?))
)

(defrule trigger1
    (timer-triggered timera)
=>
	(add (lives-in-jungle animal))
    (delete (timer-triggered timera))
)

(defrule bird-1
	(has-feathers ?)
=>
	(add (is-bird ?))
)

(defrule bird-2
	(is-flier ?)
	(is-egg-layer ?)
=>
	(add (is-bird ?))
)

(defrule carnivore-1
	(is-meat-eater ?)
=>
	(add (is-carnivore ?))
)

(defrule carnivore-2
	(has-pointed-teeth ?)
	(has-claws ?)
	(has-forward-eyes ?)
=>
	(add (is-carnivore ?))
)

(defrule ungulate-1
	(is-mammal ?)
	(has-hooves ?)
=>
	(add (is-ungulate ?))
)

(defrule ungulate-2
	(is-mammal ?)
	(is-cud-chewer ?)
=>
	(add (is-ungulate ?))
)

(defrule even-toed
	(is-mammal ?)
	(is-cud-chewer ?)
=>
	(add (is-even-toed ?))
)

(defrule cheetah
	(is-mammal ?)
	(is-carnivore ?)
	(is-tawny-colored ?)
	(has-dark-spots ?)
=>
	(add (is-cheetah ?))
	(print (""Animal is a cheetah""))
	(quit null)
)

(defrule parrot
	(is-bird ?)
	(can-talk ?)
	(lives-in-jungle ?)
=>
	(add (is-parrot ?))
	(print (""Animal is a parrot!""))
    (quit null)
)

(defrule tiger
	(is-mammal ?)
	(is-carnivore ?)
	(is-tawny-colored ?)
	(has-black-stripes ?)
=>
	(add(is-tiger ?))
	(print(""Animal is a tiger""))
	(quit null)
)

(defrule giraffe
	(is-ungulate ?)
	(has-long-neck ?)
	(has-long-legs ?)
	(has-dark-spots ?)
=>
	(add (is-giraffe ?))
	(print (""Animal is a giraffe""))
	(quit null)
)

(defrule zebra
	(is-ungulate ?)
	(has-black-stripes ?)
=>
	(add (is-zebra ?))
	(print (""Animal is a zebra""))
	(quit null)
)";

            var parser = new RuleParser(rule);
            re.AddRuleCollection(parser);

            //// now we can operate on the rules
            //re.Interpret();
            //Console.WriteLine(re.PrintWorkingMemory());
            //re.Interpret();
            //Console.WriteLine(re.PrintWorkingMemory());

            while (!re.EndRun)
            {
                re.Interpret();

                Console.WriteLine(re.PrintWorkingMemory());
                re.ProcessTimers();

                //Thread.Sleep(1000);
            }
        }
    }
}

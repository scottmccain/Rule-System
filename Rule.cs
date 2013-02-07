using System.Collections.Generic;

namespace RuleSystem
{
    public class Rule
    {
        public bool IsActive { get; set; }
        public string RuleName { get; set;  }
        public IList<MemoryElement> Antecedents { get; set; }
        public IList<MemoryElement> Consequents { get; set; }
    }
}

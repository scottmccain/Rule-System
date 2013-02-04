namespace RulesProcessing
{
    public class Rule
    {
        public bool IsActive { get; set; }
        public string RuleName { get; set;  }
        public MemoryElement Antecedent { get; set; }
        public MemoryElement Consequent { get; set; }
    }
}

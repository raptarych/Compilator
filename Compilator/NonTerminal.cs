using System.Collections.Generic;

namespace Compilator
{
    public class NonTerminal
    {
        public string Name;
        public HashSet<Rule> Rules = new HashSet<Rule>();
        public override string ToString() => Name;
    }

    public class Rule
    {
        public string Value;
        public HashSet<string> TerminalsSet = new HashSet<string>();
        public override string ToString() => Value;
    }
}

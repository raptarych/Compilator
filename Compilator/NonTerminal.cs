using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Name;
        public HashSet<string> TerminalsSet = new HashSet<string>();
        public override string ToString() => Name;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Utils
    {
        public const char Empty = 'ε';
        public const string EmptyString = "ε";
    }

    public enum LexemCharType
    {
        Text = 1,
        Separators = 2,
        Operations = 3
    }

    public enum LexemType
    {
        UNKNOWN_LEXEM = -1,
        IDENTIFIER = 1,
        KEYWORD = 2,
        OPERATION = 3,
        SEPARATOR = 4,
        CONSTANT = 5
    }

    public enum StackItemType
    {
        Terminal = 1,
        NonTerminal = 2
    }
}

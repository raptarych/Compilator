using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Lexem
    {
        public LexemType Key { get; set; }
        public byte Value { get; set; }
    }
    class Lexems
    {
        public static readonly List<string> Keywords = new List<string>()
        {
            "int",
            "float",
            "string",
            "while",
            "if",
            "then",
            "else"
        };
        public static readonly List<string> Operations = new List<string>()
        {
            "=",
            "+",
            "-",
            "*",
            "/",
            "^"
        };

        public static List<string> Identifiers = new List<string>();
        public static List<object> Constants = new List<object>();
        public static readonly List<char> Separators = new List<char>
        {
            ';',
            '(',
            ')',
            '{',
            '}',
            ']',
            '[',
            '.',
            ','
        };

    }
}

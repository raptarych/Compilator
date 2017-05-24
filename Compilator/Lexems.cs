using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Lexem
    {
        public LexemType Key { get; set; }
        public byte ValuePtr { get; set; }

        public object GetValue()
        {
            switch (Key)
            {
                case LexemType.CONSTANT:
                    return Lexems.Constants[ValuePtr];
                case LexemType.KEYWORD:
                    return Lexems.Keywords[ValuePtr];
                case LexemType.IDENTIFIER:
                    return Lexems.Identifiers[ValuePtr];
                case LexemType.OPERATION:
                    return Lexems.Operations[ValuePtr];
                default:
                    return (char) ValuePtr;
            }
                
        }

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
            "^",
            ":=",
            ":",
            "++",
            "--"
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
            ','
        };

    }
}

using System.Collections.Generic;

namespace Compilator
{
    static class CommonTables
    {
        public static Dictionary<string, object> Variables = new Dictionary<string, object>();

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

        public static readonly List<string> Identifiers = new List<string>();
        public static readonly List<object> Constants = new List<object>();
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

        public static Lexem SaveConstant(object value)
        {
            if (!Constants.Contains(value)) Constants.Add(value);
            return new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte) Constants.IndexOf(value) };
        }

    }
}

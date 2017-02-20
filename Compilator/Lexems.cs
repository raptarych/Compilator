﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Lexem
    {
        public int Key { get; set; }
        public int Value { get; set; }
    }
    class Lexems
    {
        public static readonly List<string> Keywords = new List<string>()
        {
            "int",
            "double",
            "sign",
            "float"
        };
        public static readonly List<string> Operations = new List<string>()
        {
            "=",
            "+",
            "-",
            "*",
            "/",
            "++",
            "--",
            "+=",
            "-=",
            "*=",
            "/="
        };
        public static readonly List<string> Identifiers = new List<string>()
        {
            "a",
            "b",
            "c",
            "test",
            "foo",
            "bar"
        };
        public static readonly List<char> Separators = new List<char>
        {
            ';',
            '(',
            ')',
            '{',
            '}',
            ']',
            '['
        };

    }
}

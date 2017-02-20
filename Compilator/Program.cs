using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Program
    {
        public const int IDENTIFIER = 1;
        public const int KEYWORD = 2;
        public const int OPERATION = 3;
        public const int SEPARATOR = 4;
        public const int CONSTANT = 5;
        static void LexicalAnalysis(string inputString)
        {
            var lexems = new List<Lexem>();

            //1. Разделение лексем

            //Просто:
            //var rawLexems = inputString.Split(' ');

            //Сложно:
            var rawLexems = new List<string>();
            var currentLexem = "";
            var currentLexemIsText = true;
            while (inputString.Length > 0)
            {
                var currentChar = inputString[0];
                var sCurrentChar = currentChar.ToString();
                var isText = !(Lexems.Separators.Contains(currentChar) || 
                    Lexems.Operations.Contains(sCurrentChar));

                if (currentChar == ' ')
                {
                    if (currentLexem.Length > 0) rawLexems.Add(currentLexem);
                    inputString = inputString.Substring(1, inputString.Length - 1);
                    currentLexem = "";
                    continue;
                }

                if (currentLexem.Length == 0 || isText == currentLexemIsText)
                {
                    if (Lexems.Separators.Contains(currentChar))
                    {
                        if (currentLexem.Length > 0) rawLexems.Add(currentLexem);
                        rawLexems.Add(sCurrentChar);
                        inputString = inputString.Substring(1, inputString.Length - 1);
                        currentLexem = "";
                        continue;
                    }
                    currentLexem += currentChar;
                    currentLexemIsText = isText;
                    inputString = inputString.Substring(1, inputString.Length - 1);
                    continue;
                }

                rawLexems.Add(currentLexem);
                currentLexemIsText = isText;
                currentLexem = currentChar.ToString().Trim();
                inputString = inputString.Substring(1, inputString.Length - 1);
            }
            if (currentLexem.Length > 0) rawLexems.Add(currentLexem);
            Console.Write($"\nRaw lexems: {string.Join(",", rawLexems.Select(lexem => $"\"{lexem}\""))}\n");

            //2. Разбор лексем
            foreach (var lexemString in rawLexems)
            {
                if (lexemString.Length == 0) continue;
                var firstChar = lexemString[0];
                if (firstChar >= '0' && firstChar <= '9')
                {
                    int lexemValue;
                    int.TryParse(lexemString, out lexemValue);
                    lexems.Add(new Lexem() { Key = CONSTANT, Value = lexemValue});
                    continue;
                }
                if (Lexems.Separators.Contains(firstChar))
                {
                    lexems.Add(new Lexem() { Key = SEPARATOR, Value = (int)lexemString[0] });
                }
                if (Lexems.Identifiers.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = IDENTIFIER, Value = (int)lexemString[0] });
                    continue;
                }
                if (Lexems.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = KEYWORD, Value = Lexems.Keywords.IndexOf(lexemString) });
                    continue;
                }
                if (Lexems.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = OPERATION, Value = (int) lexemString[0] });
                    continue;
                }
            }

            //Формат для вывода
            var outputString = $"Lexical analysis output: {string.Join("",lexems.Select(lexem => $"({lexem.Key},{lexem.Value})"))}";
            Console.WriteLine(outputString);

        }
        static void Main(string[] args)
        {
            var conWorker = new ConWorker();
            conWorker.AddHandler(LexicalAnalysis);
            conWorker.Start();
        }
    }
}

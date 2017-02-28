using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Program
    {
        private const int UNKNOWN_LEXEM = -1;
        private const int IDENTIFIER = 1;
        private const int KEYWORD = 2;
        private const int OPERATION = 3;
        private const int SEPARATOR = 4;
        private const int CONSTANT = 5;
        static void LexicalAnalysis(string inputString)
        {
            List<Lexem> lexems = new List<Lexem>();

            //1. Разделение лексем

            //Просто:
            //var rawLexems = inputString.Split(' ');

            //Сложно:
            List<string> rawLexems = new List<string>();
            string currentLexem = "";
            bool currentLexemIsText = true;
            while (inputString.Length > 0)
            {
                char currentChar = inputString[0];
                string sCurrentChar = currentChar.ToString();
                bool isText = !(Lexems.Separators.Contains(currentChar) || 
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
            Console.Write($"\nRaw lexems: \t\t\t{string.Join(",", rawLexems.Select(lexem => $"[{lexem}]"))}\n");

            //2. Разбор лексем
            foreach (var lexemString in rawLexems)
            {
                if (lexemString.Length == 0) continue;
                char firstChar = lexemString[0];
                if (firstChar >= '0' && firstChar <= '9')
                {
                    int lexemValue;
                    int.TryParse(lexemString, out lexemValue);
                    lexems.Add(new Lexem() { Key = CONSTANT, Value = (byte) lexemValue});
                    continue;
                }
                if (Lexems.Separators.Contains(firstChar))
                {
                    lexems.Add(new Lexem() { Key = SEPARATOR, Value = (byte) lexemString[0] });
                    continue;
                }
                if (Lexems.Identifiers.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = IDENTIFIER, Value = (byte) lexemString[0] });
                    continue;
                }
                if (Lexems.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = KEYWORD, Value = (byte) Lexems.Keywords.IndexOf(lexemString) });
                    continue;
                }
                if (Lexems.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = OPERATION, Value = (byte) lexemString[0] });
                    continue;
                }
                lexems.Add(new Lexem() { Key = UNKNOWN_LEXEM, Value = 0 });
            }

            //3. Формат для вывода
            string outputString = $"Lexical analysis output:\t{string.Join("",lexems.Select(lexem => $"({lexem.Key},{lexem.Value})"))}";
            Console.WriteLine(outputString);

        }
        static void Main(string[] args)
        {
            ConWorker conWorker = new ConWorker();
            conWorker.AddHandler(LexicalAnalysis);
            conWorker.Start();
        }
    }
}

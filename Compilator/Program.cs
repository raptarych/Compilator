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
            var lexemSplitter = new LexicalSplitter();
            List<string> rawLexems = lexemSplitter.Parse(inputString);
            Console.Write($"\nRaw lexems: {string.Join(",",rawLexems.Select(lexem => $"[{lexem}]"))}\n");

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

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
                if (firstChar == '"')
                {
                    //Константа текстовая
                    var lexem = lexemString;
                    if (lexemString.IndexOf("\"") > 0) lexem = lexemString.Substring(1, lexemString.Length - 2);
                    if (!Lexems.Constants.Contains(lexem))
                    {
                        Lexems.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = CONSTANT, Value = (byte)Lexems.Constants.IndexOf(lexem) });
                    continue;
                }
                if (firstChar >= '0' && firstChar <= '9')
                {
                    //Константа числовая
                    var lexem = 0;
                    if (!int.TryParse(lexemString, out lexem))
                    {
                        Console.WriteLine($"Invalid identifier: {lexemString});");
                        return;
                    }
                    if (!Lexems.Constants.Contains(lexem))
                    {
                        Lexems.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = CONSTANT, Value = (byte)Lexems.Constants.IndexOf(lexem) });
                    continue;
                }
                //Разделитель
                if (Lexems.Separators.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = SEPARATOR, Value = (byte)Lexems.Separators.IndexOf(lexemString) });
                    continue;
                }
                //Ключевое слово
                if (Lexems.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = KEYWORD, Value = (byte) Lexems.Keywords.IndexOf(lexemString) });
                    continue;
                }
                //Операция
                if (Lexems.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = OPERATION, Value = (byte) lexemString[0] });
                    continue;
                }
                //Идентификатор (но это не точно)
                if (!Lexems.Identifiers.Contains(lexemString))
                {
                    Lexems.Identifiers.Add(lexemString);
                }
                lexems.Add(new Lexem() { Key = CONSTANT, Value = (byte)Lexems.Identifiers.IndexOf(lexemString) });
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

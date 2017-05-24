using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Program
    {
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
            lexems = lexemSplitter.RecognizeLexems(rawLexems);

            //3. Формат для вывода
            string outputString = $"Lexical analysis output:\t{string.Join("",lexems.Select(lexem => $"({lexem.Key},{(lexem.Key == LexemType.SEPARATOR ? ((char) lexem.Value).ToString() : lexem.Value.ToString())})"))}";
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

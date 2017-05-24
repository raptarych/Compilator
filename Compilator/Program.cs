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

            //1. Лексика

            //1.1 Разделение лексем
            var lexemBlock = new LexicalBlock();
            List<string> rawLexems = lexemBlock.Parse(inputString);
            Console.Write($"\nRaw lexems: {string.Join(",",rawLexems.Select(lexem => $"[{lexem}]"))}\n");

            //1.2 Разбор лексем
            lexems = lexemBlock.RecognizeLexems(rawLexems);

            //1.3 Формат для вывода
            string outputString = $"Lexical analysis output:\t{string.Join("",lexems.Select(lexem => $"({lexem.Key},{(lexem.Key == LexemType.SEPARATOR ? ((char) lexem.ValuePtr).ToString() : lexem.ValuePtr.ToString())})"))}";
            Console.WriteLine(outputString);

            //2. Синтаксический анализ

            var syntaticBlock = new SyntacticBlock();
            syntaticBlock.ProcessInput(lexems);
            

        }
        static void Main(string[] args)
        {
            ConWorker conWorker = new ConWorker();
            conWorker.AddHandler(LexicalAnalysis);
            conWorker.Start();
        }
    }
}

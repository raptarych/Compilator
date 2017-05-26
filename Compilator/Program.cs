using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Program
    {
        public static bool Debug;
        public static Dictionary<string, object> Variables = new Dictionary<string, object>();
        static void ParseInput(string inputString)
        {
            List<Lexem> lexems = new List<Lexem>();
            
            if (inputString == "debug")
            {
                Debug = !Debug;
                Console.WriteLine($"Debug mode is {(Debug ? "on" : "off")}");
                return;
            }

            //1. Лексика

            //1.1 Разделение лексем
            var lexemBlock = new LexicalBlock();
            List<string> rawLexems = lexemBlock.Parse(inputString);
            if (Debug) Console.Write($"\nRaw lexems: {string.Join(",",rawLexems.Select(lexem => $"[{lexem}]"))}\n");

            //1.2 Разбор лексем
            lexems = lexemBlock.RecognizeLexems(rawLexems);

            //1.3 Формат для вывода
            string outputString = $"Lexical analysis output:\t{string.Join("",lexems.Select(lexem => $"({lexem.Key},{(lexem.Key == LexemType.SEPARATOR ? ((char) lexem.ValuePtr).ToString() : lexem.ValuePtr.ToString())})"))}";
            if (Debug) Console.WriteLine(outputString);

            //2. Синтаксический анализ
            try
            {
                var syntaticBlock = new SyntacticBlock();
                syntaticBlock.ProcessInput(lexems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C# exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            
            

        }
        static void Main(string[] args)
        {
            ConWorker conWorker = new ConWorker();
            conWorker.AddHandler(ParseInput);
            conWorker.Start();
        }
    }
}

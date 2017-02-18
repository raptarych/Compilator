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
            /*
            Шпаргалка:
            1 - Идентификатор
            2 - Ключевое слово (for,if,while,int,string, …)
            3 - Операция
            4 - Разделитель
            5 - Константа
            */

            //Разделение лексем
            var lexems = new List<Lexem>();
            var rawLexems = inputString.Split(' ');

            
            //Разбор лексем
            foreach (var lexemString in rawLexems)
            {
                if (lexemString.Length == 0) continue;
                if (lexemString[0] >= '0' && lexemString[0] <= '9')
                {
                    lexems.Add(new Lexem() { Key = 5, Value = (int)lexemString[0]});
                    continue;
                }
                if (Lexems.Identifiers.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = 1, Value = (int)lexemString[0] });
                    continue;
                }
                if (Lexems.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = 2, Value = Lexems.Keywords.IndexOf(lexemString) });
                    continue;
                }
                if (Lexems.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = 3, Value = (int) lexemString[0] });
                    continue;
                }
                if (Lexems.Separators.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = 4, Value = (int) lexemString[0] });
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

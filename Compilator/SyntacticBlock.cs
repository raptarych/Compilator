using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class SyntacticBlock
    {
        public Stack<string> MainStack = new Stack<string>();
        public Stack<Lexem> NamePtrStack = new Stack<Lexem>();
        public Stack<Lexem> ValuePtrStack = new Stack<Lexem>();
        public Stack<Lexem> TypeStack = new Stack<Lexem>();

        /// <summary>
        /// Словарь(нетерминал, (входной символ, правило))
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> GrammarRules;
        public Dictionary<string, Dictionary<string, string>> GetGrammarRules()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            using (var file = File.OpenText("GrammarRules.dat"))
            {
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var nonTerminal = new string(line.TakeWhile(ch => ch != '–' && ch != '-').ToArray()).Trim();
                    var terminals = new string(line.Reverse().TakeWhile(ch => ch != '\t').Reverse().ToArray()).Trim().Split(' ').Select(ch => !string.IsNullOrEmpty(ch) ? ch[0] : Utils.Empty).ToList();
                    var rule = new string(line.SkipWhile(ch => ch != '>').Skip(1).TakeWhile(ch => ch != '\t').ToArray()).Trim();

                    if (!result.ContainsKey(nonTerminal)) result[nonTerminal] = new Dictionary<string, string>();
                    terminals.ForEach(terminal => result[nonTerminal].Add(terminal.ToString(), rule));
                }
            }
            return result;
        }

        public SyntacticBlock()
        {
            GrammarRules = GetGrammarRules();
        }

        public void ProcessInput(List<Lexem> lexems)
        {
            var queue = new Queue<Lexem>(lexems);
            var iterationsNum = 0;
            while (queue.Any())
            {
                if (!MainStack.Any()) MainStack.Push(GrammarRules.First().Key);
                var currentLexem = queue.Peek();
                string charLexemType;
                switch (currentLexem.Key)
                {
                    case LexemType.KEYWORD:
                        charLexemType = "k";
                        break;
                    case LexemType.CONSTANT:
                        charLexemType = "c";
                        break;
                    case LexemType.IDENTIFIER:
                        charLexemType = "v";
                        break;
                    case LexemType.OPERATION:
                        charLexemType = (string) currentLexem.GetValue();
                        break;
                    default:
                        charLexemType = ((char) currentLexem.ValuePtr).ToString();
                        break;
                }

                var currentStackItem = MainStack.Peek();
                var currentStackItemType = currentStackItem != "k" 
                    && currentStackItem != "v" 
                    && currentStackItem != "c" 
                    && currentStackItem != Utils.EmptyString
                    && !Lexems.Identifiers.Contains(currentStackItem) 
                    && !Lexems.Operations.Contains(currentStackItem)
                    && !Lexems.Separators.Contains(currentStackItem[0])
                    ? StackItemType.NonTerminal
                    : StackItemType.Terminal;

                if (Program.Debug)
                {
                    Console.WriteLine($"\nIteration {++iterationsNum}");
                    Console.WriteLine($"Input: {string.Join(",", queue.ToArray().Select(lex => $"({lex.Key}:{lex.GetValue()})"))}");
                    Console.WriteLine($"Stack: {string.Join(",", MainStack.ToArray())}");
                    Console.ReadKey();
                }
                
                switch (currentStackItemType)
                {
                    case StackItemType.NonTerminal:
                        if (!GrammarRules.ContainsKey(currentStackItem) ||
                            !GrammarRules[currentStackItem].ContainsKey(charLexemType))
                        {
                            Console.WriteLine($"ERROR {charLexemType}, stack top: {currentStackItem}");
                            return;
                        }
                        var rule = GrammarRules[currentStackItem][charLexemType];
                        if (currentStackItem == "OPER_TRIGGER")
                        {
                            var N2Lexem = ValuePtrStack.Pop();
                            var oper = ValuePtrStack.Pop();
                            var operatorType = (string) oper.GetValue();
                            var N1Lexem = ValuePtrStack.Pop();

                            switch (operatorType)
                            {
                                case "+":
                                    var add = (int)N1Lexem.GetValue() + (int)N2Lexem.GetValue();
                                    if (!Lexems.Constants.Contains(add)) Lexems.Constants.Add(add);
                                    ValuePtrStack.Push(new Lexem() {Key = LexemType.CONSTANT, ValuePtr = (byte) Lexems.Constants.IndexOf(add) });
                                    break;
                                case "-":
                                    var minus = (int)N1Lexem.GetValue() - (int)N2Lexem.GetValue();
                                    if (!Lexems.Constants.Contains(minus)) Lexems.Constants.Add(minus);
                                    ValuePtrStack.Push(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(minus) });
                                    break;
                                case "/":
                                    var divide = (int)N1Lexem.GetValue() / (int)N2Lexem.GetValue();
                                    if (!Lexems.Constants.Contains(divide)) Lexems.Constants.Add(divide);
                                    ValuePtrStack.Push(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(divide) });
                                    break;
                                case "*":
                                    var multiply = (int)N1Lexem.GetValue() * (int)N2Lexem.GetValue();
                                    if (!Lexems.Constants.Contains(multiply)) Lexems.Constants.Add(multiply);
                                    ValuePtrStack.Push(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(multiply) });
                                    break;
                            }
                        } else if (currentStackItem == "ENDDEFINE")
                        {
                            var N1Lexem = ValuePtrStack.Pop();
                            var oper = ValuePtrStack.Pop();
                            var operatorType = (string)oper.GetValue();
                            if (operatorType == "=")
                            {
                                var identifierName = (string)NamePtrStack.Pop().GetValue();
                                var value = N1Lexem.GetValue();
                                if (!Lexems.Identifiers.Contains(identifierName)) Lexems.Identifiers.Add(identifierName);
                                Console.WriteLine($"Defined {identifierName}: {value}");
                            }
                        }

                        MainStack.Pop();
                        if (rule != Utils.EmptyString) rule.Split(' ').Reverse().ToList().ForEach(ch => MainStack.Push(ch));
                        break;
                    case StackItemType.Terminal:
                        if (charLexemType == currentStackItem || currentStackItem == Utils.EmptyString && MainStack.Count == 1)
                        {
                            switch (currentLexem.Key)
                            {
                                case LexemType.CONSTANT:
                                case LexemType.OPERATION:
                                    ValuePtrStack.Push(currentLexem);
                                    if (Program.Debug) Console.WriteLine("Value or operator written");
                                    break;
                                case LexemType.IDENTIFIER:
                                    NamePtrStack.Push(currentLexem);
                                    if (Program.Debug) Console.WriteLine("Name written");
                                    break;
                                case LexemType.KEYWORD:
                                    TypeStack.Push(currentLexem);
                                    if (Program.Debug) Console.WriteLine("Type written");
                                    break;
                            }
                            MainStack.Pop();
                            queue.Dequeue();
                        }
                        else
                        {
                            Console.WriteLine($"ERROR: {charLexemType} {currentStackItem}");
                            return;
                        }
                        break;
                }
                if (!MainStack.Any())
                {
                    Console.WriteLine("Success!");
                }
            }

            if (MainStack.Any()) Console.WriteLine("Error during interpretation");
            
        }
    }
}

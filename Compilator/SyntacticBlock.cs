using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class SyntacticBlock
    {
        public Stack<string> MainStack = new Stack<string>();
        public Stack<string> NameStack = new Stack<string>();
        public Stack<object> ValueStack = new Stack<object>();
        public Stack<string> TypeStack = new Stack<string>();

        public string GrammarString = @"P -> k DEFINE		k
DEFINE -> v = EXEC		v
EXEC -> ( EXEC ) RIGHTPART		(
EXEC -> c RIGHTPART		c
RIGHTPART -> + RIGHTPART		+
RIGHTPART -> - RIGHTPART		-
RIGHTPART -> c RIGHTPART		c
RIGHTPART -> EXEC		(
RIGHTPART -> $		)
RIGHTPART -> ENDDEFINE		;
ENDDEFINE -> ;		;";

        /// <summary>
        /// Словарь(нетерминал, (входной символ, правило))
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> GrammarRules;
        public Dictionary<string, Dictionary<string, string>> GetGrammarRules()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            var lines = GrammarString.Split('\n');
            foreach (var line in lines)
            {
                var nonTerminal = new string(line.TakeWhile(ch => ch != '–' && ch != '-').ToArray()).Trim();
                var terminals = new string(line.Reverse().TakeWhile(ch => ch != '\t').Reverse().ToArray()).Trim().Split(' ').Select(ch => !string.IsNullOrEmpty(ch) ? ch[0] : Utils.Empty).ToList();
                var rule = new string(line.SkipWhile(ch => ch != '>').Skip(1).TakeWhile(ch => ch != '\t').ToArray()).Trim();

                if (!result.ContainsKey(nonTerminal)) result[nonTerminal] = new Dictionary<string, string>();
                terminals.ForEach(terminal => result[nonTerminal].Add(terminal.ToString(), rule));
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

                Console.WriteLine($"\nIteration {++iterationsNum}");
                Console.WriteLine($"Input: {string.Join(",",queue.ToArray().Select(lex => $"({lex.Key}:{lex.GetValue()})"))}");
                Console.WriteLine($"Stack: {string.Join(",", MainStack.ToArray())}");


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

                        MainStack.Pop();
                        if (rule != Utils.EmptyString) rule.Split(' ').Reverse().ToList().ForEach(ch => MainStack.Push(ch));
                        break;
                    case StackItemType.Terminal:
                        if (charLexemType == currentStackItem || currentStackItem == Utils.EmptyString && MainStack.Count == 1)
                        {
                            switch (currentLexem.Key)
                            {
                                case LexemType.CONSTANT:
                                    ValueStack.Push(currentLexem.GetValue());
                                    Console.WriteLine("Value written");
                                    break;
                                case LexemType.IDENTIFIER:
                                    NameStack.Push((string) currentLexem.GetValue());
                                    Console.WriteLine("Name written");
                                    break;
                                case LexemType.KEYWORD:
                                    TypeStack.Push((string) currentLexem.GetValue());
                                    Console.WriteLine("Type written");
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

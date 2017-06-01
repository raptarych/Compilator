using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compilator
{
    public class SyntacticBlock
    {
        private readonly Stack<string> MainStack = new Stack<string>();
        private readonly Stack<Lexem> NamePtrStack = new Stack<Lexem>();
        private readonly Stack<Lexem> ValuePtrStack = new Stack<Lexem>();
        private readonly Stack<Lexem> TypeStack = new Stack<Lexem>();

        private static List<string> AllTerminals = new List<string>();

        private static List<string> GetAllTerminals(List<NonTerminal> nonTerminals)
        {
            var result = new List<string>();
            result = nonTerminals
                .SelectMany(terminal => terminal.Rules)
                .Select(rule => rule.Value)
                .Select(ruleValue => new string(ruleValue.TakeWhile(ch => ch != ' ').ToArray()))
                .Where(Utils.IsTerminal).Distinct().ToList();
            return result;
        }

        private static void AddRulesRecursively(List<NonTerminal> nonTerminals, NonTerminal startTerminal,
            NonTerminal currenTerminal = null, Rule ruleOfFirst = null)
        {
            var startNonTerminalName = startTerminal.Name;
            if (currenTerminal == null) currenTerminal = startTerminal;
            foreach (var rule in currenTerminal.Rules)
            {
                if (currenTerminal.Name == startNonTerminalName) ruleOfFirst = rule;
                var ruleName = rule.Value;
                var firstSym = new string(ruleName.TakeWhile(ch => ch != ' ').ToArray());
                while ((firstSym == Utils.EmptyString || firstSym.EndsWith("_TRIGGER")) && !string.IsNullOrEmpty(ruleName))
                {
                    ruleName = new string(ruleName.SkipWhile(ch => ch != ' ').Skip(1).ToArray());
                    firstSym = !string.IsNullOrEmpty(ruleName) ? new string(ruleName.TakeWhile(ch => ch != ' ').ToArray()) : firstSym;
                }
                    
                if (Utils.IsTerminal(firstSym)) ruleOfFirst?.TerminalsSet.Add(firstSym);
                else
                {
                    if (firstSym == startNonTerminalName) continue;
                    var nextNonTerminal = nonTerminals.FirstOrDefault(nt => nt.Name == firstSym);
                    if (nextNonTerminal == null)
                    {
                        if (firstSym.EndsWith("_TRIGGER")) continue;
                        if (firstSym == Utils.EmptyString)
                        {
                            var notExistingTerminals = new HashSet<string>();
                            notExistingTerminals.UnionWith(AllTerminals);
                            var existingTerminals = new HashSet<string>(currenTerminal.Rules.SelectMany(r => r.TerminalsSet).Distinct());
                            existingTerminals.UnionWith(startTerminal.Rules.SelectMany(r => r.TerminalsSet).Distinct());
                            notExistingTerminals.ExceptWith(existingTerminals);
                            foreach (var terminal in notExistingTerminals)
                                rule.TerminalsSet.Add(terminal);
                            
                            continue;
                        }
                        throw new CompilatorException($"Non terminal {firstSym} doesn't exist");
                    }
                    AddRulesRecursively(nonTerminals, startTerminal, nextNonTerminal, ruleOfFirst);
                }
            }
        }

        /// <summary>
        /// Словарь(нетерминал, (входной символ, правило))
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> GrammarRules;
        public static void GetGrammarRules()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            using (var file = File.OpenText("GrammarRules.dat"))
            {
                //Реализовать алгоритм вычисления множества выбора - реализовал написанием вспомогательной сущности нетерминала; алгоритм получился двухпроходной
                var nonTerminals = new List<NonTerminal>();

                //Проход 1 - читаем файл, создаём список всех нетерминалов с правилами
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (Program.Debug) Console.WriteLine(line);

                    var nonTerminalName = new string(line.TakeWhile(ch => ch != '–' && ch != '-').ToArray()).Trim();
                    var nonTerminal = nonTerminals.FirstOrDefault(nt => nt.Name == nonTerminalName) ?? new NonTerminal {Name = nonTerminalName};
                    var rule = new string(line.SkipWhile(ch => ch != '>').Skip(1).ToArray()).Trim();
                    nonTerminal.Rules.Add(new Rule() {Value = rule});
                    if (!nonTerminals.Contains(nonTerminal)) nonTerminals.Add(nonTerminal);
                }
                AllTerminals = GetAllTerminals(nonTerminals);
                //Проход 2 - теперь, зная все правила в одной связке, можно и сгенерировать им множество выбора; запарился и сделал рекурсивный метод
                foreach (var nonTerminal in nonTerminals)
                {
                    AddRulesRecursively(nonTerminals, nonTerminal);

                    //ну и тут же сконвертируем правила нетерминала в упрощенный формат, с которым и будет работать парсер
                    result[nonTerminal.Name] = new Dictionary<string, string>();
                    foreach (var rule in nonTerminal.Rules)
                        rule.TerminalsSet.ToList().ForEach(terminal => result[nonTerminal.Name].Add(terminal, rule.Value));
                }


            }
            GrammarRules = result;
        }

        /// <summary>
        /// Обработка арифметических операций от управляющих символов
        /// </summary>
        private void Arithmetic(object N1obj, object N2obj, string type)
        {
            object addNew = null;
            if (N1obj is int && N2obj is int)
            {
                var N1 = (int)N1obj;
                var N2 = (int)N2obj;
                switch (type)
                {
                    case "ADD":
                        addNew = N1 + N2;
                        break;
                    case "SUB":
                        addNew = N1 - N2;
                        break;
                    case "DIV":
                        if (N2 == 0) throw new CompilatorException("dividing by zero");
                        addNew = N1 / N2;
                        break;
                    case "MULT":
                        addNew = N1 * N2;
                        break;
                    case "POW":
                        addNew = (int) Math.Pow(N1, N2);
                        break;
                }
            } else if (N1obj is float || N2obj is float)
            {
                var N1 = N1obj as float? ?? (int) N1obj;
                var N2 = N2obj as float? ?? (int) N2obj;
                switch (type)
                {
                    case "ADD":
                        addNew = N1 + N2;
                        break;
                    case "SUB":
                        addNew = N1 - N2;
                        break;
                    case "DIV":
                        if (Math.Abs(N2) < 0.0001) throw new CompilatorException("dividing by zero");
                        addNew = N1 / N2;
                        break;
                    case "MULT":
                        addNew = N1 * N2;
                        break;
                    case "POW":
                        addNew = (float) Math.Pow(N1, N2);
                        break;
                }
            }
            else if (N1obj is string || N2obj is string)
            {
                var N1 = N1obj.ToString();
                var N2 = N2obj.ToString();
                if (type == "ADD")
                    addNew = N1 + N2;
                else throw new CompilatorException($"Operation {type} for string types is not currently available, please do not shot at your leg");
            }
            ValuePtrStack.Push(Lexems.SaveConstant(addNew));
        }

        /// <summary>
        /// Обработка управляющих символов
        /// </summary>
        private void HandleControlChar(string controlChar)
        {
            var trigger = controlChar.Substring(0, controlChar.Length - 8);
            switch (trigger)
            {
                case "DEFINE":
                    var defineVarType = (string) TypeStack.Peek().GetValue();
                    var defineVarName = (string) NamePtrStack.Peek().GetValue();

                    switch (defineVarType)
                    {
                        case "int":
                            Program.Variables[defineVarName] = 0;
                            break;
                        case "float":
                            Program.Variables[defineVarName] = 0f;
                            break;
                        case "string":
                            Program.Variables[defineVarName] = "";
                            break;
                    }
                    break;
                case "ADD":
                case "SUB":
                case "MULT":
                case "DIV":
                case "POW":
                    var addN2 = ValuePtrStack.Pop().GetValue();
                    var addN1 = ValuePtrStack.Pop().GetValue();
                    Arithmetic(addN1, addN2, trigger);
                    break;
                case "EQUATION":
                    var equationLeft = (string) NamePtrStack.Peek().GetValue();
                    var equationRight = ValuePtrStack.Pop().GetValue();
                    if (!Program.Variables.ContainsKey(equationLeft)) throw new CompilatorException($"Variable wasn't initialized: {equationLeft}");
                    var identificatorType = Program.Variables[equationLeft].GetType();
                    var valueType = equationRight.GetType();
                    if (valueType != identificatorType)
                    {
                        if (identificatorType == typeof(int) && valueType == typeof(float))
                        {
                            throw new CompilatorException("you are not allowed to appropriate float as int");
                        }
                        if (identificatorType == typeof(float) && valueType == typeof(int))
                            equationRight = equationRight as float? ?? (int) equationRight;
                        else if (identificatorType == typeof(string) && (valueType == typeof(float) || valueType == typeof(int)))
                            equationRight = equationRight.ToString();
                        else throw new CompilatorException($"Trying to approptiate '{valueType}' to '{identificatorType}'");
                    }
                    Program.Variables[equationLeft] = equationRight;
                    Console.WriteLine($"Eq: {equationLeft} = {equationRight}");
                    break;
                case "GET":
                    var getName = (string) NamePtrStack.Pop().GetValue();
                    if (!Program.Variables.ContainsKey(getName)) throw new CompilatorException($"Variable wasn't initialized: {getName}");
                    var getValue = Program.Variables[getName];
                    ValuePtrStack.Push(Lexems.SaveConstant(getValue));
                    break;
                case "UNARYPP":
                    var unaryPpIdentificator = (string)NamePtrStack.Peek().GetValue();
                    if (!Program.Variables.ContainsKey(unaryPpIdentificator)) throw new CompilatorException($"Variable wasn't initialized: {unaryPpIdentificator}");
                    var unaryPpValue = Program.Variables[unaryPpIdentificator];
                    if (unaryPpValue is int)
                        Program.Variables[unaryPpIdentificator] = (int) Program.Variables[unaryPpIdentificator] + 1;
                    else if (unaryPpValue is float)
                        Program.Variables[unaryPpIdentificator] = (float)Program.Variables[unaryPpIdentificator] + 1;
                    else throw new CompilatorException("Trying to increase something wrong...");
                    break;
                case "UNARYMM":
                    var unaryMmIdentificator = (string)NamePtrStack.Peek().GetValue();
                    if (!Program.Variables.ContainsKey(unaryMmIdentificator)) throw new CompilatorException($"Variable wasn't initialized: {unaryMmIdentificator}");
                    var unaryMmValue = Program.Variables[unaryMmIdentificator];
                    if (unaryMmValue is int)
                        Program.Variables[unaryMmIdentificator] = (int)Program.Variables[unaryMmIdentificator] - 1;
                    else if (unaryMmValue is float)
                        Program.Variables[unaryMmIdentificator] = (float)Program.Variables[unaryMmIdentificator] - 1;
                    else throw new CompilatorException("Trying to increase something wrong...");
                    break;
                case "REMOVE_IDENT":
                    NamePtrStack.Pop();
                    break;
            } 
        }

        /// <summary>
        /// Ввод в МП-автомат цепочки лексем
        /// </summary>
        public void ProcessInput(IEnumerable<Lexem> lexems)
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
                var currentStackItemType = Utils.IsTerminal(currentStackItem) ? StackItemType.Terminal : StackItemType.NonTerminal;

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
                        if (currentStackItem.EndsWith("_TRIGGER"))
                        {
                            HandleControlChar(currentStackItem);
                            MainStack.Pop();
                            continue;
                        }
                        if (!GrammarRules.ContainsKey(currentStackItem) ||
                            !GrammarRules[currentStackItem].ContainsKey(charLexemType))
                        {
                            if (!GrammarRules.ContainsKey(currentStackItem))
                                Console.WriteLine($"ERROR: unknown grammar {currentStackItem}");
                            else
                            Console.WriteLine($"ERROR: unexpected character {charLexemType}, expected symbols: {string.Join(" or ",GrammarRules[currentStackItem].Keys)}");
                            return;
                        }
                        var rule = GrammarRules[currentStackItem][charLexemType];

                        MainStack.Pop();
                        if (rule != Utils.EmptyString) rule.Split(' ').Reverse().ToList().ForEach(ch => MainStack.Push(ch));
                        break;
                    case StackItemType.Terminal:
                        if (charLexemType == currentStackItem || currentStackItem == Utils.EmptyString && MainStack.Count == 1)
                        {
                            MainStack.Pop();
                            queue.Dequeue();
                            switch (currentLexem.Key)
                            {
                                case LexemType.CONSTANT:
                                    ValuePtrStack.Push(currentLexem);
                                    if (Program.Debug) Console.WriteLine("Value written");
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
                        }
                        else
                        {
                            Console.WriteLine($"Unexpected symbol {charLexemType}, '{currentStackItem}' expected");
                            return;
                        }
                        break;
                }
            }
            if (MainStack.Any()) Console.WriteLine("Unexpected end");
        }
    }
}

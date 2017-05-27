﻿using System;
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
                    if (Program.Debug) Console.WriteLine(line);
                    
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

        public void Arithmetic(object val)
        {
            if (!Lexems.Constants.Contains(val)) Lexems.Constants.Add(val);
            ValuePtrStack.Push(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(val) });
        }

        public void HandleControlChar(string controlChar)
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
                    var addN2 = ValuePtrStack.Pop().GetValue();
                    var addN1 = ValuePtrStack.Pop().GetValue();
                    if (addN1 is int && addN2 is int)
                    {
                        var addNew = (int) addN2 + (int) addN1;
                        ValuePtrStack.Push(Lexems.SaveConstant(addNew));
                    }
                    break;
                case "MULT":
                    var multN2 = ValuePtrStack.Pop().GetValue();
                    var multN1 = ValuePtrStack.Pop().GetValue();
                    if (multN1 is int && multN2 is int)
                    {
                        var addNew = (int)multN2 * (int)multN1;
                        ValuePtrStack.Push(Lexems.SaveConstant(addNew));
                    }
                    break;
                case "EQUATION":
                    var equationLeft = (string) NamePtrStack.Pop().GetValue();
                    var equationRight = ValuePtrStack.Pop().GetValue();
                    if (!Program.Variables.ContainsKey(equationLeft)) throw new Exception($"Variable wasn't initialized: {equationLeft}");
                    Program.Variables[equationLeft] = equationRight;
                    Console.WriteLine($"Eq: {equationLeft} = {equationRight}");
                    break;
                case "GET":
                    var getName = (string) NamePtrStack.Pop().GetValue();
                    if (!Program.Variables.ContainsKey(getName)) throw new Exception($"Variable wasn't initialized: {getName}");
                    var getValue = Program.Variables[getName];
                    ValuePtrStack.Push(Lexems.SaveConstant(getValue));
                    break;
            } 
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
                            if (!GrammarRules.ContainsKey(currentStackItem))
                                Console.WriteLine($"ERROR: unknown grammar {currentStackItem}");
                            else
                            Console.WriteLine($"ERROR: unexpected character {charLexemType}, expected symbols: {string.Join(",",GrammarRules[currentStackItem].Keys)}");
                            return;
                        }
                        var rule = GrammarRules[currentStackItem][charLexemType];
                        
                        if (currentStackItem.EndsWith("_TRIGGER")) HandleControlChar(currentStackItem);

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
            else Console.WriteLine("Success!");

        }
    }
}

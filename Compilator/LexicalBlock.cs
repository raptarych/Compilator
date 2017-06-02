using System;
using System.Collections.Generic;
using System.Linq;
namespace Compilator
{
    class LexicalBlock
    {
        private Queue<char> Input { get; set; } = new Queue<char>();
        private char GetCurrentChar() => Input.Dequeue();
        private LexemCharType CurrentCharType { get; set; }
        private char CurrentChar { get; set; }
        private string CurrentLexem { get; set; } = "";
        private List<string> RawLexems { get; set; } = new List<string>();
        

        private LexemCharType GetType(char ch)
        {
            if (CommonTables.Separators.Any(oper => oper == ch)) return LexemCharType.Separators;
            if (CommonTables.Operations.Any(oper => oper.StartsWith(ch.ToString()))) return LexemCharType.Operations;
            return LexemCharType.Text;
        }

        public List<string> Parse(string input)
        {
            input.ToList().ForEach(ch => Input.Enqueue(ch));
            while (Input.Any())
            {
                CurrentChar = GetCurrentChar();
                if (CurrentChar == ' ')
                {
                    CloseLexem();
                    continue;
                }

                if (CurrentChar == '"')
                {
                    CloseLexem();
                    CurrentLexem += CurrentChar;
                    do
                    {
                        CurrentChar = GetCurrentChar();
                        CurrentLexem += CurrentChar;
                    } while (CurrentChar != '"');
                    CloseLexem();
                    continue;
                }

                var charType = GetType(CurrentChar);
                if (CurrentChar == '-' && Input.Any() && Input.Peek() >= '0' && Input.Peek() <= '9' && (RawLexems.Last()[0] == '=' || RawLexems.Last()[0] == '(' || GetType(RawLexems.Last()[0]) == LexemCharType.Operations))
                {
                    if (CurrentLexem.Length > 0) CloseLexem();
                    CurrentCharType = LexemCharType.Text;
                    CurrentLexem += CurrentChar;
                    continue;
                }
                if (CurrentLexem.Length == 0)
                {
                    CurrentCharType = charType;
                } else if (charType != CurrentCharType || charType == LexemCharType.Separators)
                {
                    CloseLexem();
                    CurrentCharType = charType;
                }
                CurrentLexem += CurrentChar;
                if (charType != LexemCharType.Text && 
                    CommonTables.Separators.All(l => l != CurrentLexem[0]) && 
                    !CommonTables.Operations.Any(l => l.StartsWith(CurrentLexem)))
                {
                    var lastSym = CurrentLexem.Last();
                    CurrentLexem = CurrentLexem.Substring(0, CurrentLexem.Length - 1);
                    CloseLexem();
                    CurrentLexem += lastSym;
                }
            }
            CloseLexem();
            return RawLexems;
        }

        public List<Lexem> RecognizeLexems(List<string> rawLexems)
        {
            List<Lexem> lexems = new List<Lexem>();
            foreach (var lexemString in rawLexems)
            {
                if (lexemString.Length == 0) continue;
                char firstChar = lexemString[0];
                if (firstChar == '"')
                {
                    //Константа текстовая
                    var lexem = lexemString;
                    if (lexemString.Contains("\"")) lexem = lexemString.Substring(1, lexemString.Length - 2);
                    if (!CommonTables.Constants.Contains(lexem))
                    {
                        CommonTables.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)CommonTables.Constants.IndexOf(lexem) });
                    continue;
                }
                if (firstChar >= '0' && firstChar <= '9' || firstChar == '-' && lexemString.Length > 1 && lexemString[1] != '-')
                {
                    //Константа числовая
                    object lexem;
                    int lexemInt;
                    float lexemFloat;
                    if (int.TryParse(lexemString, out lexemInt)) lexem = lexemInt;
                    else if (float.TryParse(lexemString.Replace(".", ","), out lexemFloat)) lexem = lexemFloat;
                    else
                    {
                        Console.WriteLine($"Invalid identifier: {lexemString}");
                        return new List<Lexem>();
                    }

                    if (!CommonTables.Constants.Contains(lexem))
                    {
                        CommonTables.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)CommonTables.Constants.IndexOf(lexem) });
                    continue;
                }
                //Разделитель
                if (lexemString.Length == 1 && CommonTables.Separators.Contains(lexemString[0]))
                {
                    lexems.Add(new Lexem() { Key = LexemType.SEPARATOR, ValuePtr = (byte)lexemString[0] });
                    continue;
                }
                //Ключевое слово
                if (CommonTables.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = LexemType.KEYWORD, ValuePtr = (byte)CommonTables.Keywords.IndexOf(lexemString) });
                    continue;
                }
                //Операция
                if (CommonTables.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = LexemType.OPERATION, ValuePtr = (byte)CommonTables.Operations.IndexOf(lexemString) });
                    continue;
                }
                //Идентификатор (но это не точно)
                if (!CommonTables.Identifiers.Contains(lexemString))
                {
                    CommonTables.Identifiers.Add(lexemString);
                }
                lexems.Add(new Lexem() { Key = LexemType.IDENTIFIER, ValuePtr = (byte)CommonTables.Identifiers.IndexOf(lexemString) });
            }
            return lexems;
        }
        private void CloseLexem()
        {
            if (CurrentLexem.Length > 0)
            {
                RawLexems.Add(CurrentLexem);
                CurrentLexem = "";
            }
        }
    }
}

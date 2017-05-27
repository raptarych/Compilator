using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (Lexems.Separators.Any(oper => oper == ch)) return LexemCharType.Separators;
            if (Lexems.Operations.Any(oper => oper.StartsWith(ch.ToString()))) return LexemCharType.Operations;
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

                if (CurrentLexem.Length == 0 && CurrentChar == '-' && Input.Peek() >= '0' && Input.Peek() <= '9')
                {
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
                    Lexems.Separators.All(l => l != CurrentLexem[0]) && 
                    !Lexems.Operations.Any(l => l.StartsWith(CurrentLexem)))
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
                    if (lexemString.IndexOf("\"") > 0) lexem = lexemString.Substring(1, lexemString.Length - 2);
                    if (!Lexems.Constants.Contains(lexem))
                    {
                        Lexems.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(lexem) });
                    continue;
                }
                if (firstChar >= '0' && firstChar <= '9' || firstChar == '-')
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

                    if (!Lexems.Constants.Contains(lexem))
                    {
                        Lexems.Constants.Add(lexem);
                    }
                    lexems.Add(new Lexem() { Key = LexemType.CONSTANT, ValuePtr = (byte)Lexems.Constants.IndexOf(lexem) });
                    continue;
                }
                //Разделитель
                if (lexemString.Length == 1 && Lexems.Separators.Contains(lexemString[0]))
                {
                    lexems.Add(new Lexem() { Key = LexemType.SEPARATOR, ValuePtr = (byte)lexemString[0] });
                    continue;
                }
                //Ключевое слово
                if (Lexems.Keywords.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = LexemType.KEYWORD, ValuePtr = (byte)Lexems.Keywords.IndexOf(lexemString) });
                    continue;
                }
                //Операция
                if (Lexems.Operations.Contains(lexemString))
                {
                    lexems.Add(new Lexem() { Key = LexemType.OPERATION, ValuePtr = (byte)Lexems.Operations.IndexOf(lexemString) });
                    continue;
                }
                //Идентификатор (но это не точно)
                if (!Lexems.Identifiers.Contains(lexemString))
                {
                    Lexems.Identifiers.Add(lexemString);
                }
                lexems.Add(new Lexem() { Key = LexemType.IDENTIFIER, ValuePtr = (byte)Lexems.Identifiers.IndexOf(lexemString) });
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class LexicalSplitter
    {
        private Queue<char> Input { get; set; } = new Queue<char>();
        private char GetCurrentChar() => Input.Dequeue();
        private LexemCharType CurrentCharType { get; set; }
        private char CurrentChar { get; set; }
        private string CurrentLexem { get; set; } = "";
        private List<string> RawLexems { get; set; } = new List<string>();
        enum LexemCharType
        {
            Text = 1,
            Separators = 2,
            Operations = 3
        }

        private LexemCharType GetType(char ch)
        {
            if (Lexems.Separators.Contains(ch)) return LexemCharType.Separators;
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

                if (CurrentLexem.Length == 0)
                {
                    CurrentCharType = charType;
                    CurrentLexem += CurrentChar;
                } else if (charType == CurrentCharType)
                {
                    CurrentLexem += CurrentChar;
                } else if (charType != CurrentCharType)
                {
                    CloseLexem();
                    CurrentCharType = charType;
                    CurrentLexem += CurrentChar;
                }
            }
            CloseLexem();
            return RawLexems;
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

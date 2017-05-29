namespace Compilator
{
    public class Utils
    {
        public const string EmptyString = "$";

        public static bool IsTerminal(string sym)
        {
            return sym == "c" ||
                   sym == "v" ||
                   sym == "k" ||
                   Lexems.Operations.Contains(sym) ||
                   Lexems.Keywords.Contains(sym) ||
                   sym.Length > 0 && Lexems.Separators.Contains(sym[0]);
        }
    }

    public enum LexemCharType
    {
        Text = 1,
        Separators = 2,
        Operations = 3
    }

    public enum LexemType
    {
        IDENTIFIER = 1,
        KEYWORD = 2,
        OPERATION = 3,
        SEPARATOR = 4,
        CONSTANT = 5
    }

    public enum StackItemType
    {
        Terminal = 1,
        NonTerminal = 2
    }
}

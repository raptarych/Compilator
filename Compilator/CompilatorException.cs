using System;

namespace Compilator
{
    class CompilatorException : Exception
    {
        public CompilatorException(string message) : base(message)
        {
        }
    }
}

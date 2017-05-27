using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class CompilatorException : Exception
    {
        public CompilatorException(string message) : base(message)
        {
        }
    }
}

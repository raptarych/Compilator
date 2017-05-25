using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class ConWorker
    {
        private string InputString { get; set; }
        public delegate void ProcessInput(string input);
        private ProcessInput InputFunc;
        public void Start()
        {
            Console.WriteLine("Enter command:");
            while (true)
            {
                Console.Write(">");
                InputString = Console.ReadLine();
                if (InputString == "exit") break;
                InputFunc(InputString);
            }
        }

        public void AddHandler(ProcessInput inputFunc)
        {
            InputFunc += inputFunc;
        }
    }
}

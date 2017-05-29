using System;

namespace FiniteStateMachine
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

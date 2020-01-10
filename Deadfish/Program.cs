using System;

namespace Deadfish
{
    class Program
    {
        private static string Program0 = "iissso"; // should output 0
        private static string Program288 = "diissisdo"; // should output 288
        private static string Program0_2 = "iissisdddddddddddddddddddddddddddddddddo"; // should output 0

        static void Main(string[] args)
        {
            DeadfishInterpreter interpreter = new DeadfishInterpreter(Console.Write);
            interpreter.Parse(Program0_2);
            interpreter.Execute();
        }
    }
}

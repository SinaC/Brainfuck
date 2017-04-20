using System;

namespace Malbolge
{
    //https://en.wikipedia.org/wiki/Malbolge
    //http://web.archive.org/web/20000815230017/http:/www.mines.edu/students/b/bolmstea/malbolge/
    //http://www.lscheffer.com/malbolge_interp.html
    //https://bitbucket.org/msagi/malbolge-interpreter
    class Program
    {
        static void Main(string[] args)
        {
            string helloWorld = "(=<`#9]~6ZY32Vx/4Rs+0No-&Jk)\"Fh}|Bcy?`=*z]Kw%oG4UUS0/@-ejc(:'8dc";

            Interpreter interpreter = new Interpreter(InputFunc, OutputAction);
            interpreter.Parse(helloWorld);
            interpreter.Execute();
        }

        private static void OutputAction(char c)
        {
            Console.Write(c);
        }

        private static char InputFunc()
        {
            return Console.ReadKey().KeyChar;
        }
    }
}

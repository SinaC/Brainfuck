using System;
using System.IO;

namespace Malbolge
{
    //https://en.wikipedia.org/wiki/Malbolge
    //http://web.archive.org/web/20000815230017/http:/www.mines.edu/students/b/bolmstea/malbolge/
    //http://www.lscheffer.com/malbolge_interp.html
    //https://bitbucket.org/msagi/malbolge-interpreter
    //https://www.matthias-ernst.eu/malbolgeassembler.html
    //https://esolangs.org/wiki/Malbolge_programming
    class Program
    {
        static void Main(string[] args)
        {
            string prg = "(=<`#9]~6ZY32Vx/4Rs+0No-&Jk)\"Fh}|Bcy?`=*z]Kw%oG4UUS0/@-ejc(:'8dc"; // hello world
            //string cat = "(=BA#9\"=<;:3y7x54-21q/p-,+*)\"!h%B0/.~P<<:(8&66#\"!~}|{zyxwvugJ%";
            //string prg = File.ReadAllText(@"..\..\quine.txt").Replace(Environment.NewLine, "");
            //string prg = File.ReadAllText(@"..\..\99bottles.txt").Replace(Environment.NewLine, "");

            Interpreter interpreter = new Interpreter(InputFunc, OutputAction);
            //interpreter.Parse(helloWorld);
            interpreter.Parse(prg);
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

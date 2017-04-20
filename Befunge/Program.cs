using System;

namespace Befunge
{
    class Program
    {
        static void Main(string[] args)
        {
            string helloWorld =
                ">              v\n" +
                "v  ,,,,,\"Hello\"<\n" +
                ">48*,          v\n" +
                "v,,,,,,\"World!\"<\n" +
                ">25*,@";

            string helloWorld2 =
                ">25*\"!dlrow ,olleH\":v\n" +
                "                 v:,_@\n" +
                "                 >  ^\n";

            string helloWorld3 = "64+\"!dlroW ,olleH\">:#,_@";

            string factorial =
                "&>:1-:v v *_$.@\n" +
                " ^    _$>\\:^\n";

            string eratosthenesSieve =
"2>:3g\" \"-!v\\  g30          <\n" +
" |!`\"O\":+1_:.:03p>03g+:\"O\"`|\n" +
" @               ^  p3\\\" \":<\n" +
"2 234567890123456789012345678901234567890123456789012345678901234567890123456789\n";

            string quine = "01->1# +# :# 0# g# ,# :# 5# 8# *# 4# +# -# _@";


            Interpreter interpreter = new Interpreter(InputFunc, OutputAction);
            interpreter.Parse(quine);
            interpreter.Execute();
        }

        private static void OutputAction(string c)
        {
            Console.Write(c);
        }

        private static string InputFunc()
        {
            return Console.ReadLine();
        }
    }
}

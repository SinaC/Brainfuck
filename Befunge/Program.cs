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

            string maze =
                "45*28*10p00p020p030p006p0>20g30g00g*+::\"P\"%\\\"P\"/6+gv>$\\1v@v1::\\+g02+*g00+g03-\\<\n" +
                "0_ 1!%4+1\\-\\0!::\\-\\2%2:p<pv0<< v0p+6/\"P\"\\%\"P\":\\+4%4<^<v-<$>+2%\\1-*20g+\\1+4%::v^\n" +
                "#| +2%\\1-*30g+\\1\\40g1-:v0+v2?1#<v>+:00g%!55+*>:#0>#,_^>:!|>\\#%\"P\"v#:*+*g00g0<>1\n" +
                "02!:++`\\0\\`-1g01:\\+`\\< !46v3<^$$<^1,g2+1%2/2,g1+1<v%g00:\\<*g01,<>:30p\\:20p:v^3g\n" +
                "0#$g#<1#<-#<`#<\\#<0#<^#_^/>#1+#4<>\"P\"%\\\"P\"/6+g:2%^!>,1-:#v_$55+^|$$ \"JH\" $$>#<0\n" +
                "::\"P\"%\\\"P\"/6+g40p\\40g+\\:#^\"P\"%#\\<^ ::$_,#!0#:<*\"|\"<^,\" _\"<:g000 <> /6+g4/2%+#^_\n";

            string fibonacci
                = "00:.1:.>:\"@\"8**++\\1+:67+`#@_v\n" +
                  "       ^ .:\\/*8\"@\"\\%*8\"@\":\\ <";

            string languageName3d =
                "0\" &7&%h&'&%| &7&%7%&%&'&%&'&%&7&%\"v\n" +
                "v\"'%$%'%$%3$%$%7% 0%&7&%&7&(%$%'%$\"<\n" +
                ">\"%$%7%$%&%$%&'&%7%$%7%$%, '&+(%$%\"v\n" +
                "v\"+&'&%+('%$%$%'%$%$%$%$%$%$%$%'%$\"<\n" +
                ">\"(%$%$%'%$%$%( %$+(%&%$+(%&%$+(%&\"v\n" +
                "v\"(; $%$%(+$%&%(+$%$%'%$%+&%$%$%$%\"<\n" +
                "? \";(;(+(+$%+(%&(;(3%$%&$ 7`+( \":v >\n" +
                "^v!:-1<\\,:g7+*63%4 \\/_#4:_v#:-*84_$@\n" +
                "$_\\:,\\^                   >55+,$:^:$\n";

            string myPower2 =
                "1>:.v\n" +
                " ^+:<";

            string myFactorial = // iterate from input number to 1 and push these values on stack then multiply stack values until 0 is pop (empty stack), then display computed value
                "&>:1-:v v *<\n" +
                " ^    _$>\\:|\n" +
                "           >$.@";

            // TODO
            // 1/ write values from 2 to 79 on last row starting at column 2
            // 2/ store 2 (index of first non-empty value) on last row column 0
            // 3/ store 2 (vlaue of first non-empty value) on last row column 1
            // 4/ remove from last row multiple of stored value at last,1
            // 5/ search next non-empty value in last row starting at (last,0)+1
            // 6/ if not found, stop
            // 7/ else, store index and value on last row column 0 and 1; and loop from 4
            string myEratosthenesSieve =
                "";

            Interpreter interpreter = new Interpreter(InputFunc, OutputAction);
            interpreter.Parse(maze);
            interpreter.Execute();
        }

        private static void OutputAction(string c)
        {
            Console.Write(c);
        }

        private static string InputFunc()
        {
            Console.Write("Input:");
            return Console.ReadLine();
        }
    }
}

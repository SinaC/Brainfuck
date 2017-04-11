using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brainfuck.Instructions;

namespace Brainfuck
{
    //https://en.wikipedia.org/wiki/Brainfuck
    class Program
    {
        private static string ProgramDummy =
            "++>+++++[<+>-]>[-]"; // Cell 0 = 2+5

        static string Program2Plus5 =
            "++                       Cell c0 = 2" +
            "> +++++                  Cell c1 = 5" +

            "[                        Start your loops with your cell pointer on the loop counter(c1 in our case)" +
            "< +                      Add 1 to c0" +
            "> -                      Subtract 1 from c1" +
            "]                        End your loops with the cell pointer on the loop counter" +

            "At this point our program has added 5 to 2 leaving 7 in c0 and 0 in c1" +
            "BUT we cannot output this value to the terminal since it's not ASCII encoded!" +

            "To display the ASCII character '7' we must add 48 to the value 7!" +
            "48 = 6 * 8 so let's use another loop to help us!" +

            "++++++++                 c1 = 8 and this will be our loop counter again" +
            "[" +
            "< ++++++                 Add 6 to c0" +
            "> -                      Subtract 1 from c1" +
            "]" +
            "< .                      Print out c0 which has the value 55 which translates to '7'!";

        private static string ProgramHelloWorld =
            "++++++++                 Set Cell #0 to 8" +
            "[" +

            "> ++++                   Add 4 to Cell #1; this will always set Cell #1 to 4" +
            "[                        as the cell will be cleared by the loop" +
            "> ++                     Add 2 to Cell #2" +
            "> +++                    Add 3 to Cell #3" +
            "> +++                    Add 3 to Cell #4" +
            "> +                      Add 1 to Cell #5" +
            "<<<< -                   Decrement the loop counter in Cell #1" +
            "]                        Loop till Cell #1 is zero; number of iterations is 4" +
            "> +                      Add 1 to Cell #2" +
            "> +                      Add 1 to Cell #3" +
            "> -                      Subtract 1 from Cell #4" +
            ">> +                     Add 1 to Cell #6" +
            "[<]                      Move back to the first zero cell you find; this will" +
            "                         be Cell #1 which was cleared by the previous loop" +
            "    <-                   Decrement the loop Counter in Cell #0" +
            "]                        Loop till Cell #0 is zero; number of iterations is 8" +

            "The result of this is:" +
            "Cell No :   0   1   2   3   4   5   6" +
            "Contents:   0   0  72 104  88  32   8" +
            "Pointer :   ^" +

            ">>.                      Cell #2 has value 72 which is 'H'" +
            ">---.                    Subtract 3 from Cell #3 to get 101 which is 'e'" +
            "+++++++..+++.            Likewise for 'llo' from Cell #3" +
            ">>.                      Cell #5 is 32 for the space" +
            "<-.                      Subtract 1 from Cell #4 for 87 to give a 'W'" +
            "<.                       Cell #3 was set to 'o' from the end of 'Hello'" +
            "+++.------.--------.     Cell #3 for 'rl' and 'd'" +
            ">>+.                     Add 1 to Cell #5 gives us an exclamation point" +
            ">++.                     And finally a newline from Cell #6";

        private static string ProgramTestJumpForward =
            "+>[>+[>+[>+[>+[>+>+>+>+]>]>]>+>]>+]<++"; // cell 0 should contain 3

        //http://www.hevanet.com/cristofd/brainfuck/
        private static string ProgramSerpinski =
            "++++++++[> +> ++++<< -] > ++>> +<[-[>> +<< -] +>>] > +[" +
            "    -<<<[" +
            "        ->[+[-] +> ++>>> -<<] <[<] >> ++++++[<< +++++>> -] +<< ++.[-] << " +
            "    ] >.> +[>>] > +" +
            "]";

        private static string ProgramFactorial =
            "> ++++++++++>>> +> +[>>> +[-[<<<<<[+<<<<<] >>[[-] >[<< +> +> -] <[> +< -] <[> +< -[> +< -[>" +
            "+< -[> +< -[> +< -[> +< -[> +< -[> +< -[> +< -[>[-] >>>> +> +<<<<<< -[> +< -]]]]]]]]]]] >[< +> -" +
            "] +>>>>>] <<<<<[<<<<<] >>>>>>>[>>>>>]++[-<<<<<] >>>>>> -] +>>>>>] <[> ++< -] <<<<[<[" +
            "> +< -] <<<<] >>[->[-]++++++[< ++++++++> -] >>>>]<<<<<[<[>+>+<<-]>.<<<<<]>.>>>>]";

        private static string ProgramFibonacci =
            "> ++++++++++> +> +[" +
            "    [+++++[> ++++++++< -] >.< ++++++[> --------< -] +<<<] >.>>[" +
            "        [-] <[> +< -] >>[<< +> +> -] <[> +< -[> +< -[> +< -[> +< -[> +< -[> +< -" +
            "            [> +< -[> +< -[> +< -[>[-] > +> +<<< -[> +< -]]]]]]]]]]] +>>>" +
            "    ]<<<" +
            "]";

        private static string ProgramMandelbrot =
            "+++++++++++++[->++>>>+++++>++>+<<<<<<]>>>>>++++++>--->>>>>>>>>>+++++++++++++++[[" +
            ">>>>>>>>>]+[<<<<<<<<<]>>>>>>>>>-]+[>>>>>>>>[-]>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>[-]+" +
            "<<<<<<<+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>>+>>>>>>>>>>>>>>>>>>>>>>>>>>" +
            ">+<<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+[>>>>>>[>>>>>>>[-]>>]<<<<<<<<<[<<<<<<<<<]>>" +
            ">>>>>[-]+<<<<<<++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>+<<<<<<+++++++[-[->>>" +
            ">>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>>+<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[[-]>>>>>>[>>>>>" +
            ">>[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>" +
            "[>>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<" +
            "<<]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]>>>>>>>>>+++++++++++++++[[" +
            ">>>>>>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[" +
            ">+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>[" +
            "-<<+>>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<" +
            "<<[>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<" +
            "[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>" +
            ">>>>[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+" +
            "<<<<<<[->>>[-<<<+>>>]<<<[->>>+>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>" +
            ">>>>>>>]<<<<<<<<<[>>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<<]>>[->>>>>>>>>+<<<<<<<<<]<<" +
            "+>>>>>>>>]<<<<<<<<<[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<" +
            "<]<+<<<<<<<<<]>>>>>>>>>[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>" +
            ">>>>>>>>>>>>>>>>>>>>>>>]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>" +
            ">>>>>]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+>>>>>>>>>>>>>>>>>>>>>+<<<[<<<<<<" +
            "<<<]>>>>>>>>>[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<<<<<<<<<[<<<<<" +
            "<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-<<<+>>>]<<<[->" +
            ">>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<" +
            "<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]<<<<<<<[->+>>>-<<<<]>>>>>>>>>+++++++++++++++++++" +
            "+++++++>>[-<<<<+>>>>]<<<<[->>>>+<<[-]<<]>>[<<<<<<<+<[-<+>>>>+<<[-]]>[-<<[->+>>>-" +
            "<<<<]>>>]>>>>>>>>>>>>>[>>[-]>[-]>[-]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>>>>>>[>>>>>" +
            "[-<<<<+>>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>[-<<<<<<<<" +
            "<+>>>>>>>>>]>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>>>>>>]+>[-" +
            "]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>+>>>>>>>>]<<<" +
            "<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<[->>[-<<+>>]<" +
            "<[->>+>+<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>[->>>>" +
            ">>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-]<->>>" +
            "[-<<<+>[<->-<<<<<<<+>>>>>>>]<[->+<]>>>]<<[->>+<<]<+<<<<<<<<<]>>>>>>>>>[>>>>>>[-<" +
            "<<<<+>>>>>]<<<<<[->>>>>+<<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>+>>>>>>>>" +
            "]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<[->>[-<<+" +
            ">>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>" +
            "[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-" +
            "]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>>>>>" +
            "[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" +
            "]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>" +
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>++++++++" +
            "+++++++[[>>>>>>>>>]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>>>>>>>>[-<<<<<<<+" +
            ">>>>>>>]<<<<<<<[->>>>>>>+<<<<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[" +
            "-]>>>]<<<<<<<<<[<<<<<<<<<]>>>>+>[-<-<<<<+>>>>>]>[-<<<<<<[->>>>>+<++<<<<]>>>>>[-<" +
            "<<<<+>>>>>]<->+>]<[->+<]<<<<<[->>>>>+<<<<<]>>>>>>[-]<<<<<<+>>>>[-<<<<->>>>]+<<<<" +
            "[->>>>->>>>>[>>[-<<->>]+<<[->>->[-<<<+>>>]<<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]" +
            "+>>>>>>[>>>>>>>>>]>+<]]+>>>[-<<<->>>]+<<<[->>>-<[-<<+>>]<<[->>+<<<<<<<<<<<[<<<<<" +
            "<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<" +
            "[<<<<<<<<<]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>>>>>>>]<<<<<" +
            "<<<+<[>[->>>>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>>[->>>+<<<]<]>[->>>-<<<<<<<<<" +
            "<<<<<+>>>>>>>>>>>]<<]>[->>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>>+<<<]<<" +
            "<<<<<<<<<<]>>>>[-]<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>[-<->]<[->+<]>>>>>>>>]<<<" +
            "<<<<<+<[>[->>>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>[->>>>+<<<<]>]<[->>>>-<<<<<<<" +
            "<<<<<<<+>>>>>>>>>>]<]>>[->>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>]>]<[->>>>+<<<<" +
            "]<<<<<<<<<<<]>>>>>>+<<<<<<]]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>>>>>>>>>]<<<<<<<<<" +
            "[>[->>>>>+<<<<[->>>>-<<<<<<<<<<<<<<+>>>>>>>>>>>[->>>+<<<]<]>[->>>-<<<<<<<<<<<<<<" +
            "+>>>>>>>>>>>]<<]>[->>>>+<<<[->>>-<<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>>+<<<]<<<<<<<" +
            "<<<<<]]>[-]>>[-]>[-]>>>>>[>>[-]>[-]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-<" +
            "<<<+>>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[" +
            "[>>>>>>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+" +
            "[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>" +
            "[-<<+>>]<<[->>+>+<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<" +
            "<[>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[" +
            ">[-]<->>>[-<<<+>[<->-<<<<<<<+>>>>>>>]<[->+<]>>>]<<[->>+<<]<+<<<<<<<<<]>>>>>>>>>[" +
            ">>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>]>" +
            ">>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>[-]>>>>+++++++++++++++[[>>>>>>>>>]<<<<<<<<<-<<<<<" +
            "<<<<[<<<<<<<<<]>>>>>>>>>-]+[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<" +
            "<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-" +
            "<<<+>>>]<<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>" +
            ">>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>>>" +
            "[-<<<->>>]<<<[->>>+<<<]>>>>>>>>]<<<<<<<<+<[>[->+>[-<-<<<<<<<<<<+>>>>>>>>>>>>[-<<" +
            "+>>]<]>[-<<-<<<<<<<<<<+>>>>>>>>>>>>]<<<]>>[-<+>>[-<<-<<<<<<<<<<+>>>>>>>>>>>>]<]>" +
            "[-<<+>>]<<<<<<<<<<<<<]]>>>>[-<<<<+>>>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>" +
            ">>>>>>]<<<<<<<<+<[>[->+>>[-<<-<<<<<<<<<<+>>>>>>>>>>>[-<+>]>]<[-<-<<<<<<<<<<+>>>>" +
            ">>>>>>>]<<]>>>[-<<+>[-<-<<<<<<<<<<+>>>>>>>>>>>]>]<[-<+>]<<<<<<<<<<<<]>>>>>+<<<<<" +
            "]>>>>>>>>>[>>>[-]>[-]>[-]>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>[-]>>>>>[>>>>>>>[-<<<<<" +
            "<+>>>>>>]<<<<<<[->>>>>>+<<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>+>[-<-<<<<+>>>>" +
            ">]>>[-<<<<<<<[->>>>>+<++<<<<]>>>>>[-<<<<<+>>>>>]<->+>>]<<[->>+<<]<<<<<[->>>>>+<<" +
            "<<<]+>>>>[-<<<<->>>>]+<<<<[->>>>->>>>>[>>>[-<<<->>>]+<<<[->>>-<[-<<+>>]<<[->>+<<" +
            "<<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>[-<<->>]+<<[->>->[-<<<+>>>]<" +
            "<<[->>>+<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<" +
            "<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-<<<+>>>]<<<[->>>+>>>>>>[>+>[-<->]<[->+" +
            "<]>>>>>>>>]<<<<<<<<+<[>[->>>>+<<[->>-<<<<<<<<<<<<<+>>>>>>>>>>[->>>+<<<]>]<[->>>-" +
            "<<<<<<<<<<<<<+>>>>>>>>>>]<]>>[->>+<<<[->>>-<<<<<<<<<<<<<+>>>>>>>>>>]>]<[->>>+<<<" +
            "]<<<<<<<<<<<]>>>>>[-]>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]]>>>>[-<<<<+>" +
            ">>>]<<<<[->>>>+>>>>>[>+>>[-<<->>]<<[->>+<<]>>>>>>>>]<<<<<<<<+<[>[->>>>+<<<[->>>-" +
            "<<<<<<<<<<<<<+>>>>>>>>>>>[->>+<<]<]>[->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<<]>[->>>+<<[" +
            "->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>+<<]<<<<<<<<<<<<]]>>>>[-]<<<<]>>>>[-<<<<+>>" +
            ">>]<<<<[->>>>+>[-]>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+<<+<<<<<]>>>>>>>>>[>>>>>>" +
            ">>>]<<<<<<<<<[>[->>>>+<<<[->>>-<<<<<<<<<<<<<+>>>>>>>>>>>[->>+<<]<]>[->>-<<<<<<<<" +
            "<<<<<+>>>>>>>>>>>]<<]>[->>>+<<[->>-<<<<<<<<<<<<<+>>>>>>>>>>>]<]>[->>+<<]<<<<<<<<" +
            "<<<<]]>>>>>>>>>[>>[-]>[-]>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>[-]>[-]>>>>>[>>>>>[-<<<<+" +
            ">>>>]<<<<[->>>>+<<<+<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[-<<<<<+>>>>>" +
            "]<<<<<[->>>>>+<<<+<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>" +
            ">>>>>]+>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]>[-]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+[>+>>" +
            ">>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>[-<<<<+>>>>]<<<<[->>>>+<<<<<[->>[-<<+" +
            ">>]<<[->>+>>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>" +
            "[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<]>[->>>>>>>>>+<<<<<<<<<]<+>>>>>>>>]<<<<<<<<<[>[-" +
            "]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+<<<<<<<<<]>>>>>>>>>" +
            "[>+>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>->>>>>[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<" +
            "<<[->>>[-<<<+>>>]<<<[->>>+>+<<<<]+>>>>>>>>>]<<<<<<<<[<<<<<<<<<]]>>>>>>>>>[>>>>>>" +
            ">>>]<<<<<<<<<[>>[->>>>>>>>>+<<<<<<<<<]<<<<<<<<<<<]>>[->>>>>>>>>+<<<<<<<<<]<<+>>>" +
            ">>>>>]<<<<<<<<<[>[-]<->>>>[-<<<<+>[<->-<<<<<<+>>>>>>]<[->+<]>>>>]<<<[->>>+<<<]<+" +
            "<<<<<<<<<]>>>>>>>>>[>>>>[-<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<+>>>>>>>>>>>>>>>>>" +
            ">>>>>>>>>>>>>>>>>>>]>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>+++++++++++++++[[>>>>>>>>" +
            ">]<<<<<<<<<-<<<<<<<<<[<<<<<<<<<]>>>>>>>>>-]+>>>>>>>>>>>>>>>>>>>>>+<<<[<<<<<<<<<]" +
            ">>>>>>>>>[>>>[-<<<->>>]+<<<[->>>->[-<<<<+>>>>]<<<<[->>>>+<<<<<<<<<<<<<[<<<<<<<<<" +
            "]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>[-<<<<->>>>]+<<<<[->>>>-<[-<<<+>>>]<<<[->>>+<" +
            "<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>" +
            ">>>>>>>]<<<<<<<<<[<<<<<<<<<]>>->>[-<<<<+>>>>]<<<<[->>>>+<<[-]<<]>>]<<+>>>>[-<<<<" +
            "->>>>]+<<<<[->>>>-<<<<<<.>>]>>>>[-<<<<<<<.>>>>>>>]<<<[-]>[-]>[-]>[-]>[-]>[-]>>>[" +
            ">[-]>[-]>[-]>[-]>[-]>[-]>>>]<<<<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-]>>>>]<<<<<<<<<" +
            "[<<<<<<<<<]>+++++++++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>+>>>>>>>>>+<<<<<<<<" +
            "<<<<<<[<<<<<<<<<]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+[-]>>[>>>>>>>>>]<<<<<" +
            "<<<<[>>>>>>>[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<<<<<<[<<<<<<<<<]>>>>>>>[-]+>>>]<<<<" +
            "<<<<<<]]>>>>>>>[-<<<<<<<+>>>>>>>]<<<<<<<[->>>>>>>+>>[>+>>>>[-<<<<->>>>]<<<<[->>>" +
            ">+<<<<]>>>>>>>>]<<+<<<<<<<[>>>>>[->>+<<]<<<<<<<<<<<<<<]>>>>>>>>>[>>>>>>>>>]<<<<<" +
            "<<<<[>[-]<->>>>>>>[-<<<<<<<+>[<->-<<<+>>>]<[->+<]>>>>>>>]<<<<<<[->>>>>>+<<<<<<]<" +
            "+<<<<<<<<<]>>>>>>>-<<<<[-]+<<<]+>>>>>>>[-<<<<<<<->>>>>>>]+<<<<<<<[->>>>>>>->>[>>" +
            ">>>[->>+<<]>>>>]<<<<<<<<<[>[-]<->>>>>>>[-<<<<<<<+>[<->-<<<+>>>]<[->+<]>>>>>>>]<<" +
            "<<<<[->>>>>>+<<<<<<]<+<<<<<<<<<]>+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>+<<<" +
            "<<[<<<<<<<<<]>>>>>>>>>[>>>>>[-<<<<<->>>>>]+<<<<<[->>>>>->>[-<<<<<<<+>>>>>>>]<<<<" +
            "<<<[->>>>>>>+<<<<<<<<<<<<<<<<[<<<<<<<<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>>>>[-<" +
            "<<<<<<->>>>>>>]+<<<<<<<[->>>>>>>-<<[-<<<<<+>>>>>]<<<<<[->>>>>+<<<<<<<<<<<<<<[<<<" +
            "<<<<<<]>>>[-]+>>>>>>[>>>>>>>>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<" +
            "<<[<<<<<<<<<]>>>>[-]<<<+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>-<<<<<[<<<<<<<" +
            "<<]]>>>]<<<<.>>>>>>>>>>[>>>>>>[-]>>>]<<<<<<<<<[<<<<<<<<<]>++++++++++[-[->>>>>>>>" +
            ">+<<<<<<<<<]>>>>>>>>>]>>>>>+>>>>>>>>>+<<<<<<<<<<<<<<<[<<<<<<<<<]>>>>>>>>[-<<<<<<" +
            "<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+[-]>[>>>>>>>>>]<<<<<<<<<[>>>>>>>>[-<<<<<<<+>>>>>>" +
            ">]<<<<<<<[->>>>>>>+<<<<<<<<[<<<<<<<<<]>>>>>>>>[-]+>>]<<<<<<<<<<]]>>>>>>>>[-<<<<<" +
            "<<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+>[>+>>>>>[-<<<<<->>>>>]<<<<<[->>>>>+<<<<<]>>>>>>" +
            ">>]<+<<<<<<<<[>>>>>>[->>+<<]<<<<<<<<<<<<<<<]>>>>>>>>>[>>>>>>>>>]<<<<<<<<<[>[-]<-" +
            ">>>>>>>>[-<<<<<<<<+>[<->-<<+>>]<[->+<]>>>>>>>>]<<<<<<<[->>>>>>>+<<<<<<<]<+<<<<<<" +
            "<<<]>>>>>>>>-<<<<<[-]+<<<]+>>>>>>>>[-<<<<<<<<->>>>>>>>]+<<<<<<<<[->>>>>>>>->[>>>" +
            ">>>[->>+<<]>>>]<<<<<<<<<[>[-]<->>>>>>>>[-<<<<<<<<+>[<->-<<+>>]<[->+<]>>>>>>>>]<<" +
            "<<<<<[->>>>>>>+<<<<<<<]<+<<<<<<<<<]>+++++[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>" +
            "+>>>>>>>>>>>>>>>>>>>>>>>>>>>+<<<<<<[<<<<<<<<<]>>>>>>>>>[>>>>>>[-<<<<<<->>>>>>]+<" +
            "<<<<<[->>>>>>->>[-<<<<<<<<+>>>>>>>>]<<<<<<<<[->>>>>>>>+<<<<<<<<<<<<<<<<<[<<<<<<<" +
            "<<]>>>>[-]+>>>>>[>>>>>>>>>]>+<]]+>>>>>>>>[-<<<<<<<<->>>>>>>>]+<<<<<<<<[->>>>>>>>" +
            "-<<[-<<<<<<+>>>>>>]<<<<<<[->>>>>>+<<<<<<<<<<<<<<<[<<<<<<<<<]>>>[-]+>>>>>>[>>>>>>" +
            ">>>]>[-]+<]]+>[-<[>>>>>>>>>]<<<<<<<<]>>>>>>>>]<<<<<<<<<[<<<<<<<<<]>>>>[-]<<<++++" +
            "+[-[->>>>>>>>>+<<<<<<<<<]>>>>>>>>>]>>>>>->>>>>>>>>>>>>>>>>>>>>>>>>>>-<<<<<<[<<<<" +
            "<<<<<]]>>>]";

        //http://calmerthanyouare.org/2015/01/07/optimizing-brainfuck.html
        //https://www.nayuki.io/page/optimizing-brainfuck-compiler
        //https://github.com/rdebath/Brainfuck/tree/master/testing
        //https://github.com/matslina/bfoptimization/tree/master/progs

        static void Main(string[] args)
        {
            string prg = ProgramDummy;
            BrainfuckInterpreterTest t = new BrainfuckInterpreterTest();
            List<InstructionBase> i0 = t.ToIntermediateRepresentation(prg);
            //List<InstructionBase> i1 = t.OptimizeClearLoop(i0);
            List<InstructionBase> i1 = t.OptimizeContract(i0);

            //BrainfuckInterpreterTreeBased interpreter = new BrainfuckInterpreterTreeBased();
            //BrainfuckInterpreterListBased interpreter = new BrainfuckInterpreterListBased();
            //List<string> analyze = interpreter.Analyse(prg);
            //foreach(string s in analyze)
            //    Debug.WriteLine(s);
            //interpreter.Parse(prg);
            //string cStatements = interpreter.ToCStatements();
            //Debug.WriteLine(cStatements);
            //string bfStatements = interpreter.ToBrainfuckStatement(80);
            //Debug.WriteLine(bfStatements);
            //interpreter.Execute();

            //NaiveInterpreter(ProgramMandelbrot);
        }

        private static void RleOptimizedInterpreter(string program)
        {
            // instructions mapping
            // 0 ->15: INC*x (x: 1->16)
            // 16->31: DEC*x (x: 1->16)
            // 32->47: PTR++*x (x: 1->16)
            // 48->63: PTR--*x (x: 1->16)
            // 64: [
            // 65: ]
            // 66: .
            // 67: ,
            // 255: end of program
            byte[] instructions = new byte[program.Length + 1]; // RLE compressed will never be longer than original program (+1 for end-of-program instruction)
            int[] loopIndex = new int[65536];

            // parse program and convert to instructions
            Stack<int> loopStack = new Stack<int>();
            int instructionPtr = 0;
            int programPtr = 0;
            while (programPtr < program.Length)
            {
                char instruction = program[programPtr];
                // check RLE
                int instructionCount = 1;
                while (true)
                {
                    programPtr++;
                    if (programPtr >= program.Length)
                        break;
                    char nextInstruction = program[programPtr];
                    if (nextInstruction == instruction && instructionCount < 15 && (nextInstruction == '+' || nextInstruction == '-' || nextInstruction == '>' || nextInstruction == '<'))
                        instructionCount++;
                    else
                        break;
                }
                // instruction + count found
                switch (instruction)
                {
                    case '[':
                        loopStack.Push(instructionPtr);
                        instructions[instructionPtr++] = 64;
                        break;
                    case ']':
                        if (loopStack.Count == 0)
                        {
                            Console.WriteLine($"Unmatched ']' at position {programPtr}");
                            Debug.WriteLine($"Unmatched ']' at position {programPtr}");
                            return;
                        }
                        int loopStart = loopStack.Pop(); // take matching '[' from the stack,
                        loopIndex[instructionPtr] = loopStart; // save it as the match for the current ']',
                        loopIndex[loopStart] = instructionPtr; // and save the current ']' as the match for it
                        instructions[instructionPtr++] = 65;
                        break;
                    case '+':
                        instructions[instructionPtr++] = (byte) (instructionCount - 1);
                        break;
                    case '-':
                        instructions[instructionPtr++] = (byte) (15 + instructionCount);
                        break;
                    case '>':
                        instructions[instructionPtr++] = (byte) (31 + instructionCount);
                        break;
                    case '<':
                        instructions[instructionPtr++] = (byte) (47 + instructionCount);
                        break;
                    case '.':
                        instructions[instructionPtr++] = 66;
                        break;
                    case ',':
                        instructions[instructionPtr++] = 67;
                        break;
                }
            }
            if (loopStack.Count > 0)
            {
                Console.WriteLine($"Unmatched ']' at position {loopStack.Peek()}"); // TODO: store programPtr+instructionPtr to allow nice display
                Debug.WriteLine($"Unmatched ']' at position {loopStack.Peek()}");
            }
            instructions[instructionPtr] = 255;

            Debug.WriteLine($"RLE optimization: {program.Length} instructions -> {instructionPtr} instructions");

            // interpret program
            byte[] memory = new byte[65536];
            int memoryPtr = 0;
            instructionPtr = 0;
            while (true)
            {
                byte instruction = instructions[instructionPtr];
                if (instruction < 16)
                {
                    Debug.WriteLine($"*ptr+={instruction + 1}");
                    memory[memoryPtr] += (byte) (instruction + 1);
                }
                else if (instruction >= 16 && instruction < 32)
                {
                    Debug.WriteLine($"*ptr-={instruction - 15}");
                    memory[memoryPtr] -= (byte) (instruction - 15);
                }
                else if (instruction >= 32 && instruction < 48)
                {
                    Debug.WriteLine($"ptr+={instruction - 31}");
                    memoryPtr += instruction - 31;
                }
                else if (instruction >= 48 && instruction < 64)
                {
                    Debug.WriteLine($"ptr-={instruction - 47}");
                    memoryPtr -= instruction - 47;
                }
                else if (instruction == 64)
                {
                    Debug.WriteLine("while(*ptr){");
                    if (memory[memoryPtr] == 0)
                    {
                        Debug.WriteLine("Jump forward");
                        instructionPtr = loopIndex[instructionPtr];
                    }
                }
                else if (instruction == 65)
                {
                    Debug.WriteLine("}");
                    if (memory[memoryPtr] != 0)
                        instructionPtr = loopIndex[instructionPtr];
                }
                else if (instruction == 66)
                {
                    Debug.WriteLine("putchar(*ptr)");
                    Console.Write((char) memory[memoryPtr]);
                }
                else if (instruction == 67)
                {
                    Debug.WriteLine("*ptr=getchar()");
                    memory[memoryPtr] = (byte) Console.ReadKey().KeyChar;
                }
                else
                    return;
                instructionPtr++;
            }
        }

        private static void JumpOptimizedInterpreter(string program)
        {
            int[] loopIndex = new int[65536];
            byte[] array = new byte[65536];
            int arrayPtr = 0;

            // Preprocess loop index
            Stack<int> loopProgramPtrStack = new Stack<int>();
            for (int programPtr = 0; programPtr < program.Length; programPtr++)
            {
                char instruction = program[programPtr];
                if (instruction == '[')
                    loopProgramPtrStack.Push(programPtr);
                else if (instruction == ']')
                {
                    if (loopProgramPtrStack.Count == 0)
                    {
                        Console.WriteLine($"Unmatched ']' at position {programPtr}");
                        Debug.WriteLine($"Unmatched ']' at position {programPtr}");
                        return;
                    }
                    int loopStart = loopProgramPtrStack.Pop(); // take matching '[' from the stack,
                    loopIndex[programPtr] = loopStart; // save it as the match for the current ']',
                    loopIndex[loopStart] = programPtr; // and save the current ']' as the match for it
                }
            }
            if (loopProgramPtrStack.Count > 0)
            {
                Console.WriteLine($"Unmatched ']' at position {loopProgramPtrStack.Peek()}");
                Debug.WriteLine($"Unmatched ']' at position {loopProgramPtrStack.Peek()}");
            }
            // Interpret program
            for (int programPtr = 0; programPtr < program.Length; programPtr++)
            {
                char instruction = program[programPtr];
                switch (instruction)
                {
                    case '>':
                        Debug.WriteLine("++ptr");
                        arrayPtr++;
                        break;
                    case '<':
                        Debug.WriteLine("--ptr");
                        arrayPtr--;
                        break;
                    case '+':
                        Debug.WriteLine("++*ptr");
                        array[arrayPtr]++;
                        break;
                    case '-':
                        Debug.WriteLine("--*ptr");
                        array[arrayPtr]--;
                        break;
                    case '.':
                        Debug.WriteLine("putchar(*ptr)");
                        Console.Write((char) array[arrayPtr]);
                        break;
                    case ',':
                        Debug.WriteLine("*ptr=getchar()");
                        array[arrayPtr] = (byte) Console.ReadKey().KeyChar;
                        break;
                    case '[':
                        Debug.WriteLine("while(*ptr){");
                        if (array[arrayPtr] == 0)
                        {
                            Debug.WriteLine("Jump forward");
                            programPtr = loopIndex[programPtr];
                        }
                        break;
                    case ']':
                        Debug.WriteLine("}");
                        if (array[arrayPtr] != 0)
                            programPtr = loopIndex[programPtr];
                        break;
                }
            }
        }

        private static void NaiveInterpreter(string program)
        {
            byte[] array = new byte[65536];
            int arrayPtr = 0;

            Stack<int> loopIndex = new Stack<int>();
            int programPtr = 0;
            while (true)
            {
                if (arrayPtr < 0 || arrayPtr >= array.Length)
                {
                    Console.WriteLine("Ptr is out of range");
                    return;
                }
                if (programPtr >= program.Length)
                    return; // end of program
                char instruction = program[programPtr];
                switch (instruction)
                {
                    case '>':
                        Debug.WriteLine("++ptr");
                        arrayPtr++;
                        break;
                    case '<':
                        Debug.WriteLine("--ptr");
                        arrayPtr--;
                        break;
                    case '+':
                        Debug.WriteLine("++*ptr");
                        array[arrayPtr]++;
                        break;
                    case '-':
                        Debug.WriteLine("--*ptr");
                        array[arrayPtr]--;
                        break;
                    case '.':
                        Debug.WriteLine("putchar(*ptr)");
                        Console.Write((char) array[arrayPtr]);
                        break;
                    case ',':
                        Debug.WriteLine("*ptr=getchar()");
                        array[arrayPtr] = (byte) Console.ReadKey().KeyChar;
                        break;
                    case '[':
                        Debug.WriteLine("while(*ptr){");
                        if (array[arrayPtr] == 0) // jump forward to command after matching ]
                        {
                            Debug.WriteLine("Jump forward");
                            int loopCount = 1;
                            while (loopCount > 0)
                            {
                                programPtr++;
                                if (programPtr >= program.Length)
                                {
                                    Debug.WriteLine("Unexpected end of program");
                                    Console.WriteLine("Unexpected end of program");
                                    return;
                                }
                                if (program[programPtr] == '[')
                                    loopCount++;
                                else if (program[programPtr] == ']')
                                    loopCount--;
                            }
                        }
                        else
                            loopIndex.Push(programPtr);
                        break;
                    case ']':
                        Debug.WriteLine("}");
                        if (array[arrayPtr] != 0)
                            programPtr = loopIndex.Peek();
                        else
                            loopIndex.Pop();
                        break;
                }
                programPtr++;
            }
        }
    }
}

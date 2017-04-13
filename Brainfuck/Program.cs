using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brainfuck.Instructions;

namespace Brainfuck
{
    //https://en.wikipedia.org/wiki/Brainfuck
    class Program
    {
        //http://calmerthanyouare.org/2015/01/07/optimizing-brainfuck.html
        //https://www.nayuki.io/page/optimizing-brainfuck-compiler
        //https://github.com/rdebath/Brainfuck/tree/master/testing
        //https://github.com/matslina/bfoptimization/tree/master/progs

        static void Main(string[] args)
        {
            // OptimizeClearLoop or OptimizeCopyMultiplyLoop should be called first to allow OptimizeOffset to perform better

            string prg = BrainfuckPrograms.ProgramMandelbrot;
            BrainfuckInterpreterOptimized t = new BrainfuckInterpreterOptimized();
            List<InstructionBase> i0 = t.Parse(prg);
            //List<InstructionBase> i1 = t.OptimizeClearLoop(i0);
            //List<InstructionBase> i1 = t.OptimizeScanLoop(i0);
            //List<InstructionBase> i1 = t.OptimizeContract(i0);
            //List<InstructionBase> i1 = t.OptimizeCancel(i0);
            //List<InstructionBase> i1 = t.OptimizeCopyMultiplyLoop(i0);
            //List<InstructionBase> i1 = t.OptimizeOffset(i0);

            //List<InstructionBase> i1 = t.OptimizeClearLoop(i0);
            //List<InstructionBase> i2 = t.OptimizeOffset(i1);

            //List<InstructionBase> i1 = t.OptimizeOffset(i0);
            //List<InstructionBase> i2 = t.OptimizeCopyMultiplyLoop(i1);

            List<InstructionBase> i1 = t.OptimizeCopyMultiplyLoop(i0);
            List<InstructionBase> i2 = t.OptimizeOffset(i1);

            //List<InstructionBase> i1 = t.OptimizeCopyMultiplyLoop(i0);
            //List<InstructionBase> i2 = t.OptimizeContract(i1);

            //List<InstructionBase> i1 = t.OptimizeContract(i0);
            //List<InstructionBase> i2 = t.OptimizeCopyMultiplyLoop(i1);

            //Debug.Write(t.ToCStatements(i2));
            //Debug.WriteLine(t.ToIntermediateRepresentation(i2));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //t.Execute(i0);
            //t.Execute(i1);
            t.Execute(i2);
            sw.Stop();
            long elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed:{elapsed}ms");

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
    }
}

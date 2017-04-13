using System;
using System.Diagnostics;
using System.Linq;

namespace Brainfuck.Instructions
{
    public class OutInstruction : InstructionBase
    {
        public int Offset { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            Console.Write((char)memory[memoryPtr+Offset]);
            Debug.WriteLine(memory[memoryPtr + Offset]+"  ==> "+ (char)memory[memoryPtr + Offset]);
        }

        public override string ToCStatement()
        {
            if (Offset > 0)
                return $"putchar(mem[p+{Offset}]);";
            if (Offset < 0)
                return $"putchar(mem[p-{-Offset}]);";
            return "putchar(mem[p]);";
        }

        public override string ToBrainfuckStatement()
        {
            if (Offset > 0)
                return string.Concat(Enumerable.Repeat(">", Offset)) + "." + string.Concat(Enumerable.Repeat("<", Offset));
            if (Offset < 0)
                return string.Concat(Enumerable.Repeat("<", Offset)) + "." + string.Concat(Enumerable.Repeat(">", Offset));
            return ".";
        }

        public override string ToIntermediateRepresentation()
        {
            return $"OUT({Offset})";
        }
    }
}

using System;
using System.Linq;

namespace Brainfuck.Instructions
{
    public class InInstruction : InstructionBase
    {
        public int Offset { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr+Offset] = (byte)Console.ReadKey().KeyChar;
        }

        public override string ToCStatement()
        {
            if (Offset > 0)
                return $"mem[p+{Offset}] = getchar();";
            if (Offset < 0 )
                return $"mem[p-{-Offset}] = getchar();";
            return $"mem[p] = getchar();";
        }

        public override string ToBrainfuckStatement()
        {
            if (Offset > 0)
                return string.Concat(Enumerable.Repeat(">", Offset)) + "," + string.Concat(Enumerable.Repeat("<", Offset));
            if (Offset < 0)
                return string.Concat(Enumerable.Repeat("<", Offset)) + "," + string.Concat(Enumerable.Repeat(">", Offset));
            return ".";
        }

        public override string ToIntermediateRepresentation()
        {
            return $"IN({Offset})";
        }
    }
}

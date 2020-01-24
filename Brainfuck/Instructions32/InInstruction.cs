using System;
using System.Linq;

namespace Brainfuck.Instructions32
{
    public class InInstruction : InstructionBase
    {
        public int Offset { get; set; }

        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr+Offset] = Console.ReadKey().KeyChar;
        }

        public override string ToCStatement() // TODO
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

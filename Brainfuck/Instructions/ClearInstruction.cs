using System.Linq;

namespace Brainfuck.Instructions
{
    public class ClearInstruction : InstructionBase
    {
        public int Offset { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr+Offset] = 0;
        }

        public override string ToCStatement()
        {
            if (Offset > 0)
                return $"mem[p+{Offset}] = 0;";
            if (Offset < 0)
                return $"mem[p+{-Offset}] = 0;";
            return "mem[p] = 0;";
        }

        public override string ToBrainfuckStatement()
        {
            if (Offset > 0)
                return string.Concat(Enumerable.Repeat(">", Offset)) + "[-]" + string.Concat(Enumerable.Repeat("<", Offset));
            if (Offset < 0)
                return string.Concat(Enumerable.Repeat("<", Offset)) + "[-]" + string.Concat(Enumerable.Repeat(">", Offset));
            return "[-]";
        }

        public override string ToIntermediateRepresentation()
        {
            return $"CLEAR({Offset})";
        }
    }
}

using System.Linq;

namespace Brainfuck.Instructions
{
    public class AddInstruction : InstructionBase
    {
        public byte X { get; set; }
        public int Shift { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr+Shift] += X;
        }

        public override string ToCStatement()
        {
            if (Shift > 0)
                return $"mem[p+{Shift}] += {X};";
            if (Shift < 0)
                return $"mem[p-{Shift}] += {X};";
            return $"mem[p] += {X};";
        }

        public override string ToBrainfuckStatement()
        {
            if (Shift > 0)
                return string.Concat(Enumerable.Repeat(">", Shift)) + string.Concat(Enumerable.Repeat("+", X));
            if (Shift < 0 )
                return string.Concat(Enumerable.Repeat("<", Shift)) + string.Concat(Enumerable.Repeat("+", X));
            return string.Concat(Enumerable.Repeat("+", X));
        }

        public override string ToIntermediateRepresentation()
        {
            return $"ADD({X},{Shift})";
        }
    }
}

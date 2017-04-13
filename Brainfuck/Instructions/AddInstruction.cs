using System;
using System.Linq;

namespace Brainfuck.Instructions
{
    public class AddInstruction : ArithmeticInstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr+Offset] += X;
        }

        public override string ToCStatement()
        {
            if (Offset > 0)
                return $"mem[p+{Offset}] += {X};";
            if (Offset < 0)
                return $"mem[p-{-Offset}] += {X};";
            return $"mem[p] += {X};";
        }

        public override string ToBrainfuckStatement()
        {
            if (Offset > 0)
                return string.Concat(Enumerable.Repeat(">", Offset)) + string.Concat(Enumerable.Repeat("+", X)) + string.Concat(Enumerable.Repeat("<", Offset));
            if (Offset < 0 )
                return string.Concat(Enumerable.Repeat("<", Offset)) + string.Concat(Enumerable.Repeat("+", X)) + string.Concat(Enumerable.Repeat(">", Offset));
            return string.Concat(Enumerable.Repeat("+", X));
        }

        public override string ToIntermediateRepresentation()
        {
            return $"ADD({Offset}, {X})";
        }

        public override Type OppositeType => typeof(SubInstruction);
    }
}

using System;
using System.Linq;

namespace Brainfuck.Instructions
{
    public class RightInstruction : PositionInstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memoryPtr += X;
        }

        public override string ToCStatement()
        {
            return $"p += {X};";
        }

        public override string ToBrainfuckStatement()
        {
            return string.Concat(Enumerable.Repeat(">", X));
        }

        public override string ToIntermediateRepresentation()
        {
            return $"RIGHT({X})";
        }

        public override Type OppositeType => typeof(LeftInstruction);
    }
}

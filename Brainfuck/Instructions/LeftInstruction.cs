using System.Linq;

namespace Brainfuck.Instructions
{
    public class LeftInstruction : InstructionBase
    {
        public int X { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memoryPtr -= X;
        }

        public override string ToCStatement()
        {
            return $"p -= {X};";
        }

        public override string ToBrainfuckStatement()
        {
            return string.Concat(Enumerable.Repeat("<", X));
        }

        public override string ToIntermediateRepresentation()
        {
            return $"LEFT({X})";
        }
    }
}

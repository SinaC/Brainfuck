using System;

namespace Brainfuck.Instructions
{
    public abstract class ArithmeticInstructionBase : InstructionBase
    {
        public byte X { get; set; }
        public int Offset { get; set; }

        public abstract Type OppositeType { get; }
    }
}

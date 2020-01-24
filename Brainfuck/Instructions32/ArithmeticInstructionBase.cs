using System;

namespace Brainfuck.Instructions32
{
    public abstract class ArithmeticInstructionBase : InstructionBase
    {
        public int X { get; set; }
        public int Offset { get; set; }

        public abstract Type OppositeType { get; }
    }
}

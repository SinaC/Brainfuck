using System;

namespace Brainfuck.Instructions32
{
    public abstract class PositionInstructionBase : InstructionBase
    {
        public int X { get; set; }

        public abstract Type OppositeType { get; }
    }
}

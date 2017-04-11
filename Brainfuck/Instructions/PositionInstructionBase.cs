using System;

namespace Brainfuck.Instructions
{
    public abstract class PositionInstructionBase : InstructionBase
    {
        public int X { get; set; }

        public abstract Type OppositeType { get; }
    }
}

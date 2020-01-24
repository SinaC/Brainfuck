﻿using System;
using System.Linq;

namespace Brainfuck.Instructions32
{
    public class LeftInstruction : PositionInstructionBase
    {
        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memoryPtr -= X;
        }

        public override string ToCStatement() // TODO
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

        public override Type OppositeType => typeof(RightInstruction);
    }
}

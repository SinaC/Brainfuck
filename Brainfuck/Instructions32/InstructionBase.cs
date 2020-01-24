using System.Diagnostics;

namespace Brainfuck.Instructions32
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class InstructionBase
    {
        public abstract void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr);
        public abstract string ToCStatement();
        public abstract string ToBrainfuckStatement();
        public abstract string ToIntermediateRepresentation();

        private string DebuggerDisplay => ToIntermediateRepresentation();
    }
}

namespace Brainfuck.Instructions
{
    public class ClearInstruction : InstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr] = 0;
        }

        public override string ToCStatement()
        {
            return "mem[p] = 0;";
        }

        public override string ToBrainfuckStatement()
        {
            return "[-]";
        }

        public override string ToIntermediateRepresentation()
        {
            return "CLEAR";
        }
    }
}

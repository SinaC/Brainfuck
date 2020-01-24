namespace Brainfuck.Instructions32
{
    public class ScanRightInstruction : InstructionBase
    {
        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            while (memory[memoryPtr] != 0)
                memoryPtr++;
        }

        public override string ToCStatement() // TODO
        {
            return "p += (long)(memchr(mem + p, 0, sizeof(mem)) - (void *)(mem + p));";
        }

        public override string ToBrainfuckStatement()
        {
            return "[>]";
        }

        public override string ToIntermediateRepresentation()
        {
            return "SCANRIGHT";
        }
    }
}

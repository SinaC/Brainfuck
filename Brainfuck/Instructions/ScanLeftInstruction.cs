namespace Brainfuck.Instructions
{
    public class ScanLeftInstruction : InstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            while (memory[memoryPtr] != 0)
                memoryPtr--;
        }

        public override string ToCStatement()
        {
            return "p -= (long)((void *)(mem + p) - memrchr(mem, 0, p + 1));";
        }

        public override string ToBrainfuckStatement()
        {
            return "[<]";
        }

        public override string ToIntermediateRepresentation()
        {
            return "SCANLEFT";
        }
    }
}

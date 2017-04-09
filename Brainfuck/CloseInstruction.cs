namespace Brainfuck
{
    public class CloseInstruction : InstructionBase
    {
        public int OpenInstructionPtr { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            if (memory[memoryPtr] != 0)
                instructionPtr = OpenInstructionPtr;
        }

        public override string ToCStatement()
        {
            return "}";
        }

        public override string ToString()
        {
            return "CLOSE";
        }
    }
}

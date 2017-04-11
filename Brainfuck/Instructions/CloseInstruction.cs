namespace Brainfuck.Instructions
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

        public override string ToBrainfuckStatement()
        {
            return "]";
        }

        public override string ToIntermediateRepresentation()
        {
            return "CLOSE";
        }
    }
}

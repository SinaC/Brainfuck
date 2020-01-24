namespace Brainfuck.Instructions32
{
    public class CloseInstruction : InstructionBase
    {
        public int OpenInstructionPtr { get; set; }

        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
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

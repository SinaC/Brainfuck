namespace Brainfuck.Instructions32
{
    public class OpenInstruction : InstructionBase
    {
        public int CloseInstructionPtr { get; set; }

        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            if (memory[memoryPtr] == 0)
                instructionPtr = CloseInstructionPtr;
        }

        public override string ToCStatement()
        {
            return "while(mem[p]){";
        }

        public override string ToBrainfuckStatement()
        {
            return "[";
        }

        public override string ToIntermediateRepresentation()
        {
            return "OPEN";
        }
    }
}

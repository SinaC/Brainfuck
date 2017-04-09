namespace Brainfuck
{
    public class SubInstruction : InstructionBase
    {
        public byte X { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr] -= X;
        }

        public override string ToCStatement()
        {
            return $"mem[p] -= {X};";
        }

        public override string ToString()
        {
            return $"SUB({X})";
        }
    }
}

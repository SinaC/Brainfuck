namespace Brainfuck
{
    public class LeftInstruction : InstructionBase
    {
        public int X { get; set; }

        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memoryPtr -= X;
        }

        public override string ToCStatement()
        {
            return $"p -= {X};";
        }

        public override string ToString()
        {
            return $"LEFT({X})";
        }
    }
}

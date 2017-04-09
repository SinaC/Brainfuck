namespace Brainfuck
{
    public abstract class InstructionBase
    {
        public abstract void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr);
        public abstract string ToCStatement();
    }
}

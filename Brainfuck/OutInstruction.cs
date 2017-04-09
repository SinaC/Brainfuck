using System;

namespace Brainfuck
{
    public class OutInstruction : InstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            Console.Write((char)memory[memoryPtr]);
        }

        public override string ToCStatement()
        {
            return "putchar(mem[p]);";
        }

        public override string ToString()
        {
            return "OUT";
        }
    }
}

using System;

namespace Brainfuck.Instructions
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

        public override string ToBrainfuckStatement()
        {
            return ".";
        }

        public override string ToIntermediateRepresentation()
        {
            return "OUT";
        }
    }
}

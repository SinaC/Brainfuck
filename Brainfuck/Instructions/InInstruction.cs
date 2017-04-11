using System;

namespace Brainfuck.Instructions
{
    public class InInstruction : InstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            memory[memoryPtr] = (byte)Console.ReadKey().KeyChar;
        }

        public override string ToCStatement()
        {
            return "mem[p] = getchar();";
        }

        public override string ToBrainfuckStatement()
        {
            return ",";
        }

        public override string ToIntermediateRepresentation()
        {
            return "IN";
        }
    }
}

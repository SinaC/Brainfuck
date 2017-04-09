using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck
{
    public class ScanRightInstruction : InstructionBase
    {
        public override void Execute(byte[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            while (memory[memoryPtr] != 0)
                memoryPtr++;
        }

        public override string ToCStatement()
        {
            return "p += (long)(memchr(mem + p, 0, sizeof(mem)) - (void *)(mem + p));";
        }

        public override string ToString()
        {
            return "SCANRIGHT";
        }
    }
}

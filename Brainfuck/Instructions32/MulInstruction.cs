using System;

namespace Brainfuck.Instructions32
{
    public class MulInstruction : InstructionBase
    {
        public short Factor { get; set; }
        public int Offset { get; set; }

        public override void Execute(int[] memory, ref int memoryPtr, ref int instructionPtr)
        {
            if (Factor > 0)
                memory[memoryPtr + Offset] += memory[memoryPtr] * Factor;
            else
                memory[memoryPtr + Offset] -= memory[memoryPtr] * -Factor;
        }

        public override string ToCStatement() // TODO
        {
            if (Offset > 0)
                return $"mem[p+{Offset}] += mem[p] * {Factor};";
            if (Offset < 0)
                return $"mem[p-{-Offset}] += mem[p] * {Factor};";
            return $"mem[p] += mem[p] * {Factor};"; // SHOULD NEVER HAPPEN
        }

        public override string ToBrainfuckStatement()
        {
            // It's not possible to generate BF statements for this instruction
            throw new NotImplementedException();
        }

        public override string ToIntermediateRepresentation()
        {
            return $"MUL({Offset}, {Factor})";
        }
    }
}

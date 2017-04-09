using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brainfuck
{
    public class BrainfuckInterpreter
    {
        public List<InstructionBase> Instructions { get; } = new List<InstructionBase>();

        public void Parse(string input)
        {
            // clear unwanted characters
            char[] allowedCharacters = {'+', '-', '>', '<', '.', ',', '[', ']'};
            string program = new string(input.ToCharArray().Where(x => allowedCharacters.Contains(x)).ToArray());

            // search clear loop [-] and replace them with precompiler instruction C
            program = program.Replace("[-]", "C");
            // search for scan left loop [<] and replace them with precompiler instruction L
            program = program.Replace("[<]", "L");
            // search for scan right loop [>] and replace them with precompiler instruction R
            program = program.Replace("[>]", "R");

            // TODO:
            // copy loop
            // [>+<-] or [->+<] => mem[p+1] += mem[p] and mem[p] = 0
            // [>>+<<-] or [->>+<<] => mem[p+2] += mem[p] and mem[p] = 0
            // [>>>>>>>>>+<<<<<<<<<-] or [->>>>>>>>>+<<<<<<<<<] => mem[p+9] += mem[p] and mem[p] = 0
            // can be more complex
            // [->+>+<<] mem[p+1] += mem[p] and mem[p+2] += mem[p] and mem[p] = 0

            // parse program and convert to instructions
            Stack<int> loopStack = new Stack<int>();
            int instructionPtr = 0;
            int programPtr = 0;
            while (programPtr < program.Length)
            {
                char instruction = program[programPtr];
                // check RLE
                int instructionCount = 1;
                while (true)
                {
                    programPtr++;
                    if (programPtr >= program.Length)
                        break;
                    char nextInstruction = program[programPtr];
                    if (nextInstruction == instruction && instructionCount < 255 && (nextInstruction == '+' || nextInstruction == '-' || nextInstruction == '>' || nextInstruction == '<'))
                        instructionCount++;
                    else
                        break;
                }
                // instruction + count -> create instruction
                switch (instruction)
                {
                    case '[':
                        loopStack.Push(instructionPtr);
                        Instructions.Add(new OpenInstruction()); // CloseInstructionPtr will be set later
                        instructionPtr++;
                        break;
                    case ']':
                        if (loopStack.Count == 0)
                            throw new InvalidOperationException($"Unmatched ']' at position {programPtr}.");
                        int loopStart = loopStack.Pop(); // take matching '[' from the stack,
                        Instructions.Add(new CloseInstruction // save it as the match for the current ']',
                        {
                            OpenInstructionPtr = loopStart
                        });
                        (Instructions[loopStart] as OpenInstruction).CloseInstructionPtr = instructionPtr; // and save the current ']' as the match for it
                        instructionPtr++;
                        break;
                    case '+':
                        Instructions.Add(new AddInstruction
                        {
                            X = (byte) instructionCount
                        });
                        instructionPtr++;
                        break;
                    case '-':
                        Instructions.Add(new SubInstruction
                        {
                            X = (byte) instructionCount
                        });
                        instructionPtr++;
                        break;
                    case '>':
                        Instructions.Add(new RightInstruction
                        {
                            X = (byte) instructionCount
                        });
                        instructionPtr++;
                        break;
                    case '<':
                        Instructions.Add(new LeftInstruction
                        {
                            X = (byte) instructionCount
                        });
                        instructionPtr++;
                        break;
                    case '.':
                        Instructions.Add(new OutInstruction());
                        instructionPtr++;
                        break;
                    case ',':
                        Instructions.Add(new InInstruction());
                        instructionPtr++;
                        break;
                        // precompiler-instruction
                    case 'C':
                        Instructions.Add(new ClearInstruction());
                        instructionPtr++;
                        break;
                    case 'L':
                        Instructions.Add(new ScanLeftInstruction());
                        instructionPtr++;
                        break;
                    case 'R':
                        Instructions.Add(new ScanRightInstruction());
                        instructionPtr++;
                        break;
                }
            }
            if (loopStack.Count > 0)
                throw new InvalidOperationException($"Unmatched ']' at position { loopStack.Peek()}."); // TODO: display original programPtr
        }

        public void Execute()
        {
            byte[] memory = new byte[65536];
            int memoryPtr = 0;
            int instructionPtr = 0;
            while (true)
            {
                if (instructionPtr >= Instructions.Count)
                    break; // stop program
                InstructionBase instruction = Instructions[instructionPtr];
                if (instruction == null)
                    throw new InvalidOperationException("Null instruction found");
                instruction.Execute(memory, ref memoryPtr, ref instructionPtr);
                instructionPtr++;
            }
        }

        public string ToCStatements()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#include <stdio.h>");
            sb.AppendLine("#include <string.h>");
            sb.AppendLine("unsigned char mem[65536];");
            sb.AppendLine("int main() {");
            sb.AppendLine("   int p = 0;");
            int indent = 1;
            foreach (InstructionBase instruction in Instructions)
            {
                if (instruction is CloseInstruction)
                    indent--;
                sb.AppendLine("".PadLeft(indent*3) + instruction.ToCStatement());
                if (instruction is OpenInstruction)
                    indent++;
            }
            sb.AppendLine("   return 0;");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string ToIntermediaryRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            foreach (InstructionBase instruction in Instructions)
                sb.AppendLine(instruction.ToString());
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brainfuck.Instructions;

namespace Brainfuck
{
    // TODO:
    // copy loop
    // [>+<-] or [->+<] ==> mem[p+1] += mem[p] and mem[p] = 0
    // [>>+<<-] or [->>+<<] ==> mem[p+2] += mem[p] and mem[p] = 0
    // [>>>>>>>>>+<<<<<<<<<-] or [->>>>>>>>>+<<<<<<<<<] ==> mem[p+9] += mem[p] and mem[p] = 0
    // can be more complex
    // [->+>+<<] ==> mem[p+1] += mem[p] and mem[p+2] += mem[p] and mem[p] = 0
    // [>+>>>+>+<<<<<-] ==> mem[p+1] += mem[p] and mem[p+4] += mem[p] and mem[p+5] += mem[p] and mem[p] = 0
    // [->>>>+<+<+<+<] ==> mem[p+4] += mem[p] and mem[o+3] += mem[p] and mem[p+2] += mem[p] and mem[p+1] += mem[p] and mem[p] = 0
    // > and < can be interchanged
    // multiply loop
    // [>+++++<-] ==> mem[p+1] += 5*mem[p] and mem[p] = 0
    // [>++++>++>>>+++>+<<<<<<-] ==> mem[p+1] += 4*mem[p] and mem[p+2] += 2*mem[p] and mem[p+5] += 3*mem[p] and mem[p] = 0
    //
    // fusing movements into adds
    // >++< ==> mem[p+1] += 2 
    //
    // postponing movements
    // >+>-> ==> mem[p+1] += 1 and mem[p+2] -= 1 and p += 3
    //
    // assign followed by add
    // CLEAR ADD(X) ==> mem[p] = X

    public class BrainfuckInterpreterListBased : IBrainfuckInterpreter
    {
        private static readonly char[] AllowedCharacters = { '+', '-', '>', '<', '.', ',', '[', ']' };

        public List<InstructionBase> Instructions { get; } = new List<InstructionBase>();

        //public List<string> Analyse(string input)
        //{
        //    List<string> results = new List<string>();

        //    string program = new string(input.ToCharArray().Where(x => AllowedCharacters.Contains(x)).ToArray());

        //    // Count useless characters
        //    int uselessCharactersCount = input.Length - program.Length;
        //    if (uselessCharactersCount > 0)
        //        results.Add($"Useless characters: {uselessCharactersCount}");

        //    // Count clear loop
        //    int clearLoopCount = Regex.Matches(program, @"\[-\]").Count;
        //    if (clearLoopCount > 0)
        //        results.Add($"Clear loop: {clearLoopCount}");

        //    // Count scan left loop
        //    int scanLeftLoopCount = Regex.Matches(program, @"\[<\]").Count;
        //    if (scanLeftLoopCount > 0)
        //        results.Add($"Scan left loop: {scanLeftLoopCount}");

        //    // Count scan right loop
        //    int scanRightLoopCount = Regex.Matches(program, @"\[>\]").Count;
        //    if (scanRightLoopCount > 0)
        //        results.Add($"Scan right loop: {scanRightLoopCount}");

        //    // Count 'RLE'
        //    int addCount = 0;
        //    int subCount = 0;
        //    int leftCount = 0;
        //    int rightCount = 0;
        //    int programPtr = 0;
        //    while (programPtr < program.Length)
        //    {
        //        char instruction = program[programPtr];
        //        // check RLE
        //        int instructionCount = 1;
        //        while (true)
        //        {
        //            programPtr++;
        //            if (programPtr >= program.Length)
        //                break;
        //            char nextInstruction = program[programPtr];
        //            if (nextInstruction == instruction && instructionCount < 255 && (nextInstruction == '+' || nextInstruction == '-' || nextInstruction == '>' || nextInstruction == '<'))
        //                instructionCount++;
        //            else
        //                break;
        //        }
        //        if (instructionCount > 1)
        //        {
        //            switch (instruction)
        //            {
        //                case '+':
        //                    addCount++;
        //                    break;
        //                case '-':
        //                    subCount++;
        //                    break;
        //                case '<':
        //                    leftCount++;
        //                    break;
        //                case '>':
        //                    rightCount++;
        //                    break;
        //            }
        //        }
        //    }
        //    if (addCount > 0)
        //        results.Add($"Multiple consecutive +: {addCount}");
        //    if (subCount > 0)
        //        results.Add($"Multiple consecutive -: {subCount}");
        //    if (leftCount > 0)
        //        results.Add($"Multiple consecutive <: {leftCount}");
        //    if (rightCount > 0)
        //        results.Add($"Multiple consecutive >: {rightCount}");

        //    return results;
        //}

        public void Parse(string input)
        {
            // clear unwanted characters
            string program = new string(input.ToCharArray().Where(x => AllowedCharacters.Contains(x)).ToArray());

            // search clear loop [-] and replace them with precompiler instruction C
            program = program.Replace("[-]", "C");
            // search for scan left loop [<] and replace them with precompiler instruction L
            program = program.Replace("[<]", "L");
            // search for scan right loop [>] and replace them with precompiler instruction R
            program = program.Replace("[>]", "R");

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
                    default:
                        throw new InvalidOperationException($"Unknown instruction {instruction}");
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

        public string ToBrainfuckStatement(int lineLength = 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (InstructionBase instruction in Instructions)
                sb.Append(instruction.ToBrainfuckStatement());
            string s = sb.ToString();
            if (lineLength > 0)
                return string.Join(Environment.NewLine, s.SplitInParts(lineLength));
            return s;
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

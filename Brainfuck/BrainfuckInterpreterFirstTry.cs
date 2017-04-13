using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Brainfuck
{
    public class BrainfuckInterpreterFirstTry
    {
        private static void RleOptimizedInterpreter(string program)
        {
            // instructions mapping
            // 0 ->15: INC*x (x: 1->16)
            // 16->31: DEC*x (x: 1->16)
            // 32->47: PTR++*x (x: 1->16)
            // 48->63: PTR--*x (x: 1->16)
            // 64: [
            // 65: ]
            // 66: .
            // 67: ,
            // 255: end of program
            byte[] instructions = new byte[program.Length + 1]; // RLE compressed will never be longer than original program (+1 for end-of-program instruction)
            int[] loopIndex = new int[65536];

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
                    if (nextInstruction == instruction && instructionCount < 15 && (nextInstruction == '+' || nextInstruction == '-' || nextInstruction == '>' || nextInstruction == '<'))
                        instructionCount++;
                    else
                        break;
                }
                // instruction + count found
                switch (instruction)
                {
                    case '[':
                        loopStack.Push(instructionPtr);
                        instructions[instructionPtr++] = 64;
                        break;
                    case ']':
                        if (loopStack.Count == 0)
                        {
                            Console.WriteLine($"Unmatched ']' at position {programPtr}");
                            Debug.WriteLine($"Unmatched ']' at position {programPtr}");
                            return;
                        }
                        int loopStart = loopStack.Pop(); // take matching '[' from the stack,
                        loopIndex[instructionPtr] = loopStart; // save it as the match for the current ']',
                        loopIndex[loopStart] = instructionPtr; // and save the current ']' as the match for it
                        instructions[instructionPtr++] = 65;
                        break;
                    case '+':
                        instructions[instructionPtr++] = (byte)(instructionCount - 1);
                        break;
                    case '-':
                        instructions[instructionPtr++] = (byte)(15 + instructionCount);
                        break;
                    case '>':
                        instructions[instructionPtr++] = (byte)(31 + instructionCount);
                        break;
                    case '<':
                        instructions[instructionPtr++] = (byte)(47 + instructionCount);
                        break;
                    case '.':
                        instructions[instructionPtr++] = 66;
                        break;
                    case ',':
                        instructions[instructionPtr++] = 67;
                        break;
                }
            }
            if (loopStack.Count > 0)
            {
                Console.WriteLine($"Unmatched ']' at position {loopStack.Peek()}"); // TODO: store programPtr+instructionPtr to allow nice display
                Debug.WriteLine($"Unmatched ']' at position {loopStack.Peek()}");
            }
            instructions[instructionPtr] = 255;

            Debug.WriteLine($"RLE optimization: {program.Length} instructions -> {instructionPtr} instructions");

            // interpret program
            byte[] memory = new byte[65536];
            int memoryPtr = 0;
            instructionPtr = 0;
            while (true)
            {
                byte instruction = instructions[instructionPtr];
                if (instruction < 16)
                {
                    Debug.WriteLine($"*ptr+={instruction + 1}");
                    memory[memoryPtr] += (byte)(instruction + 1);
                }
                else if (instruction >= 16 && instruction < 32)
                {
                    Debug.WriteLine($"*ptr-={instruction - 15}");
                    memory[memoryPtr] -= (byte)(instruction - 15);
                }
                else if (instruction >= 32 && instruction < 48)
                {
                    Debug.WriteLine($"ptr+={instruction - 31}");
                    memoryPtr += instruction - 31;
                }
                else if (instruction >= 48 && instruction < 64)
                {
                    Debug.WriteLine($"ptr-={instruction - 47}");
                    memoryPtr -= instruction - 47;
                }
                else if (instruction == 64)
                {
                    Debug.WriteLine("while(*ptr){");
                    if (memory[memoryPtr] == 0)
                    {
                        Debug.WriteLine("Jump forward");
                        instructionPtr = loopIndex[instructionPtr];
                    }
                }
                else if (instruction == 65)
                {
                    Debug.WriteLine("}");
                    if (memory[memoryPtr] != 0)
                        instructionPtr = loopIndex[instructionPtr];
                }
                else if (instruction == 66)
                {
                    Debug.WriteLine("putchar(*ptr)");
                    Console.Write((char)memory[memoryPtr]);
                }
                else if (instruction == 67)
                {
                    Debug.WriteLine("*ptr=getchar()");
                    memory[memoryPtr] = (byte)Console.ReadKey().KeyChar;
                }
                else
                    return;
                instructionPtr++;
            }
        }

        private static void JumpOptimizedInterpreter(string program)
        {
            int[] loopIndex = new int[65536];
            byte[] array = new byte[65536];
            int arrayPtr = 0;

            // Preprocess loop index
            Stack<int> loopProgramPtrStack = new Stack<int>();
            for (int programPtr = 0; programPtr < program.Length; programPtr++)
            {
                char instruction = program[programPtr];
                if (instruction == '[')
                    loopProgramPtrStack.Push(programPtr);
                else if (instruction == ']')
                {
                    if (loopProgramPtrStack.Count == 0)
                    {
                        Console.WriteLine($"Unmatched ']' at position {programPtr}");
                        Debug.WriteLine($"Unmatched ']' at position {programPtr}");
                        return;
                    }
                    int loopStart = loopProgramPtrStack.Pop(); // take matching '[' from the stack,
                    loopIndex[programPtr] = loopStart; // save it as the match for the current ']',
                    loopIndex[loopStart] = programPtr; // and save the current ']' as the match for it
                }
            }
            if (loopProgramPtrStack.Count > 0)
            {
                Console.WriteLine($"Unmatched ']' at position {loopProgramPtrStack.Peek()}");
                Debug.WriteLine($"Unmatched ']' at position {loopProgramPtrStack.Peek()}");
            }
            // Interpret program
            for (int programPtr = 0; programPtr < program.Length; programPtr++)
            {
                char instruction = program[programPtr];
                switch (instruction)
                {
                    case '>':
                        Debug.WriteLine("++ptr");
                        arrayPtr++;
                        break;
                    case '<':
                        Debug.WriteLine("--ptr");
                        arrayPtr--;
                        break;
                    case '+':
                        Debug.WriteLine("++*ptr");
                        array[arrayPtr]++;
                        break;
                    case '-':
                        Debug.WriteLine("--*ptr");
                        array[arrayPtr]--;
                        break;
                    case '.':
                        Debug.WriteLine("putchar(*ptr)");
                        Console.Write((char)array[arrayPtr]);
                        break;
                    case ',':
                        Debug.WriteLine("*ptr=getchar()");
                        array[arrayPtr] = (byte)Console.ReadKey().KeyChar;
                        break;
                    case '[':
                        Debug.WriteLine("while(*ptr){");
                        if (array[arrayPtr] == 0)
                        {
                            Debug.WriteLine("Jump forward");
                            programPtr = loopIndex[programPtr];
                        }
                        break;
                    case ']':
                        Debug.WriteLine("}");
                        if (array[arrayPtr] != 0)
                            programPtr = loopIndex[programPtr];
                        break;
                }
            }
        }

        private static void NaiveInterpreter(string program)
        {
            byte[] array = new byte[65536];
            int arrayPtr = 0;

            Stack<int> loopIndex = new Stack<int>();
            int programPtr = 0;
            while (true)
            {
                if (arrayPtr < 0 || arrayPtr >= array.Length)
                {
                    Console.WriteLine("Ptr is out of range");
                    return;
                }
                if (programPtr >= program.Length)
                    return; // end of program
                char instruction = program[programPtr];
                switch (instruction)
                {
                    case '>':
                        Debug.WriteLine("++ptr");
                        arrayPtr++;
                        break;
                    case '<':
                        Debug.WriteLine("--ptr");
                        arrayPtr--;
                        break;
                    case '+':
                        Debug.WriteLine("++*ptr");
                        array[arrayPtr]++;
                        break;
                    case '-':
                        Debug.WriteLine("--*ptr");
                        array[arrayPtr]--;
                        break;
                    case '.':
                        Debug.WriteLine("putchar(*ptr)");
                        Console.Write((char)array[arrayPtr]);
                        break;
                    case ',':
                        Debug.WriteLine("*ptr=getchar()");
                        array[arrayPtr] = (byte)Console.ReadKey().KeyChar;
                        break;
                    case '[':
                        Debug.WriteLine("while(*ptr){");
                        if (array[arrayPtr] == 0) // jump forward to command after matching ]
                        {
                            Debug.WriteLine("Jump forward");
                            int loopCount = 1;
                            while (loopCount > 0)
                            {
                                programPtr++;
                                if (programPtr >= program.Length)
                                {
                                    Debug.WriteLine("Unexpected end of program");
                                    Console.WriteLine("Unexpected end of program");
                                    return;
                                }
                                if (program[programPtr] == '[')
                                    loopCount++;
                                else if (program[programPtr] == ']')
                                    loopCount--;
                            }
                        }
                        else
                            loopIndex.Push(programPtr);
                        break;
                    case ']':
                        Debug.WriteLine("}");
                        if (array[arrayPtr] != 0)
                            programPtr = loopIndex.Peek();
                        else
                            loopIndex.Pop();
                        break;
                }
                programPtr++;
            }
        }
    }
}

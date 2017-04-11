using System;
using System.Collections.Generic;
using System.Linq;
using Brainfuck.Instructions;

namespace Brainfuck
{
    public class BrainfuckInterpreterTest
    {
        public List<InstructionBase> ToIntermediateRepresentation(string input)
        {
            List<InstructionBase> instructions = new List<InstructionBase>();
            for (int i = 0; i < input.Length; i++)
            {
                char instruction = input[i];
                switch (instruction)
                {
                    case '+':
                        instructions.Add(new AddInstruction
                        {
                            Shift = 0,
                            X = 1
                        });
                        break;
                    case '-':
                        instructions.Add(new SubInstruction
                        {
                            Shift = 0,
                            X = 1
                        });
                        break;
                    case '>':
                        instructions.Add(new RightInstruction
                        {
                            X = 1
                        });
                        break;
                    case '<':
                        instructions.Add(new LeftInstruction
                        {
                            X = 1
                        });
                        break;
                    case '[':
                        instructions.Add(new OpenInstruction());
                        break;
                    case ']':
                        instructions.Add(new CloseInstruction());
                        break;
                    case ',':
                        instructions.Add(new InInstruction());
                        break;
                    case '.':
                        instructions.Add(new OutInstruction());
                        break;
                }
            }
            return instructions;
        }

        // Replace clear loop [-] with single instruction
        public List<InstructionBase> OptimizeClearLoop(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>();

            for (int i = 0; i < instructions.Count; i++)
            {
                InstructionBase instruction = instructions[i];
                bool optimizationFound = false;
                if (instruction is OpenInstruction && i < instructions.Count-2)
                {
                    SubInstruction sub = instructions[i+1] as SubInstruction;
                    if (sub?.X == 1 && sub.Shift == 0 && instructions[i + 2] is CloseInstruction)
                    {
                        optimizationFound = true;
                        optimized.Add(new ClearInstruction());
                        i += 2;
                    }
                }
                if (!optimizationFound)
                    optimized.Add(instruction);
            }

            return optimized;
        }

        // Contracts multiple Add, Sub, Left and Right into a single instruction
        public List<InstructionBase> OptimizeContract(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>
            {
                instructions[0]
            };

            for (int i = 1; i < instructions.Count ; i++)
            {
                InstructionBase previous = optimized.Last();
                InstructionBase instruction = instructions[i];
                // TODO: check offset
                // TODO: common instruction type: ValueInstructionBase
                if (instruction is AddInstruction && previous is AddInstruction)
                    (optimized.Last() as AddInstruction).X += (instruction as AddInstruction).X;
                else if (instruction is SubInstruction && previous is SubInstruction)
                    (optimized.Last() as SubInstruction).X += (instruction as SubInstruction).X;
                else if (instruction is LeftInstruction && previous is LeftInstruction)
                    (optimized.Last() as LeftInstruction).X += (instruction as LeftInstruction).X;
                else if (instruction is RightInstruction && previous is RightInstruction)
                    (optimized.Last() as RightInstruction).X += (instruction as RightInstruction).X;
                else
                    optimized.Add(instruction);
            }

            return optimized;
        }
    }
}

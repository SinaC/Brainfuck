using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Brainfuck.Instructions;

namespace Brainfuck
{
    public class BrainfuckInterpreterOptimizer
    {
        public void Execute(List<InstructionBase> instructions)
        {
            // Set open/close instruction ptr
            Stack<int> loopStack = new Stack<int>();
            for (int instructionPtr = 0; instructionPtr < instructions.Count; instructionPtr++)
            {
                InstructionBase instruction = instructions[instructionPtr];
                if (instruction is OpenInstruction)
                    loopStack.Push(instructionPtr); // save OpenInstructionPtr, will be set later
                else if (instruction is CloseInstruction)
                {
                    int loopStartPtr = loopStack.Pop();
                    ((CloseInstruction)instruction).OpenInstructionPtr = loopStartPtr; // set OpenInstructionPtr to previously saved ptr
                    ((OpenInstruction)instructions[loopStartPtr]).CloseInstructionPtr = instructionPtr; // set CloseInstructionPtr to current ptr
                }
            }
            //
            byte[] memory = new byte[65536];
            int memoryPtr = 0;
            int ip = 0;
            while (true)
            {
                if (ip >= instructions.Count)
                    break; // stop program
                InstructionBase instruction = instructions[ip];
                if (instruction == null)
                    throw new InvalidOperationException("Null instruction found");
                instruction.Execute(memory, ref memoryPtr, ref ip);
                ip++;
            }
        }

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
                            Offset = 0,
                            X = 1
                        });
                        break;
                    case '-':
                        instructions.Add(new SubInstruction
                        {
                            Offset = 0,
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
                if (instruction is OpenInstruction && i < instructions.Count - 2)
                {
                    SubInstruction sub = instructions[i + 1] as SubInstruction;
                    if (sub?.X == 1 && sub.Offset == 0 && instructions[i + 2] is CloseInstruction)
                    {
                        Debug.WriteLine($"Clear loop found at position {i}");
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

        // Replace scan loop [<] and [>] with single instruction
        public List<InstructionBase> OptimizeScanLoop(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>();

            for (int i = 0; i < instructions.Count; i++)
            {
                InstructionBase instruction = instructions[i];
                bool optimizationFound = false;
                if (instruction is OpenInstruction && i < instructions.Count - 2)
                {
                    LeftInstruction left = instructions[i + 1] as LeftInstruction;
                    if (left?.X == 1 && instructions[i + 2] is CloseInstruction)
                    {
                        Debug.WriteLine($"Left scan loop found at position {i}");
                        optimizationFound = true;
                        optimized.Add(new ScanLeftInstruction());
                        i += 2;
                    }
                    else
                    {
                        RightInstruction right = instructions[i + 1] as RightInstruction;
                        if (right?.X == 1 && instructions[i + 2] is CloseInstruction)
                        {
                            Debug.WriteLine($"Right scan loop found at position {i}");
                            optimizationFound = true;
                            optimized.Add(new ScanRightInstruction());
                            i += 2;
                        }
                    }
                }
                if (!optimizationFound)
                    optimized.Add(instruction);
            }

            return optimized;
        }

        // !! Following method modify input parameter instruction !!
        // Contract multiple Add, Sub, Left and Right into a single instruction
        //>>>+++<<<--- can be contracted into Right(3), Add(3), Left(3), Sub(3)
        public List<InstructionBase> OptimizeContract(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>
            {
                instructions[0]
            };

            for (int i = 1; i < instructions.Count ; i++)
            {
                InstructionBase previous = optimized.Last();
                InstructionBase current = instructions[i];

                if (previous.GetType() == current.GetType())
                {
                    ArithmeticInstructionBase previousArithmetic = previous as ArithmeticInstructionBase;
                    ArithmeticInstructionBase currentArithmetic = current as ArithmeticInstructionBase;
                    if (previousArithmetic?.Offset == 0 && currentArithmetic?.Offset == 0)
                        previousArithmetic.X += currentArithmetic.X;
                    else
                    {
                        PositionInstructionBase previousShift = previous as PositionInstructionBase;
                        PositionInstructionBase currentShift = current as PositionInstructionBase;
                        if (previousShift != null && currentShift != null)
                            previousShift.X += currentShift.X;
                        else
                            optimized.Add(current);
                    }
                }
                else
                    optimized.Add(current);
            }

            return optimized;
        }

        // !! Following method modify input parameter instruction !!
        // Cancel out adjacent Add, Sub and Left Right
        //+++-->>+-<<< is equivalent to +<
        public List<InstructionBase> OptimizeCancel(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>();
            foreach (InstructionBase current in instructions)
            {
                if (optimized.Count == 0)
                    optimized.Add(current);
                else
                {
                    InstructionBase previous = optimized.Last();

                    ArithmeticInstructionBase previousArithmetic = previous as ArithmeticInstructionBase;
                    ArithmeticInstructionBase currentArithmetic = current as ArithmeticInstructionBase;
                    if (previousArithmetic?.Offset == 0 && currentArithmetic?.Offset == 0
                        && previousArithmetic.GetType() == currentArithmetic.OppositeType)
                    {
                        int x = previousArithmetic.X - currentArithmetic.X; // !!! possible overflow
                        if (x < 0)
                        {
                            if (previous is AddInstruction)
                                optimized[optimized.Count-1] = new SubInstruction
                                {
                                    X = (byte)-x
                                };
                            else
                                optimized[optimized.Count-1] = new AddInstruction
                                {
                                    X = (byte)-x
                                };
                        }
                        else if (x > 0)
                            previousArithmetic.X = (byte) x;
                        else
                            optimized.RemoveAt(optimized.Count - 1);
                    }
                    else
                    {
                        PositionInstructionBase previousShift = previous as PositionInstructionBase;
                        PositionInstructionBase currentShift = current as PositionInstructionBase;
                        if (previousShift != null && currentShift != null
                            && previousShift.GetType() == currentShift.OppositeType)
                        {
                            int x = previousShift.X - currentShift.X;
                            if (x < 0)
                            {
                                if (previous is LeftInstruction)
                                    optimized[optimized.Count - 1] = new RightInstruction
                                    {
                                        X = -x
                                    };
                                else
                                    optimized[optimized.Count - 1] = new LeftInstruction
                                    {
                                        X = -x
                                    };
                            }
                            else if (x > 0)
                                previousShift.X = x;
                            else
                                optimized.RemoveAt(optimized.Count - 1);
                        }
                        else
                            optimized.Add(current);
                    }
                }
            }

            return optimized;
        }

        // Replaces copy and multiplication loops with Mul (clear loop [-] will also be replaced with Clear)
        public List<InstructionBase> OptimizeCopyMultiplyLoop(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>();

            int i = 0;
            while (true)
            {
                int previousI = i;

                // Find next leaf loop (loop without inner loop)
                bool found = false;
                while (i < instructions.Count)
                {
                    if (instructions[i] is OpenInstruction)
                    {
                        found = true;
                        break;
                    }
                    i++;
                }
                if (!found) // no Open found
                {
                    optimized.AddRange(instructions.Skip(previousI)); // copy remaining instructions
                    break;
                }
                int j = i + 1;
                found = false;
                while (j < instructions.Count)
                {
                    if (instructions[j] is CloseInstruction)
                    {
                        found = true;
                        break;
                    }
                    if (instructions[j] is OpenInstruction)
                        i = j;
                    j++;
                }
                if (!found) // no leaf loop found
                {
                    optimized.AddRange(instructions.Skip(previousI)); // copy remaining instructions
                    break;
                }
                // copy instructions before loop
                optimized.AddRange(instructions.Skip(previousI).Take(i-previousI));
                // check it contains only Add, Sub, Left, Right
                List<InstructionBase> loopInstructions = instructions.Skip(i + 1).Take(j - i - 1).ToList();
                bool ok = loopInstructions.All(x => x is ArithmeticInstructionBase || x is PositionInstructionBase);
                if (!ok)
                {
                    optimized.AddRange(instructions.Skip(i).Take(j-i+1)); // copy loop instructions
                    i = j + 1;
                    continue; // search another loop
                }
                // interpret loop and track pointer position and what arithmetic operations it carries out
                Dictionary<int, int> mem = new Dictionary<int, int>();
                int p = 0;
                foreach (InstructionBase instruction in loopInstructions)
                {
                    if (instruction is AddInstruction)
                    {
                        AddInstruction add = (AddInstruction) instruction;
                        mem[p + add.Offset] = Get(mem, p, 0) + add.X;
                    }
                    else if (instruction is SubInstruction)
                    {
                        SubInstruction sub = (SubInstruction) instruction;
                        mem[p + sub.Offset] = Get(mem, p, 0) - sub.X;
                    }
                    else if (instruction is LeftInstruction)
                    {
                        LeftInstruction left = (LeftInstruction) instruction;
                        p -= left.X;
                    }
                    else // Right
                    {
                        RightInstruction right = (RightInstruction) instruction;
                        p += right.X;
                    }
                }
                // if pointed ended where it started and we substracted exactly 1 from cell 0, then loop can be replaced with copy/mul instruction
                if (p != 0 || Get(mem, 0, 0) != -1) // not a copy/mul instruction
                {
                    optimized.AddRange(instructions.Skip(i).Take(j - i + 1)); // copy loop instructions
                    i = j+1;
                    continue;
                }
                mem.Remove(0);
                // add Mul operations
                foreach (KeyValuePair<int, int> kv in mem)
                {
                    optimized.Add(new MulInstruction
                    {
                        Factor = (byte)kv.Value,
                        Offset = kv.Key,
                    });
                }
                optimized.Add(new ClearInstruction());
                i = j + 1;
            }

            return optimized;
        }

        private int Get(Dictionary<int, int> dict, int key, int defaultValue)
        {
            int value;
            if (!dict.TryGetValue(key, out value))
                value = defaultValue;
            return value;
        }
    }
}

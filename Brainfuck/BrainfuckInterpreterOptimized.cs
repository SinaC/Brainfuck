using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Brainfuck.Instructions;

namespace Brainfuck
{
    // TODO: store position in original program (2 positions when instructions are merged)
    public class BrainfuckInterpreterOptimized
    {
        public void Execute(List<InstructionBase> instructions)
        {
            // Set open/close instruction ptr
            Stack<int> loopStack = new Stack<int>();
            for (int instructionPtr = 0; instructionPtr < instructions.Count; instructionPtr++)
            {
                InstructionBase instruction = instructions[instructionPtr];
                switch (instruction)
                {
                    case OpenInstruction _:
                        loopStack.Push(instructionPtr); // save OpenInstructionPtr, will be set later
                        break;
                    case CloseInstruction _ when loopStack.Count == 0:
                        throw new InvalidOperationException($"Unmatched CloseInstruction at position {instructionPtr}.");
                    case CloseInstruction closeInstruction:
                    {
                        int loopStartPtr = loopStack.Pop();
                        closeInstruction.OpenInstructionPtr = loopStartPtr; // set OpenInstructionPtr to previously saved ptr
                        ((OpenInstruction)instructions[loopStartPtr]).CloseInstructionPtr = instructionPtr; // set CloseInstructionPtr to current ptr
                        break;
                    }
                }
            }
            if (loopStack.Count > 0)
                throw new InvalidOperationException($"Unmatched OpenInstruction at position { loopStack.Peek()}.");
            //
            byte[] memory = new byte[65536];
            int memoryPtr = 32765; // starts in the middle
            int ip = 0;
            while (true)
            {
                if (ip >= instructions.Count)
                    break; // stop program
                InstructionBase instruction = instructions[ip];
                instruction.Execute(memory, ref memoryPtr, ref ip);
                ip++;
            }
        }

        public List<InstructionBase> Parse(string input)
        {
            List<InstructionBase> instructions = new List<InstructionBase>();
            Stack<int> loopStack = new Stack<int>();
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
                        loopStack.Push(i);
                        instructions.Add(new OpenInstruction());
                        break;
                    case ']':
                        if (loopStack.Count == 0)
                            throw new InvalidOperationException($"Unmatched ']' at position {i}.");
                        loopStack.Pop();
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
            if (loopStack.Count > 0)
                throw new InvalidOperationException($"Unmatched '[' at position { loopStack.Peek()}.");
            return instructions;
        }

        public string ToCStatements(List<InstructionBase> instructions)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#include <stdio.h>");
            sb.AppendLine("#include <string.h>");
            sb.AppendLine("unsigned char mem[65536];");
            sb.AppendLine("int main() {");
            sb.AppendLine("   int p = 32767;");
            int indent = 1;
            foreach (InstructionBase instruction in instructions)
            {
                if (instruction is CloseInstruction)
                    indent--;
                sb.AppendLine("".PadLeft(indent * 3) + instruction.ToCStatement());
                if (instruction is OpenInstruction)
                    indent++;
            }
            sb.AppendLine("   return 0;");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string ToIntermediateRepresentation(List<InstructionBase> instructions)
        {
            StringBuilder sb = new StringBuilder();
            foreach (InstructionBase instruction in instructions)
                sb.AppendLine(instruction.ToIntermediateRepresentation());
            return sb.ToString();
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
                        Debug.WriteLine("Clear loop found");
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
                        Debug.WriteLine("Left scan loop found");
                        optimizationFound = true;
                        optimized.Add(new ScanLeftInstruction());
                        i += 2;
                    }
                    else
                    {
                        RightInstruction right = instructions[i + 1] as RightInstruction;
                        if (right?.X == 1 && instructions[i + 2] is CloseInstruction)
                        {
                            Debug.WriteLine("Right scan loop found");
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

        // Replaces copy and multiplication loops with Mul (clear loop [-] will also be replaced with Clear)
        // [->++>>>+++++>++<+<<<<<] becomes Mul(1,2) Mul(4,5) Mul(5,2) Mul(6,1) Clear(0)
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
                foreach (InstructionBase instruction in loopInstructions) // contains only Add/Sub/Left/Right
                {
                    switch (instruction)
                    {
                        // Add
                        case AddInstruction add:
                            mem[p+add.Offset] = mem.Get(p + add.Offset, 0) + add.X;
                            break;
                        // Sub
                        case SubInstruction sub:
                            mem[p + sub.Offset] = mem.Get(p + sub.Offset, 0) - sub.X;
                            break;
                        // Left
                        case LeftInstruction left:
                            p -= left.X;
                            break;
                        // Right
                        case RightInstruction right:
                            p += right.X;
                            break;
                    }
                }
                // if pointed ended where it started and we subtracted exactly 1 from cell 0, then loop can be replaced with copy/mul instruction
                if (p != 0 || mem.Get(0, 0) != -1) // not a copy/mul instruction
                {
                    optimized.AddRange(instructions.Skip(i).Take(j - i + 1)); // copy loop instructions
                    i = j+1;
                    continue;
                }
                mem.Remove(0);
                Debug.Write($"Replacing loop {string.Join(" ", loopInstructions.Select(x => x.ToIntermediateRepresentation()))} with ");
                // add Mul operations
                foreach (KeyValuePair<int, int> kv in mem)
                {
                    MulInstruction mulInstruction = new MulInstruction
                    {
                        Factor = (short) kv.Value,
                        Offset = kv.Key,
                    };
                    optimized.Add(mulInstruction);
                    Debug.Write($"{mulInstruction.ToIntermediateRepresentation()}, ");
                }
                // add Clear operation
                ClearInstruction clearInstruction = new ClearInstruction();
                optimized.Add(clearInstruction);
                Debug.WriteLine(clearInstruction.ToIntermediateRepresentation());
                //
                i = j + 1;
            }

            return optimized;
        }

        // Add offset to operations where applicable (Add, Sub, In, Out, Clear)
        // >>>++.>>->> becomes Add(3,2), Out(3), Sub(5,1), Right(7)
        public List<InstructionBase> OptimizeOffset(List<InstructionBase> instructions)
        {
            List<InstructionBase> optimized = new List<InstructionBase>();

            int i = 0;
            while (i < instructions.Count)
            {
                int previousI = i;
                // find next block of Add, Sub, Left, Right, In, Out
                while (i < instructions.Count)
                {
                    InstructionBase instruction = instructions[i];
                    if (instruction is ArithmeticInstructionBase || instruction is PositionInstructionBase || instruction is ClearInstruction || instruction is InInstruction || instruction is OutInstruction)
                        break;
                    i++;
                }
                if (i >= instructions.Count)
                {
                    optimized.AddRange(instructions.Skip(previousI)); // copy remaining instructions
                    break;
                }
                int j = i+1;
                while (j < instructions.Count)
                {
                    InstructionBase instruction = instructions[j];
                    if (!(instruction is ArithmeticInstructionBase || instruction is PositionInstructionBase || instruction is ClearInstruction || instruction is InInstruction || instruction is OutInstruction))
                        break;
                    j++;
                }
                Debug.WriteLine($"Optimizable block detected at position {i} => {j}");
                // copy instructions before block
                optimized.AddRange(instructions.Skip(previousI).Take(i - previousI));
                // interpret block and track what arithmetic operations are applied to each offset, as soon a non-arithmetic operation is encountered, we dump the arithmetic operations performed on that offset followed by non-arithmetic operation
                List<InstructionBase> blockInstructions = instructions.Skip(i).Take(j - i).ToList();
                Dictionary<int, int> memValue = new Dictionary<int, int>();
                Dictionary<int, bool> memOperation = new Dictionary<int, bool>(); // true: ADD/SUB  false: ASSIGN (CLEAR + ADD/SUB)
                int p = 0;
                foreach (InstructionBase instruction in blockInstructions)
                {
                    if (instruction is AddInstruction add)
                    {
                        memValue[p + add.Offset] = memValue.Get(p + add.Offset, 0) + add.X;
                        memOperation[p + add.Offset] = memOperation.Get(p + add.Offset, true); // keep existing value or set true (ADD/SUB) by default
                    }
                    else if (instruction is SubInstruction sub)
                    {
                        memValue[p + sub.Offset] = memValue.Get(p + sub.Offset, 0) - sub.X;
                        memOperation[p + sub.Offset] = memOperation.Get(p + sub.Offset, true); // keep existing value or set true (ADD/SUB) by default
                    }
                    else if (instruction is ClearInstruction clear)
                    {
                        memValue[p + clear.Offset] = 0;
                        memOperation[p + clear.Offset] = false; // set false (ASSIGN)
                    }
                    else if (instruction is LeftInstruction left)
                    {
                        p -= left.X;
                    }
                    else if (instruction is RightInstruction right)
                    {
                        p += right.X;
                    }
                    else if (instruction is InInstruction || instruction is OutInstruction) // If in/out is encountered, we have to stop Add/Sub merging because value will be used
                    {
                        // Dump instruction at p + i.Offset
                        InInstruction inInstruction = instruction as InInstruction;
                        OutInstruction outInstruction = instruction as OutInstruction;
                        int offset = p + (inInstruction?.Offset ?? outInstruction.Offset);
                        bool operation = memOperation.Get(offset, true);
                        if (!operation) // Clear if ASSIGN
                            optimized.Add(new ClearInstruction
                            {
                                Offset = offset
                            });
                        // Add/Sub
                        int value = memValue.Get(offset, 0);
                        if (value > 0)
                            optimized.Add(new AddInstruction
                            {
                                X = (byte)value,
                                Offset = offset
                            });
                        else if (value < 0)
                            optimized.Add(new SubInstruction
                            {
                                X = (byte)-value,
                                Offset = offset
                            });
                        // In/Out
                        if (inInstruction != null)
                            optimized.Add(new InInstruction
                            {
                                Offset = offset
                            });
                        else
                            optimized.Add(new OutInstruction
                            {
                                Offset = offset
                            });
                        // Clear memory for this offset
                        memValue.Remove(offset);
                        memOperation.Remove(offset);
                    }
                }
                // Dump instructions from memorized/compressed operations
                foreach (KeyValuePair<int, int> kv in memValue)
                {
                    int offset = kv.Key;
                    bool operation = memOperation.Get(kv.Key, true);
                    if (!operation)
                        optimized.Add(new ClearInstruction
                        {
                            Offset = offset
                        });
                    // Add/Sub
                    int value = kv.Value;
                    if (value > 0)
                        optimized.Add(new AddInstruction
                        {
                            X = (byte)value,
                            Offset = offset
                        });
                    else if (value < 0)
                        optimized.Add(new SubInstruction
                        {
                            X = (byte)-value,
                            Offset = offset
                        });
                }
                // Dump remaining Left/Right
                if (p > 0)
                    optimized.Add(new RightInstruction
                    {
                        X = p
                    });
                else if (p < 0)
                    optimized.Add(new LeftInstruction
                    {
                        X = -p
                    });

                i = j; // +1 has already been done while searching block end
            }

            return optimized;
        }

        // Following commented methods (crappy code) are handled by OptimizeCopyMultiplyLoop and OptimizeOffset
        /*
        // !! Following method modify input parameter instructions !!
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
                    if (previousArithmetic?.Offset == 0 && currentArithmetic?.Offset == 0) // TODO: should work if Offsets are equal   ADD(3,4) SUB(3,1) -> ADD(3,3)
                    {
                        Debug.Write($"Contracting {previousArithmetic.ToIntermediateRepresentation()} and {currentArithmetic.ToIntermediateRepresentation()}");
                        previousArithmetic.X += currentArithmetic.X;
                        Debug.WriteLine($" into {previousArithmetic.ToIntermediateRepresentation()}");
                    }
                    else
                    {
                        PositionInstructionBase previousShift = previous as PositionInstructionBase;
                        PositionInstructionBase currentShift = current as PositionInstructionBase;
                        if (previousShift != null && currentShift != null)
                        {
                            Debug.Write($"Contracting {previousShift.ToIntermediateRepresentation()} and {currentShift.ToIntermediateRepresentation()}");
                            previousShift.X += currentShift.X;
                            Debug.WriteLine($" into {previousShift.ToIntermediateRepresentation()}");
                        }
                        else
                            optimized.Add(current);
                    }
                }
                else
                    optimized.Add(current);
            }

            return optimized;
        }

        // !! Following method modify input parameter instructions !!
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
        */
    }
}

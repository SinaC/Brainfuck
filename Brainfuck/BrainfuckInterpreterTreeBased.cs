using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brainfuck.Instructions;

namespace Brainfuck
{
    public abstract class NodeBase
    {
        public NodeBase Parent { get; set; }
    }

    public abstract class WithChildrenNodeBase : NodeBase
    {
        public List<NodeBase> Children { get; set; }

        protected WithChildrenNodeBase()
        {
            Children = new List<NodeBase>();
        }
    }

    public class RootNode : WithChildrenNodeBase
    {
    }

    public class InstructionNode : NodeBase
    {
        public InstructionBase Instruction { get; set; }

        public virtual void Execute(byte[] memory, ref int memoryPtr)
        {
            int instructionPtr = 0; // not used
            Instruction.Execute(memory, ref memoryPtr, ref instructionPtr);
        }
    }

    public class LoopNode : WithChildrenNodeBase
    {
    }

    public class BrainfuckInterpreterTreeBased : IBrainfuckInterpreter
    {
        private static readonly char[] AllowedCharacters = { '+', '-', '>', '<', '.', ',', '[', ']' };

        public RootNode Root { get; } = new RootNode();

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
            WithChildrenNodeBase currentParentNode = Root;
            int loopDepthCount = 0;
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
                        loopDepthCount++;
                        LoopNode loopNode = new LoopNode
                        {
                            Children = new List<NodeBase>
                            {
                                new InstructionNode
                                {
                                    Instruction = new OpenInstruction(),
                                    Parent = currentParentNode,
                                }
                            },
                            Parent = currentParentNode
                            //--// Don't add OpenInstruction, it's implicit in LoopNode (CloseInstructionPtr will be next in PreOrder)
                        };
                        currentParentNode.Children.Add(loopNode);
                        currentParentNode = loopNode;
                        break;
                    case ']':
                        if (loopDepthCount == 0)
                            throw new InvalidOperationException($"Unmatched ']' at position {programPtr}.");
                        loopDepthCount--;
                        //--// Don't add CloseInstruction, it's implicit in LoopNode (OpenInstructionPtr is Parent.Children.First())
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new CloseInstruction(),
                            Parent = currentParentNode
                        });
                        currentParentNode = (WithChildrenNodeBase) currentParentNode.Parent;
                        break;
                    case '+':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new AddInstruction
                            {
                                X = (byte) instructionCount
                            },
                            Parent = currentParentNode
                        });
                        break;
                    case '-':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new SubInstruction
                            {
                                X = (byte) instructionCount
                            },
                            Parent = currentParentNode
                        });
                        break;
                    case '>':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new RightInstruction
                            {
                                X = (byte) instructionCount
                            },
                            Parent = currentParentNode
                        });
                        break;
                    case '<':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new LeftInstruction
                            {
                                X = (byte) instructionCount
                            },
                            Parent = currentParentNode
                        });
                        break;
                    case '.':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new OutInstruction(),
                            Parent = currentParentNode
                        });
                        break;
                    case ',':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new InInstruction(),
                            Parent = currentParentNode
                        });
                        break;
                    // precompiler-instruction
                    case 'C':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new ClearInstruction(),
                            Parent = currentParentNode
                        });
                        break;
                    case 'L':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new ScanLeftInstruction(),
                            Parent = currentParentNode
                        });
                        break;
                    case 'R':
                        currentParentNode.Children.Add(new InstructionNode
                        {
                            Instruction = new ScanRightInstruction(),
                            Parent = currentParentNode
                        });
                        break;
                }
            }
            if (loopDepthCount > 0)
                throw new InvalidOperationException("Unmatched ']'"); // TODO: display original programPtr

            // Search copy loop and multiplication loop
            Stack<LoopNode> nodes = new Stack<LoopNode>();
            foreach (LoopNode loopNode in Root.Children.OfType<LoopNode>()) // don't need to reverse order
                nodes.Push(loopNode);
            while (nodes.Count > 0)
            {
                LoopNode loopNode = nodes.Pop();
                if (loopNode.Children.OfType<LoopNode>().Any())
                {
                    foreach (LoopNode node in loopNode.Children.OfType<LoopNode>()) // don't need to reverse order
                        nodes.Push(node);
                }
                else if (loopNode.Children.Count >= 6 && loopNode.Children.OfType<InstructionNode>().Skip(1).Take(loopNode.Children.Count-2)/*skip Open/Close*/.All(x => x.Instruction is AddInstruction || x.Instruction is SubInstruction || x.Instruction is LeftInstruction || x.Instruction is RightInstruction)) // terminal loop node (with only +-<>), potentially optimisable
                {
                    List<InstructionNode> sequence = loopNode.Children.OfType<InstructionNode>().Skip(1).Take(loopNode.Children.Count - 2).ToList();
                    // copy loop
                    // [>+<-] or [->+<] => mem[p+1] += mem[p] and mem[p] = 0
                    // [>>+<<-] or [->>+<<] => mem[p+2] += mem[p] and mem[p] = 0
                    // [>>>>>>>>>+<<<<<<<<<-] or [->>>>>>>>>+<<<<<<<<<] => mem[p+9] += mem[p] and mem[p] = 0
                    // can be more complex
                    // [->+>+<<] => mem[p+1] += mem[p] and mem[p+2] += mem[p] and mem[p] = 0
                    // [>+>>>+>+<<<<<-] => mem[p+1] += mem[p] and mem[p+4] += mem[p] and mem[p+5] += mem[p] and mem[p] = 0
                    // [->>>>+<+<+<+<] => mem[p+4] += mem[p] and mem[o+3] += mem[p] and mem[p+2] += mem[p] and mem[p+1] += mem[p] and mem[p] = 0
                    // > and < can be interchanged => mem[p-x] instead if mem[p+x]
                    // multiply loop
                    // [>+++++<-] => mem[p+1] += 5*mem[p] and mem[p] = 0
                    // [>++++>++>>>+++>+<<<<<<-] => mem[p+1] += 4*mem[p] and mem[p+2] += 2*mem[p] and mem[p+5] += 3*mem[p] and mem[p] = 0

                    // a sequence of >{+} followed by a sequence of < (same number as >) followed by a -
                    // same as - followed by a sequence of >{+} followed by a sequence of < (same number as >)

                    // a sequence of <{+} followed by a sequence of > (same number as <) followed by a -
                    // same as - followed by a sequence of <{+} followed by a sequence of > (same number as <)
                    if (sequence.First().Instruction is SubInstruction || sequence.Last().Instruction is SubInstruction)
                    {
                    }
                }
            }
        }

        public void Execute()
        {
            // Tree is only used for code optimisation
            // Set open/close instruction ptr
            List<InstructionBase> instructions = Instructions.ToList();
            Stack<int> loopStack = new Stack<int>();
            for (int instructionPtr = 0; instructionPtr < instructions.Count; instructionPtr++)
            {
                InstructionBase instruction = instructions[instructionPtr];
                if (instruction is OpenInstruction)
                    loopStack.Push(instructionPtr); // save OpenInstructionPtr, will be set later
                else if (instruction is CloseInstruction)
                {
                    int loopStartPtr = loopStack.Pop();
                    ((CloseInstruction) instruction).OpenInstructionPtr = loopStartPtr; // set OpenInstructionPtr to previously saved ptr
                    ((OpenInstruction) instructions[loopStartPtr]).CloseInstructionPtr = instructionPtr; // set CloseInstructionPtr to current ptr
                }
            }
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
                sb.AppendLine("".PadLeft(indent * 3) + instruction.ToCStatement());
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

        protected IEnumerable<InstructionBase> Instructions
        {
            get
            {
                Stack<NodeBase> nodes = new Stack<NodeBase>();
                foreach (NodeBase c in Root.Children.AsEnumerable().Reverse())
                    nodes.Push(c);
                while (nodes.Count > 0)
                {
                    NodeBase current = nodes.Pop();
                    if (current is InstructionNode)
                        yield return ((InstructionNode) current).Instruction;
                    else if (current is LoopNode)
                    {
                        foreach (NodeBase child in ((LoopNode) current).Children.AsEnumerable().Reverse())
                            nodes.Push(child);
                    }
                }
            }
        }
    }
}

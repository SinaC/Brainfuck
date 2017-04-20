using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Befunge
{
    //https://en.wikipedia.org/wiki/Befunge
    //https://github.com/catseye/Befunge-93/blob/master/doc/Befunge-93.markdown
    public class Interpreter
    {
        //!!! if stack is empty, pop will not failed and return 0
        public enum Directions
        {
            Right,
            Down,
            Left,
            Up
        }
        public char[,] Grid { get; }
        public Stack<int> Stack { get; }

        public Action<string> OutputAction { get; }
        public Func<string> InputFunc { get; }

        public Interpreter(Func<string> inputFunc, Action<string> outputAction)
        {
            Grid = new char[20, 80];
            Stack = new Stack<int>();

            InputFunc = inputFunc;
            OutputAction = outputAction;
        }

        public void Parse(string program)
        {
            // Empty grid
            ClearGrid();

            string[] lines = program.Split('\n');
            Parse(lines);
        }

        public void Parse(IEnumerable<string> program)
        {
            // Empty grid
            ClearGrid();

            // Insert program in grid
            string[] lines = program.ToArray();
            for (int j = 0; j < lines.Length; j++)
            {
                string line = lines[j];
                for (int i = 0; i < line.Length; i++)
                {
                    char instruction = line[i];
                    Grid[j, i] = instruction;
                }
            }
        }

        public void Execute()
        {
            Random random = new Random();

            int maxX = Grid.GetLength(1);
            int maxY = Grid.GetLength(0);

            int x = 0;
            int y = 0;
            Directions direction = 0;
            bool stringMode = false;
            bool skip = false;

            while (true)
            {
                // cyclic boundaries
                if (x < 0 || y < 0 || x >= maxX || y >= maxY)
                    switch (direction)
                    {
                        case Directions.Right:
                            x = 0;
                            break;
                        case Directions.Down:
                            y = 0;
                            break;
                        case Directions.Left:
                            x = maxX - 1;
                            break;
                        case Directions.Up:
                            y = maxY - 1;
                            break;
                    }
                //
                char instruction = Grid[y, x];
                Debug.WriteLine($"Instruction at {x},{y} = {instruction}  string:{stringMode} skip:{skip}");

                // skip or not
                if (skip)
                {
                    skip = false; // skip instruction
                    Debug.WriteLine("BRIDGE OFF");
                }
                // string mode or not
                else if (stringMode)
                {
                    // push instruction as string
                    if (instruction == '"')
                    {
                        Debug.WriteLine("STRING MODE OFF");
                        stringMode = false;
                    }
                    else
                    {
                        int operand = instruction;
                        Debug.WriteLine($"STRING MODE: PUSH {operand}");
                        Push(operand);
                    }
                }
                else
                {
                    switch (instruction)
                    {
                        //Push this number on the stack
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            int digit = instruction - 48;
                            Push(digit);
                            Debug.WriteLine($"PUSH DIGIT {digit}");
                            break;
                        //Addition: Pop a and b, then push a+b
                        case '+':
                        //Subtraction: Pop a and b, then push b-a
                        case '-':
                        //Multiplication: Pop a and b, then push a*b
                        case '*':
                        //Integer division: Pop a and b, then push b/a, rounded towards 0.
                        case '/':
                        //Modulo: Pop a and b, then push the remainder of the integer division of b/a.
                        case '%':
                            MathInstruction(instruction);
                            break;
                        //Logical NOT: Pop a value. If the value is zero, push 1; otherwise, push zero.
                        case '!':
                        {
                            int operand = Pop();
                            int result = operand == 0 ? 1 : 0;
                            Debug.WriteLine($"NOT({operand})={result}");
                            Push(result);
                        }
                            break;
                        //Greater than: Pop a and b, then push 1 if b>a, otherwise zero.
                        case '`':
                            {
                                int operand2 = Pop();
                                int operand1 = Pop();
                                int result = operand1 > operand2 ? 1 : 0;
                                Debug.WriteLine($"GREATER({operand1},{operand2})={result}");
                                Push(result);
                            }
                            break;
                        //Start moving right
                        case '>':
                            direction = Directions.Right;
                            Debug.WriteLine("MOVE RIGHT");
                            break;
                        //Start moving left
                        case '<':
                            direction = Directions.Left;
                            Debug.WriteLine("MOVE LEFT");
                            break;
                        //Start moving up
                        case '^':
                            direction = Directions.Up;
                            Debug.WriteLine("MOVE UP");
                            break;
                        //Start moving down
                        case 'v':
                            direction = Directions.Down;
                            Debug.WriteLine("MOVE DOWN");
                            break;
                        //Start moving in a random cardinal direction
                        case '?':
                            direction = (Directions) random.Next(4);
                            Debug.WriteLine($"MOVE RANDOM={direction}");
                            break;
                        //Pop a value; move right if value=0, left otherwise
                        case '_':
                            {
                                int operand = Pop();
                                if (operand == 0)
                                    direction = Directions.Right;
                                else
                                    direction = Directions.Left;
                                Debug.WriteLine($"CHANGE HORIZONTAL({operand})={direction}");
                            }
                            break;
                        //Pop a value; move down if value=0, up otherwise
                        case '|':
                            {
                                int operand = Pop();
                                if (operand == 0)
                                    direction = Directions.Down;
                                else
                                    direction = Directions.Up;
                                Debug.WriteLine($"CHANGE VERTICAL({operand})={direction}");
                            }
                            break;
                        //Start string mode: push each character's ASCII value all the way up to the next "
                        case '\"':
                            stringMode = true;
                            Debug.WriteLine("STRING MODE ON");
                            break;
                        //Duplicate value on top of the stack
                        case ':':
                            {
                                int operand = Pop();
                                Push(operand);
                                Push(operand);
                                Debug.WriteLine($"Action: DUP({operand})");
                            }
                            break;
                        //Swap two values on top of the stack 
                        case '\\':
                            {
                                int operand2 = Pop();
                                int operand1 = Pop();
                                Push(operand2);
                                Push(operand1);
                                Debug.WriteLine($"SWAP({operand1},{operand2})");
                            }
                            break;
                        //Pop value from the stack and discard it
                        case '$':
                            {
                                Pop(); // pop and discard
                                Debug.WriteLine("POP");
                            }
                            break;
                        //Pop value and output as an integer followed by a space
                        case '.':
                            {
                                int operand = Pop();
                                Debug.WriteLine($"OUT NUMBER({operand})");
                                OutputAction?.Invoke(operand.ToString());
                            }
                            break;
                        //Pop value and output as ASCII character
                        case ',':
                            {
                                int outputAsInt = Pop();
                                char operand = (char) (outputAsInt%255);
                                OutputAction?.Invoke(operand.ToString());
                                Debug.WriteLine($"OUT CHAR({operand})");
                            }
                            break;
                        //Bridge: Skip next cell
                        case '#':
                            skip = true;
                            Debug.WriteLine("BRIDGE ON");
                            break;
                        //A "put" call (a way to store a value for later use). Pop y, x, and v, then change the character at (x,y) in the program to the character with ASCII value v
                        case 'p':
                            {
                                int putY = Pop();
                                int putX = Pop();
                                int putValue = Pop();
                                Grid[putY, putX] = (char) putValue;
                                Debug.WriteLine($"PUT({putX},{putY})={putValue}");
                            }
                            break;
                        //A "get" call (a way to retrieve data in storage). Pop y and x, then push ASCII value of the character at that position in the program
                        case 'g':
                            {
                                int getY = Pop();
                                int getX = Pop();
                                int getValue = Grid[getY, getX];
                                Push(getValue);
                                Debug.WriteLine($"GET({getX},{getY})={getValue}");
                            }
                            break;
                        //Ask user for a number and push it
                        case '&':
                        {
                            string inputAsString = InputFunc?.Invoke();
                            if (string.IsNullOrWhiteSpace(inputAsString))
                                Debug.WriteLine("IN NUMBER failed: null or empty input");
                            else
                            {
                                int operand;
                                if (!int.TryParse(inputAsString, out operand))
                                    Debug.WriteLine($"IN NUMBER failed: invalid input {inputAsString}");
                                else
                                {
                                    Push(operand);
                                    Debug.WriteLine($"IN NUMBER({operand})");
                                }
                            }
                        }
                            break;
                        //Ask user for a character and push its ASCII value
                        case '~':
                        {
                            string inputAsString = InputFunc?.Invoke();
                            if (string.IsNullOrEmpty(inputAsString))
                                Debug.WriteLine("IN CHAR failed: null or empty input");
                            else
                            {
                                int inputChar = inputAsString[0]%255;
                                Debug.WriteLine($"IN CHAR({inputChar})");
                                Push(inputChar);
                            }
                        }
                            break;
                        //End program
                        case '@':
                            Debug.WriteLine("END OF PROGRAM");
                            return;
                        //NOP
                        case ' ':
                            Debug.WriteLine("NOP");
                            break;
                        //Unknown instruction
                        default:
                            Debug.WriteLine($"Unknown instruction:{instruction}");
                            break;
                    }
                }
                // Move instruction pointers according to direction
                switch (direction)
                {
                    case Directions.Right:
                        x += 1;
                        break;
                    case Directions.Down:
                        y += 1;
                        break;
                    case Directions.Left:
                        x -= 1;
                        break;
                    case Directions.Up:
                        y -= 1;
                        break;
                }
            }
        }

        private void MathInstruction(char instruction)
        {
            string debugInstructionString;
            if (instruction == '+')
                debugInstructionString = "ADD";
            else if (instruction == '-')
                debugInstructionString = "SUB";
            else if (instruction == '*')
                debugInstructionString = "MUL";
            else if (instruction == '/')
                debugInstructionString = "DIV";
            else if (instruction == '%')
                debugInstructionString = "MOD";
            else
                throw new InvalidOperationException($"Unknown mathematical operation: {instruction}");

            int operand2 = Pop();
            int operand1 = Pop();
            int result;
            if (instruction == '+')
                result = operand1 + operand2;
            else if (instruction == '-')
                result = operand1 - operand2;
            else if (instruction == '*')
                result = operand1 * operand2;
            else if (instruction == '/')
                result = operand1/operand2;
            else if (instruction == '%')
                result = operand1%operand2;
            else
                throw new InvalidOperationException($"Unknown mathematical operation: {instruction}");
            Debug.WriteLine($"{debugInstructionString}({operand1},{operand2})={result}");
            Push(result);
        }

        private void ClearGrid()
        {
            // Empty grid
            for (int i = 0; i < Grid.GetLength(1); i++)
                for (int j = 0; j < Grid.GetLength(0); j++)
                    Grid[j, i] = ' ';
        }

        private int Pop()
        {
            return Stack.Count == 0 ? 0 : Stack.Pop();
        }

        private void Push(int operand)
        {
            Stack.Push(operand);
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IEsolang;

namespace Malbolge
{
    //https://en.wikipedia.org/wiki/Malbolge
    //http://www.lscheffer.com/malbolge_spec.html
    //http://www.lscheffer.com/malbolge.shtml
    public class Interpreter : IInterpreter
    {
        private const int MaxValue = 59048;

        // Decryption table
        private string DecryptionTable = "+b(29e*j1VMEKLyC})8&m#~W>qxdRp0wkrUo[D7,XTcA\"lI.v%{gJh4G\\-=O@5`_3i<?Z';FNQuY]szf$!BS/|t:Pn6^Ha";
        // Encryption table (wiki table: 9m<.TVac`uY*MK'X~xDl}REokN:#?G"i@5z]&gqtyfr$(we4{WP)H-Zn,[%\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb    same as this one but right-rolled by 33)
        private string EncryptionTable = "5z]&gqtyfr$(we4{WP)H-Zn,[%\\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb9m<.TVac`uY*MK'X~xDl}REokN:#?G\"i@";

        private string ValidInstructions = "ji*p</vo"; // (wiki operation id - 33) % 96 => index in decryption table

        public int[] Memory { get; } // array of 10-tribits (value from 0 to 59048)

        public Action<char> OutputAction { get; }
        public Func<char> InputFunc { get; }

        public Interpreter(Func<char> inputFunc, Action<char> outputAction)
        {
            Memory = new int[MaxValue+1];

            InputFunc = inputFunc;
            OutputAction = outputAction;
        }

        public void Parse(string program)
        {
            Debug.WriteLine("Parsing start");

            int c = 0;

            // Decrypt input program and copy it to memory
            foreach (char instruction in program)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(instruction))
                {
                    Debug.WriteLine("Skipping whitespace");
                    continue;
                }
                // Get instruction
                if (instruction > 32 && instruction < 127) // [33-126]
                {
                    int opCodeEncrypted = (c + instruction - 33)%94;
                    char opCode = DecryptionTable[opCodeEncrypted];
                    Debug.WriteLine($"Opcode:{opCodeEncrypted} => {opCode}");

                    if (!ValidInstructions.Contains(opCode))
                        throw new InvalidOperationException($"Invalid character in source: {instruction}");
                }
                // non-valid instruction are copied as is in memory (bug or feature ?)

                if (c >= Memory.Length)
                    throw new InvalidOperationException("Program is too long");

                Memory[c++] = instruction;
            }

            // Fill rest of memory using Crazy operation
            while (c < Memory.Length)
            {
                Memory[c] = Crazy(Memory[c - 1], Memory[c - 2]);
                c++;
            }

            Debug.WriteLine("Parsing end");
        }

        public void Execute()
        {
            Debug.WriteLine("Execution start");
            // init registers
            int a = 0; // accumulator
            int c = 0; // code pointer
            int d = 0; // data pointer
            while (true)
            {
                int instruction = Memory[c];
                if (instruction > 32 && instruction < 127) // [33-126]
                {
                    int opCodeEncrypted = (c + instruction - 33)%94;
                    char opCode = DecryptionTable[opCodeEncrypted];
                    Debug.WriteLine($"Opcode:{opCodeEncrypted} => {opCode}");

                    // Perform instruction
                    switch (opCode)
                    {
                        case 'j': // 40: mov d,[d]
                            Debug.WriteLine("Instruction: MOV");
                            d = Memory[d];
                            break;
                        case 'i': // 4: jmp[d]
                            Debug.WriteLine("Instruction: JMP");
                            c = Memory[d];
                            break;
                        case '*': // 39: rotr [d] | mov a,d
                            Debug.WriteLine("Instruction: ROTR");
                            Memory[d] = Memory[d]/3 + Memory[d]%3*19683;
                            a = Memory[d];
                            break;
                        case 'p': // 62: crazy [d],a | mov a,[d]
                            Debug.WriteLine("Instruction: CRAZY");
                            Memory[d] = Crazy(a, Memory[d]);
                            a = Memory[d];
                            break;
                        case '<': // 5: stdout
                            Debug.WriteLine("Instruction: STDOUT");
                            OutputAction?.Invoke((char) (a & 0xFF));
                            break;
                        case '/': // 23: stdin
                            Debug.WriteLine("Instruction: STDIN");
                            char? input = InputFunc?.Invoke();
                            if (input == null)
                                Debug.WriteLine("Null input");
                            else
                            {
                                if (input.Value == '\n')
                                    a = 10;
                                //else if (input == EOF) // TODO
                                //  a = int.MaxValue;
                                else
                                    a = input.Value;
                            }
                            break;
                        case 'v': // 81: end
                            Debug.WriteLine("Instruction: END OF PROGRAM");
                            return;
                        case 'o': // 68: nop
                            Debug.WriteLine("Instruction: NOP");
                            break;
                        default:
                            Debug.WriteLine("Instruction: INVALID");
                            break;
                    }

                    // Encrypt instruction
                    Memory[c] = EncryptionTable[Memory[c] - 33];
                    // Advance c and d
                    if (c == MaxValue)
                        c = 0;
                    else
                        c++;
                    if (d == MaxValue)
                        d = 0;
                    else
                        d++;
                }
                else
                    Debug.WriteLine($"Skipping invalid instruction {instruction}");
            }
        }


        //                                      0  1   2    3     4
        private static readonly int[] Power9 = {1, 9, 81, 729, 6561}; // used to perform tribit extraction faster

        // original crazy operation
        // crazy      input 2
        //            0  1  2
        //           --------
        //        0 | 1  0  0
        // input1 1 | 1  0  2
        //        2 | 2  2  1
        private static readonly int[,] CrazyMap = // crazy operation 2 tribits at a time
        {
            // input2
            //00 01 02 10 11 12 20 21 22
            {4, 3, 3, 1, 0, 0, 1, 0, 0}, //00
            {4, 3, 5, 1, 0, 2, 1, 0, 2}, //01
            {5, 5, 4, 2, 2, 1, 2, 2, 1}, //02
            {4, 3, 3, 1, 0, 0, 7, 6, 6}, //10  input1
            {4, 3, 5, 1, 0, 2, 7, 6, 8}, //11
            {5, 5, 4, 2, 2, 1, 8, 8, 7}, //12
            {7, 6, 6, 7, 6, 6, 4, 3, 3}, //20
            {7, 6, 8, 7, 6, 8, 4, 3, 5}, //21
            {8, 8, 7, 8, 8, 7, 5, 5, 4}, //22
        };

        private static int Crazy(int x, int y)
        {
            int result = 0;
            for (int j = 0; j < 5; j++) // perform 2 tribits in one operation
                result += CrazyMap[y/Power9[j]%9, x/Power9[j]%9]*Power9[j]; // crazy + shift
            return result;
        }

        public static string TritToString(int trit)
        {
            StringBuilder sb = new StringBuilder(10);
            for (int i = 0; i < 10; i++)
            {
                sb.Append((char) ('0' + (trit%3)));
                trit /= 3;
            }
            return sb.ToString();
        }
    }
}

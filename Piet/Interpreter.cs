using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Piet
{
    //http://www.dangermouse.net/esoteric/piet.html
    //http://www.dangermouse.net/esoteric/piet/tools.html
    //http://www.rapapaing.com/blog/?page_id=6  debugger
    public class Interpreter
    {
        private static readonly Color LightRed = Color.FromRgb(0xFF, 0xC0, 0xC0);
        private static readonly Color Red = Color.FromRgb(0xFF, 0x00, 0x00);
        private static readonly Color DarkRed = Color.FromRgb(0xC0, 0x00, 0x00);
        
        private static readonly Color LightYellow = Color.FromRgb(0xFF, 0xFF, 0xC0);
        private static readonly Color Yellow = Color.FromRgb(0xFF, 0xFF, 0x00);
        private static readonly Color DarkYellow = Color.FromRgb(0xC0, 0xC0, 0x00);

        private static readonly Color LightGreen = Color.FromRgb(0xC0, 0xFF, 0xC0);
        private static readonly Color Green = Color.FromRgb(0x00, 0xFF, 0x00);
        private static readonly Color DarkGreen = Color.FromRgb(0x00, 0xC0, 0x00);

        private static readonly Color LightCyan = Color.FromRgb(0xC0, 0xFF, 0xFF);
        private static readonly Color Cyan = Color.FromRgb(0x00, 0xFF, 0xFF);
        private static readonly Color DarkCyan = Color.FromRgb(0x00, 0xC0, 0xC0);

        private static readonly Color LightBlue = Color.FromRgb(0xC0, 0xC0, 0xFF);
        private static readonly Color Blue = Color.FromRgb(0x00, 0x00, 0xFF);
        private static readonly Color DarkBlue = Color.FromRgb(0x00, 0x00, 0xC0);

        private static readonly Color LightMagenta = Color.FromRgb(0xFF, 0xC0, 0xFF);
        private static readonly Color Magenta = Color.FromRgb(0xFF, 0x00, 0xFF);
        private static readonly Color DarkMagenta = Color.FromRgb(0xC0, 0x00, 0xC0);

        private static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);
        private static readonly Color Black = Color.FromRgb(0x00, 0x00, 0x00);

        // Colors <-> Offset
        private static readonly Color[] Colors =
        {
            LightRed, LightYellow, LightGreen, LightCyan, LightBlue, LightMagenta,
            Red, Yellow, Green, Cyan, Blue, Magenta,
            DarkRed, DarkYellow, DarkGreen, DarkCyan, DarkBlue, DarkMagenta,
            //
            White, Black
        };

        private static readonly string[] ColorsName =
        {
            "LightRed", "LightYellow", "LightGreen", "LightCyan", "LightBlue", "LightMagenta",
            "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta",
            "DarkRed", "DarkYellow", "DarkGreen", "DarkCyan", "DarkBlue", "DarkMagenta",
            //
            "White", "Black"
        };

        private Func<int, string> ColorIndexToString => idx => idx == 999 ? "Fill" : ( idx == -1 ? "Invalid" : ColorsName[idx]);

        private static int WhiteIndex = 18;
        private static int BlackIndex = 19;
        private static int FillIndex = 999; // neutral color for fill
        private static int InvalidIndex = -1;

        // Hue Cycle: Red -> Yellow -> Green -> Cyan -> Blue -> Magenta -> Red
        private Func<int, int, int> HueDiff => (index1, index2) => (6 + Hue(index2) - Hue(index1)) % 6;
        private Func<int, int> Hue => index => index < 18 ? index%6 : index;
        // Lightness Cycle: Light -> Normal -> Dark -> Light
        private Func<int, int, int> LightnessDiff => (index1, index2) => (3 + Lightness(index2) - Lightness(index1))%3;
        private Func<int, int> Lightness => index => index < 18 ? index/6 : index;

        // Codels
        public int CodelsWidth { get; private set; }
        public int CodelsHeight { get; private set; }
        public int[] Codels { get; private set; }

        private int this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= CodelsWidth || y < 0 || y >= CodelsHeight)
                    return InvalidIndex;
                int index = x + y*CodelsWidth;
                return Codels[index];
            }
            set
            {
                if (x < 0 || x >= CodelsWidth || y < 0 || y >= CodelsHeight)
                    throw new ArgumentOutOfRangeException();
                int index = x + y * CodelsWidth;
                Codels[index] = value;
            }
        }

        //
        public enum PointerDirections
        {
            Right,
            Down,
            Left,
            Up
        }
        public PointerDirections DirectionPointer { get; private set; } // aka DP

        public enum CodelChoosers
        {
            Left,
            Right
        }

        public CodelChoosers CodelChooser { get; private set; } // aka CC

        private int ToggleCount { get; set; }

        private Func<CodelChoosers, CodelChoosers> ToggleCodelChooser => cc => cc == CodelChoosers.Left ? CodelChoosers.Right : CodelChoosers.Left;

        private Func<PointerDirections, PointerDirections> TurnDirectionPointerClockwise => dp =>
        {
            switch (dp)
            {
                case PointerDirections.Right:
                    return PointerDirections.Down;
                    case PointerDirections.Down:
                    return PointerDirections.Left;
                    case PointerDirections.Left:
                    return PointerDirections.Up;
                    case PointerDirections.Up:
                    return PointerDirections.Right;
                default:
                    throw new InvalidOperationException($"Invalid PointerDirections:{dp}");
            }
        };

        private Func<PointerDirections, PointerDirections> TurnDirectionPointerCounterClockwise => dp =>
        {
            switch (dp)
            {
                case PointerDirections.Down:
                    return PointerDirections.Right;
                case PointerDirections.Right:
                    return PointerDirections.Up;
                case PointerDirections.Up:
                    return PointerDirections.Left;
                case PointerDirections.Left:
                    return PointerDirections.Down;
                default:
                    throw new InvalidOperationException($"Invalid PointerDirections:{dp}");
            }
        };

        private Func<PointerDirections, int> DirectionX => dp => dp == PointerDirections.Left ? -1 : (dp == PointerDirections.Right ? 1 : 0);
        private Func<PointerDirections, int> DirectionY => dp => dp == PointerDirections.Up ? -1 : (dp == PointerDirections.Down ? 1 : 0);

        public PietStack Stack { get; private set; }

        public Action<string> OutputAction { get; }
        public Func<string> InputFunc { get;  }

        public Interpreter(Func<string> inputFunc, Action<string> outputAction)
        {
            InputFunc = inputFunc;
            OutputAction = outputAction;
        }

        public void Parse(string imageFilename, int codelSize)
        {
            // Read image and extract color as byte array
            BitmapImage img = new BitmapImage(new Uri(imageFilename, UriKind.Absolute));

            Debug.Assert(img.PixelWidth % codelSize == 0);
            Debug.Assert(img.PixelHeight % codelSize == 0);

            if (img.Format == PixelFormats.Indexed8)
                ParseIndexed8(img, codelSize);
            else if (img.Format == PixelFormats.Bgr32)
                ParseBgr32(img, codelSize);
            else
                throw new ArgumentException("Only Indexed8 and Bgr32 image can be used.");
        }

        public void Execute()
        {
            Stack = new PietStack();

            int codelX = 0; // upper left
            int codelY = 0; // upper left
            DirectionPointer = PointerDirections.Right;
            CodelChooser = CodelChoosers.Left;
            ToggleCount = 0;

            while (true)
            {
                bool canContinue = ExecuteStep(ref codelX, ref codelY);
                if (!canContinue)
                    break;
            }
            Debug.WriteLine("Execution ended");
        }

        public BitmapSource GenerateImageFromCodels()
        {
            int bytesPerPixel = 4;
            int stride = CodelsWidth*bytesPerPixel;
            byte[] byteArray = new byte[CodelsHeight*stride];
            int byteArrayIndex = 0;
            for (int y = 0; y < CodelsHeight; y++)
                for (int x = 0; x < CodelsWidth; x++)
                {
                    int codelsIndex = y*CodelsWidth + x;
                    int colorIndex = Codels[codelsIndex];
                    Color color = colorIndex > 0 && colorIndex < Colors.Length ? Colors[colorIndex] : System.Windows.Media.Colors.HotPink;
                    byteArray[byteArrayIndex] = color.B;
                    byteArray[byteArrayIndex + 1] = color.G;
                    byteArray[byteArrayIndex + 2] = color.R;
                    byteArray[byteArrayIndex + 3] = 0xFF;
                    byteArrayIndex += 4;
                }
            return BitmapSource.Create(CodelsWidth, CodelsHeight, 72, 72, PixelFormats.Bgra32, null, byteArray, stride);
        }

        private bool ExecuteStep(ref int codelX, ref int codelY)
        {
            // Current codel color
            int colorIndex = this[codelX, codelY];
            bool whiteCrossed = colorIndex == WhiteIndex;

            // cannot start on black
            if (colorIndex == BlackIndex)
            {
                Debug.WriteLine("Starting on black codel -> step failed");
                return false;
            }

            // Max 8 tries
            for (int t = 0; t < 8; t++)
            {
                Debug.WriteLine($"Try {t} with {codelX},{codelY},{ColorIndexToString(colorIndex)} dp:{DirectionPointer} cc:{CodelChooser}");
                int bestX = codelX, bestY = codelY, codelCount;
                if (colorIndex == WhiteIndex)
                {
                    Debug.WriteLine("Special case: white codel"); // no need to display
                    codelCount = 1;
                }
                else
                {
                    // Search best X, Y
                    codelCount = 0;
                    bool found = SearchFurthestCodel(codelX, codelY, colorIndex, FillIndex, ref bestX, ref bestY, ref codelCount);
                    if (!found)
                        throw new ApplicationException("Internal error");
                    ResetSearch(codelX, codelY, colorIndex, FillIndex);
                    int bestColorIndex = this[bestX, bestY];
                    Debug.WriteLine($"Best found: dp:{DirectionPointer} cc:{CodelChooser} => from {codelX},{codelY},{ColorIndexToString(colorIndex)} to {bestX},{bestY},{ColorIndexToString(bestColorIndex)}");
                }

                // Find adjacent codel to best codel according to DP
                int adjacentX = bestX + DirectionX(DirectionPointer);
                int adjacentY = bestY + DirectionY(DirectionPointer);
                int adjacentColorIndex = this[adjacentX, adjacentY];

                Debug.WriteLine($"Adjacent: dp:{DirectionPointer} cc:{CodelChooser} => {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}");

                // White block (pass-thru)
                if (adjacentColorIndex == WhiteIndex)
                {
                    Debug.WriteLine("White block hit");
                    // white coloured block are free zone thru which interpreter passes unhindered (see http://www.dangermouse.net/esoteric/piet.html)
                    // if transition between colored blocks occurs via a slide across a white block, no command is executed
                    while (adjacentColorIndex == WhiteIndex)
                    {
                        Debug.WriteLine($"White codel passed to {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}");
                        adjacentX += DirectionX(DirectionPointer);
                        adjacentY += DirectionY(DirectionPointer);
                        adjacentColorIndex = this[adjacentX, adjacentY];
                    }

                    Debug.WriteLine($"White block crossed at {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}");

                    whiteCrossed = true;

                    // code seen in npiet but causes an infinite loop with some programs
                    // black or wall
                    //if (adjacentColorIndex == InvalidIndex || adjacentColorIndex == BlackIndex)
                    //{
                    //    // before clarification of white block behaviour
                    //    // when sliding into a black block or wall from white, we set white block as the current block
                    //    adjacentColorIndex = WhiteIndex;
                    //    adjacentX -= DirectionX(DirectionPointer);
                    //    adjacentY -= DirectionY(DirectionPointer);
                    //    Debug.WriteLine($"Entering white block at {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}");

                    //    // after clarification of white block behaviour
                    //    //List<Tuple<int,int, PointerDirections, CodelChoosers>> visited = new List<Tuple<int, int, PointerDirections, CodelChoosers>>();
                    //    //while (adjacentColorIndex == InvalidIndex || adjacentColorIndex == BlackIndex)
                    //    //{
                    //    //    adjacentColorIndex = WhiteIndex;
                    //    //    adjacentX -= DirectionX(DirectionPointer);
                    //    //    adjacentY -= DirectionY(DirectionPointer);
                    //    //    Debug.WriteLine($"Hitting black block when sliding at {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)} dp:{DirectionPointer} cc:{CodelChooser}");
                    //    //    //
                    //    //    CodelChooser = ToggleCodelChooser(CodelChooser);
                    //    //    DirectionPointer = TurnDirectionPointerClockwise(DirectionPointer);
                    //    //    //
                    //    //    foreach(Tuple<int, int, PointerDirections, CodelChoosers> tuple in visited)
                    //    //        if (tuple.Item1 == adjacentX && tuple.Item2 == adjacentY && tuple.Item3 == DirectionPointer && tuple.Item4 == CodelChooser)
                    //    //        {
                    //    //            Debug.WriteLine("Already hit that black/wall => exiting");
                    //    //            return false;
                    //    //        }
                    //    //    visited.Add(new Tuple<int, int, PointerDirections, CodelChoosers>(adjacentX, adjacentY, DirectionPointer, CodelChooser));
                    //    //    while (adjacentColorIndex == WhiteIndex)
                    //    //    {
                    //    //        Debug.WriteLine($"White codel passed to {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}");
                    //    //        adjacentX -= DirectionX(DirectionPointer);
                    //    //        adjacentY -= DirectionY(DirectionPointer);
                    //    //        adjacentColorIndex = this[adjacentX, adjacentY];
                    //    //    }
                    //    //}
                    //}
                }

                // Black or wall
                if (adjacentColorIndex == InvalidIndex || adjacentColorIndex == BlackIndex)
                {
                    Debug.WriteLine("Black or wall hit");
                    if (colorIndex == WhiteIndex) // currently in white
                    {
                        // turn DP and toggle CC
                        DirectionPointer = TurnDirectionPointerClockwise(DirectionPointer);
                        CodelChooser = ToggleCodelChooser(CodelChooser);
                        Debug.WriteLine($"Currently in white => turn DP {DirectionPointer} and toggle CC {CodelChooser}");
                    }
                    else
                    {
                        // Toggle DP or CC
                        if (ToggleCount%2 == 0)
                        {
                            CodelChooser = ToggleCodelChooser(CodelChooser);
                            Debug.WriteLine($"Currently not in white => toggle CC: {CodelChooser}");
                        }
                        else
                        {
                            DirectionPointer = TurnDirectionPointerClockwise(DirectionPointer);
                            Debug.WriteLine($"Currently not in white => turn DP: {DirectionPointer}");
                        }
                        ToggleCount++;
                    }
                }
                else
                {
                    Debug.WriteLine($"instruction? {codelX},{codelY},{ColorIndexToString(colorIndex)} => {adjacentX},{adjacentY},{ColorIndexToString(adjacentColorIndex)}  codel count:{codelCount}");


                    if (whiteCrossed)
                        Debug.WriteLine("White block crossed -> no instruction");
                    else
                    {
                        // Perform instruction
                        PerformInstruction(colorIndex, adjacentColorIndex, codelCount);
                    }

                    // use adjacent as new start point
                    codelX = adjacentX;
                    codelY = adjacentY;
                    return true;
                }
            }

            Debug.WriteLine("8 tries done. no more step allowed");

            return false;
        }
        
        /*
         *       Lightness
         * Hue   0         1           2
         * 0    /         push        pop
         * 1    add       sub         mul
         * 2    div       mod         not
         * 3    greater   pointer     switch
         * 4    dup       roll        in number
         * 5    in char   out number  out char
        */
        private void PerformInstruction(int fromColorIndex, int toColorIndex, int codelCount)
        {
            int hueDiff = HueDiff(fromColorIndex, toColorIndex);
            int lightnessDiff = LightnessDiff(fromColorIndex, toColorIndex);

            Debug.WriteLine($"PerformInstruction: From{ColorIndexToString(fromColorIndex)} to {ColorIndexToString(toColorIndex)} => Hue:{hueDiff} Lightness:{lightnessDiff}");

            switch (hueDiff)
            {
                case 0: // NOP, Push, Pop
                    switch (lightnessDiff)
                    {
                        case 0: // NOP
                            Debug.WriteLine("Action: NOP");
                            break;
                        case 1: // Push
                            Debug.WriteLine($"Action: PUSH {codelCount}");
                            Stack.Push(codelCount);
                            break;
                        case 2: // Pop
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: POP failed: stack underflow");
                            else
                            {
                                Debug.WriteLine("Action: POP");
                                Stack.Pop(); // Pop and discard
                            }
                            break;
                    }
                    break;
                case 1: // Add, Sub, Mul
                    switch (lightnessDiff)
                    {
                        case 0: // Add
                            // Pop 2 values, add them and push back result
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: ADD failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 + operand2;
                                Debug.WriteLine($"Action: ADD({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                        case 1: // Sub
                            // Pop 2 values, sub (top from second top) them and push back result
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: SUB failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 - operand2;
                                Debug.WriteLine($"Action: SUB({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                        case 2: // Mul
                            // Pop 2 values, multiply them and push back result
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: MUL failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 * operand2;
                                Debug.WriteLine($"Action: MUL({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                    }
                    break;
                case 2: // Div, Mod, Not
                    switch (lightnessDiff)
                    {
                        case 0: // Div
                            // Pop 2 values, divide (second top by top) them and push back result
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: DIV failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 / operand2;
                                Debug.WriteLine($"Action: DIV({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                        case 1: // Mod
                            // Pop 2 values, mod (second top modulo top) them and push back result
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: MOD failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 % operand2;
                                Debug.WriteLine($"Action: MOD({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                        case 2: // Not
                            // Pop 1 value, not (0->1, 0 otherwise) it and push back result
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: NOT failed: stack underflow");
                            else
                            {
                                int operand = Stack.Pop();
                                int result = operand == 0 ? 1 : 0;
                                Debug.WriteLine($"Action: NOT({operand})={result}");
                                Stack.Push(result);
                            }
                            break;
                    }
                    break;
                case 3: // greater, pointer, switch
                    switch (lightnessDiff)
                    {
                        case 0: // Greater
                            // Pop 2 values, if second top > top push 1, push 0 otherwise
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: GREATER failed: stack underflow");
                            else
                            {
                                int operand2 = Stack.Pop();
                                int operand1 = Stack.Pop();
                                int result = operand1 > operand2 ? 1 : 0;
                                Debug.WriteLine($"Action: GREATER({operand1},{operand2})={result}");
                                Stack.Push(result);
                            }
                            break;
                        case 1: // Pointer
                            // Pop 1 value, turn DP clockwise if positive, counterclockwise otherwise
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: POINTER failed: stack underflow");
                            else
                            {
                                int operand = Stack.Pop();
                                int absOperand = operand;
                                Func<PointerDirections, PointerDirections> func;
                                if (absOperand > 0)
                                    func = TurnDirectionPointerClockwise;
                                else
                                {
                                    absOperand = -absOperand;
                                    func = TurnDirectionPointerCounterClockwise;
                                }
                                for (int i = 0; i < absOperand % 4; i++)
                                    DirectionPointer = func(DirectionPointer);
                                Debug.WriteLine($"Action: POINTER({operand})={DirectionPointer}");
                            }
                            break;
                        case 2: // Switch
                            // Pop 1 value, toggle CC that many times
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: SWITCH failed: stack underflow");
                            else
                            {
                                int operand = Stack.Pop();
                                for (int i = 0; i < operand % 4; i++)
                                    CodelChooser = ToggleCodelChooser(CodelChooser);
                                Debug.WriteLine($"Action: SWITCH({operand})={CodelChooser}");
                            }
                            break;
                    }
                    break;
                case 4: // dup, roll, in number
                    switch (lightnessDiff)
                    {
                        case 0: // Duplicate
                            // Push a copy of top value
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: DUPLICATE failed: stack underflow");
                            else
                            {
                                int operand = Stack.Peek();
                                Stack.Push(operand);
                                Debug.WriteLine($"Action: DUPLICATE({operand})");
                            }
                            break;
                        case 1: // Roll
                            // Pop 2 values, roll remaining stack entries to a depth equals to the second top by a number of rolls equal to top (see http://www.dangermouse.net/esoteric/piet.html)
                            //  roll of depth k: move top at position k in stack and move up every value above k'th position
                            if (Stack.Count < 2)
                                Debug.WriteLine("Action: ROLL failed: stack underflow");
                            else
                            {
                                int roll = Stack.Pop();
                                int depth = Stack.Pop();
                                if (depth < 0)
                                    Debug.WriteLine($"Action: ROLL failed: negative depth {depth}");
                                else if (Stack.Count < depth)
                                    Debug.WriteLine($"Action: ROLL failed: stack underflow {depth}");
                                else
                                    Stack.Roll(roll, depth);
                            }
                            break;
                        case 2: // In number
                            // Read a number from stdin and push it
                            string inputAsString = InputFunc?.Invoke();
                            int inputNumber;
                            if (!int.TryParse(inputAsString, out inputNumber))
                                Debug.WriteLine($"Action: IN NUMBER failed: invalid input {inputAsString}");
                            else
                            {
                                Debug.WriteLine($"Action: IN NUMBER({inputNumber})");
                                Stack.Push(inputNumber);
                            }
                            break;
                    }
                    break;
                case 5: // in char, out number, out char
                    switch (lightnessDiff)
                    {
                        case 0: // In char
                            // Read a char from stdin and push it
                            string inputAsString = InputFunc?.Invoke();
                            if (string.IsNullOrEmpty(inputAsString))
                                Debug.WriteLine("Action: IN CHAR failed: null or empty input");
                            else
                            {
                                int inputChar = inputAsString[0] % 255;
                                Debug.WriteLine($"Action: IN CHAR({inputChar})");
                                Stack.Push(inputChar);
                            }
                            break;
                        case 1: // Out number
                            // Pop 1 value, print it on stdout as number
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: OUT NUMBER failed: stack underflow");
                            else
                            {
                                int operand = Stack.Pop();
                                Debug.WriteLine($"Action: OUT NUMBER({operand})");
                                OutputAction?.Invoke(operand.ToString());
                            }
                            break;
                        case 2: // Out char
                            // Pop 1 value, print it on stdout as char
                            if (Stack.Count == 0)
                                Debug.WriteLine("Action: OUT CHAR failed: stack underflow");
                            else
                            {
                                int outputAsInt = Stack.Pop();
                                char operand = (char) (outputAsInt%255);
                                Debug.WriteLine($"Action: OUT CHAR({operand})");
                                OutputAction?.Invoke(operand.ToString());
                            }
                            break;
                    }
                    break;
            }
        }

        // Search furthest codel in current codel color region
        private bool SearchFurthestCodel(int x, int y, int currentColorIndex, int fillColorIndex, ref int bestX, ref int bestY, ref int codelCount)
        {
            // Use a simple recursive fill algorithm with a neutral color (markColorIndex)
            int color = this[x, y];
            if (color == InvalidIndex || color != currentColorIndex || color == fillColorIndex)
                // invalid codel reached
                return false;

            //Debug.WriteLine($"Fill: {x},{y},{ColorIndexToString(color)}");

            // check if codel is the furthest according db/cc direction
            // DP       CC      Codel
            // Right    Left    Top
            //          Right   Bottom
            // Down     Left    Right
            //          Right   Left
            // Left     Left    Bottom
            //          Right   Top
            // Up       Left    Left
            //          Right   Right
            bool found = false;
            if (DirectionPointer == PointerDirections.Right && x >= bestX) // right
            {
                if ((x > bestX) || (CodelChooser == CodelChoosers.Left && y < bestY) || (CodelChooser == CodelChoosers.Right && y > bestY))
                    found = true;
            }
            else if (DirectionPointer == PointerDirections.Down && y >= bestY) // down
            {
                if ((y > bestY) || (CodelChooser == CodelChoosers.Left && x > bestX) || (CodelChooser == CodelChoosers.Right && x < bestX))
                    found = true;
            }
            else if(DirectionPointer == PointerDirections.Left && x <= bestX) // left
            {
                if ((x < bestX) || (CodelChooser == CodelChoosers.Left && y > bestY) || (CodelChooser == CodelChoosers.Right && y < bestY))
                    found = true;
            }
            else if (DirectionPointer == PointerDirections.Up && y <= bestY) // up
            {
                if ((y < bestY) || (CodelChooser == CodelChoosers.Left && x < bestX) || (CodelChooser == CodelChoosers.Right && x > bestX))
                    found = true;
            }

            if (found)
            {
                //Debug.WriteLine($"New best: dp:{DirectionPointer} cc:{CodelChooser}    {bestX},{bestY},{this[bestX, bestY]} => {x},{y},{ColorIndexToString(color)}");
                bestX = x;
                bestY = y;
            }

            this[x, y] = fillColorIndex; // set to mark color

            // one mode codel found in this color block
            codelCount++;

            // Recursive call on neighbourhood
            SearchFurthestCodel(x + 1, y, currentColorIndex, fillColorIndex, ref bestX, ref bestY, ref codelCount); // right
            SearchFurthestCodel(x, y + 1, currentColorIndex, fillColorIndex, ref bestX, ref bestY, ref codelCount); // down
            SearchFurthestCodel(x - 1, y, currentColorIndex, fillColorIndex, ref bestX, ref bestY, ref codelCount); // left
            SearchFurthestCodel(x, y - 1, currentColorIndex, fillColorIndex, ref bestX, ref bestY, ref codelCount); // up

            return true;
        }

        private bool ResetSearch(int x, int y, int originalColorIndex, int fillColorIndex)
        {
            int color = this[x, y];
            if (color == InvalidIndex || color == originalColorIndex || color != fillColorIndex)
                // invalid codel reached
                return false;
            // set original color
            this[x, y] = originalColorIndex;

            // Recursive call on neighbourhood
            ResetSearch(x + 1, y, originalColorIndex, fillColorIndex); // right
            ResetSearch(x, y + 1, originalColorIndex, fillColorIndex); // down
            ResetSearch(x - 1, y, originalColorIndex, fillColorIndex); // left
            ResetSearch(x, y - 1, originalColorIndex, fillColorIndex); // up

            return true;
        }

        private void ParseIndexed8(BitmapImage img, int codelSize)
        {
            int bytesPerPixel = img.Format.BitsPerPixel/8;
            Debug.Assert(bytesPerPixel == 1);
            if (img.Palette?.Colors == null)
                throw new InvalidOperationException("Missing palette in Indexed8 image.");
            int stride = img.PixelWidth*bytesPerPixel;
            int size = img.PixelHeight*stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
            // Create codels
            CodelsWidth = img.PixelWidth/ codelSize;
            CodelsHeight = img.PixelHeight/ codelSize;
            int codelsSize = CodelsWidth * CodelsHeight;
            Codels = new int[codelsSize];
            int codelOffset = 0;
            for(int y = 0; y < img.PixelHeight; y+=codelSize)
                for (int x = 0; x < img.PixelWidth; x += codelSize)
                {
                    int pixelOffset = x * bytesPerPixel + y * stride;
                    byte pixelColorIndex = pixels[pixelOffset];
                    Color pixelColor = img.Palette.Colors[pixelColorIndex];
                    byte b = pixelColor.B;
                    byte g = pixelColor.G;
                    byte r = pixelColor.R;
                    int colorIndex = Array.FindIndex(Colors, c => c.R == r && c.B == b && c.G == g);
                    Codels[codelOffset] = colorIndex == -1
                        ? WhiteIndex 
                        : colorIndex;
                    codelOffset++;
                }
        }

        private void ParseBgr32(BitmapImage img, int codelSize)
        {
            int bytesPerPixel = img.Format.BitsPerPixel / 8;
            Debug.Assert(bytesPerPixel == 4);
            int stride = img.PixelWidth * bytesPerPixel;
            int size = img.PixelHeight * stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
            // Create codels
            CodelsWidth = img.PixelWidth / codelSize;
            CodelsHeight = img.PixelHeight / codelSize;
            int codelsSize = CodelsWidth * CodelsHeight;
            Codels = new int[codelsSize];
            int codelOffset = 0;
            for (int y = 0; y < img.PixelHeight; y += codelSize)
            {
                for (int x = 0; x < img.PixelWidth; x += codelSize)
                {
                    int pixelOffset = x*bytesPerPixel + y*stride;
                    byte b = pixels[pixelOffset];
                    byte g = pixels[pixelOffset + 1];
                    byte r = pixels[pixelOffset + 2];
                    int colorIndex = Array.FindIndex(Colors, c => c.R == r && c.B == b && c.G == g);
                    Codels[codelOffset] = colorIndex == -1 ? WhiteIndex : colorIndex;
                    codelOffset++;

                    //Debug.Write($"{colorIndex:X2} ");
                }
                //Debug.WriteLine("");
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Piet
{
    //http://www.dangermouse.net/esoteric/piet.html
    //http://www.dangermouse.net/esoteric/piet/tools.html
    public class NaiveInterpreter
    {
        public static readonly Color LightRed = Color.FromRgb(0xFF, 0xC0, 0xC0);
        public static readonly Color Red = Color.FromRgb(0xFF, 0x00, 0x00);
        public static readonly Color DarkRed = Color.FromRgb(0xC0, 0x00, 0x00);

        public static readonly Color LightYellow = Color.FromRgb(0xFF, 0xFF, 0xC0);
        public static readonly Color Yellow = Color.FromRgb(0xFF, 0xFF, 0x00);
        public static readonly Color DarkYellow = Color.FromRgb(0xC0, 0xC0, 0x00);

        public static readonly Color LightGreen = Color.FromRgb(0xC0, 0xFF, 0xC0);
        public static readonly Color Green = Color.FromRgb(0x00, 0xFF, 0x00);
        public static readonly Color DarkGreen = Color.FromRgb(0x00, 0xC0, 0x00);

        public static readonly Color LightCyan = Color.FromRgb(0xC0, 0xFF, 0xFF);
        public static readonly Color Cyan = Color.FromRgb(0x00, 0xFF, 0xFF);
        public static readonly Color DarkCyan = Color.FromRgb(0x00, 0xC0, 0xC0);

        public static readonly Color LightBlue = Color.FromRgb(0xC0, 0xC0, 0xFF);
        public static readonly Color Blue = Color.FromRgb(0x00, 0x00, 0xFF);
        public static readonly Color DarkBlue = Color.FromRgb(0x00, 0x00, 0xC0);

        public static readonly Color LightMagenta = Color.FromRgb(0xFF, 0xC0, 0xFF);
        public static readonly Color Magenta = Color.FromRgb(0xFF, 0x00, 0xFF);
        public static readonly Color DarkMagenta = Color.FromRgb(0xC0, 0x00, 0xC0);

        public static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);
        public static readonly Color Black = Color.FromRgb(0x00, 0x00, 0x00);

        // Colors <-> Offset
        public static readonly Color[] Colors =
        {
            LightRed, LightYellow, LightGreen, LightCyan, LightBlue, LightMagenta,
            Red, Yellow, Green, Cyan, Blue, Magenta,
            DarkRed, DarkYellow, DarkGreen, DarkCyan, DarkBlue, DarkMagenta,
            //
            White, Black
        };

        public static int WhiteIndex = 18;
        public static int BlackIndex = 19;
        public static int MarkIndex = 999; // dummy color for fill

        // Hue Cycle: Red -> Yellow -> Green -> Cyan -> Blue -> Magenta -> Red
        public Func<int, int> NextHue => index => (index/6)*6 + (index + 1)%6; // Next column in Colors table
        public Func<int, int> Hue => index => index < 18 ? index%6 : index;
        // Lightness Cycle: Light -> Normal -> Dark -> Light
        public Func<int, int> NextLightness => index => (index + 6)%18;
        public Func<int, int> Lightness => index => index < 18 ? index/6 : index;
        // Advance Color
        public Func<int, int, int, int> AdvanceColor => (index, hue, lightness) => (index%6 + hue)%6 + 6*((index/6 + lightness)%3);

        // Codels
        public int CodelsWidth { get; private set; }
        public int CodelsHeight { get; private set; }
        public int[] Codels { get; private set; }
        public int CodelX { get; private set; }
        public int CodelY { get; private set; }

        private int this[int x, int y]
        {
            get
            {
                if (x < 0 || x > CodelsWidth || y < 0 || y > CodelsHeight)
                    return -1;
                int index = x + y*CodelsWidth;
                return Codels[index];
            }
            set
            {
                if (x < 0 || x > CodelsWidth || y < 0 || y > CodelsHeight)
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
        public PointerDirections DirectionPointer { get; private set; }

        public enum CodelChoosers
        {
            Left,
            Right
        }

        public CodelChoosers CodelChooser { get; private set; }

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
            CodelX = 0; // upper left
            CodelY = 0; // upper left
            DirectionPointer = PointerDirections.Right;
            CodelChooser = CodelChoosers.Left;

            // TODO: loop
            ExecuteStep();
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

        private bool ExecuteStep()
        {
            // Current cell color
            int colorIndex = this[CodelX, CodelY];

            // cannot start on black
            if (colorIndex == BlackIndex)
            {
                Debug.WriteLine("Starting on black codel -> step failed");
                return false;
            }

            // Max 8 tries
            for (int t = 0; t < 8; t++)
            {
                
            }

            // TODO

            // Search best X, Y
            int bestX = CodelX, bestY = CodelY, cellCount = 0; // cellCount = 1 if white
            bool found = Fill(CodelX, CodelY, colorIndex, MarkIndex, ref bestX, ref bestY, ref cellCount);
            if (!found)
                throw new ApplicationException("Internal error");
            ResetFill(CodelX, CodelY, colorIndex, MarkIndex);
            Debug.WriteLine($"Best found: dp:{DirectionPointer} cc:{CodelChooser}  from {CodelX},{CodelY}: {bestX},{bestY}");

            // TODO

            return true;
        }

        private bool Fill(int x, int y, int currentColorIndex, int markColorIndex, ref int bestX, ref int bestY, ref int cellCount)
        {
            int color = this[x, y];
            if (color < 0 || color != currentColorIndex || color == markColorIndex)
                // invalid cell reached
                return false;

            Debug.WriteLine($"Fill: {x},{y} = {color}");

            // check if codel is the furthest according db/cc direction
            bool found = false;
            if (DirectionPointer == PointerDirections.Left && x <= bestX)
            {
                if ((x < bestX) || (CodelChooser == CodelChoosers.Left && y > bestY) || (CodelChooser == CodelChoosers.Right && y < bestY))
                    found = true;
            }
            else if (DirectionPointer == PointerDirections.Right && x >= bestX)
            {
                if ((x > bestX) || (CodelChooser == CodelChoosers.Left && y < bestY) || (CodelChooser == CodelChoosers.Right && y > bestY))
                    found = true;
            }
            else if (DirectionPointer == PointerDirections.Up && y <= bestY)
            {
                if ((y < bestY) || (CodelChooser == CodelChoosers.Left && x < bestX) || (CodelChooser == CodelChoosers.Right && x > bestX))
                    found = true;
            }
            else if (DirectionPointer == PointerDirections.Up && y >= bestY)
            {
                if ((y > bestY) || (CodelChooser == CodelChoosers.Left && x > bestX) || (CodelChooser == CodelChoosers.Right && x < bestX))
                    found = true;
            }

            if (found)
            {
                Debug.WriteLine($"New best: dp:{DirectionPointer} cc:{CodelChooser}    {bestX},{bestY} => {x},{y}");
                bestX = x;
                bestY = y;
            }

            this[x, y] = markColorIndex; // set to mark color

            cellCount++;

            // Recursive call on neighbourhood
            Fill(x + 1, y, currentColorIndex, markColorIndex, ref bestX, ref bestY, ref cellCount); // right
            Fill(x, y + 1, currentColorIndex, markColorIndex, ref bestX, ref bestY, ref cellCount); // down
            Fill(x - 1, y, currentColorIndex, markColorIndex, ref bestX, ref bestY, ref cellCount); // left
            Fill(x, y - 1, currentColorIndex, markColorIndex, ref bestX, ref bestY, ref cellCount); // up

            return true;
        }

        private bool ResetFill(int x, int y, int originalColorIndex, int markColorIndex)
        {
            int color = this[x, y];
            if (color < 0 || color == originalColorIndex || color != markColorIndex)
                // invalid cell reached
                return false;
            // set original color
            this[x, y] = originalColorIndex;

            // Recursive call on neighbourhood
            ResetFill(x + 1, y, originalColorIndex, markColorIndex); // right
            ResetFill(x, y + 1, originalColorIndex, markColorIndex); // down
            ResetFill(x - 1, y, originalColorIndex, markColorIndex); // left
            ResetFill(x, y - 1, originalColorIndex, markColorIndex); // up

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
                    Codels[codelOffset] = colorIndex == -1 ? WhiteIndex : colorIndex;
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

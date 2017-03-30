using System;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace Praedonum.Modules.DeadPixel.Models
{
    public class Pixel
    {
        #region Properties

        public Brush Brush { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        #endregion

        #region Constructor / Destructor

        private Pixel() { }

        private Pixel(Brush brush, int x, int y, int width, int height)
        {
            Brush = brush;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates a pixel based off of the dimensions passed
        /// </summary>
        /// <param name="x">Screen position on the x-axis</param>
        /// <param name="y">Screen position on the y-axis</param>
        /// <param name="size">Square size of the pixel</param>
        public static Pixel CreatePixel(int x, int y, int size)
        {
            return new Pixel(Brushes.Black, x, y, size, size);
        }

        /// <summary>
        /// Creates a random pixel based off of the dimensions passed
        /// </summary>
        /// <param name="rangeX">Tuple range of x-axis values</param>
        /// <param name="rangeY">Tuple range of y-axis values</param>
        /// <param name="size">Square size of the pixel</param>
        public static Pixel CreateRandomPixel(Tuple<int, int> rangeX, Tuple<int, int> rangeY, int size)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return new Pixel(Brushes.Black, random.Next(rangeX.Item1, rangeX.Item2), random.Next(rangeY.Item1, rangeY.Item2), size, size);
        }

        #endregion

        #region Functions

        public void Draw()
        {
            IntPtr desktopPtr = CreateDC("DISPLAY", null, null, IntPtr.Zero);

            using (Graphics g = Graphics.FromHdc(desktopPtr))
            {
                g.FillRectangle(Brush, X, Y, Width, Height);
            }

            ReleaseDC(IntPtr.Zero, desktopPtr);
        }


        #endregion

        #region Externals

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateDC(string strDriver, string strDevice, string strOutput, IntPtr pData);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        #endregion
    }
}

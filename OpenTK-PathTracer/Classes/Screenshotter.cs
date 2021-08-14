using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer
{
    class Screenshotter
    {
        public static Bitmap DoScreenshot(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.Finish();
            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    static class Helper
    {
        public static T[] AddArray<T>(this T[] arr0, T[] arr1) where T : struct
        {
            int oldLength = arr0.Length;
            Array.Resize(ref arr0, arr0.Length + arr1.Length);
            for (int i = 0; i < arr1.Length; i++)
                arr0[i + oldLength] = arr1[i];

            return arr0;
        }

        public static string GetPathContent(this string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} does not exist");

            return File.ReadAllText(path);
        }

        public static void ParallelLoadCubemapImages(Texture texture, string[] paths, PixelInternalFormat pixelInternalFormat)
        {
            if (texture.Target != TextureTarget.TextureCubeMap)
                throw new ArgumentException($"texture must be {TextureTarget.TextureCubeMap}");

            if (paths.Length != 6)
                throw new ArgumentException($"Number of images must be equal to six");

            if (!paths.All(p => File.Exists(p)))
                throw new FileNotFoundException($"At least on of the specified paths is invalid");

            Bitmap[] bitmaps = new Bitmap[6];
            Task.Run(() =>
            {
                Parallel.For(0, 6, i =>
                {
                    bitmaps[i] = new Bitmap(paths[i]);
                });
            }).Wait();
            if (!bitmaps.All(i => i.Width == i.Height && i.Width == bitmaps[0].Width))
                throw new ArgumentException($"Individual cubemap textures must be squares and every texture must be of the same size");
            int size = bitmaps[0].Width;
            texture.ImmutableAllocate(size, size, 1, (SizedInternalFormat)PixelInternalFormat.Srgb8Alpha8);
            for (int i = 0; i < 6; i++)
            {
                System.Drawing.Imaging.BitmapData bitmapData = bitmaps[i].LockBits(new Rectangle(0, 0, size, size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texture.SubTexture3D(size, size, 1, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0, 0, 0, 0, i);
                bitmaps[i].UnlockBits(bitmapData);
                bitmaps[i].Dispose();
            }
        }
    }
}
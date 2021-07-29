using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class EnvironmentMap
    {
        public Texture CubemapTexture { get; private set; }
        public EnvironmentMap()
        {
            CubemapTexture = new Texture(TextureTarget.TextureCubeMap, TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgb, PixelFormat.Rgb, false);
        }

        public enum Side
        {
            PositiveX = 34069,
            NegativeX = 34070,
            PositiveY = 34071,
            NegativeY = 34072,
            PositiveZ = 34073,
            NegativeZ = 34074,
        }

        public void SetSide(Bitmap image, Side side)
        {
            CubemapTexture.SetTexImage2DCubeMap(image, (TextureTarget)side);
        }
        public void SetSideParallel(string[] paths)
        {
            if (paths.Length != 6)
            {
                Console.WriteLine("EnvironmentMap: Number of images must be equal to six");
                return;
            }
            if (!paths.All(p => System.IO.File.Exists(p)))
            {
                Console.WriteLine("EnvironmentMap: At least on of the specified paths is invalid");
                return;
            }

            Bitmap[] bitmaps = new Bitmap[6];
            Task taskImageLoader = Task.Run(() =>
            {
                Parallel.For(0, 6, i =>
                {
                    bitmaps[i] = new Bitmap(paths[i]);
                });
            });

            
            Action action = new Action(() =>
            {
                taskImageLoader.Wait();
                CubemapTexture.SetTexImage2DCubeMap(bitmaps);
            });
            ThreadManager.ExecuteOnMainThread(action, ThreadManager.Priority.ASAP);
        }
    }
}

using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class EnvironmentMap : IDisposable
    {
        public Texture CubemapTexture;

        public EnvironmentMap()
        {
            CubemapTexture = new Texture(TextureTarget.TextureCubeMap, TextureWrapMode.ClampToBorder, PixelInternalFormat.SrgbAlpha, PixelFormat.Rgba, false);
        }

        public EnvironmentMap(Texture texture)
        {
            if (texture.TextureTarget != TextureTarget.TextureCubeMap)
                throw new ArgumentException($"Specified texture is not of type {TextureTarget.TextureCubeMap}");
            CubemapTexture = texture;
        }

        
        public void SetFace(Bitmap image, Texture.Face face)
        {
            CubemapTexture.SetTexImage2DCubeMap(image, face);
        }

        public void SetAllFacesParallel(string[] paths)
        {
            if (paths.Length != 6)
                throw new ArgumentException($"Number of images must be equal to six");
            
            if (!paths.All(p => System.IO.File.Exists(p)))
                throw new System.IO.FileNotFoundException($"At least on of the specified paths is invalid");

            Bitmap[] bitmaps = new Bitmap[6];
            Task taskImageLoader = Task.Run(() =>
            {
                Parallel.For(0, 6, i =>
                {
                    bitmaps[i] = new Bitmap(paths[i]);
                });
            });

            taskImageLoader.Wait();
            CubemapTexture.SetTexImage2DCubeMap(bitmaps);
        }

        public void Dispose()
        {
            CubemapTexture.Dispose();
        }
    }
}

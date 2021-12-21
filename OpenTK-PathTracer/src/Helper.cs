using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    static class Helper
    {
        public const string SHADER_DIRECTORY_PATH = "res/shaders/";
        public static readonly double APIVersion = Convert.ToDouble($"{GL.GetInteger(GetPName.MajorVersion)}{GL.GetInteger(GetPName.MinorVersion)}") / 10.0;

        public static string GetPathContent(this string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} does not exist");

            return File.ReadAllText(path);
        }

        public static void ParallelLoadCubemapImages(Texture texture, string[] paths, SizedInternalFormat sizedInternalFormat)
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
            texture.ImmutableAllocate(size, size, 1, sizedInternalFormat);
            for (int i = 0; i < 6; i++)
            {
                System.Drawing.Imaging.BitmapData bitmapData = bitmaps[i].LockBits(new Rectangle(0, 0, size, size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texture.SubTexture3D(size, size, 1, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0, 0, 0, 0, i);
                bitmaps[i].UnlockBits(bitmapData);
                bitmaps[i].Dispose();
            }
        }

        private static HashSet<string> GetExtensions()
        {
            HashSet<string> hashSet = new HashSet<string>();
            for (int i = 0; i < GL.GetInteger(GetPName.NumExtensions); i++)
                hashSet.Add(GL.GetString(StringNameIndexed.Extensions, i));

            return hashSet;
        }

        private static readonly HashSet<string> glExtensions = new HashSet<string>(GetExtensions());


        /// <summary>
        /// </summary>
        /// <param name="extension">The extension to check against. Examples: GL_ARB_bindless_texture or WGL_EXT_swap_control</param>
        /// <returns>True if the extension is available</returns>
        public static bool IsExtensionsAvailable(string extension)
        {
            return glExtensions.Contains(extension);
        }

        /// <summary>
        /// </summary>
        /// <param name="extension">The extension to check against. Examples: GL_ARB_direct_state_access or GL_ARB_compute_shader</param>
        /// <param name="first">The major API version the extension became part of the core profile</param>
        /// <returns>True if this OpenGL version is in the specified range or the extension is otherwise available</returns>
        public static bool IsCoreExtensionAvailable(string extension, double first)
        {
            return (APIVersion >= first) || IsExtensionsAvailable(extension);
        }
    }
}
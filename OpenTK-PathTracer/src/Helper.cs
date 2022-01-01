using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

        public static unsafe void ParallelLoadCubemapImages(Texture texture, string[] paths, SizedInternalFormat sizedInternalFormat)
        {
            if (texture.Target != TextureTarget.TextureCubeMap)
                throw new ArgumentException($"texture must be {TextureTarget.TextureCubeMap}");

            if (paths.Length != 6)
                throw new ArgumentException($"Number of images must be equal to six");

            if (!paths.All(p => File.Exists(p)))
                throw new FileNotFoundException($"At least on of the specified paths is invalid");

            Image<Rgba32>[] images = new Image<Rgba32>[6];
            Task.Run(() =>
            {
                Parallel.For(0, 6, i =>
                {
                    images[i] = Image.Load<Rgba32>(paths[i]);
                });
            }).Wait();
            if (!images.All(i => i.Width == i.Height && i.Width == images[0].Width))
                throw new ArgumentException($"Individual cubemap textures must be squares and every texture must be of the same size");
            
            int size = images[0].Width;
            texture.ImmutableAllocate(size, size, 1, sizedInternalFormat);
            for (int i = 0; i < 6; i++)
            {
                fixed (void* ptr = images[i].GetPixelRowSpan(0))
                {
                    texture.SubTexture3D(size, size, 1, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr, 0, 0, 0, i);
                    images[i].Dispose();
                }
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
        /// <param name="extension">Extension to check against. Examples: GL_ARB_direct_state_access or GL_ARB_compute_shader</param>
        /// <param name="first">API version the extension became part of the core profile</param>
        /// <returns>True if this GL version >=<paramref name="first"/> or the extension is otherwise available</returns>
        public static bool IsCoreExtensionAvailable(string extension, double first)
        {
            return (APIVersion >= first) || IsExtensionsAvailable(extension);
        }
    }
}
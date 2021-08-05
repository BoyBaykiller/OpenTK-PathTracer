using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenTK_PathTracer.Render.Objects
{
    class Texture : IDisposable
    {
        public static readonly Texture Zero = new Texture(TextureTarget.Texture2D, TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba, PixelFormat.Rgba, false);
        public readonly int ID;
        public TextureTarget TextureTarget { get; private set; }
        public PixelInternalFormat PixelInternalFormat { get; private set; }
        public PixelFormat PixelFormat { get; private set; }
        public TextureWrapMode TextureWrapMode { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }


        public Texture(TextureTarget textureTarget, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool enableMipmap, float[] borderColor = null)
        {
            ID = GL.GenTexture();
            TextureTarget = textureTarget;
            PixelFormat = pixelFormat;

            SetParameters(textureWrapMode, pixelInternalFormat, enableMipmap, borderColor == null ? null : borderColor);
        }

        public static Texture GetTexture2D(TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, int width, int height, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2D, textureWrapMode, pixelInternalFormat, pixelFormat, false, borderColor == null ? null : borderColor);
            texture.SetTexImage(width, height, 0);

            return texture;
        }
        public static Texture GetTexture2D(string path, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2D, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor == null ? null : borderColor);
            texture.SetTexImage2D(path);
            if (generateMipmap)
                texture.GenerateMipMap();

            return texture;
        }

        public static Texture GetTextureCubeMap(TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, int size, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.TextureCubeMap, textureWrapMode, pixelInternalFormat, pixelFormat, false, borderColor == null ? null : borderColor);
            texture.SetTexImage(size, size, 0);

            return texture;
        }
        public static Texture GetTextureCubeMap(string[] paths, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.TextureCubeMap, textureWrapMode, pixelInternalFormat, pixelFormat, false, borderColor == null ? null : borderColor);
            texture.SetTexImage2DCubeMap(paths);

            return texture;
        }
        public static Texture GetTextureCubeMap(Bitmap[] images, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.TextureCubeMap, textureWrapMode, pixelInternalFormat, pixelFormat, false, borderColor == null ? null : borderColor);
            texture.SetTexImage2DCubeMap(images);

            return texture;
        }

        public static Texture GetTexture2DArray(TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, int width, int height, int textureCount, bool enableMipmap)
        {
            var texture = new Texture(TextureTarget.Texture2DArray, textureWrapMode, pixelInternalFormat, pixelFormat, enableMipmap, null);
            texture.SetTexImage(width, height, textureCount);
            return texture;
        }

        public void SetParameters(TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, bool enableMipMap, float[] borderColor = null)
        {
            Bind();
            PixelInternalFormat = pixelInternalFormat;
            TextureWrapMode = textureWrapMode;

            if (enableMipMap) // enableMipMap
            {
                GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
                GL.TexParameter(TextureTarget, TextureParameterName.TextureLodBias, -0.5f);
            }
            else
            {
                GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            }
            
            GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapS, (int)textureWrapMode);
            GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapT, (int)textureWrapMode);
            if (TextureTarget == TextureTarget.TextureCubeMap)
                GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapR, (int)textureWrapMode);

            if (borderColor != null)
                GL.TexParameter(TextureTarget, TextureParameterName.TextureBorderColor, borderColor);
        }

        public void SetTexImage2D(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Texture: Could not find the file {path}");
                return;
            }
            SetTexImage2D(new Bitmap(path));
        }
        public void SetTexImage2D(Bitmap image)
        {
            BitmapData _data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            
            Bind();
            GL.TexImage2D(TextureTarget, 0, PixelInternalFormat, image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, _data.Scan0);
            image.UnlockBits(_data);

            Width = image.Width;
            Height = image.Height;
            image.Dispose();
        }

        public void SetTexImage2DCubeMap(string path, TextureTarget textureTarget)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Texture: Could not find the file {path}");
                return;
            }
            SetTexImage2DCubeMap(new Bitmap(path), textureTarget);
        }

        public void SetTexImage2DCubeMap(Bitmap image, TextureTarget textureTarget)
        {
            Width = image.Width;
            Height = image.Height;

            Bind();
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            GL.TexImage2D(textureTarget, 0, PixelInternalFormat, image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);
            image.Dispose();
        }

        public void SetTexImage2DCubeMap(string[] paths)
        {
            bool _notfound = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (!File.Exists(paths[i]))
                {
                    _notfound = true;
                    Console.WriteLine($"Texture: Could not find the file {paths[i]}");
                }
            }
            if (_notfound)
                return;
            
            SetTexImage2DCubeMap(paths.Select(b => new Bitmap(b)).ToArray());
        }
        public void SetTexImage2DCubeMap(Bitmap[] images)
        {
            if (!images.All(i => i.Width == images[0].Width && i.Height == images[0].Height))
            {
                Console.WriteLine("Texture: Cubemap textures have different size");
                return;
            }
                
            for (int i = 0; i < 6; i++)
            {
                SetTexImage2DCubeMap(images[i], TextureTarget.TextureCubeMapPositiveX + i);
            }
        }

        public void SetSubTexImage2DArray(string path, int index)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Texture: Could not find the file {path}");
                return;
            }

            SetSubTexImage2DArray(new Bitmap(path), index);
        }
        public void SetSubTexImage2DArray(Bitmap image, int index)
        {
            BitmapData _data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            Bind();
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, index, image.Width, image.Height, 1, PixelFormat.Rgb, PixelType.UnsignedByte, _data.Scan0);
            image.UnlockBits(_data);
            image.Dispose();
        }

        public void SetTexImage(int width, int height, int depth = 1)
        {
            Bind();
            switch (TextureTarget)
            {
                case TextureTarget.Texture2D:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, width, height, 0, PixelFormat, PixelType.Float, IntPtr.Zero);
                    break;

                case TextureTarget.TextureCubeMap:
                    for (int i = 0; i < 6; i++)
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat, width, height, 0, PixelFormat, PixelType.Float, IntPtr.Zero);
                    break;

                case TextureTarget.Texture2DArray:
                    GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat, width, height, depth, 0, PixelFormat, PixelType.Float, IntPtr.Zero);
                    Depth = depth;
                    break;

                default:
                    Console.WriteLine($"Texture: {TextureTarget} is unsupported by this layer of abstraction");
                    break;
            }

            Width = width; Height = height; 
        }

        public void AttchToImageUnit(int unit, int level, bool layered, int layer, TextureAccess textureAccess, SizedInternalFormat sizedInternalFormat)
        {
            GL.BindImageTexture(unit, ID, level, layered, layer, textureAccess, sizedInternalFormat);
        }

        public void GenerateMipMap()
        {
            GL.GenerateTextureMipmap(ID);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget, ID);
        }
        public void Unbind()
        {
            GL.BindTexture(TextureTarget, 0);
        }

        public void AttachToUnit(int unit)
        {
            GL.BindTextureUnit(unit, ID);
        }
        public static void AttachTextureToUnit(int unit, int textureID)
        {
            GL.BindTextureUnit(unit, textureID);
        }

        public static void DetachTextureFromUnit(int unit)
        {
            Texture.Zero.AttachToUnit(unit);
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
        }
    }
}
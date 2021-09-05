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
        public enum Face
        {
            PositiveX = 34069,
            NegativeX = 34070,
            PositiveY = 34071,
            NegativeY = 34072,
            PositiveZ = 34073,
            NegativeZ = 34074,
        }

        public readonly int ID;
        public TextureTarget TextureTarget { get; private set; }
        public PixelInternalFormat PixelInternalFormat { get; private set; }
        public PixelFormat PixelFormat { get; private set; }
        public TextureWrapMode TextureWrapMode { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        private long _textureHandle = -1;
        public long TextureHandle
        {
            get
            {
                if (_textureHandle == -1)
                    throw new Exception("Texture: Texture is not made bindless yet. Call MakeBindless()");

                return _textureHandle;
            }
        }

        public Texture(TextureTarget textureTarget, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool enableMipmap, float[] borderColor = null)
        {
            GL.CreateTextures(textureTarget, 1, out ID);
            TextureTarget = textureTarget;
            PixelFormat = pixelFormat;

            SetParameters(textureWrapMode, pixelInternalFormat, enableMipmap, borderColor);
        }

        public static Texture GetTexture2D(string path, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2D, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.SetTexImage2D(path);
            if (generateMipmap)
                texture.GenerateMipMap();

            return texture;
        }
        public static Texture GetTexture2D(Bitmap bitmap, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2D, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.SetTexImage2D(bitmap);
            if (generateMipmap)
                texture.GenerateMipMap();

            return texture;
        }

        public static Texture GetTextureCubeMap(string[] paths, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.TextureCubeMap, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.SetTexImage2DCubeMap(paths);
            if (generateMipmap)
                texture.GenerateMipMap();

            return texture;
        }
        public static Texture GetTextureCubeMap(Bitmap[] images, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.TextureCubeMap, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.SetTexImage2DCubeMap(images);
            if (generateMipmap)
                texture.GenerateMipMap();
            return texture;
        }

        public static Texture GetTexture2DArray(string[] paths, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, int width, int height, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2DArray, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.Allocate(width, height, paths.Length);
            texture.SetSubTexImage2DArray(paths, 0);
            if (generateMipmap)
                texture.GenerateMipMap();
            return texture;
        }
        public static Texture GetTexture2DArray(Bitmap[] images, TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, int width, int height, bool generateMipmap, float[] borderColor = null)
        {
            var texture = new Texture(TextureTarget.Texture2DArray, textureWrapMode, pixelInternalFormat, pixelFormat, generateMipmap, borderColor);
            texture.Allocate(width, height, images.Length);
            texture.SetSubTexImage2DArray(images, 0);
            if (generateMipmap)
                texture.GenerateMipMap();
            return texture;
        }

        public void SetParameters(TextureWrapMode textureWrapMode, PixelInternalFormat pixelInternalFormat, bool enableMipMap, float[] borderColor = null)
        {
            Bind();
            PixelInternalFormat = pixelInternalFormat;
            TextureWrapMode = textureWrapMode;

            if (enableMipMap)
            {
                /// Explanation for Mipmap filters from https://learnopengl.com/Getting-started/Textures:
                /// GL_NEAREST_MIPMAP_NEAREST: takes the nearest mipmap to match the pixel size and uses nearest neighbor interpolation for texture sampling.
                /// GL_LINEAR_MIPMAP_NEAREST: takes the nearest mipmap level and samples that level using linear interpolation.
                /// GL_NEAREST_MIPMAP_LINEAR: linearly interpolates between the two mipmaps that most closely match the size of a pixel and samples the interpolated level via nearest neighbor interpolation.
                /// GL_LINEAR_MIPMAP_LINEAR: linearly interpolates between the two closest mipmaps and samples the interpolated level via linear interpolation.
                GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

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
            {
                GL.TexParameter(TextureTarget, TextureParameterName.TextureWrapR, (int)textureWrapMode);
                GL.TexParameter(TextureTarget, (TextureParameterName)All.TextureCubeMapSeamless, 1);
            }
                

            if (borderColor != null)
                GL.TexParameter(TextureTarget, TextureParameterName.TextureBorderColor, borderColor);
        }

        public void SetTexImage2D(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture: Could not find the file {path}");
            SetTexImage2D(new Bitmap(path));
        }
        public void SetTexImage2D(Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            Bind();
            GL.TexImage2D(TextureTarget, 0, PixelInternalFormat, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);

            Width = image.Width;
            Height = image.Height;
            image.Dispose();
        }


        public void SetTexImage2DCubeMap(string path, Face textureTarget)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture: Could not find the file {path}");
            SetTexImage2DCubeMap(new Bitmap(path), textureTarget);
        }
        public void SetTexImage2DCubeMap(Bitmap image, Face textureTarget)
        {
            Width = image.Width;
            Height = image.Height;

            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            Bind();
            GL.TexImage2D((TextureTarget)textureTarget, 0, PixelInternalFormat, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
            
            image.UnlockBits(data);
            image.Dispose();
        }
        public void SetTexImage2DCubeMap(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
                if (!File.Exists(paths[i]))
                    throw new FileNotFoundException($"Texture: Could not find the file {paths[i]}");
            SetTexImage2DCubeMap(paths.Select(b => new Bitmap(b)).ToArray());
        }
        public void SetTexImage2DCubeMap(Bitmap[] images)
        {
            if (images.Length != 6)
                throw new ArgumentException("EnvironmentMap: Number of images must be equal to six");

            if (!images.All(i => i.Width == images[0].Width && i.Height == images[0].Height))
                throw new ArgumentException("Texture: Cubemap textures have different size");

            for (int i = 0; i < 6; i++)
                SetTexImage2DCubeMap(images[i], Face.PositiveX + i);
        }


        public void SetSubTexImage2DArray(string path, int index)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture: Could not find the file {path}");

            SetSubTexImage2DArray(new Bitmap(path), index);
        }
        public void SetSubTexImage2DArray(Bitmap image, int index)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Bind();
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, index, image.Width, image.Height, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);
            image.Dispose();
        }
        public void SetSubTexImage2DArray(string[] paths, int offset)
        {
            for (int i = 0; i < paths.Length; i++)
                if (!File.Exists(paths[i]))
                    throw new FileNotFoundException($"Texture: Could not find the file {paths[i]}");

            SetSubTexImage2DArray(paths.Select(b => new Bitmap(b)).ToArray(), offset);
        }
        public void SetSubTexImage2DArray(Bitmap[] images, int offset)
        {
            for (int i = 0; i < images.Length; i++)
                SetSubTexImage2DArray(images[i], offset + i);
        }


        public void Allocate(int width, int height, int depth = 1)
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
                    break;

                default:
                    Console.WriteLine($"Texture: {TextureTarget} is unsupported by this layer of abstraction");
                    break;
            }

            Width = width; Height = height; Depth = depth;
        }

        public void GenerateMipMap()
        {
            GL.GenerateTextureMipmap(ID);
        }

        public void MakeBindless()
        {
            _textureHandle = GL.Arb.GetTextureHandle(ID);
        }

        public void MakeResident()
        {
            GL.Arb.MakeTextureHandleResident(TextureHandle);
        }
        public void UnmakeResident()
        {
            GL.Arb.MakeTextureHandleNonResident(TextureHandle);
        }

        public void AttachToImageUnit(int unit, int level, bool layered, int layer, TextureAccess textureAccess, SizedInternalFormat sizedInternalFormat)
        {
            GL.BindImageTexture(unit, ID, level, layered, layer, textureAccess, sizedInternalFormat);
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

        public static void AttachToUnit(int unit, int textureID)
        {
            GL.BindTextureUnit(unit, textureID);
        }
        public static void DetachFromUnit(int unit)
        {
            GL.BindTextureUnit(unit, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
        }
    }
}
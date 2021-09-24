using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenTK_PathTracer.Render.Objects
{
    class Texture : IDisposable
    {
        public enum PixelTypeSize
        {
            TextureRedSize = 32860,
            TextureGreenSize = 32861,
            TextureBlueSize = 32862,
            TextureAlphaSize = 32863,
        }

        public readonly int ID;
        public readonly TextureTarget TextureTarget;
        public int Width { get; private set; } = 1;
        public int Height { get; private set; } = 1;
        public int Depth { get; private set; } = 1;
        public PixelInternalFormat PixelInternalFormat { get; private set; }

        public Texture(TextureTarget3d textureTarget3D)
        {
            TextureTarget = (TextureTarget)textureTarget3D;

            GL.CreateTextures(TextureTarget, 1, out ID);
            SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }

        public Texture(TextureTarget2d textureTarget2D)
        {
            TextureTarget = (TextureTarget)textureTarget2D;

            GL.CreateTextures(TextureTarget, 1, out ID);
            SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }

        public Texture(TextureTarget1d textureTarget1D)
        {
            TextureTarget = (TextureTarget)textureTarget1D;

            GL.CreateTextures(TextureTarget, 1, out ID);
            SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }

        public void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            /// Explanation for Mipmap filters from https://learnopengl.com/Getting-started/Textures:
            /// GL_NEAREST_MIPMAP_NEAREST: takes the nearest mipmap to match the pixel size and uses nearest neighbor interpolation for texture sampling.
            /// GL_LINEAR_MIPMAP_NEAREST: takes the nearest mipmap level and samples that level using linear interpolation.
            /// GL_NEAREST_MIPMAP_LINEAR: linearly interpolates between the two mipmaps that most closely match the size of a pixel and samples the interpolated level via nearest neighbor interpolation.
            /// GL_LINEAR_MIPMAP_LINEAR: linearly interpolates between the two closest mipmaps and samples the interpolated level via linear interpolation.

            GL.TextureParameter(ID, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TextureParameter(ID, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            GL.TextureParameter(ID, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TextureParameter(ID, TextureParameterName.TextureWrapT, (int)wrapT);
        }

        public void SetTextureWrap(TextureWrapMode wrapS, TextureWrapMode wrapT, TextureWrapMode wrapR)
        {
            GL.TextureParameter(ID, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TextureParameter(ID, TextureParameterName.TextureWrapT, (int)wrapT);
            GL.TextureParameter(ID, TextureParameterName.TextureWrapR, (int)wrapR);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget, ID);
        }

        public void AttachImage(int unit, int level, bool layered, int layer, TextureAccess textureAccess, SizedInternalFormat sizedInternalFormat)
        {
            GL.BindImageTexture(unit, ID, level, layered, layer, textureAccess, sizedInternalFormat);
        }
        public void AttachSampler(int unit)
        {
            GL.BindTextureUnit(unit, ID);
        }

        public void SubTexture3D(int width, int heigth, int depth, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels, int level = 0, int xOffset = 0, int yOffset = 0, int zOffset = 0)
        {
            GL.TextureSubImage3D(ID, level, xOffset, yOffset, zOffset, width, heigth, depth, pixelFormat, pixelType, pixels);
        }
        public void SubTexture2D(int width, int heigth, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels, int level = 0, int xOffset = 0, int yOffset = 0)
        {
            GL.TextureSubImage2D(ID, level, xOffset, yOffset, width, heigth, pixelFormat, pixelType, pixels);
        }
        public void SubTexture1D(int width, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels, int level = 0, int xOffset = 0)
        {
            GL.TextureSubImage1D(ID, level, xOffset, width, pixelFormat, pixelType, pixels);
        }

        public void GenerateMipmap()
        {
            GL.GenerateTextureMipmap(ID);
        }

        /// <summary>
        /// Uses ARB_seamless_cubemap_per_texture to allow for seamless cubemapping
        /// </summary>
        /// <param name="param"></param>
        public void SetSeamlessCubeMapPerTexture(bool param)
        {
            if (TextureTarget == TextureTarget.TextureCubeMap)
                GL.TextureParameter(ID, (TextureParameterName)All.TextureCubeMapSeamless, param ? 1 : 0);
        }

        public void SetBorderColor(Vector4 color)
        {
            GL.TextureParameter(ID, TextureParameterName.TextureBorderColor, new float[] { color.X, color.Y, color.Z, color.W });
        }

        public void SetMipmapLodBias(float bias)
        {
            GL.TextureParameter(ID, TextureParameterName.TextureLodBias, bias);
        }

        public void Allocate(int width, int height, int depth, PixelInternalFormat pixelInternalFormat)
        {
            Bind();
            int[] oneDTargets = (int[])Enum.GetValues(typeof(TextureTarget1d));
            int[] twoDTargets = (int[])Enum.GetValues(typeof(TextureTarget2d));
            if (oneDTargets.Contains((int)TextureTarget))
            {
                GL.TexImage1D(TextureTarget, 0, pixelInternalFormat, width, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width;
            }
            else if (twoDTargets.Contains((int)TextureTarget))
            {
                if (TextureTarget == TextureTarget.TextureCubeMap)
                    for (int i = 0; i < 6; i++)
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, pixelInternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                else
                    GL.TexImage2D(TextureTarget, 0, pixelInternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width; Height = height;
            }
            else
            {
                GL.TexImage3D(TextureTarget, 0, pixelInternalFormat, width, height, depth, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                Width = width; Height = height; Depth = depth;
            }
            PixelInternalFormat = pixelInternalFormat;
        }

        public void Allocate(int width, int height, int depth, PixelInternalFormat pixelInternalFormat, IntPtr intPtr, PixelFormat pixelFormat, PixelType pixelType)
        {
            Bind();
            int[] oneDTargets = (int[])Enum.GetValues(typeof(TextureTarget1d));
            int[] twoDTargets = (int[])Enum.GetValues(typeof(TextureTarget2d));
            if (oneDTargets.Contains((int)TextureTarget))
            {
                GL.TexImage1D(TextureTarget, 0, pixelInternalFormat, width, 0, pixelFormat, pixelType, intPtr);
                Width = width;
            }
            else if (twoDTargets.Contains((int)TextureTarget))
            {
                GL.TexImage2D(TextureTarget, 0, pixelInternalFormat, width, height, 0, pixelFormat, pixelType, intPtr);
                Width = width; Height = height;
            }
            else
            {
                GL.TexImage3D(TextureTarget, 0, pixelInternalFormat, width, height, depth, 0, pixelFormat, pixelType, intPtr);
                Width = width; Height = height; Depth = depth;
            }
            PixelInternalFormat = pixelInternalFormat;
        }

        public long GetTextureBindlessHandle()
        {
            long textureHandle = GL.Arb.GetTextureHandle(ID);
            GL.Arb.MakeTextureHandleResident(textureHandle);
            return textureHandle;
        }
        public static bool UnmakeTextureBindless(long textureHandle)
        {
            if (GL.Arb.IsTextureHandleResident(textureHandle))
            {
                GL.Arb.MakeTextureHandleNonResident(textureHandle);
                return true;
            }
            return false;
        }

        public long GetImageBindlessHandle(int level, bool layered, int layer, PixelFormat pixelFormat, TextureAccess textureAccess)
        {
            long imageHandle = GL.Arb.GetImageHandle(ID, level, layered, layer, pixelFormat);
            GL.Arb.MakeImageHandleResident(imageHandle, (All)textureAccess);
            return imageHandle;
        }
        public static bool UnmakeImageBindless(long imageHandle)
        {
            if (GL.Arb.IsImageHandleResident(imageHandle))
            {
                GL.Arb.MakeImageHandleNonResident(imageHandle);
                return true;
            }
            return false;
        }

        public Bitmap GetTextureContent(int mipmapLevel = 0)
        {
            if (!TryGetSizeMipmap(out int width, out int height, mipmapLevel))
                throw new ArgumentException($"Can not get size from Texture {ID} at level {mipmapLevel}");

            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.GetTextureImage(ID, mipmapLevel, PixelFormat.Bgra, PixelType.UnsignedByte, GetPixelSize(mipmapLevel) * width * height, bmpData.Scan0);
            GL.Finish();

            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        public bool TryGetSizeMipmap(out int width, out int height, int mipmapLevel = 0)
        {
            GL.GetTextureLevelParameter(ID, mipmapLevel, GetTextureParameter.TextureWidth, out width);
            GL.GetTextureLevelParameter(ID, mipmapLevel, GetTextureParameter.TextureHeight, out height);

            return width * height != 0;
        }

        public int GetPixelTypeComponentSize(PixelTypeSize pixelTypeSize, int mipmapLevel = 0)
        {
            GL.GetTextureLevelParameter(ID, mipmapLevel, (GetTextureParameter)pixelTypeSize, out int bitSize);
            return bitSize / 8;
        }

        public int GetPixelSize(int mipmapLevel = 0)
        {
            int r = GetPixelTypeComponentSize(PixelTypeSize.TextureRedSize, mipmapLevel);
            int g = GetPixelTypeComponentSize(PixelTypeSize.TextureGreenSize, mipmapLevel);
            int b = GetPixelTypeComponentSize(PixelTypeSize.TextureBlueSize, mipmapLevel);
            int a = GetPixelTypeComponentSize(PixelTypeSize.TextureAlphaSize, mipmapLevel);

            return r + g + b + a;
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
        }
    }
}

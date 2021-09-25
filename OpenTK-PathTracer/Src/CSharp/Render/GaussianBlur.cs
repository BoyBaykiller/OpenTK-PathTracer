using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class GaussianBlur
    {
        public readonly Query Query;
        private readonly ShaderProgram shaderProgram;
        public readonly Texture Result;
        public GaussianBlur(int width, int height)
        {
            Query = new Query(600);

            Result = new Texture(TextureTarget2d.Texture2D);
            Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba8);

            shaderProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "Res/Shaders/GaussianBlur/compute.glsl".GetPathContent()));
        }

        /// <summary>
        /// Assumes that <seealso cref="Result"/> and <paramref name="src"/> have the same size
        /// </summary>
        /// <param name="src"></param>
        public void Run(Texture src)
        {
            Query.Start();
            shaderProgram.Use();

            for (int i = 0; i < 30; i++)
            {
                shaderProgram.Upload(0, i % 2 == 0);
                (i % 2 == 0 ? src : Result).AttachImage(0, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba8);
                (i % 2 == 0 ? Result : src).AttachImage(1, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
                GL.DispatchCompute((int)MathF.Ceiling(src.Width * src.Height / 32.0f), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            }
            
            Query.StopAndReset();
        }

        public void SetSize(int width, int height)
        {
            Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
        }
    }
}

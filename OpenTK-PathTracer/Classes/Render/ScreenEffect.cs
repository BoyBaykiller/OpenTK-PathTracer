using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class ScreenEffect : RenderEffect
    {
        public ScreenEffect(Shader fragmentShader, int width, int height)
        {
            if (fragmentShader.ShaderType != ShaderType.FragmentShader)
                throw new System.ArgumentException("ScreenEffect: Only pass in shaders of type FragmentShader");

            Framebuffer = new Framebuffer();
            Result = Texture.GetTexture2D(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba, PixelFormat.Rgba, width, height, false);

            Framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);

            Program = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\screenQuad.vs"), fragmentShader });
        }

        public override void Run(params object[] textureArr)
        {
            Query.Start();

            GL.Viewport(0, 0, Result.Width, Result.Height);

            Framebuffer.Bind();
            Program.Use();

            for (int i = 0; i < textureArr.Length; i++)
                ((Texture)textureArr[i]).AttachToUnit(i);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.SetTexImage(width, height);
        }
    }
}

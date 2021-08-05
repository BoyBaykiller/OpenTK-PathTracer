using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class ScreenEffect : RenderEffect
    {
        public ScreenEffect(Shader fragmentShader, int width, int height)
        {
            //Query = new Query(1000);
            
            framebuffer = new Framebuffer();
            Result = Texture.GetTexture2D(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgb, PixelFormat.Rgb, width, height);
            framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);

            program = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\screenQuad.vs"), fragmentShader });
        }

        public override void Run(params object[] textureArr)
        {
            //Query.Start();
            GL.Viewport(0, 0, Result.Width, Result.Height);

            framebuffer.Bind();
            program.Use();

            for (int i = 0; i < textureArr.Length; i++)
                ((Texture)textureArr[i]).AttachToUnit(i);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.SetTexImage(width, height);
        }
    }
}

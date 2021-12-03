using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class ScreenEffect
    {
        private readonly Framebuffer framebuffer;
        public readonly Texture Result;
        private readonly ShaderProgram shaderProgram;
        public ScreenEffect(Shader fragmentShader, int width, int height)
        {
            if (fragmentShader.ShaderType != ShaderType.FragmentShader)
                throw new System.ArgumentException($"Only pass in shaders of type {ShaderType.FragmentShader}");

            framebuffer = new Framebuffer();

            Result = new Texture(TextureTarget2d.Texture2D);
            Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba8);

            framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
            
            shaderProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, "res/shaders/screenQuad.glsl".GetPathContent()), fragmentShader);
        }

        public void Render(params Texture[] textures)
        {
            framebuffer.Bind();
            shaderProgram.Use();

            for (int i = 0; i < textures.Length; i++)
                textures[i].AttachSampler(i);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
        }

        public void SetSize(int width, int height)
        {
            Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
        }
    }
}

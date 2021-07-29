using System;

using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class Final : RenderEffect
    {
        private readonly ShaderProgram outputScreenProgram;
        public Final()
        {
            Query = new Query(1000);
            outputScreenProgram = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\ToScreen\screenQuad.vs"), new Shader(ShaderType.FragmentShader, @"Src\Shaders\ToScreen\fragment.frag") });
            Result = Texture.Zero; // no result. output will directly be written to screen for this class
        }

        public override void Run(params object[] param)
        {
            //Query.Start();
            GL.Viewport(0, 0, Result.Width, Result.Height);

            Framebuffer.Bind(0, FramebufferTarget.DrawFramebuffer);
            outputScreenProgram.Use();
            ((Texture)param[0]).AttachToUnit(0);
            ((Texture)param[1]).AttachToUnit(1);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.SetTexImage(width, height);
        }
    }
}

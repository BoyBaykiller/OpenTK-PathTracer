using OpenTK;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class Rasterizer : RenderEffect
    {
        public int Width { get => Result.Width; }
        public int Height { get => Result.Height; }

        public Framebuffer Framebuffer { get; private set; } = new Framebuffer();
        
        private readonly ShaderProgram program;
        private readonly VAO vao;
        private readonly BufferObject vbo;
        public Rasterizer(int width, int height)
        {
            Query = new Query(1000);
            Result = Texture.GetTexture2D(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgb, PixelFormat.Rgb, width, height);
            Framebuffer.SetRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
            program = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\Rasterizer\vertex.vs"), new Shader(ShaderType.FragmentShader, @"Src\Shaders\Rasterizer\fragment.frag") });
            vao = new VAO();
            {
                vbo = new BufferObject(BufferTarget.ArrayBuffer, vertecis.Length * sizeof(float), BufferUsageHint.StaticDraw);
                vbo.SubData(0, vertecis.Length * sizeof(float), vertecis);
                vao.SetAttribPointer(0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0 * sizeof(float));
            }
        }

        public override void Run(params object[] arrAABB)
        {
            //Query.Start();

            Framebuffer.Bind();
            program.Use();
            vao.Bind();

            //GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            AABB[] aabbs = (AABB[])arrAABB[0];

            for (int i = 0; i < aabbs.Length; i++)
            {
                //Matrix4 model = Matrix4.CreateScale(aabbs[i].Dimensions) * Matrix4.CreateTranslation(aabbs[i].Position);
                Matrix4 model = Matrix4.CreateScale(aabbs[i].Max - aabbs[i].Min) * Matrix4.CreateTranslation(aabbs[i].Position);
                program.Upload(0, model);
                GL.DrawArrays(PrimitiveType.Quads, 0, vertecis.Length / 3);
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.SetTexImage(width, height);
        }

        /// <summary>
        /// Represents the coordinates of a simple unit cube
        /// </summary>
        private static readonly float[] vertecis = new float[]
        {
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f, 

                 // Front
                -0.5f,  0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f, 

                 // Left
                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,

                 // Right
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,

                 // Up
                -0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,

                 // Down
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f, -0.5f,
        };
    }
}

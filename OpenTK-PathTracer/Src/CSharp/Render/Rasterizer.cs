using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class Rasterizer
    {
        public readonly Framebuffer Framebuffer;
        public readonly Texture Result;
        private readonly ShaderProgram shaderProgram;
        private readonly BufferObject vbo;
        private readonly VAO vao;
        
        public Rasterizer(int width, int height)
        {
            Framebuffer = new Framebuffer();
           
            Result = new Texture(TextureTarget2d.Texture2D);
            Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba8);

            Framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);

            shaderProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, "Res/Shaders/Rasterisation/vertex.glsl".GetPathContent()), new Shader(ShaderType.FragmentShader, "Res/Shaders/Rasterisation/fragment.glsl".GetPathContent()));

            vbo = new BufferObject(BufferUsageHint.StaticDraw);
            vbo.Allocate(unitCubeVerts.Length * sizeof(float), unitCubeVerts);
            
            vao = new VAO(vbo, 3 * sizeof(float));
            vao.SetAttribFormat(0, 3, VertexAttribType.Float, 0);
        }

        public void Run(params AABB[] aabbs)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            Framebuffer.Clear(ClearBufferMask.ColorBufferBit);
            shaderProgram.Use();
            vao.Bind();
            for (int i = 0; i < aabbs.Length; i++)
            {
                Matrix4 model = Matrix4.CreateScale(aabbs[i].Dimensions) * Matrix4.CreateTranslation(aabbs[i].Position);
                
                shaderProgram.Upload(0, model);
                GL.DrawArrays(PrimitiveType.Quads, 0, unitCubeVerts.Length / 3);
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public void SetSize(int width, int height)
        {
            Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
        }

        private static readonly float[] unitCubeVerts = new float[]
        {
                // Back
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

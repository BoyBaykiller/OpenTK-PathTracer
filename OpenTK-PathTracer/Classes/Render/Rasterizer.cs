using OpenTK;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;
using System.Drawing;

namespace OpenTK_PathTracer.Render
{
    class Rasterizer : RenderEffect
    {
        private readonly VAO vao;
        private readonly BufferObject vbo;
        public Rasterizer(int width, int height)
        {
            Framebuffer = new Framebuffer();
           
            Result = new Texture(TextureTarget.Texture2D, TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba, PixelFormat.Rgba, false);
            Result.Allocate(width, height);
            
            Framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
            
            Program = new ShaderProgram(new Shader(ShaderType.VertexShader, @"Src\Shaders\Rasterizer\vertex.vert"), new Shader(ShaderType.FragmentShader, @"Src\Shaders\Rasterizer\fragment.frag"));

            vao = new VAO();
            {
                vbo = new BufferObject(BufferTarget.ArrayBuffer, unitCubeVerts.Length * sizeof(float), BufferUsageHint.StaticDraw);
                vbo.Append(unitCubeVerts.Length * sizeof(float), unitCubeVerts);
                vao.SetAttribPointer(0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0 * sizeof(float));
            }
        }

        public override void Run(params object[] aabbArr)
        {
            //Query.Start();
            Framebuffer.Clear(ClearBufferMask.ColorBufferBit);
            vao.Bind();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            
            AABB[] aabbs = (AABB[])aabbArr[0];
            for (int i = 0; i < aabbs.Length; i++)
            {
                Matrix4 model = Matrix4.CreateScale(aabbs[i].Dimensions) * Matrix4.CreateTranslation(aabbs[i].Position);

                Program.Upload(0, model);
                GL.DrawArrays(PrimitiveType.Quads, 0, unitCubeVerts.Length / 3);
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.Allocate(width, height);
            //Result.SetTexImage2D(new Bitmap(width, height));
            Result.Clear();
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

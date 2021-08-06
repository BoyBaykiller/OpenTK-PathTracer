﻿using OpenTK;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class Rasterizer : RenderEffect
    {
        private readonly VAO vao;
        private readonly BufferObject vbo;
        public Rasterizer(int width, int height)
        {
            //Query = new Query(1000);
            
            framebuffer = new Framebuffer();
            Result = Texture.GetTexture2D(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgb, PixelFormat.Rgb, width, height);
            framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
            
            program = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\Rasterizer\vertex.vs"), new Shader(ShaderType.FragmentShader, @"Src\Shaders\Rasterizer\fragment.frag") });

            vao = new VAO();
            {
                vbo = new BufferObject(BufferTarget.ArrayBuffer, unitCubeVerts.Length * sizeof(float), BufferUsageHint.StaticDraw);
                vbo.SubData(0, unitCubeVerts.Length * sizeof(float), unitCubeVerts);
                vao.SetAttribPointer(0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), 0 * sizeof(float));
            }
            
        }

        public override void Run(params object[] aabbArr)
        {
            //Query.Start();
            framebuffer.Clear(ClearBufferMask.ColorBufferBit);
            program.Use();
            vao.Bind();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            
            AABB[] aabbs = (AABB[])aabbArr[0];
            for (int i = 0; i < aabbs.Length; i++)
            {
                //Matrix4 model = Matrix4.CreateScale(aabbs[i].Dimensions) * Matrix4.CreateTranslation(aabbs[i].Position);
                Matrix4 model = Matrix4.CreateScale(aabbs[i].Max - aabbs[i].Min) * Matrix4.CreateTranslation(aabbs[i].Position);
                program.Upload(0, model);
                GL.DrawArrays(PrimitiveType.Quads, 0, unitCubeVerts.Length / 3);
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            Result.SetTexImage(width, height);
        }

        private static readonly float[] unitCubeVerts = new float[]
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
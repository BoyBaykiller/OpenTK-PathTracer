#define _USE_COMPUTE
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    class PathTracer
    {
        private int _numSpheres;
        public int NumSpheres
        {
            get => _numSpheres;

            set
            {
                _numSpheres = value;
                shaderProgram.Upload("uboGameObjectsSize", new Vector2(value, NumCuboids));
            }
        }


        private int _numCuboids;
        public int NumCuboids
        {
            get => _numCuboids;

            set
            {
                _numCuboids = value;
                shaderProgram.Upload("uboGameObjectsSize", new Vector2(NumSpheres, value));
            }
        }


        private int _rayDepth;
        public int RayDepth
        {
            get => _rayDepth;

            set
            {
                _rayDepth = value;
                shaderProgram.Upload("rayDepth", value);
            }
        }

        private int _spp;
        public int SPP
        {
            get => _spp;

            set
            {
                _spp = value;
                shaderProgram.Upload("SPP", value);
            }
        }

        private float _focalLength;
        public float FocalLength
        {
            get => _focalLength;

            set
            {
                _focalLength = value;
                shaderProgram.Upload("focalLength", value);
            }
        }

        private float _apertureDiameter;
        public float ApertureDiameter
        {
            get => _apertureDiameter;

            set
            {
                _apertureDiameter = value;
                shaderProgram.Upload("apertureDiameter", value);
            }
        }

        public Texture EnvironmentMap;
        public readonly Texture Result;
#if USE_COMPUTE
        private static readonly ShaderProgram shaderProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, File.ReadAllText("res/shaders/PathTracing/compute.glsl")));
#else
        private readonly Framebuffer framebuffer;
        private static readonly ShaderProgram shaderProgram = new ShaderProgram(
            new Shader(ShaderType.VertexShader, File.ReadAllText("res/shaders/screenQuad.glsl")),
            new Shader(ShaderType.FragmentShader, File.ReadAllText("res/shaders/PathTracing/fragCompute.glsl")));
#endif
        public PathTracer(Texture environmentMap, int width, int height, int rayDepth, int spp, float focalLength, float apertureDiamater)
        {
            Result = new Texture(TextureTarget2d.Texture2D);
            Result.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba32f);
#if !USE_COMPUTE
            framebuffer = new Framebuffer();
            framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
#endif

            RayDepth = rayDepth;
            SPP = spp;
            FocalLength = focalLength;
            ApertureDiameter = apertureDiamater;
            EnvironmentMap = environmentMap;
        }

        public int Samples => thisRenderNumFrame * SPP;
        private int thisRenderNumFrame;
        public void Render()
        {
            shaderProgram.Use();
            shaderProgram.Upload(0, thisRenderNumFrame++);
            EnvironmentMap.AttachSampler(1);
#if USE_COMPUTE
            Result.AttachImage(0, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.DispatchCompute((Result.Width + 8 - 1) / 8, (Result.Height + 8 - 1) / 8, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
#else
            framebuffer.Bind();
            Result.AttachSampler(0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
#endif
        }

        public void SetSize(int width, int height)
        {
            thisRenderNumFrame = 0;
            Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
        }

        public void ResetRenderer()
        {
            thisRenderNumFrame = 0;
        }
    }
}
#define USE_COMPUTE
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
#if !USE_COMPUTE
        private readonly Framebuffer framebuffer;
#endif
        private readonly ShaderProgram shaderProgram;
        public PathTracer(Texture environmentMap, int width, int height, int rayDepth, int spp, float focalLength, float apertureDiamater)
        {
            Result = new Texture(TextureTarget2d.Texture2D);
            Result.MutableAllocate(width, height, 1, PixelInternalFormat.Rgba32f);

#if USE_COMPUTE
            shaderProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "Res/Shaders/PathTracing/compute.glsl".GetPathContent()));
#else
            framebuffer = new Framebuffer();
            framebuffer.AddRenderTarget(FramebufferAttachment.ColorAttachment0, Result);
            shaderProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, "Res/Shaders/screenQuad.glsl".GetPathContent()), new Shader(ShaderType.FragmentShader, "Res/Shaders/PathTracing/fragCompute.glsl".GetPathContent()));
#endif

            RayDepth = rayDepth;
            SPP = spp;
            FocalLength = focalLength;
            ApertureDiameter = apertureDiamater;
            EnvironmentMap = environmentMap;
        }

        public int Samples => ThisRenderNumFrame * SPP;
        public int ThisRenderNumFrame;
        public void Run()
        {
            shaderProgram.Use();
            shaderProgram.Upload(0, ThisRenderNumFrame++);
            EnvironmentMap.AttachSampler(1);
#if USE_COMPUTE
            Result.AttachImage(0, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.DispatchCompute((Result.Width * Result.Height + 32 - 1) / 32, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);
#else
            framebuffer.Bind();
            Result.AttachImage(0, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
#endif
        }

        public void SetSize(int width, int height)
        {
            ThisRenderNumFrame = 0;
            Result.MutableAllocate(width, height, 1, Result.PixelInternalFormat);
        }
    }
}
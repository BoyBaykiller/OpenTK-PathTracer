using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    class PathTracer : RenderEffectBase
    {
        private int _numSpheres;
        public int NumSpheres
        {
            get => _numSpheres;

            set
            {
                _numSpheres = value;
                Program.Upload("uboGameObjectsSize", new Vector2(value, NumCuboids));
            }
        }


        private int _numCuboids;
        public int NumCuboids
        {
            get => _numCuboids;

            set
            {
                _numCuboids = value;
                Program.Upload("uboGameObjectsSize", new Vector2(NumSpheres, value));
            }
        }


        private int _rayDepth;
        public int RayDepth
        {
            get => _rayDepth;

            set
            {
                _rayDepth = value;
                Program.Upload("rayDepth", value);
            }
        }

        private int _ssp;
        public int SSP
        {
            get => _ssp;

            set
            {
                _ssp = value;
                Program.Upload("SSP", value);
            }
        }

        private float _focalLength;
        public float FocalLength
        {
            get => _focalLength;

            set
            {
                _focalLength = value;
                Program.Upload("focalLength", value);
            }
        }

        private float _apertureRadius;
        public float ApertureDiameter
        {
            get => _apertureRadius;

            set
            {
                _apertureRadius = value;
                Program.Upload("apertureDiameter", value);
            }
        }

        public readonly EnvironmentMap EnvironmentMap;
        public PathTracer(EnvironmentMap environmentMap, int width, int height, int rayDepth, int ssp, float focalLength, float apertureRadius)
        {
            Result = new Texture(TextureTarget.Texture2D, TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, false);
            Result.Allocate(width, height);

            Program = new ShaderProgram(new Shader(ShaderType.ComputeShader, @"Src\Shaders\PathTracing\compute.comp"));

            // Testing ARB_bindless_texture
            BufferObject bufferObject = new BufferObject(BufferRangeTarget.UniformBuffer, 2, 1 * Vector4.SizeInBytes, BufferUsageHint.StaticDraw);
            environmentMap.CubemapTexture.MakeBindless();
            environmentMap.CubemapTexture.MakeResident();

            bufferObject.Append(Vector4.SizeInBytes, environmentMap.CubemapTexture.TextureHandle);

            RayDepth = rayDepth;
            SSP = ssp;
            FocalLength = focalLength;
            ApertureDiameter = apertureRadius;
            EnvironmentMap = environmentMap;
        }

        public int Samples => ThisRenderNumFrame * SSP;
        public int ThisRenderNumFrame;
        public override void Run(params object[] _)
        {
            //Query.Start();

            Program.Use();
            Program.Upload(0, ThisRenderNumFrame++);
            
            Result.AttachToImageUnit(0, 0, false, 0, TextureAccess.ReadWrite, (SizedInternalFormat)Result.PixelInternalFormat);

            GL.DispatchCompute((int)MathF.Ceiling(Width * Height / 32.0f), 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            ThisRenderNumFrame = 0;
            base.SetSize(width, height);
        }
    }
}
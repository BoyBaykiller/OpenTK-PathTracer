using System;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    class PathTracing : RenderEffect
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
        public float ApertureRadius
        {
            get => _apertureRadius;

            set
            {
                _apertureRadius = value;
                Program.Upload("apertureRadius", value);
            }
        }

        public readonly EnvironmentMap environmentMap;
        public PathTracing(EnvironmentMap environmentMap, int width, int height, int rayDepth, int ssp, float focalLength, float apertureRadius)
        {
            //Query = new Query(1000);

            Result = Texture.GetTexture2D(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba32f, PixelFormat.Rgb, width, height);
            Program = new ShaderProgram(new Shader[] { new Shader(ShaderType.ComputeShader, @"Src\Shaders\PathTracing\compute.comp") });


            RayDepth = rayDepth;
            SSP = ssp;
            FocalLength = focalLength;
            ApertureRadius = apertureRadius;
            this.environmentMap = environmentMap;
        }

        public int Samples => ThisRenderNumFrame * SSP;
        public int ThisRenderNumFrame;
        public override void Run(params object[] _)
        {
            //Query.Start();

            Program.Upload(0, ++ThisRenderNumFrame);
            Result.AttchToImageUnit(0, 0, false, 0, TextureAccess.ReadWrite, (SizedInternalFormat)Result.PixelInternalFormat);
            environmentMap.CubemapTexture.AttachToUnit(0);
            
            //GL.DispatchCompute((int)MathF.Ceiling(Width / 8.0f), (int)MathF.Ceiling(Height / 4.0f), 1);
            GL.DispatchCompute((int)MathF.Ceiling(Width * Height / 32.0f), 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            ThisRenderNumFrame = 0;
            Result.SetTexImage(width, height);
        }
    }
}

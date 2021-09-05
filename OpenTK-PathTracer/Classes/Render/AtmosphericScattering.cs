using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class AtmosphericScattering : RenderEffect
    {
        private readonly BufferObject bufferObject;


        private int _inScatteringSamples;
        public int InScatteringSamples
        {
            get => _inScatteringSamples;

            set
            {
                Program.Upload("inScatteringSamples", value);
                _inScatteringSamples = value;
            }
        }

        private int _densitySamples;
        public int DensitySamples
        {
            get => _densitySamples;

            set
            {
                Program.Upload("densitySamples", value);
                _densitySamples = value;
            }
        }

        private float _scatteringStrength;
        public float ScatteringStrength
        {
            get => _scatteringStrength;

            set
            {
                Program.Upload("scatteringStrength", value);
                _scatteringStrength = value;
            }
        }


        private float _densityFallOff;
        public float DensityFallOff
        {
            get => _densityFallOff;

            set
            {
                Program.Upload("densityFallOff", value);
                _densityFallOff = value;
            }
        }


        private float _atmossphereRadius;
        public float AtmossphereRadius
        {
            get => _atmossphereRadius;

            set
            {
                Program.Upload("atmossphereRad", value);
                _atmossphereRadius = value;
            }
        }


        private Vector3 _waveLenghts;
        public Vector3 WaveLengths
        {
            get => _waveLenghts;

            set
            {
                Program.Upload("waveLengths", value);
                _waveLenghts = value;
            }
        }

        private Vector3 _lightPos;
        public Vector3 LightPos
        {
            get => _lightPos;

            set
            {
                Program.Upload("lightPos", value);
                _lightPos = value;
            }
        }

        public AtmosphericScattering(int size, int inScatteringSamples, int densitySamples, float scatteringStrength, float densityFallOff, float atmossphereRadius, Vector3 waveLengths, Vector3 lightPos)
        {
            Result = new Texture(TextureTarget.TextureCubeMap, TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, false);
            Result.Allocate(size, size);

            Program = new ShaderProgram(new Shader(ShaderType.ComputeShader, @"Src\Shaders\AtmosphericScattering\compute.comp"));
            bufferObject = new BufferObject(BufferRangeTarget.UniformBuffer, 3, Vector4.SizeInBytes * 4 * 7 + Vector4.SizeInBytes, BufferUsageHint.StreamRead);

            Matrix4 invProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1, 0.1f, 10f).Inverted();
            Matrix4[] invViews = new Matrix4[]
            {
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)), // PositiveX
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)), // NegativeX
               
                // Fix: Conventions say that these should be reversed. Am I doing smth reversed in shader?! (May be reversed other GPUs)
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)), // NegativeY
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)), // PositiveY

                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)), // PositiveZ
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)), // NegativeZ
            };
            Vector3 position = new Vector3(20.43f, -201.99f, -20.67f);

            bufferObject.Append(Vector4.SizeInBytes * 4, invProjection);
            bufferObject.Append(Vector4.SizeInBytes * 4 * invViews.Length, invViews);
            bufferObject.Append(Vector4.SizeInBytes, new Vector4(position, 1.0f));

            InScatteringSamples = inScatteringSamples;
            DensitySamples = densitySamples;
            ScatteringStrength = scatteringStrength;
            DensityFallOff = densityFallOff;
            AtmossphereRadius = atmossphereRadius;
            WaveLengths = waveLengths;
            LightPos = lightPos;
        }

        public override void Run(params object[] viewPos)
        {
            //Query.Start();

            Result.AttachToImageUnit(0, 0, true, 0, TextureAccess.WriteOnly, (SizedInternalFormat)Result.PixelInternalFormat);
            Program.Use();

            if (viewPos.Length == 1)
                bufferObject.SubData(Vector4.SizeInBytes * 4 * 7, Vector4.SizeInBytes, new Vector4((Vector3)viewPos[0], 1.0f));

            GL.DispatchCompute((int)MathF.Ceiling(Width / 32.0f), (int)MathF.Ceiling(Width / 32.0f), 6);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            if (width != height)
            {
                Console.WriteLine("AtmosphericScattering: Cubemaps must be squares");
                return;
            }

            Result.Allocate(width, height);
        }
    }
}

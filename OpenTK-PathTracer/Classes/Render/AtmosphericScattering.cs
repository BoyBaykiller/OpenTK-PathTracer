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
                program.Upload("inScatteringSamples", value);
                _inScatteringSamples = value;
            }
        }

        private int _densitySamples;
        public int DensitySamples
        {
            get => _densitySamples;

            set
            {
                program.Upload("densitySamples", value);
                _densitySamples = value;
            }
        }

        private float _densityFallOff;
        public float DensityFallOff
        {
            get => _densityFallOff;

            set
            {
                program.Upload("densityFallOff", value);
                _densityFallOff = value;
            }
        }

        private float _atmossphereRadius;
        public float AtmossphereRadius
        {
            get => _atmossphereRadius;

            set
            {
                program.Upload("atmossphereRad", value);
                _atmossphereRadius = value;
            }
        }

        private Vector3 _waveLenghts;
        public Vector3 WaveLengths
        {
            get => _waveLenghts;

            set
            {
                program.Upload("waveLengths", value);
                _waveLenghts = value;
            }
        }

        public AtmosphericScattering(int size, int inScatteringSamples, int densitySamples, float densityFallOff, float atmossphereRadius, Vector3 waveLengths)
        {
            Query = new Query(1000);

            Result = Texture.GetTextureCubeMap(TextureWrapMode.ClampToBorder, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, size);
            program = new ShaderProgram(new Shader[] { new Shader(ShaderType.ComputeShader, @"Src\Shaders\AtmosphericScattering\compute.comp") });
            bufferObject = new BufferObject(BufferRangeTarget.UniformBuffer, 2, Vector4.SizeInBytes * 4 * 7 + Vector4.SizeInBytes, BufferUsageHint.StaticDraw);

            Matrix4 invProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1, 0.1f, 100f).Inverted();
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
            Vector3 position = new Vector3(20.43f, -200.249f, -20.67f);

            bufferObject.SubData(0, Vector4.SizeInBytes * 4, invProjection);
            bufferObject.Append(Vector4.SizeInBytes * 4 * invViews.Length, invViews);
            bufferObject.Append(Vector4.SizeInBytes, new Vector4(position, 1.0f));

            InScatteringSamples = inScatteringSamples;
            DensitySamples = densitySamples;
            DensityFallOff = densityFallOff;
            AtmossphereRadius = atmossphereRadius;
            WaveLengths = waveLengths;
        }

        public override void Run(params object[] viewPos)
        {
            Query.Start();

            Result.AttchToImageUnit(0, 0, true, 0, TextureAccess.WriteOnly, (SizedInternalFormat)Result.PixelInternalFormat);
            program.Use();
            GL.DispatchCompute((int)MathF.Ceiling(Width / 32.0f), (int)MathF.Ceiling(Width / 32.0f), 6);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            if (viewPos.Length == 1)
                bufferObject.SubData(Vector4.SizeInBytes * 4 * 7, Vector4.SizeInBytes, new Vector4((Vector3)viewPos[0], 1.0f));

            Query.StopAndReset();
        }

        public override void SetSize(int width, int height)
        {
            if (width != height)
            {
                Console.WriteLine("AtmosphericScattering: Cubemaps must be squares");
                return;
            }

            Result.SetTexImage(width, height);
            Matrix4 invProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1, 0.1f, 100f).Inverted();
            bufferObject.SubData(0, Vector4.SizeInBytes * 4, invProjection);
        }
    }
}

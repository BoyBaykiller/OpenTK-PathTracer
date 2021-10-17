using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    class AtmosphericScattering
    {
        private int _inScatteringSamples;
        public int InScatteringSamples
        {
            get => _inScatteringSamples;

            set
            {
                shaderProgram.Upload("inScatteringSamples", value);
                _inScatteringSamples = value;
            }
        }


        private int _densitySamples;
        public int DensitySamples
        {
            get => _densitySamples;

            set
            {
                shaderProgram.Upload("densitySamples", value);
                _densitySamples = value;
            }
        }


        private float _scatteringStrength;
        public float ScatteringStrength
        {
            get => _scatteringStrength;

            set
            {
                shaderProgram.Upload("scatteringStrength", value);
                _scatteringStrength = value;
            }
        }


        private float _densityFallOff;
        public float DensityFallOff
        {
            get => _densityFallOff;

            set
            {
                shaderProgram.Upload("densityFallOff", value);
                _densityFallOff = value;
            }
        }


        private float _atmosphereRadius;
        public float AtmosphereRadius
        {
            get => _atmosphereRadius;

            set
            {
                shaderProgram.Upload("atmosphereRad", value);
                _atmosphereRadius = value;
            }
        }


        private Vector3 _waveLenghts;
        public Vector3 WaveLengths
        {
            get => _waveLenghts;

            set
            {
                shaderProgram.Upload("waveLengths", value);
                _waveLenghts = value;
            }
        }


        private Vector3 _lightPos;
        public Vector3 LightPos
        {
            get => _lightPos;

            set
            {
                shaderProgram.Upload("lightPos", value);
                _lightPos = value;
            }
        }


        private Vector3 _viewPos;
        public Vector3 ViewPos
        {
            get => _viewPos;

            set
            {
                shaderProgram.Upload("viewPos", value);
                _viewPos = value;
            }
        }

        public readonly Query Query;
        public readonly Texture Result;
        private readonly ShaderProgram shaderProgram;
        private readonly BufferObject bufferObject;
        public AtmosphericScattering(int size, int inScatteringSamples, int densitySamples, float scatteringStrength, float densityFallOff, float atmosphereRadius, Vector3 waveLengths, Vector3 lightPos, Vector3 viewPos)
        {
            Query = new Query(600);

            Result = new Texture(TextureTarget2d.TextureCubeMap);
            Result.MutableAllocate(size, size, 1, PixelInternalFormat.Rgba32f);

            shaderProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "Res/Shaders/AtmosphericScattering/compute.glsl".GetPathContent()));
            
            bufferObject = new BufferObject(BufferRangeTarget.UniformBuffer, 3);
            bufferObject.MutableAllocate(Vector4.SizeInBytes * 4 * 7 + Vector4.SizeInBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);

            Matrix4 invProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1, 0.1f, 10f).Inverted();
            Matrix4[] invViews = new Matrix4[]
            {
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)), // PositiveX
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)), // NegativeX
               
                // Fix: Conventions say that these should be reversed. Am I doing smth reversed in shader?!
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)), // NegativeY
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)), // PositiveY

                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)), // PositiveZ
                Camera.GenerateMatrix(Vector3.Zero, new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)), // NegativeZ
            };

            bufferObject.Append(Vector4.SizeInBytes * 4, invProjection);
            bufferObject.Append(Vector4.SizeInBytes * 4 * invViews.Length, invViews);

            InScatteringSamples = inScatteringSamples;
            DensitySamples = densitySamples;
            ScatteringStrength = scatteringStrength;
            DensityFallOff = densityFallOff;
            AtmosphereRadius = atmosphereRadius;
            WaveLengths = waveLengths;
            LightPos = lightPos;
            ViewPos = viewPos;
        }


        /// <summary>
        /// This method computes a whole cubemap rather than just whats visible. It is meant for precomputation and should not be called frequently for performance reasons
        /// </summary>
        /// <param name="renderParams"></param>
        public void Run()
        {
            Query.Start();

            Result.AttachImage(0, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
            shaderProgram.Use();

            GL.DispatchCompute((int)MathF.Ceiling(Result.Width / 8.0f), (int)MathF.Ceiling(Result.Width / 8.0f), 6);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);

            Query.StopAndReset();
        }

        public void SetSize(int width, int height)
        {
            if (width != height)
            {
                Console.WriteLine($"Cubemaps must be squares");
                return;
            }

            Result.MutableAllocate(width, width, 1, Result.PixelInternalFormat);
        }
    }
}

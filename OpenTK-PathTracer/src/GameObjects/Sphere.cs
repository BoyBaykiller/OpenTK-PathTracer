using System;
using OpenTK;

namespace OpenTK_PathTracer.GameObjects
{
    class Sphere : BaseGameObject
    {
        public const int GPU_INSTANCE_SIZE = 16 + Material.GPU_INSTANCE_SIZE;
        
        public int Instance;
        public float Radius;
        public Sphere(Vector3 position, float radius, int instance, Material material)
        {
            Position = position;
            Radius = radius;
            Material = material;
            Instance = instance;
        }

        public override int BufferOffset => 0 + Instance * GPU_INSTANCE_SIZE;

        private readonly Vector4[] gpuData = new Vector4[GPU_INSTANCE_SIZE / Vector4.SizeInBytes];
        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Position;
            gpuData[0].W = Radius;

            Array.Copy(Material.GetGPUFriendlyData(), 0, gpuData, 1, gpuData.Length - 1);

            return gpuData;
        }

        public override bool IntersectsRay(Ray ray, out float t1, out float t2)
        {
            // Source: https://antongerdelan.net/opengl/raycasting.html
            t1 = t2 = 0;

            Vector3 sphereToRay = ray.Origin - this.Position;
            float b = Vector3.Dot(ray.Direction, sphereToRay);
            float c = Vector3.Dot(sphereToRay, sphereToRay) - this.Radius * this.Radius;
            float discriminant = b * b - c;
            if (discriminant < 0)
                return false; // only imaginary collision

            float squareRoot = MathF.Sqrt(discriminant);
            t1 = -b - squareRoot;
            t2 = -b + squareRoot;

            return true;
        }
    }
}

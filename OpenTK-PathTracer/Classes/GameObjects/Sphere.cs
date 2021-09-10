using System;
using OpenTK;

namespace OpenTK_PathTracer.GameObjects
{
    class Sphere : GameObjectBase
    {
        public static Sphere Zero => new Sphere(position: Vector3.Zero, radius: 0.5f, instance: 0, Material.Zero);
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

        public override Vector3 Min => Position - new Vector3(Radius);
        public override Vector3 Max => Position + new Vector3(Radius);

        private readonly Vector4[] gpuData = new Vector4[1];
        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Position;
            gpuData[0].W = Radius;
            
            return gpuData.AddArray(Material.GetGPUFriendlyData());
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

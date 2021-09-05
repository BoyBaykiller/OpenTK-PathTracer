using OpenTK;
using System;

namespace OpenTK_PathTracer.GameObjects
{
    class Sphere : GameObject, IDisposable
    {
        public static Sphere Zero => new Sphere(position: Vector3.Zero, radius: 0.5f, instance: 0, Material.Zero);
        public static readonly int GPUInstanceSize = Vector4.SizeInBytes + Material.GPUInstanceSize;

        public int Instance;

        public float Radius;
        public Sphere(Vector3 position, float radius, int instance, Material material)
        {
            Position = position;
            Radius = radius;
            Material = material;
            Instance = instance;
        }

        public override int BufferOffset => 0 + Instance * GPUInstanceSize;

        public override Vector3 Min => Position - new Vector3(Radius);
        public override Vector3 Max => Position + new Vector3(Radius);

        readonly Vector4[] gpuData = new Vector4[1];

        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Position;
            gpuData[0].W = Radius;
            
            return gpuData.AddArray(Material.GetGPUFriendlyData());
        }

        public override bool IntersectsRay(Ray ray, out float t1, out float t2)
        {
            // Source: https://antongerdelan.net/opengl/raycasting.html
            t1 = 0; t2 = 0;

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
        
        public override bool IntersectsAABB(AABB aabb)
        {
            //Vector3 point = new Vector3(MathF.Max(aabb.Min.X, Math.Min(this.Position.X, aabb.Max.X)), MathF.Max(aabb.Min.Y, Math.Min(this.Position.Y, aabb.Max.Y)), MathF.Max(aabb.Min.Z, Math.Min(this.Position.Z, aabb.Max.Z)));
            //return Vector3.DistanceSquared(point, this.Position) <= this.Radius * this.Radius;

            return (this.Position.X + this.Radius) > aabb.Min.X &&
                    (this.Position.X - this.Radius) < aabb.Max.X &&
                    (this.Position.Y + this.Radius) > aabb.Min.Y &&
                    (this.Position.Y - this.Radius) < aabb.Max.Y &&
                    (this.Position.Z + this.Radius) > aabb.Min.Z &&
                    (this.Position.Z - this.Radius) < aabb.Max.Z;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

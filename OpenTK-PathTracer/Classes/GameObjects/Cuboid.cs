using OpenTK;
using System;

namespace OpenTK_PathTracer.GameObjects
{
    class Cuboid : GameObject
    {
        public static readonly int GPUInstanceSize = Vector4.SizeInBytes * 2 + Material.GPUInstanceSize;
        public static int GlobalClassBufferOffset;
        public int Instance { get; private set; }

        public Vector3 Dimensions;
        public Cuboid(Vector3 position, Vector3 dimensions, int instance, Material material)
        {
            Position = position;
            Dimensions = dimensions;
            Material = material;
            Instance = instance;
        }


        public override int BufferOffset => GlobalClassBufferOffset + Instance * GPUInstanceSize;

        public override Vector3 Min => Position - Dimensions * 0.5f;
        public override Vector3 Max => Position + Dimensions * 0.5f;

        readonly Vector4[] gpuData = new Vector4[2];
        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Position - Dimensions * 0.5f;
            gpuData[1].Xyz = Position + Dimensions * 0.5f;

            return gpuData.AddArray(Material.GetGPUFriendlyData());
        }

        public override bool IntersectsRay(Ray ray, out float t1, out float t2)
        {
            // Source: https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
            t1 = float.MinValue;
            t2 = float.MaxValue;

            Vector3 t0s = Vector3.Divide((this.Min - ray.Origin), ray.Direction);
            Vector3 t1s = Vector3.Divide((this.Max - ray.Origin), ray.Direction);

            Vector3 tsmaller = Vector3.ComponentMin(t0s, t1s);
            Vector3 tbigger = Vector3.ComponentMax(t0s, t1s);

            t1 = Math.Max(t1, Math.Max(tsmaller.X, Math.Max(tsmaller.Y, tsmaller.Z)));
            t2 = Math.Min(t2, Math.Min(tbigger.X, Math.Min(tbigger.Y, tbigger.Z)));
            return t1 <= t2;
        }
        
        public override bool IntersectsAABB(AABB aabb)
        {
            return this.Min.X <= aabb.Max.X &&
                   this.Max.X >= aabb.Min.X &&
                   this.Min.Y <= aabb.Max.Y &&
                   this.Max.Y >= aabb.Min.Y &&
                   this.Min.Z <= aabb.Max.Z &&
                   this.Max.Z >= aabb.Min.Z;
        }
    }
}

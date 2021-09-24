using System;
using OpenTK;

namespace OpenTK_PathTracer.GameObjects
{
    class Cuboid : BaseGameObject
    {
        public static Cuboid Zero => new Cuboid(position: Vector3.Zero, dimensions: Vector3.One, instance: 0, Material.Zero);
        public const int GPU_INSTANCE_SIZE = 16 * 2 + Material.GPU_INSTANCE_SIZE;
        
        public int Instance;
        public Vector3 Dimensions;
        public Cuboid(Vector3 position, Vector3 dimensions, int instance, Material material)
        {
            Position = position;
            Dimensions = dimensions;
            Material = material;
            Instance = instance;
        }


        public override int BufferOffset => Sphere.GPU_INSTANCE_SIZE * MainWindow.MAX_GAMEOBJECTS_SPHERES + Instance * GPU_INSTANCE_SIZE;

        public override Vector3 Min => Position - Dimensions * 0.5f;
        public override Vector3 Max => Position + Dimensions * 0.5f;

        private readonly Vector4[] gpuData = new Vector4[2];
        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Min;
            gpuData[1].Xyz = Max;

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
    }
}

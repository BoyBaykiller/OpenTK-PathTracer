using System;
using OpenTK;

namespace OpenTK_PathTracer
{
    struct Ray
    {
        public Vector3 Origin, Direction;
        public Ray(Vector3 origion, Vector3 direction)
        {
            Origin = origion;
            Direction = direction;
        }

        public Vector3 GetPoint(float deltaTime)
        {
            return Origin + Direction * deltaTime;
        }

        public bool IntersectsAABB(AABB aabb, out float t1, out float t2)
        {
            // Source: https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
            t1 = float.MinValue;
            t2 = float.MaxValue;

            Vector3 t0s = Vector3.Divide(aabb.Min - Origin, Direction);
            Vector3 t1s = Vector3.Divide(aabb.Max - Origin, Direction);

            Vector3 tsmaller = new Vector3(MathF.Min(t0s.X, t1s.X), MathF.Min(t0s.Y, t1s.Y), MathF.Min(t0s.Z, t1s.Z));
            Vector3 tbigger = new Vector3(MathF.Max(t0s.X, t1s.X), MathF.Max(t0s.Y, t1s.Y), MathF.Max(t0s.Z, t1s.Z));

            t1 = Math.Max(t1, Math.Max(tsmaller.X, Math.Max(tsmaller.Y, tsmaller.Z)));
            t2 = Math.Min(t2, Math.Min(tbigger.X, Math.Min(tbigger.Y, tbigger.Z)));
            return t1 <= t2;
        }

        public static Ray GetWorldSpaceRay(Matrix4 inverseProjection, Matrix4 inverseView, Vector3 worldPosition, Vector2 normalizedDeviceCoords)
        {
            Vector4 _rayEye = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1.0f, 1.0f) * inverseProjection; _rayEye.Z = -1.0f; _rayEye.W = 0.0f; // vector * matrix, because OpenTK is stupid
            return new Ray(worldPosition, (_rayEye * inverseView).Xyz.Normalized());
        }

        public override string ToString()
        {
            return $"<O: {Origin}, D: {Direction}>";
        }
    }
}

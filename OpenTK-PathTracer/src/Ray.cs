using System;
using OpenTK;

namespace OpenTK_PathTracer
{
    struct Ray
    {
        public Vector3 Origin, Direction;

        public Vector3 GetPoint(float deltaTime)
        {
            return Origin + Direction * deltaTime;
        }

        public static Ray GetWorldSpaceRay(Matrix4 inverseProjection, Matrix4 inverseView, Vector3 worldPosition, Vector2 normalizedDeviceCoords)
        {
            Vector4 rayEye = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1.0f, 1.0f) * inverseProjection; rayEye.Z = -1.0f; rayEye.W = 0.0f; // vector * matrix, because OpenTK...
            return new Ray { Origin = worldPosition, Direction = (rayEye * inverseView).Xyz.Normalized() };
        }
    }
}

﻿using OpenTK;

namespace OpenTK_PathTracer.GameObjects
{
    abstract class GameObjectBase : UBOCompatibleBase, Grid.IGridCompatible
    {
        public Material Material;
        public Vector3 Position;

        public abstract bool IntersectsRay(Ray ray, out float t1, out float t2);

        public abstract Vector3 Min { get; }
        public abstract Vector3 Max { get; }
        public abstract bool IntersectsAABB(AABB aabb);
    }
}
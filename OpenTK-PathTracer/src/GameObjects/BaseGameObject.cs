﻿using OpenTK;

namespace OpenTK_PathTracer.GameObjects
{
    abstract class BaseGameObject : BaseSTD140Compatible
    {
        public Material Material;
        public Vector3 Position;

        public abstract bool IntersectsRay(Ray ray, out float t1, out float t2);
    }
}

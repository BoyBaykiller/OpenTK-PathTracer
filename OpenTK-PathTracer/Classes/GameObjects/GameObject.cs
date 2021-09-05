using OpenTK;
using System;

namespace OpenTK_PathTracer.GameObjects
{
    abstract class GameObject : UBOCompatible, Grid.IGridCompatible/*, IDisposable*/
    {
        private bool disposed = false;

        public Material Material;
        public Vector3 Position;
        public abstract bool IntersectsRay(Ray ray, out float t1, out float t2);

        public abstract Vector3 Min { get; }
        public abstract Vector3 Max { get; }
        public abstract bool IntersectsAABB(AABB aabb);

        //~GameObject() {
        //    Dispose();
        //}

        public override string ToString()
        {
            return $"<P: {Position}, D: {Max - Min}, M: {Material}>";
        }

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposed)
        //        return;

        //    if (disposing)
        //    {
        //        // TODO: Dispose
        //    }

        //    disposed = true;
        //}


    }
}

using System;
using OpenTK;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    abstract class UBOCompatibleBase
    {
        public static BufferObject BufferObject;

        public abstract int BufferOffset { get; }
        
        public abstract Vector4[] GetGPUFriendlyData();

        public void Upload()
        {
            Vector4[] data = GetGPUFriendlyData();
            BufferObject.SubData(BufferOffset, Vector4.SizeInBytes * data.Length, data);
        }
    }
}

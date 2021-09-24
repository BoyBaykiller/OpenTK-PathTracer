using System;
using OpenTK;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    abstract class BaseUBOCompatible
    {
        public abstract int BufferOffset { get; }
        
        public abstract Vector4[] GetGPUFriendlyData();

        public void Upload(BufferObject uniformBuffer)
        {
            Vector4[] data = GetGPUFriendlyData();
            uniformBuffer.SubData(BufferOffset, Vector4.SizeInBytes * data.Length, data);
        }
    }
}

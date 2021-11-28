using OpenTK;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    abstract class BaseSTD140Compatible
    {
        public abstract int BufferOffset { get; }
        
        public abstract Vector4[] GetGPUFriendlyData();

        public void Upload(BufferObject buffer)
        {
            Vector4[] data = GetGPUFriendlyData();
            buffer.SubData(BufferOffset, Vector4.SizeInBytes * data.Length, data);
        }
    }
}

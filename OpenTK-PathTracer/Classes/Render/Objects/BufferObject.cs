using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    public class BufferObject : IDisposable
    {
        public static readonly Dictionary<BufferRangeTarget, List<int>> bufferTypeBindingIndexDict = new Dictionary<BufferRangeTarget, List<int>>()
        {
            { BufferRangeTarget.AtomicCounterBuffer, new List<int>() },
            { BufferRangeTarget.ShaderStorageBuffer, new List<int>() },
            { BufferRangeTarget.TransformFeedbackBuffer, new List<int>() },
            { BufferRangeTarget.UniformBuffer, new List<int>() }
        };

        public readonly int ID;
        public int BufferOffset;
        public int Size { get; private set; }

        public BufferObject(BufferRangeTarget bufferRangeTarget, int bindingIndex, int size, BufferStorageFlags bufferStorageFlags)
        {
            if (bufferTypeBindingIndexDict[bufferRangeTarget].Contains(bindingIndex))
            {
                Console.WriteLine($"BindingIndex {bindingIndex} is already bound to a {bufferRangeTarget}");
                bufferTypeBindingIndexDict[bufferRangeTarget].Add(bindingIndex);
            }
            GL.CreateBuffers(1, out ID);
            Allocate(size, bufferStorageFlags);
            GL.BindBufferBase(bufferRangeTarget, bindingIndex, ID);
        }

        public BufferObject(int size, BufferStorageFlags bufferStorageFlags)
        {
            GL.CreateBuffers(1, out ID);
            Allocate(size, bufferStorageFlags);
        }

        public void Bind(BufferTarget bufferTarget)
        {
            GL.BindBuffer(bufferTarget, ID);
        }

        /// <summary>
        /// Sets <seealso cref="BufferOffset"/> to 0 and overrides the content with 0
        /// </summary>
        public void Reset()
        {
            BufferOffset = 0;
            GL.NamedBufferSubData(ID, IntPtr.Zero, Size, new byte[Size]);
        }

        public void Append<T>(int size, T data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, ref data);
            BufferOffset += size;
        }
        public void Append<T2>(int size, T2[] data) where T2 : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, data);
            BufferOffset += size;
        }
        public void Append(int size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, data);
            BufferOffset += size;
        }

        public void SubData<T3>(int offset, int size, T3 data) where T3 : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, ref data);
            BufferOffset = offset + size;
        }
        public void SubData<T4>(int offset, int size, T4[] data) where T4 : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
            BufferOffset = offset + size;
        }
        public void SubData(int offset, int size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
            BufferOffset = offset + size;
        }

        private void Allocate(int size, BufferStorageFlags bufferStorageFlags)
        {
            GL.NamedBufferStorage(ID, size, IntPtr.Zero, bufferStorageFlags);
            Size = size;
        }

        public void GetSubData<T5>(int offset, int size, T5[] data) where T5 : struct
        {
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, data);
        }
        public void GetSubData(int offset, int size, out IntPtr data)
        {
            data = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, data);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}

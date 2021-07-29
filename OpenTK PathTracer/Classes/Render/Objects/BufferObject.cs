using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    public class BufferObject
    {
        public static readonly Dictionary<BufferRangeTarget, List<int>> bufferTypeBindingIndexDict = new Dictionary<BufferRangeTarget, List<int>>()
        {
            { BufferRangeTarget.AtomicCounterBuffer, new List<int>() },
            { BufferRangeTarget.ShaderStorageBuffer, new List<int>() },
            { BufferRangeTarget.TransformFeedbackBuffer, new List<int>() },
            { BufferRangeTarget.UniformBuffer, new List<int>() }
        };

        public readonly int ID;
        public readonly BufferTarget BufferTarget;
        public readonly BufferUsageHint BufferUsageHint;
        public int BufferOffset;
        public int Size { get; private set; }


        /// <summary>
        /// Creates a <paramref name="BufferObject"/> and allocates the associated memory on the GPU
        /// </summary>
        /// <param name="bindingIndex">The index in the shader</param>
        /// <param name="size"></param>
        /// <param name="bufferUsageHint"></param>
        public BufferObject(BufferRangeTarget bufferRangeTarget, int bindingIndex, int size, BufferUsageHint bufferUsageHint)
        {
            if (bufferTypeBindingIndexDict[bufferRangeTarget].Contains(bindingIndex))
            {
                Console.WriteLine($"BufferObject: BindingIndex {bindingIndex} is already bound to an other {bufferRangeTarget}");
                bufferTypeBindingIndexDict[bufferRangeTarget].Add(bindingIndex);
            }

            BufferTarget = (BufferTarget)bufferRangeTarget;
            BufferUsageHint = bufferUsageHint;
            ID = GL.GenBuffer();
            Allocate(size);
            GL.BindBufferBase(bufferRangeTarget, bindingIndex, ID);
        }

        public BufferObject(BufferTarget bufferTarget, int size, BufferUsageHint bufferUsageHint)
        {
            BufferTarget = bufferTarget;
            BufferUsageHint = bufferUsageHint;
            ID = GL.GenBuffer();
            Allocate(size);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget, ID);
        }

        /// <summary>
        /// Sets <paramref name="WritingOffset"/> to 0 and overrides the content to 0
        /// </summary>
        /// <param name="removeData"></param>
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

        public void Allocate(int size)
        {
            Bind();
            GL.BufferData(BufferTarget, size, IntPtr.Zero, BufferUsageHint);
            Size = size;
        }
    }
}

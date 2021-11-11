using System;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class VAO : IDisposable
    {
        private static int lastBindedID = -1;

        public readonly int ID;
        public VAO()
        {
            GL.CreateVertexArrays(1, out ID);
        }

        public VAO(BufferObject elementArrayBuffer)
        {
            GL.CreateVertexArrays(1, out ID);
            GL.VertexArrayElementBuffer(ID, elementArrayBuffer.ID);
        }

        public void AddSourceBuffer(BufferObject sourceBuffer, int bindingIndex, int vertexSize, int bufferOffset = 0)
        {
            GL.VertexArrayVertexBuffer(ID, bindingIndex, sourceBuffer.ID, (IntPtr)bufferOffset, vertexSize);
        }

        public void SetAttribFormat(int bindingIndex, int index, int attribTypeElements, VertexAttribType vertexAttribType, int offset, bool normalize = false, int divisor = 0)
        {
            GL.EnableVertexArrayAttrib(ID, index);
            GL.VertexArrayAttribFormat(ID, index, attribTypeElements, vertexAttribType, normalize, offset);
            GL.VertexArrayAttribBinding(ID, index, bindingIndex);
            GL.VertexArrayBindingDivisor(ID, bindingIndex, divisor);
        }

        public void Bind()
        {
            if (lastBindedID != ID)
            {
                GL.BindVertexArray(ID);
                lastBindedID = ID;
            }
        }

        public static void Bind(int id)
        {
            if (lastBindedID != id)
            {
                GL.BindVertexArray(id);
                lastBindedID = id;
            }
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(ID);
        }
    }
}

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class VAO
    {
        private static int lastBindedID = -1;

        public readonly int ID;
        public VAO()
        {
            ID = GL.GenVertexArray();
            GL.BindVertexArray(ID);
        }

        public void SetAttribPointer(int index, int floats, VertexAttribPointerType vertexAttribPointerType, int stride, int offset, bool normalize = false)
        {
            GL.VertexAttribPointer(index, floats, vertexAttribPointerType, normalize, stride, offset);
            GL.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            if (lastBindedID != ID)
            {
                GL.BindVertexArray(ID);
                lastBindedID = ID;
            }
        }

        public static void Bind(int iD)
        {
            if (lastBindedID != iD)
            {
                GL.BindVertexArray(iD);
                lastBindedID = iD;
            }
        }
    }
}

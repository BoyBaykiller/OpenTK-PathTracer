using OpenTK_PathTracer.Render.Objects;


namespace OpenTK_PathTracer.Render
{
    abstract class RenderEffect
    {
        public Texture Result { get; protected set; }
        //public Query Query { get; } = new Query(600);
        public  ShaderProgram Program { get; protected set; }
        public Framebuffer Framebuffer { get; protected set; }

        public int Width => Result.Width;
        public int Height => Result.Height;

        public abstract void Run(params object[] param);
        public abstract void SetSize(int width, int height);
    }
}

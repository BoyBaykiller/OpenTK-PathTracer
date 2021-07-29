using OpenTK_PathTracer.Render.Objects;


namespace OpenTK_PathTracer.Render
{
    abstract class RenderEffect
    {
        public Texture Result { get; protected set; }
        public Query Query { get; protected set; }
        public abstract void Run(params object[] param);
        public abstract void SetSize(int width, int height);
    }
}

using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer.Render
{
    abstract class RenderEffectBase
    {
        public readonly Query Query = new Query(600);
        public Texture Result { get; protected set; }
        public  ShaderProgram Program { get; protected set; }
        public Framebuffer Framebuffer { get; protected set; }

        public int Width => Result.Width;
        public int Height => Result.Height;

        public abstract void Run(params object[] param);


        /// <summary>
        /// Resizes <seealso cref="Result"/> accordingly
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void SetSize(int width, int height)
        {
            Result.Allocate(width, height);
        }
    }
}

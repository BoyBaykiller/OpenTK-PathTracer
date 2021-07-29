using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Framebuffer
    {
        public readonly int ID;
        private static int lastBindedID = -1;
        public Framebuffer()
        {
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, default, default, default, 0);
        }


        /// <summary>
        /// Attaches a RenderTarget and binds this Framebuffer
        /// </summary>
        /// <param name="framebufferAttachment"></param>
        /// <param name="texture"></param>
        public void SetRenderTarget(FramebufferAttachment framebufferAttachment, Texture texture)
        {
            Bind();
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, framebufferAttachment, texture.ID, 0);
        }

        /// <summary>
        /// Binds this Framebuffer and creates a new RenderBuffer which gets attached to this Framebuffer
        /// </summary>
        /// <param name="renderbufferStorage"></param>
        /// <param name="framebufferAttachment"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rBO"></param>
        public void SetRenderbuffer(RenderbufferStorage renderbufferStorage, FramebufferAttachment framebufferAttachment, int width, int height, out int rBO)
        {
            Bind();

            rBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, renderbufferStorage, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, framebufferAttachment, RenderbufferTarget.Renderbuffer, rBO);
        }

        public void Bind(FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != ID)
            {
                GL.BindFramebuffer(framebufferTarget, ID);
                lastBindedID = ID;
            }  
        }

        public static void Bind(int ID, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != ID)
            {
                GL.BindFramebuffer(framebufferTarget, ID);
                lastBindedID = ID;
            }
        }

        public FramebufferStatus GetGBOStatus()
        {
            return GL.CheckNamedFramebufferStatus(ID, FramebufferTarget.Framebuffer);
        }
    }
}

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Framebuffer : IDisposable
    {
        private static int lastBindedID = -1;

        public readonly int ID;
        public Framebuffer()
        {
            GL.CreateFramebuffers(1, out ID);
        }

        public void Clear(ClearBufferMask clearBufferMask)
        {
            Bind();
            GL.Clear(clearBufferMask);
        }

        public void AddRenderTarget(FramebufferAttachment framebufferAttachment, Texture texture)
        {
            GL.NamedFramebufferTexture(ID, framebufferAttachment, texture.ID, 0);
        }
        public void SetRenderTarget(params DrawBuffersEnum[] drawBuffersEnums)
        {
            GL.NamedFramebufferDrawBuffers(ID, drawBuffersEnums.Length, drawBuffersEnums);
        }
        public void SetReadTarget(ReadBufferMode readBufferMode)
        {
            GL.NamedFramebufferReadBuffer(ID, readBufferMode);
        }

        public void Bind(FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != ID)
            {
                GL.BindFramebuffer(framebufferTarget, ID);
                lastBindedID = ID;
            }  
        }

        public FramebufferStatus GetFBOStatus()
        {
            return GL.CheckNamedFramebufferStatus(ID, FramebufferTarget.Framebuffer);
        }

        public static void Bind(int id, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != id)
            {
                GL.BindFramebuffer(framebufferTarget, id);
                lastBindedID = id;
            }
        }

        public static void Clear(int id, ClearBufferMask clearBufferMask)
        {
            Framebuffer.Bind(id);
            GL.Clear(clearBufferMask);
        }

        public static unsafe Image<Rgba32> GetBitmapFramebufferAttachment(int id, FramebufferAttachment framebufferAttachment, int width, int height, int x = 0, int y = 0)
        {
            Image<Rgba32> image = new Image<Rgba32>(width, height);
            GL.NamedFramebufferReadBuffer(id, (ReadBufferMode)framebufferAttachment);

            Bind(id, FramebufferTarget.ReadFramebuffer);
            fixed (void* ptr = image.GetPixelRowSpan(0))
            {
                GL.ReadPixels(x, y, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr);
            }
            GL.Finish();

            image.Mutate(p => p.Flip(FlipMode.Vertical));

            return image;
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(ID);
        }
    }
}

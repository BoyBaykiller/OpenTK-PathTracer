using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Framebuffer : IDisposable
    {
        private static int lastBindedID = -1;

        private int idRBO;

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

        public void SetRenderbuffer(RenderbufferStorage renderbufferStorage, FramebufferAttachment framebufferAttachment, int width, int height)
        {
            GL.CreateRenderbuffers(1, out idRBO);
            GL.NamedRenderbufferStorage(ID, renderbufferStorage, width, height);
            GL.NamedFramebufferRenderbuffer(ID, framebufferAttachment, RenderbufferTarget.Renderbuffer, idRBO);
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

        public Bitmap GetBitmapRenderTarget(FramebufferAttachment framebufferAttachment, int width, int height, int x = 0, int y = 0)
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            SetReadTarget((ReadBufferMode)framebufferAttachment);
            
            Bind();
            GL.ReadPixels(x, y, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.Finish();

            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        public static Bitmap GetBitmapDefaultFramebuffer(int width, int height, int x = 0, int y = 0)
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Bind(0);
            GL.ReadPixels(x, y, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.Finish();

            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }


        public void Dispose()
        {
            GL.DeleteFramebuffer(ID);
        }
    }
}

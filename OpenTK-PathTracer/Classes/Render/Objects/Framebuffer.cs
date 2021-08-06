﻿using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Framebuffer
    {
        public readonly int ID;

        private int _rbo = -1;
        public int RBO
        {
            get
            {
                if (_rbo == -1)
                    throw new System.Exception("No RBO was attached to the framebuffer yet");

                return _rbo;
            }
        }

        private static int lastBindedID = -1;
        public Framebuffer()
        {
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
        }

        public void Clear(ClearBufferMask clearBufferMask)
        {
            Bind();
            GL.Clear(clearBufferMask);
        }

        public void AddRenderTarget(FramebufferAttachment framebufferAttachment, Texture texture)
        {
            Bind();
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, framebufferAttachment, texture.ID, 0);

        }

        public void SetRenderbuffer(RenderbufferStorage renderbufferStorage, FramebufferAttachment framebufferAttachment, int width, int height)
        {
            Bind();

            _rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, renderbufferStorage, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, framebufferAttachment, RenderbufferTarget.Renderbuffer, _rbo);
        }

        /// <summary>
        /// On default OpenGL only renders to ColorAttachment0 of a Framebuffer. 
        /// That means if you have two Rendertargets attached you need to tell OpenGL through this function to actually render to both of them
        /// </summary>
        /// <param name="drawBuffersEnums"></param>
        public void DrawRenderTargets(DrawBuffersEnum[] drawBuffersEnums)
        {
            Bind();
            GL.DrawBuffers(drawBuffersEnums.Length, drawBuffersEnums);
        }

        public void Bind(FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != ID)
            {
                GL.BindFramebuffer(framebufferTarget, ID);
                lastBindedID = ID;
            }  
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


        public FramebufferStatus GetGBOStatus()
        {
            return GL.CheckNamedFramebufferStatus(ID, FramebufferTarget.Framebuffer);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;


namespace OpenTK_PathTracer.GUI
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        // Veldrid objects
        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        private GUI.Objects.Texture _fontTexture;
        private GUI.Objects.Shader _shader;
        
        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGuiNET.ImGui.CreateContext();
            ImGuiNET.ImGui.SetCurrentContext(context);
            var io = ImGuiNET.ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGuiNET.ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            Util.CreateVertexArray("ImGuiNET.ImGui", out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            Util.CreateVertexBuffer("ImGuiNET.ImGui", out _vertexBuffer);
            Util.CreateElementBuffer("ImGuiNET.ImGui", out _indexBuffer);
            GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

                uniform mat4 projection_matrix;

                layout(location = 0) in vec2 in_position;
                layout(location = 1) in vec2 in_texCoord;
                layout(location = 2) in vec4 in_color;

                out vec4 color;
                out vec2 texCoord;

                void main()
                {
                    gl_Position = projection_matrix * vec4(in_position, 0, 1);
                    color = in_color;
                    texCoord = in_texCoord;
                }"
            ;
            string FragmentSource = @"#version 330 core

                uniform sampler2D in_fontTexture;

                in vec4 color;
                in vec2 texCoord;

                out vec4 outputColor;

                void main()
                {
                    outputColor = color * texture(in_fontTexture, texCoord);
                }"
            ;

            _shader = new Objects.Shader("ImGuiNET.ImGui", VertexSource, FragmentSource);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
            GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

            GL.EnableVertexArrayAttrib(_vertexArray, 2);
            GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiNET.ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = new GUI.Objects.Texture("ImGuiNET.ImGui Text Atlas", width, height, pixels);
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.GLTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGuiNET.ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGuiNET.ImGui.Render();
                RenderImDrawData(ImGuiNET.ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGuiNET.ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGuiNET.ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGuiNET.ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiNET.ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        MouseState PrevMouseState;
        KeyboardState PrevKeyboardState;
        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiNET.ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();

            MouseState MouseState = Mouse.GetCursorState();
            KeyboardState KeyboardState = Keyboard.GetState();

            io.MouseDown[0] = MouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = MouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = MouseState.MiddleButton == ButtonState.Pressed;

            var screenPoint = new System.Drawing.Point(MouseState.X, MouseState.Y);
            var point = wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            io.MouseWheel = MouseState.Scroll.Y - PrevMouseState.Scroll.Y;
            io.MouseWheelH = MouseState.Scroll.X - PrevMouseState.Scroll.X;
            
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Key.ControlLeft) || KeyboardState.IsKeyDown(Key.ControlRight);
            io.KeyAlt = KeyboardState.IsKeyDown(Key.AltLeft) || KeyboardState.IsKeyDown(Key.AltRight);
            io.KeyShift = KeyboardState.IsKeyDown(Key.ShiftLeft) || KeyboardState.IsKeyDown(Key.ShiftRight);
            io.KeySuper = KeyboardState.IsKeyDown(Key.WinLeft) || KeyboardState.IsKeyDown(Key.WinRight);

            PrevMouseState = MouseState;
            PrevKeyboardState = KeyboardState;
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiNET.ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiNET.ImGuiKey.Z] = (int)Key.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear ImGuiNET.ImGui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear ImGuiNET.ImGui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiNET.ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            // _shader.UseShader();
            OpenTK_PathTracer.Render.Objects.ShaderProgram.Use(_shader.Program);
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_matrix"), false, ref mvp);
            GL.Uniform1(_shader.GetUniformLocation("in_fontTexture"), 0);

            GL.BindVertexArray(_vertexArray);

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);

                GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                int vtx_offset = 0;
                int idx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                        if ((io.BackendFlags & ImGuiNET.ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            OpenTK_PathTracer.Render.Objects.VAO.Bind(0);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _fontTexture.Dispose();
            _shader.Dispose();
        }
    }
}
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK_PathTracer.Render;
using OpenTK_PathTracer.GameObjects;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    class MainWindow : GameWindow
    {
        public const int MAX_GAMEOBJECTS_SPHERES = 256, MAX_GAMEOBJECTS_CUBOIDS = 64;
        public const float EPSILON = 0.005f, FOV = 103;

        public MainWindow() : base(832, 832, new GraphicsMode(0, 0, 0, 0)) {  /*WindowState = WindowState.Maximized;*/  }


        public Matrix4 inverseProjection;
        private Vector2 nearFarPlane = new Vector2(EPSILON, 1000f);
        public int FPS, UPS;
        private int fps, ups;

        public readonly Camera Camera = new Camera(new Vector3(-18.93f, -5.07f, -17.75f), new Vector3(0, 1, 0), 6.7f, 3.5f); // new Vector3(-9, 12, 4) || new Vector3(-11, 9.3f, -9.7f)


        public bool IsRenderInBackground = true;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (Focused || IsRenderInBackground)
            {
                //AtmosphericScatterer.ViewPos = Camera.Position;
                //AtmosphericScatterer.Render();

                PathTracer.Render();

                PostProcesser.Render(PathTracer.Result);
                GL.Viewport(0, 0, Width, Height);
                Framebuffer.Bind(0);
                PostProcesser.Result.AttachSampler(0);
                finalProgram.Use();
                GL.DrawArrays(PrimitiveType.Quads, 0, 4);

                if (Focused)
                {
                    Gui.Render(this, (float)e.Time, out bool frameChanged);
                    if (frameChanged)
                        PathTracer.ThisRenderNumFrame = 0;
                }
                SwapBuffers();
                fps++;
            }

            base.OnRenderFrame(e);
        }

        private readonly Stopwatch fpsTimer = Stopwatch.StartNew();
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (fpsTimer.ElapsedMilliseconds >= 1000)
            {
                Title = $"FPS: {fps}; RayDepth: {PathTracer.RayDepth}; UPS: {ups} Position {Camera.Position}";
                FPS = fps;
                UPS = ups;
                fps = 0;
                ups = 0;
                fpsTimer.Restart();
            }
            ThreadManager.InvokeQueuedActions();
            KeyboardManager.Update();
            MouseManager.Update();

            if (Focused)
            {
                if (Gui.ImGuiIOPtr.WantCaptureMouse && !CursorVisible)
                {
                    Point _point = PointToScreen(new Point(Width / 2, Height / 2));
                    Mouse.SetPosition(_point.X, _point.Y);
                }

                Gui.Update(this);

                if (KeyboardManager.IsKeyDown(Key.Escape))
                    Close();

                if (KeyboardManager.IsKeyTouched(Key.V))
                    VSync = VSync == VSyncMode.Off ? VSyncMode.On : VSyncMode.Off;

                if (KeyboardManager.IsKeyTouched(Key.F11))
                    WindowState = WindowState == WindowState.Normal ? WindowState.Fullscreen : WindowState.Normal;

                if (KeyboardManager.IsKeyTouched(Key.E) && !Gui.ImGuiIOPtr.WantCaptureKeyboard)
                {
                    CursorVisible = !CursorVisible;
                    CursorGrabbed = !CursorGrabbed;

                    if (!CursorVisible)
                    {
                        MouseManager.Update();
                        Camera.Velocity = Vector3.Zero;
                    }
                }

                if (!CursorVisible)
                {
                    Camera.ProcessInputs((float)e.Time, out bool frameChanged);
                    if (frameChanged)
                        PathTracer.ThisRenderNumFrame = 0;
                }

                BasicDataUBO.SubData(Vector4.SizeInBytes * 4, Vector4.SizeInBytes * 4, Camera.View.Inverted());
                BasicDataUBO.SubData(Vector4.SizeInBytes * 8, Vector4.SizeInBytes, Camera.Position);
            }

            ups++;
            base.OnUpdateFrame(e);
        }

        public readonly List<BaseGameObject> GameObjects = new List<BaseGameObject>();
        private ShaderProgram finalProgram;
        public BufferObject BasicDataUBO, GameObjectsUBO;
        public PathTracer PathTracer;
        public ScreenEffect PostProcesser;
        public AtmosphericScattering AtmosphericScatterer;
        public Texture SkyBox;
        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine($"OpenGL: {Helper.APIMajor}.{Helper.APIMinor}");
            Console.WriteLine($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Console.WriteLine($"GPU: {GL.GetString(StringName.Renderer)}");
            if (!Helper.IsCoreExtensionAvailable("GL_ARB_direct_state_access", 4, 5))
                throw new NotSupportedException("Your system does not support GL_ARB_direct_state_access");

            if (!Helper.IsCoreExtensionAvailable("GL_ARB_texture_storage", 4, 2))
                throw new NotSupportedException("Your system does not support GL_ARB_texture_storage");

            GL.DepthMask(false);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Multisample);
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            VSync = VSyncMode.Off;
            CursorVisible = false;
            CursorGrabbed = true;

            SkyBox = new Texture(TextureTarget2d.TextureCubeMap);
            Helper.ParallelLoadCubemapImages(SkyBox, new string[]
            {
                "res/Textures/EnvironmentMap/posx.png",
                "res/Textures/EnvironmentMap/negx.png",
                "res/Textures/EnvironmentMap/posy.png",
                "res/Textures/EnvironmentMap/negy.png",
                "res/Textures/EnvironmentMap/posz.png",
                "res/Textures/EnvironmentMap/negz.png"
            }, (SizedInternalFormat)PixelInternalFormat.Srgb8Alpha8);

            AtmosphericScatterer = new AtmosphericScattering(128, 100, 10, 2.1f, 35.0f, 0.01f, new Vector3(680, 550, 440), new Vector3(0, 500 + 800.0f, 0), new Vector3(20.43f, -201.99f + 800.0f, -20.67f));
            PathTracer = new PathTracer(SkyBox, Width, Height, 13, 1, 20f, 0.14f);
            PostProcesser = new ScreenEffect(new Shader(ShaderType.FragmentShader, "res/shaders/PostProcessing/fragment.glsl".GetPathContent()), Width, Height);
            finalProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, "res/shaders/screenQuad.glsl".GetPathContent()), new Shader(ShaderType.FragmentShader, "res/shaders/final.glsl".GetPathContent()));
            
            BasicDataUBO = new BufferObject(BufferRangeTarget.UniformBuffer, 0);
            BasicDataUBO.ImmutableAllocate(Vector4.SizeInBytes * 4 * 2 + Vector4.SizeInBytes, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
            GameObjectsUBO = new BufferObject(BufferRangeTarget.UniformBuffer, 1);
            GameObjectsUBO.ImmutableAllocate(Sphere.GPU_INSTANCE_SIZE * MAX_GAMEOBJECTS_SPHERES + Cuboid.GPU_INSTANCE_SIZE * MAX_GAMEOBJECTS_CUBOIDS, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);

            float width = 40, height = 25, depth = 25;
            #region SetupSpheres
            int balls = 6;
            float radius = 1.3f;
            Vector3 dimensions = new Vector3(width * 0.6f, height, depth);
            for (float x = 0; x < balls; x++)
                for (float y = 0; y < balls; y++)
                    GameObjects.Add(new Sphere(new Vector3(dimensions.X / balls * x * 1.1f - dimensions.X / 2, (dimensions.Y / balls) * y - dimensions.Y / 2 + radius, -5), radius, PathTracer.NumSpheres++, new Material(albedo: new Vector3(0.59f, 0.59f, 0.99f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: x / (balls - 1), specularRoughness: y / (balls - 1), indexOfRefraction: 1f, refractionChance: 0.0f, refractionRoughnes: 0.1f)));
            
            Vector3 delta = dimensions / balls;
            for (float x = 0; x < balls; x++)
            {
                Material material = Material.Zero;
                material.Albedo = new Vector3(0.9f, 0.25f, 0.25f);
                material.SpecularChance = 0.02f;
                material.IOR = 1.05f;
                material.RefractionChance = 0.98f;
                material.AbsorbanceColor = new Vector3(1, 2, 3) * (x / balls);
                Vector3 position = new Vector3(-dimensions.X / 2 + radius + delta.X * x, 3f, -20f);
                GameObjects.Add(new Sphere(position, radius, PathTracer.NumSpheres++, material));


                Material material1 = Material.Zero;
                material1.SpecularChance = 0.02f;
                material1.SpecularRoughness = (x / balls);
                material1.IOR = 1.1f;
                material1.RefractionChance = 0.98f;
                material1.RefractionRoughnes = x / balls;
                material1.AbsorbanceColor = Vector3.Zero;
                position = new Vector3(-dimensions.X / 2 + radius + delta.X * x, -6f, -20f);
                GameObjects.Add(new Sphere(position, radius, PathTracer.NumSpheres++, material1));
            }
            #endregion
            
            #region SetupCuboids
            
            Cuboid down = new Cuboid(new Vector3(0, -height / 2, -10), new Vector3(width, EPSILON, depth), PathTracer.NumCuboids++, new Material(albedo: new Vector3(0.2f, 0.04f, 0.04f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.051f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f));

            //Cuboid up = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height, down.Position.Z - down.Dimensions.Z / 4f), new Vector3(down.Dimensions.X, EPSILON, down.Dimensions.Z / 2), PathTracer.NumCuboids++, new Material(albedo: new Vector3(0.6f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 0.023f, specularRoughness: 0.051f, indexOfRefraction: 1f, refractionChance: 0.0f, refractionRoughnes: 0));
            Cuboid upLight = new Cuboid(new Vector3(0, 20.5f - EPSILON, 6), new Vector3(down.Dimensions.X * 0.3f, EPSILON, down.Dimensions.Z * 0.3f), PathTracer.NumCuboids++, new Material(albedo: new Vector3(0.04f), emissiv: new Vector3(0.917f, 0.945f, 0.513f) * 5f, refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f));

            Cuboid back = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height / 2, down.Position.Z - depth / 2), new Vector3(width, height, EPSILON), PathTracer.NumCuboids++, new Material(albedo: new Vector3(1.0f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f));
            Cuboid front = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height / 2 + EPSILON, down.Position.Z + depth / 2 - 0.3f / 2), new Vector3(width, height - EPSILON * 2, 0.3f), PathTracer.NumCuboids++, new Material(albedo: new Vector3(1f), emissiv: Vector3.Zero, refractionColor: new Vector3(0.01f), specularChance: 0.04f, specularRoughness: 0f, indexOfRefraction: 1f, refractionChance: 0.954f, refractionRoughnes: 0));

            Cuboid right = new Cuboid(new Vector3(down.Position.X + width / 2, down.Position.Y + height / 2, down.Position.Z), new Vector3(EPSILON, height, depth), PathTracer.NumCuboids++, new Material(albedo: new Vector3(0.8f, 0.8f, 0.4f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 1.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f));
            Cuboid left = new Cuboid(new Vector3(down.Position.X - width / 2, down.Position.Y + height / 2, down.Position.Z), new Vector3(EPSILON, height, depth), PathTracer.NumCuboids++, new Material(albedo: new Vector3(0.24f, 0.6f, 0.24f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f));

            Cuboid middle = new Cuboid(new Vector3(-15f, -10.5f + EPSILON, -15), new Vector3(3f, 6, 3f), PathTracer.NumCuboids++, new Material(albedo: new Vector3(1.0f), emissiv: new Vector3(0.0f), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0));

            GameObjects.AddRange(new Cuboid[] { down, upLight, back, front, right, left, middle });
            #endregion

            for (int i = 0; i < GameObjects.Count; i++)
                GameObjects[i].Upload(GameObjectsUBO);

            base.OnLoad(e);
        }

        private int lastWidth = -1, lastHeight = -1;
        protected override void OnResize(EventArgs e)
        {
            if ((lastWidth != Width || lastHeight != Height) && Width != 0 && Height != 0) // dont resize when minimizing and maximizing
            {
                PathTracer.SetSize(Width, Height);
                PostProcesser.SetSize(Width, Height);
                Gui.SetSize(Width, Height);

                inverseProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), Width / (float)Height, nearFarPlane.X, nearFarPlane.Y).Inverted();
                BasicDataUBO.SubData(0, Vector4.SizeInBytes * 4, inverseProjection);
                lastWidth = Width; lastHeight = Height;
            }
            base.OnResize(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            Gui.ImGuiController.PressChar(e.KeyChar);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
                MouseManager.Update();
        }

        protected override void OnClosed(EventArgs e)
        {
            ImGuiNET.ImGui.SaveIniSettingsToDisk("res/imgui.ini");
            base.OnClosed(e);
        }

        public bool RayTrace(Ray ray, out BaseGameObject gameObject, out float t1, out float t2)
        {
            t1 = t2 = 0;
            gameObject = null;
            float tMin = float.MaxValue;
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].IntersectsRay(ray, out float tempT1, out float tempT2) && tempT2 > 0 && tempT1 < tMin)
                {
                    t1 = tempT1; t2 = tempT2;
                    tMin = GetSmallestPositive(t1, t2);
                    gameObject = GameObjects[i];
                }
            }

            return tMin != float.MaxValue;
        }
        public static float GetSmallestPositive(float t1, float t2)
        {
            return t1 < 0 ? t2 : t1;
        }
        
        public void SetGameObjectsRandomMaterial<T>(int maxNum) where T : BaseGameObject
        {
            int changed = 0;
            for (int i = 0; i < GameObjects.Count && changed < maxNum; i++)
            {
                if (GameObjects[i] is T)
                {
                    GameObjects[i].Material = Material.GetRndMaterial();
                    GameObjects[i].Upload(GameObjectsUBO);
                    changed++;
                }
            }
        }
    }
}
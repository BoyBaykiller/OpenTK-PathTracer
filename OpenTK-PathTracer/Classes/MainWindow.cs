﻿using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using OpenTK_PathTracer.Render;
using OpenTK_PathTracer.Render.GUI;
using OpenTK_PathTracer.GameObjects;
using OpenTK_PathTracer.Render.Objects;

namespace OpenTK_PathTracer
{
    class MainWindow : GameWindow
    {
        public MainWindow() : base(832, 832, new GraphicsMode(0, 0, 0, 0)) { /*WindowState = WindowState.Fullscreen;*/ }

        public const float Epsilon = 0.001f, FOV = 103;

        Matrix4 projection, inverseProjection;

        Vector2 nearFarPlane = new Vector2(Epsilon, 2000f);
        public int FPS, UPS;
        private int fps, ups;

        readonly Camera camera = new Camera(new Vector3(-9, 12, 4), Vector3.Normalize(new Vector3(0.3f, -0.3f, -0.5f)), new Vector3(0, 1, 0)); // new Vector3(-9, 12, 4) || new Vector3(-11, 9.3f, -9.7f)

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //AtmosphericScattering.Run(camera.Position);
            PathTracing.Run();
            
            //Rasterizer.Run((from c in grid.Cells select c.AABB).ToArray()); // new AABB[] { new AABB(Vector3.One, Vector3.One) } (from c in grid.Cells select c.AABB).ToArray()

            PostProcess.Run(new Texture[] { PathTracing.Result, Rasterizer.Result });

            GL.Viewport(0, 0, Width, Height);
            Framebuffer.Clear(0, ClearBufferMask.ColorBufferBit);
            PostProcess.Result.AttachToUnit(0);
            finalProgram.Use();
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            if (onWindowFocus)
            {
                FinalGUIRenderer.Run(this, (float)e.Time, out bool frameChanged);
                if (frameChanged)
                    PathTracing.ThisRenderNumFrame = 0;
            }

            SwapBuffers();
            fps++;
            base.OnRenderFrame(e);
        }


        readonly Stopwatch fpsTimer = Stopwatch.StartNew();
        KeyboardState lastKeyBoardState;
        MouseState lastMouseState;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (fpsTimer.ElapsedMilliseconds >= 1000)
            {
                Title = $"FPS: {fps}; RayDepth: {PathTracing.RayDepth}; UPS: {ups} SSP: {PathTracing.SSP}; Position {camera.Position}";
                FPS = fps;
                UPS = ups;
                fps = 0;
                ups = 0;
                fpsTimer.Restart();
            }
            ThreadManager.InvokeQueuedActions();

            KeyboardState keyBoardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            
            if (FinalGUIRenderer.ImGuiIOPtr.WantCaptureMouse && !CursorVisible)
            {
                Point _point = PointToScreen(new Point(Width / 2, Height / 2));
                Mouse.SetPosition(_point.X, _point.Y);
            }
            if (onWindowFocus)
            {
                //grid.Update(gameObjects);

                if (keyBoardState.IsKeyDown(Key.Escape))
                    Close();

                if (keyBoardState.IsKeyDown(Key.V) && !lastKeyBoardState.IsKeyDown(Key.V))
                    VSync = VSync == VSyncMode.Off ? VSyncMode.On : VSyncMode.Off;

                if (keyBoardState.IsKeyDown(Key.F11) && lastKeyBoardState.IsKeyUp(Key.F11))
                {
                    WindowState = WindowState == WindowState.Normal ? WindowState.Fullscreen : WindowState.Normal;
                }

                if (keyBoardState.IsKeyDown(Key.E) && lastKeyBoardState.IsKeyUp(Key.E))
                {
                    CursorVisible = !CursorVisible;
                    CursorGrabbed = !CursorGrabbed;
                    
                    if (!CursorVisible)
                    {
                        camera.lastMouseState = mouseState;
                        camera.Velocity = Vector3.Zero;
                    }
                }

                if (!CursorVisible)
                {
                    camera.ProcessInputs((float)args.Time, keyBoardState, mouseState, out bool frameChanged);
                    if (frameChanged)
                        PathTracing.ThisRenderNumFrame = 0;
                }

                if (CursorVisible && !FinalGUIRenderer.ImGuiIOPtr.WantCaptureMouse && mouseState.IsButtonDown(MouseButton.Left) && lastMouseState.IsButtonUp(MouseButton.Left))
                {
                    Point windowSpaceCoords = PointToClient(new Point(Mouse.GetCursorState().X, Mouse.GetCursorState().Y)); windowSpaceCoords.Y = Height - windowSpaceCoords.Y; // [0, Width][0, Height]
                    Vector2 normalizedDeviceCoords = Vector2.Divide(new Vector2(windowSpaceCoords.X, windowSpaceCoords.Y), new Vector2(Width, Height)) * 2.0f - new Vector2(1.0f);
                    Ray rayWorld = Ray.GetWorldSpaceRay(inverseProjection, camera.View.Inverted(), camera.Position, normalizedDeviceCoords);

                    RayTrace(rayWorld, out GameObjectPropertyRenderer.RayObject);
                    //RayTrace(grid, rayWorld, out GameObjectPropertyRenderer.RayObject);
                }
            }
            int oldOffset = BasicDataUBO.BufferOffset;
            BasicDataUBO.Append(Vector4.SizeInBytes * 4 * 3, new Matrix4[] { camera.View, camera.View.Inverted(), camera.View * projection });
            BasicDataUBO.Append(Vector4.SizeInBytes, camera.Position);
            BasicDataUBO.BufferOffset = oldOffset;

            lastKeyBoardState = keyBoardState;
            lastMouseState = mouseState;
            ups++;
            base.OnUpdateFrame(args);
        }


        readonly List<GameObject> gameObjects = new List<GameObject>();
        ShaderProgram finalProgram;
        public static BufferObject BasicDataUBO, GameObjectsUBO, GridCellsSSBO;
        public PathTracing PathTracing;
        public Rasterizer Rasterizer;
        public ScreenEffect PostProcess;
        public AtmosphericScattering AtmosphericScattering;
        int instancesSpheres = 0, instancesCuboids = 0;

        protected override void OnLoad(EventArgs e)
        {
            Vector2 uboGameObjectsSize = new Vector2(343, 64);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Multisample);
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            VSync = VSyncMode.Off;
            CursorVisible = false;
            CursorGrabbed = true;

            BasicDataUBO = new BufferObject(BufferRangeTarget.UniformBuffer, 0, Vector4.SizeInBytes * 4 * 5 + Vector4.SizeInBytes + Vector4.SizeInBytes, BufferUsageHint.StreamRead);
            GameObjectsUBO = new BufferObject(BufferRangeTarget.UniformBuffer, 1, (int)(Sphere.GPUInstanceSize * uboGameObjectsSize.X + Cuboid.GPUInstanceSize * uboGameObjectsSize.Y), BufferUsageHint.StreamRead);
            finalProgram = new ShaderProgram(new Shader[] { new Shader(ShaderType.VertexShader, @"Src\Shaders\screenQuad.vs"), new Shader(ShaderType.FragmentShader, @"Src\Shaders\final.frag") });

            AtmosphericScattering = new AtmosphericScattering(1024, 50, 10, 40, 1, new Vector3(700, 530, 440));
            AtmosphericScattering.Run();

            PathTracing = new PathTracing(new EnvironmentMap(AtmosphericScattering.Result), Width, Height, 8, 1);
            Rasterizer = new Rasterizer(Width, Height);
            PostProcess = new ScreenEffect(new Shader(ShaderType.FragmentShader, @"Src\Shaders\PostProcessing\fragment.frag"), Width, Height);

            Sphere.GlobalClassBufferOffset = 0;
            float width = 40, height = 25, depth = 25;

            Sphere.GlobalClassBufferOffset = 0;
            {
                int balls = 6;
                float radius = 1.3f;
                Vector3 dimensions = new Vector3(width * 0.6f, height, depth);
                for (float x = 0; x < balls; x++)
                {
                    for (float y = 0; y < balls; y++)
                    {
                        gameObjects.Add(new Sphere(new Vector3(dimensions.X / balls * x * 1.1f - dimensions.X / 2, (dimensions.Y / balls) * y - dimensions.Y / 2 + radius, -17), radius, instancesSpheres++, new Material(albedo: new Vector3(0.59f, 0.59f, 0.99f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: x / (balls - 1), specularRoughness: y / (balls - 1), indexOfRefraction: 1f, refractionChance: 0.0f, refractionRoughnes: 0.1f)));
                    }
                }

                Vector3 delta = dimensions / balls;
                for (float x = 0; x < balls; x++)
                {
                    Material material = Material.Zero;
                    material.Albedo = new Vector3(0.9f, 0.25f, 0.25f);
                    material.SpecularChance = 0.02f;
                    material.IOR = 1.1f;
                    material.RefractionChance = 0.98f;
                    material.RefractionColor = new Vector3(1, 2, 3) * (x / balls);
                    Vector3 position = new Vector3(-dimensions.X / 2 + radius + delta.X * x, 0f, -5f);
                    gameObjects.Add(new Sphere(position, radius, instancesSpheres++, material));


                    Material material1 = Material.Zero;
                    material1.SpecularChance = 0.02f;
                    material1.SpecularRoughness = (x / balls);
                    material1.IOR = 1.1f;
                    material1.RefractionChance = 0.98f;
                    material1.RefractionRoughnes = x / balls;
                    material1.RefractionColor = Vector3.Zero;
                    position = new Vector3(-dimensions.X / 2 + radius + delta.X * x, -10f, -5f);
                    gameObjects.Add(new Sphere(position, radius, instancesSpheres++, material1));
                }
            }

            Cuboid.GlobalClassBufferOffset = (int)(Sphere.GPUInstanceSize * uboGameObjectsSize.X);
            {
                Cuboid down = new Cuboid(new Vector3(0, -height / 2, -10), new Vector3(width, Epsilon, depth), instancesCuboids++, new Material(albedo: new Vector3(1, 0.4f, 0.04f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 0.11f, specularRoughness: 0.051f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));

                Cuboid up = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height, down.Position.Z), new Vector3(down.Dimensions.X, Epsilon, down.Dimensions.Z), instancesCuboids++, new Material(albedo: new Vector3(0.6f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 0.023f, specularRoughness: 0.051f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));
                Cuboid upLight0 = new Cuboid(new Vector3(up.Position.X, down.Position.Y + height - Epsilon, down.Position.Z), new Vector3(down.Dimensions.X / 3, Epsilon, down.Dimensions.Z / 3), instancesCuboids++, new Material(albedo: new Vector3(0.04f), emissiv: new Vector3(0.917f, 0.945f, 0.513f) * 1.5f, refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 1f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));

                Cuboid back = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height / 2, down.Position.Z - depth / 2), new Vector3(width, height, Epsilon), instancesCuboids++, new Material(albedo: new Vector3(0.6f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));
                //Cuboid front = new Cuboid(new Vector3(down.Position.X, down.Position.Y + height / 2 + Epsilon, down.Position.Z + depth / 2 - 0.3f / 2), new Vector3(width, height - Epsilon * 2, 0.3f), instancesCuboids++, new Material(albedo: new Vector3(1f), emissiv: new Vector3(0), refractionColor: new Vector3(0.01f), specularChance: 0.04f, specularRoughness: 0f, indexOfRefraction: 1f, refractionChance: 0.954f, refractionRoughnes: 0));

                Cuboid right = new Cuboid(new Vector3(down.Position.X + width / 2, down.Position.Y + height / 2, down.Position.Z), new Vector3(Epsilon, height, depth), instancesCuboids++, new Material(albedo: new Vector3(1, 0.4f, 0.4f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 0.5f, specularRoughness: 0, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));
                Cuboid left = new Cuboid(new Vector3(down.Position.X - width / 2, down.Position.Y + height / 2, down.Position.Z), new Vector3(Epsilon, height, depth), instancesCuboids++, new Material(albedo: new Vector3(0.24f, 0.6f, 0.24f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));

                Cuboid middle = new Cuboid(new Vector3(right.Position.X * 0.55f, down.Position.Y + 12f / 2, back.Position.Z * 0.4f), new Vector3(3f, 12, 3f), instancesCuboids++, new Material(albedo: new Vector3(0.917f, 0.945f, 0.513f), emissiv: new Vector3(0), refractionColor: Vector3.Zero, specularChance: 1.0f, specularRoughness: 0f, indexOfRefraction: 1f, refractionChance: 0, refractionRoughnes: 0));

                gameObjects.AddRange(new Cuboid[] { down, up, upLight0, back, right, left, middle });
            }

            for (int i = 0; i < gameObjects.Count; i++)
                gameObjects[i].Upload(GameObjectsUBO);


            {
                int ssboCellsSize = 8 * 8 * 8;
                //GridCellsSSBO = new BufferObject(BufferRangeTarget.ShaderStorageBuffer, 0, ssboCellsSize * Grid.Cell.GPUInstanceSize + 1000 * sizeof(int), BufferUsageHint.StreamRead);

                Grid grid;
                grid = new Grid(4, 4, 3);
                grid.Update(gameObjects);

                //Vector4[] gridGPUData = grid.GetGPUFriendlyGridData();
                //GridCellsSSBO.SubData(0, Vector4.SizeInBytes * gridGPUData.Length, gridGPUData);

                //int[] indecisData = grid.GetGPUFriendlyIndecisData();
                //GridCellsSSBO.SubData(ssboCellsSize * Grid.Cell.GPUInstanceSize, indecisData.Length * sizeof(int), indecisData);
                //PathTracing.program.Upload("ssboCellsSize", grid.Cells.Count);
            }


            PathTracing.NumSpheres = instancesSpheres;
            PathTracing.NumCuboids = instancesCuboids;
        }

        protected override void OnResize(EventArgs e)
        {
            PathTracing.SetSize(Width, Height);
            Rasterizer.SetSize(Width, Height);
            PostProcess.SetSize(Width, Height);
            FinalGUIRenderer.Resize(Width, Height);

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), Width / (float)Height, nearFarPlane.X, nearFarPlane.Y);
            inverseProjection = projection.Inverted();
            BasicDataUBO.BufferOffset = 0;
            BasicDataUBO.Append(Vector4.SizeInBytes * 4 * 2, new Matrix4[] { projection, inverseProjection });
            BasicDataUBO.Append(Vector2.SizeInBytes + Vector2.SizeInBytes, nearFarPlane);
            base.OnResize(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            FinalGUIRenderer.ImGuiController.PressChar(e.KeyChar);
        }
       
        public bool onWindowFocus = true;
        protected override void OnFocusedChanged(EventArgs e)
        {
            onWindowFocus = !onWindowFocus;
            if (onWindowFocus)
                camera.lastMouseState = Mouse.GetState();
        }

        protected override void OnClosed(EventArgs e)
        {
            ImGuiNET.ImGui.SaveIniSettingsToDisk("imgui.ini");
            base.OnClosed(e);
        }

        public bool RayTrace(Ray ray, out GameObject gameObject)
        {
            gameObject = null;
            float tMin = float.MaxValue;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i].IntersectsRay(ray, out float t1, out float t2) && t2 > 0 && t1 < tMin)
                {
                    gameObject = gameObjects[i];
                    tMin = GetRelevantT(t1, t2);
                }
            }

            return tMin != float.MaxValue;
        }
        public bool RayTrace(Grid grid, Ray ray, out GameObject gameObject)
        {
            gameObject = null;
            float tMin = float.MaxValue, cellMin = float.MaxValue;
            float t1, t2;

            //TODO: Use DDA algorithm for traversal
            for (int i = 0; i < grid.Cells.Count; i++)
            {
                if (ray.IntersectsAABB(grid.Cells[i].AABB, out float cellT1, out float cellT2) && cellT2 > 0 && cellT1 <= cellMin)
                {
                    for (int j = grid.Cells[i].Start; j < grid.Cells[i].End; j++)
                    {
                        if (gameObjects[grid.Indecis[j]].IntersectsRay(ray, out t1, out t2) && t1 <= cellT2 && t2 > 0 && t1 < tMin)
                        {
                            gameObject = gameObjects[grid.Indecis[j]];
                            tMin = GetRelevantT(t1, t2);
                            cellMin = cellT1;
                        }
                    }
                }
            }

            return tMin != float.MaxValue;
        }
        public static float GetRelevantT(float t1, float t2) => t1 < 0 ? t2 : t1;
        

        public void NewRandomBalls(float xRange, float yRange, float zRange)
        {
            int balls = 6;
            float radius = 1.3f;
            Vector3 dimensions = new Vector3(xRange * 0.6f, yRange, zRange);

            int instance = 0;
            for (float x = 0; x < balls; x++)
            {
                for (float y = 0; y < balls; y++)
                {
                    bool isEmissiv = rnd.NextDouble() < 0.2f;
                    gameObjects[instance] = new Sphere(new Vector3(dimensions.X / balls * x * 1.1f - dimensions.X / 2, (dimensions.Y / balls) * y - dimensions.Y / 2 + radius, -17), radius, instance, new Material(albedo: RndVector(), emissiv: isEmissiv ? RndVector() : Vector3.Zero, refractionColor: RndVector() * 2, specularChance: (float)rnd.NextDouble(), specularRoughness: (float)rnd.NextDouble(), indexOfRefraction: (float)rnd.NextDouble() + 1, refractionChance: (float)rnd.NextDouble(), refractionRoughnes: (float)rnd.NextDouble()));
                    gameObjects[instance++].Upload(GameObjectsUBO);
                }
            }
        }

        static Random rnd = new Random();
        public Vector3 RndVector() => new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
    }
}
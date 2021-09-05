using System;
using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using OpenTK_PathTracer.GUI;
using OpenTK_PathTracer.GameObjects;

namespace OpenTK_PathTracer.Render.GUI
{
    struct Final
    {
        public static ImGuiController ImGuiController = new ImGuiController(0, 0);
        private static GameObjectBase pickedObject;

        public static ImGuiIOPtr ImGuiIOPtr => ImGui.GetIO();

        public static void Run(MainWindow mainWindow, float frameTime, out bool frameChanged)
        {
            ImGuiController.Update(mainWindow, frameTime);

            float windowAlpha = mainWindow.CursorVisible ? 0.8f : 0.3f;
            frameChanged = false;
            ImGui.SetNextWindowBgAlpha(windowAlpha);

            ImGui.Begin("PathTracing", ImGuiWindowFlags.AlwaysAutoResize);
            {
                ImGui.Text($"VSync: {mainWindow.VSync}");
                ImGui.Text($"FPS: {mainWindow.FPS}"); ImGui.SameLine(); ImGui.Text($"UPS: {mainWindow.UPS}");
                ImGui.Checkbox("RenderInBackground", ref mainWindow.IsRenderInBackground);
                int temp = mainWindow.PathTracer.SSP;
                if (ImGui.SliderInt("SSP", ref temp, 1, 10))
                {
                    frameChanged = true;
                    mainWindow.PathTracer.SSP = temp;
                }


                temp = mainWindow.PathTracer.RayDepth;
                if (ImGui.SliderInt("RayDepth", ref temp, 1, 50))
                {
                    frameChanged = true;
                    mainWindow.PathTracer.RayDepth = temp;
                }

                float floatTemp = mainWindow.PathTracer.FocalLength;
                if (ImGui.InputFloat("FocalLength", ref floatTemp, 0.1f))
                {
                    frameChanged = true;
                    mainWindow.PathTracer.FocalLength = MathF.Max(floatTemp, 0);
                }
                
                floatTemp = mainWindow.PathTracer.ApertureDiameter;
                if (ImGui.InputFloat("ApertureDiameter", ref floatTemp, 0.002f))
                {
                    frameChanged = true;
                    mainWindow.PathTracer.ApertureDiameter = MathF.Max(floatTemp, 0);
                }
                ImGui.Text($"f-number: f/{mainWindow.PathTracer.FocalLength / mainWindow.PathTracer.ApertureDiameter}");

                if (ImGui.Button("SpheresRandomMaterial"))
                {
                    frameChanged = true;
                    mainWindow.NewRandomBalls(40, 25, 25);
                }
                if (ImGui.Button("Screenshot"))
                {
                    System.IO.Directory.CreateDirectory("Screenshots");
                    System.Drawing.Bitmap bmp = Screenshotter.DoScreenshot(mainWindow.Width, mainWindow.Height);
                    bmp.Save($@"Screenshots\Samples_{mainWindow.PathTracer.Samples}.png", System.Drawing.Imaging.ImageFormat.Jpeg);
                    bmp.Dispose();
                }

                ImGui.End();
            }

            ImGui.Begin("AtmossphericScattering", ImGuiWindowFlags.AlwaysAutoResize);
            {
                ImGui.Text($"Computation Time: {MathF.Round(mainWindow.AtmosphericScatterer.Query.ElapsedMilliseconds, 2)} ms");

                int tempInt = mainWindow.AtmosphericScatterer.InScatteringSamples;
                if (ImGui.SliderInt("InScatteringSamples", ref tempInt, 1, 100))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.InScatteringSamples = tempInt;
                    mainWindow.AtmosphericScatterer.Run();
                }

                tempInt = mainWindow.AtmosphericScatterer.DensitySamples;
                if (ImGui.SliderInt("DensitySamples", ref tempInt, 1, 40))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.DensitySamples = tempInt;
                    mainWindow.AtmosphericScatterer.Run();
                }

                float temp = mainWindow.AtmosphericScatterer.ScatteringStrength;
                if (ImGui.DragFloat("ScatteringStrength", ref temp, 0.15f, 0.1f, 10))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.ScatteringStrength = temp;
                    mainWindow.AtmosphericScatterer.Run();
                }

                temp = mainWindow.AtmosphericScatterer.DensityFallOff;
                if (ImGui.DragFloat("DensityFallOff", ref temp, 0.5f, 0.1f, 40))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.DensityFallOff = temp;
                    mainWindow.AtmosphericScatterer.Run();
                }

                temp = mainWindow.AtmosphericScatterer.AtmossphereRadius;
                if (ImGui.DragFloat("AtmossphereRadius", ref temp, 0.2f, 0.1f, 100))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.AtmossphereRadius = temp;
                    mainWindow.AtmosphericScatterer.Run();
                }

                System.Numerics.Vector3 nVector3;
                nVector3 = Vector3ToNVector3(mainWindow.AtmosphericScatterer.WaveLengths);
                if (ImGui.InputFloat3("Wavelength (nm)", ref nVector3))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScatterer.WaveLengths = NVector3ToVector3(nVector3);
                    mainWindow.AtmosphericScatterer.Run();
                }

                ImGui.End();
            }

            ImGui.Begin("AddPrimitiv");
            {
                if (ImGui.Button("Sphere"))
                {
                    Ray viewRay = new Ray(mainWindow.Camera.Position, mainWindow.Camera.ViewDir);
                    Sphere newSphere = Sphere.Zero;
                    newSphere.Position = viewRay.GetPoint(1.5f);
                    newSphere.Instance = mainWindow.PathTracer.NumSpheres++;
                    newSphere.Upload();
                    mainWindow.GameObjects.Add(newSphere);
                    
                    pickedObject = newSphere;
                    frameChanged = true;
                }

                if (ImGui.Button("Cuboid"))
                {
                    Ray viewRay = new Ray(mainWindow.Camera.Position, mainWindow.Camera.ViewDir);
                    Cuboid newCuboid = Cuboid.Zero;
                    newCuboid.Position = viewRay.GetPoint(1.5f);
                    newCuboid.Instance = mainWindow.PathTracer.NumCuboids++;
                    newCuboid.Upload();
                    mainWindow.GameObjects.Add(newCuboid);

                    pickedObject = newCuboid;
                    frameChanged = true;
                }
                ImGui.End();
            }

            if (pickedObject != null)
            {
                ImGui.Begin("GameObjectProperties", ImGuiWindowFlags.AlwaysAutoResize);
                {
                    bool hasInput = false;
                    System.Numerics.Vector3 nVector3;
                    nVector3 = Vector3ToNVector3(pickedObject.Position);
                    if (ImGui.DragFloat3("Position", ref nVector3))
                    {
                        pickedObject.Position = NVector3ToVector3(nVector3);
                        hasInput = true;
                    }

                    nVector3 = Vector3ToNVector3(pickedObject.Material.Albedo);
                    if (ImGui.InputFloat3("Albedo", ref nVector3))
                    {
                        pickedObject.Material.Albedo = NVector3ToVector3(nVector3);
                        hasInput = true;
                    }

                    nVector3 = Vector3ToNVector3(pickedObject.Material.Emissiv);
                    if (ImGui.InputFloat3("Emissiv", ref nVector3))
                    {
                        pickedObject.Material.Emissiv = NVector3ToVector3(nVector3);
                        hasInput = true;
                    }

                    nVector3 = Vector3ToNVector3(pickedObject.Material.RefractionColor);
                    if (ImGui.InputFloat3("RefractionColor", ref nVector3))
                    {
                        pickedObject.Material.RefractionColor = NVector3ToVector3(nVector3);
                        hasInput = true;
                    }

                    ImGui.NewLine();

                    if (ImGui.SliderFloat("SpecularChance", ref pickedObject.Material.SpecularChance, 0, 1))
                    {
                        pickedObject.Material.SpecularChance = Math.Clamp(pickedObject.Material.SpecularChance, 0, 1.0f - pickedObject.Material.RefractionChance);
                        hasInput = true;
                    }

                    if (ImGui.SliderFloat("SpecularRoughness", ref pickedObject.Material.SpecularRoughness, 0, 1))
                        hasInput = true;

                    if (ImGui.SliderFloat("IndexOfRefraction", ref pickedObject.Material.IOR, 1, 5))
                        hasInput = true;

                    if (ImGui.SliderFloat("RefractionChance", ref pickedObject.Material.RefractionChance, 0, 1))
                    {
                        pickedObject.Material.RefractionChance = Math.Clamp(pickedObject.Material.RefractionChance, 0, 1.0f - pickedObject.Material.SpecularChance);
                        hasInput = true;
                    }

                    if (ImGui.SliderFloat("RefractionRoughnes", ref pickedObject.Material.RefractionRoughnes, 0, 1))
                        hasInput = true;

                    if (hasInput)
                    {
                        pickedObject.Upload();
                        frameChanged = true;
                    }
                    ImGui.End();
                }
            }
            ImGuiController.Render();
        }

        public static void Update(MainWindow mainWindow)
        {
            if (mainWindow.CursorVisible && !ImGuiIOPtr.WantCaptureMouse)
            {
                if (MouseManager.IsButtonTouched(MouseButton.Left))
                {
                    System.Drawing.Point windowSpaceCoords = mainWindow.PointToClient(new System.Drawing.Point(Mouse.GetCursorState().X, Mouse.GetCursorState().Y)); windowSpaceCoords.Y = mainWindow.Height - windowSpaceCoords.Y; // [0, Width][0, Height]
                    Vector2 normalizedDeviceCoords = Vector2.Divide(new Vector2(windowSpaceCoords.X, windowSpaceCoords.Y), new Vector2(mainWindow.Width, mainWindow.Height)) * 2.0f - new Vector2(1.0f);
                    Ray rayWorld = Ray.GetWorldSpaceRay(mainWindow.inverseProjection, mainWindow.Camera.View.Inverted(), mainWindow.Camera.Position, normalizedDeviceCoords);

                    mainWindow.RayTrace(rayWorld, out pickedObject, out _, out _);

                    /// DEBUG
                    //if (pickedObject is Sphere sphere)
                    //{
                    //    /// Delete from GPU
                    //    int start = pickedObject.BufferOffset + Sphere.GPU_INSTANCE_SIZE;
                    //    int size = Sphere.GPU_INSTANCE_SIZE * mainWindow.PathTracer.NumSpheres - start;

                    //    /// Copys data from GPU to CPU and then with tiny offset back to GPU
                    //    mainWindow.GameObjectsUBO.GetSubData(start, size, out IntPtr followingSphereData);
                    //    mainWindow.GameObjectsUBO.SubData(pickedObject.BufferOffset, size, followingSphereData); // override selected sphere
                    //    System.Runtime.InteropServices.Marshal.FreeHGlobal(followingSphereData);

                    //    /// Delete from CPU
                    //    mainWindow.GameObjects.Remove(sphere);
                    //    mainWindow.PathTracer.NumSpheres--;

                    //    for (int i = 0; i < mainWindow.GameObjects.Count; i++)
                    //        if (mainWindow.GameObjects[i] is Sphere temp && temp != null && temp.Instance > sphere.Instance)
                    //            temp.Instance--;

                    //    mainWindow.PathTracer.ThisRenderNumFrame = 0;
                    //    pickedObject = null;
                    //}
                }
            }
        }

        private static OpenTK.Vector3 NVector3ToVector3(System.Numerics.Vector3 v) => new OpenTK.Vector3(v.X, v.Y, v.Z);
        private static System.Numerics.Vector3 Vector3ToNVector3(OpenTK.Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);

        private static OpenTK.Vector2 NVector2ToVector2(System.Numerics.Vector2 v) => new OpenTK.Vector2(v.X, v.Y);
        private static System.Numerics.Vector2 Vector2ToNVector2(OpenTK.Vector2 v) => new System.Numerics.Vector2(v.X, v.Y);

        public static void Resize(int width, int height)
        {
            ImGuiController.WindowResized(width, height);
        }
    }
}

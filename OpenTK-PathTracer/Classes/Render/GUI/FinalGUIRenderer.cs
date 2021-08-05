using ImGuiNET;

using OpenTK_PathTracer.GUI;
using System;

namespace OpenTK_PathTracer.Render.GUI
{
    static class FinalGUIRenderer
    {
        public static ImGuiController ImGuiController;
        public static void Resize(int width, int height)
        {
            ImGuiController = new ImGuiController(width, height);
        }

        public static ImGuiIOPtr ImGuiIOPtr => ImGui.GetIO();
        public static void Run(MainWindow mainWindow, float frameTime, out bool frameChanged)
        {
            ImGuiController.Update(mainWindow, frameTime);

            float windowAlpha = mainWindow.CursorVisible ? 0.8f : 0.3f;

            ImGui.SetNextWindowBgAlpha(windowAlpha);
            GameObjectPropertyRenderer.Run(out frameChanged);
            if (frameChanged)
                GameObjectPropertyRenderer.RayObject.Upload(MainWindow.GameObjectsUBO);

            ImGui.SetNextWindowBgAlpha(windowAlpha);
            
            ImGui.Begin("Overview");
            {
                ImGui.Text($"VSync: {mainWindow.VSync}");
                ImGui.Text($"FPS: {mainWindow.FPS}"); ImGui.SameLine(); ImGui.Text($"UPS: {mainWindow.UPS}");

                int temp = mainWindow.PathTracing.SSP;
                if (ImGui.SliderInt("SSP", ref temp, 1, 10))
                {
                    frameChanged = true;
                    mainWindow.PathTracing.SSP = temp;
                }


                temp = mainWindow.PathTracing.RayDepth;
                if (ImGui.SliderInt("RayDepth", ref temp, 1, 50))
                {
                    frameChanged = true;
                    mainWindow.PathTracing.RayDepth = temp;
                }

                if (ImGui.Button("SpheresRandomMaterial"))
                {
                    frameChanged = true;
                    mainWindow.NewRandomBalls(40, 25, 25);
                }

                ImGui.End();
            }


            ImGui.Begin("AtmossphericScattering", ImGuiWindowFlags.AlwaysAutoResize);
            {
                ImGui.Text($"Computation Time: {MathF.Round(mainWindow.AtmosphericScattering.Query.ElapsedMilliseconds, 2)} ms");

                int tempInt = mainWindow.AtmosphericScattering.InScatteringSamples;
                if (ImGui.SliderInt("InScatteringSamples", ref tempInt, 1, 100))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScattering.InScatteringSamples = tempInt;
                    mainWindow.AtmosphericScattering.Run();
                }

                tempInt = mainWindow.AtmosphericScattering.DensitySamples;
                if (ImGui.SliderInt("DensitySamples", ref tempInt, 1, 40))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScattering.DensitySamples = tempInt;
                    mainWindow.AtmosphericScattering.Run();
                }

                float temp = mainWindow.AtmosphericScattering.DensityFallOff;
                if (ImGui.SliderFloat("DensityFallOff", ref temp, 0.1f, 100))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScattering.DensityFallOff = temp;
                    mainWindow.AtmosphericScattering.Run();
                }

                temp = mainWindow.AtmosphericScattering.AtmossphereRadius;
                if (ImGui.SliderFloat("AtmossphereRadius", ref temp, 1, 400))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScattering.AtmossphereRadius = temp;
                    mainWindow.AtmosphericScattering.Run();
                }

                System.Numerics.Vector3 nVector3;
                nVector3 = Vector3ToNVector3(mainWindow.AtmosphericScattering.WaveLengths);
                if (ImGui.InputFloat3("Wavelength (nm)", ref nVector3))
                {
                    frameChanged = true;
                    mainWindow.AtmosphericScattering.WaveLengths = NVector3ToVector3(nVector3);
                    mainWindow.AtmosphericScattering.Run();
                }

                ImGui.End();
            }
           

            ImGuiController.Render();
        }

        private static OpenTK.Vector3 NVector3ToVector3(System.Numerics.Vector3 v) => new OpenTK.Vector3(v.X, v.Y, v.Z);
        private static System.Numerics.Vector3 Vector3ToNVector3(OpenTK.Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
}

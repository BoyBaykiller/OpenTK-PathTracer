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

                if (ImGui.Button("SpheresRandomMaterial"))
                {
                    frameChanged = true;
                    mainWindow.NewRandomBalls(40, 25, 25);
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
            }
            ImGuiController.Render();
        }

        private static OpenTK.Vector3 NVector3ToVector3(System.Numerics.Vector3 v) => new OpenTK.Vector3(v.X, v.Y, v.Z);
        private static System.Numerics.Vector3 Vector3ToNVector3(OpenTK.Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
}

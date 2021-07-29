using ImGuiNET;

using OpenTK_PathTracer.GUI;


namespace OpenTK_PathTracer.Render.GUI
{
    static class FinalGUIRenderer
    {
        public static ImGuiController ImGuiController;
        public static void Resize(int width, int height)
        {
            ImGuiController = new ImGuiController(width, height);
        }

        public static ImGuiIOPtr ImGuiIOPtr { get => ImGui.GetIO(); }
        public static void Run(MainWindow mainWindow, float frameTime, out bool frameChanged)
        {
            OpenTK.Graphics.OpenGL4.GL.Viewport(0, 0, mainWindow.Width, mainWindow.Height);
            ImGuiController.Update(mainWindow, frameTime);

            float windowAlpha = mainWindow.onGUI ? 0.8f : 0.3f;

            ImGui.SetNextWindowBgAlpha(windowAlpha);
            GameObjectPropertyRenderer.Run(out frameChanged);
            if (frameChanged)
                GameObjectPropertyRenderer.RayObject.Upload(MainWindow.GameObjectsUBO);

            ImGui.SetNextWindowBgAlpha(windowAlpha);
            ImGui.Begin("Overview");
            ImGui.Text($"VSync: {mainWindow.VSync}");
            ImGui.Text($"FPS: {mainWindow.FPS}"); ImGui.SameLine(); ImGui.Text($"UPS: {mainWindow.UPS}");

            int temp = mainWindow.pathTracing.SSP;
            if (ImGui.SliderInt("SSP", ref temp, 1, 10))
            {
                frameChanged = true;
                mainWindow.pathTracing.SSP = temp;
            }
                

            temp = mainWindow.pathTracing.RayDepth;
            if (ImGui.SliderInt("RayDepth", ref temp, 1, 50))
            {
                frameChanged = true;
                mainWindow.pathTracing.RayDepth = temp;
            }

            if (ImGui.Button("SpheresRandomMaterial"))
            {
                frameChanged = true;
                mainWindow.NewRandomBalls(40, 25, 25);
            }

            ImGui.End();

            ImGuiController.Render();
        }
    }
}

using System;

using ImGuiNET;

using OpenTK_PathTracer.GameObjects;

namespace OpenTK_PathTracer.Render.GUI
{
    static class GameObjectPropertyRenderer
    {
        public static GameObject RayObject;
        public static void Run(out bool hadInput)
        {
            hadInput = false;

            ImGui.Begin("GameObjectProperties", ImGuiWindowFlags.AlwaysAutoResize);
            if (RayObject == null)
            {
                ImGui.Text("No object selected (Press e to go into select mode)");
                return;
            }

            System.Numerics.Vector3 nVector3;
            nVector3 = Vector3ToNVector3(RayObject.Position);
            if (ImGui.DragFloat3("Position", ref nVector3))
            {
                hadInput = true;
                RayObject.Position = NVector3ToVector3(nVector3);
            }

            nVector3 = Vector3ToNVector3(RayObject.Material.Albedo);
            if (ImGui.InputFloat3("Albedo", ref nVector3))
            {
                hadInput = true;
                RayObject.Material.Albedo = NVector3ToVector3(nVector3);
            }

            nVector3 = Vector3ToNVector3(RayObject.Material.Emissiv);
            if (ImGui.InputFloat3("Emissiv", ref nVector3))
            {
                hadInput = true;
                RayObject.Material.Emissiv = NVector3ToVector3(nVector3);
            }

            nVector3 = Vector3ToNVector3(RayObject.Material.RefractionColor);
            if (ImGui.InputFloat3("RefractionColor", ref nVector3))
            {
                hadInput = true;
                RayObject.Material.RefractionColor = NVector3ToVector3(nVector3);
            }

            ImGui.NewLine();

            if (ImGui.SliderFloat("SpecularChance", ref RayObject.Material.SpecularChance, 0, 1))
            {
                RayObject.Material.SpecularChance = Math.Clamp(RayObject.Material.SpecularChance, 0, 1.0f - RayObject.Material.RefractionChance);
                hadInput = true;
            }

            if (ImGui.SliderFloat("SpecularRoughness", ref RayObject.Material.SpecularRoughness, 0, 1))
                hadInput = true;

            if (ImGui.SliderFloat("IndexOfRefraction", ref RayObject.Material.IOR, 1, 5))
                hadInput = true;

            if (ImGui.SliderFloat("RefractionChance", ref RayObject.Material.RefractionChance, 0, 1))
            {
                RayObject.Material.RefractionChance = Math.Clamp(RayObject.Material.RefractionChance, 0, 1.0f - RayObject.Material.SpecularChance);
                hadInput = true;
            }

            if (ImGui.SliderFloat("RefractionRoughnes", ref RayObject.Material.RefractionRoughnes, 0, 1))
                hadInput = true;
            ImGui.End();
        }

        private static OpenTK.Vector3 NVector3ToVector3(System.Numerics.Vector3 v) => new OpenTK.Vector3(v.X, v.Y, v.Z);
        private static System.Numerics.Vector3 Vector3ToNVector3(OpenTK.Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
}

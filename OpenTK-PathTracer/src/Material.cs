﻿using System;
using OpenTK;

namespace OpenTK_PathTracer
{
    class Material : BaseSTD140Compatible
    {
        public static Material Zero => new Material(albedo: Vector3.One, emissiv: Vector3.Zero, refractionColor: Vector3.Zero, specularChance: 0.0f, specularRoughness: 0.0f, indexOfRefraction: 1.0f, refractionChance: 0.0f, refractionRoughnes: 0.0f);
        public const int GPU_INSTANCE_SIZE = 16 * 4;

        public Vector3 Albedo;
        public Vector3 Emissiv;
        public Vector3 AbsorbanceColor;
        public float SpecularChance;
        public float SpecularRoughness;
        public float IOR;
        public float RefractionChance;
        public float RefractionRoughnes;
        public Material(Vector3 albedo, Vector3 emissiv, Vector3 refractionColor, float specularChance, float specularRoughness, float indexOfRefraction, float refractionChance, float refractionRoughnes)
        {
            // Note: diffuse chance is 1.0f - (SpecularChance + RefractionChance). So must add up to 1.0

            Albedo = albedo;
            Emissiv = emissiv;
            AbsorbanceColor = refractionColor;
            SpecularChance = Math.Clamp(specularChance, 0.0f, 1.0f);
            SpecularRoughness = specularRoughness;
            IOR = Math.Max(indexOfRefraction, 1.0f);
            RefractionChance = Math.Clamp(refractionChance, 0.0f, 1.0f - SpecularChance);
            RefractionRoughnes = refractionRoughnes;
        }

        public override int BufferOffset => throw new NotSupportedException("Material is not meant to be directly uploaded to the GPU");

        private readonly Vector4[] gpuData = new Vector4[4];
        public override Vector4[] GetGPUFriendlyData()
        {
            gpuData[0].Xyz = Albedo;
            gpuData[0].W = SpecularChance;
           
            gpuData[1].Xyz = Emissiv;
            gpuData[1].W = SpecularRoughness;

            gpuData[2].Xyz = AbsorbanceColor;
            gpuData[2].W = RefractionChance;

            gpuData[3].X = RefractionRoughnes;
            gpuData[3].Y = IOR;
            return gpuData;
        }

        private readonly static Random rnd = new Random();
        public static Material GetRndMaterial()
        {
            bool isEmissiv = rnd.NextDouble() < 0.2;
            return new Material(albedo: RndVector3(), emissiv: isEmissiv ? RndVector3() : Vector3.Zero, refractionColor: RndVector3() * 2.0f, specularChance: (float)rnd.NextDouble() * 0.5f, specularRoughness: (float)rnd.NextDouble(), indexOfRefraction: (float)rnd.NextDouble() + 1, refractionChance: (float)rnd.NextDouble() * 0.5f, refractionRoughnes: (float)rnd.NextDouble());
        }

        private static Vector3 RndVector3() => new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
    }
}
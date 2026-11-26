#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ToyotaFortuner.Editor
{
    public static class ToyotaFortunerPbrTools
    {
        private const string Root = "Assets/Vehicle/ToyotaFortuner";
        private const string PbrFolder = Root + "/Textures/PBR";
        private const string GeneratedFolder = Root + "/Textures/Generated";
        private const string MaterialFolder = Root + "/Materials";

        [MenuItem("Tools/Toyota Fortuner/Create PBR Material Set")]
        public static void CreatePbrMaterialSet()
        {
            EnsureFolder(Root + "/Materials");
            EnsureFolder(GeneratedFolder);

            Texture2D baseColor = FindTexture("BaseColor", "Albedo", "Base_Color");
            Texture2D normal = FindTexture("Normal", "NormalMap");
            Texture2D ambientOcclusion = FindTexture("AO", "AmbientOcclusion", "Occlusion");
            Texture2D detail = FindTexture("Detail", "Decal", "DetailDecal");
            Texture2D metallicSmoothness = AssetDatabase.LoadAssetAtPath<Texture2D>(
                GeneratedFolder + "/ToyotaFortuner_MetallicSmoothness.png");

            CreateMaterial(
                "ToyotaFortuner_BodyPaint",
                baseColor,
                normal,
                metallicSmoothness,
                ambientOcclusion,
                new Color(0.8f, 0.8f, 0.8f, 1f),
                0.15f,
                false);

            CreateMaterial(
                "ToyotaFortuner_Glass",
                baseColor,
                normal,
                metallicSmoothness,
                null,
                new Color(0.04f, 0.08f, 0.10f, 0.35f),
                0.15f,
                true);

            CreateMaterial(
                "ToyotaFortuner_Rubber",
                null,
                normal,
                null,
                null,
                new Color(0.018f, 0.018f, 0.018f, 1f),
                0.85f,
                false);

            CreateMaterial(
                "ToyotaFortuner_Chrome",
                null,
                normal,
                null,
                null,
                new Color(0.9f, 0.9f, 0.9f, 1f),
                0.18f,
                false,
                1f);

            CreateMaterial(
                "ToyotaFortuner_Lights",
                baseColor,
                normal,
                null,
                null,
                Color.white,
                0.3f,
                false);

            CreateMaterial(
                "ToyotaFortuner_Interior",
                null,
                normal,
                null,
                null,
                new Color(0.035f, 0.035f, 0.035f, 1f),
                0.65f,
                false);

            CreateMaterial(
                "ToyotaFortuner_Decal",
                detail,
                null,
                null,
                null,
                Color.white,
                0.45f,
                true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Toyota Fortuner PBR material set created or updated.");
        }

        [MenuItem("Tools/Toyota Fortuner/Pack Metallic + Roughness For URP")]
        public static void PackMetallicAndRoughness()
        {
            Texture2D metallic = FindTexture("Metallic", "Metalness");
            Texture2D roughness = FindTexture("Roughness");
            if (metallic == null || roughness == null)
            {
                EditorUtility.DisplayDialog(
                    "Toyota Fortuner PBR",
                    "Metallic and Roughness textures were not found in Assets/Vehicle/ToyotaFortuner/Textures/PBR.",
                    "OK");
                return;
            }

            string metallicPath = AssetDatabase.GetAssetPath(metallic);
            string roughnessPath = AssetDatabase.GetAssetPath(roughness);
            bool metallicReadable = MakeReadable(metallicPath);
            bool roughnessReadable = MakeReadable(roughnessPath);

            try
            {
                Color32[] metallicPixels = metallic.GetPixels32();
                Color32[] roughnessPixels = roughness.GetPixels32();
                int width = metallic.width;
                int height = metallic.height;
                Color32[] packedPixels = new Color32[width * height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int outputIndex = y * width + x;
                        int roughnessX = Mathf.Clamp(
                            Mathf.RoundToInt((x / (float)Mathf.Max(1, width - 1)) * (roughness.width - 1)),
                            0,
                            roughness.width - 1);
                        int roughnessY = Mathf.Clamp(
                            Mathf.RoundToInt((y / (float)Mathf.Max(1, height - 1)) * (roughness.height - 1)),
                            0,
                            roughness.height - 1);
                        Color32 metallicPixel = metallicPixels[outputIndex];
                        Color32 roughnessPixel = roughnessPixels[roughnessY * roughness.width + roughnessX];
                        byte smoothness = (byte)(255 - roughnessPixel.r);
                        packedPixels[outputIndex] = new Color32(
                            metallicPixel.r,
                            metallicPixel.r,
                            metallicPixel.r,
                            smoothness);
                    }
                }

                Texture2D packed = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                packed.SetPixels32(packedPixels);
                packed.Apply(false, true);
                EnsureFolder(GeneratedFolder);
                string outputPath = Path.Combine(
                    Application.dataPath,
                    "Vehicle/ToyotaFortuner/Textures/Generated/ToyotaFortuner_MetallicSmoothness.png");
                File.WriteAllBytes(outputPath, packed.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(packed);
                AssetDatabase.ImportAsset(GeneratedFolder + "/ToyotaFortuner_MetallicSmoothness.png");
                Debug.Log("Packed Metallic (RGB) + Smoothness (A) texture generated for URP/Built-in Lit.");
            }
            finally
            {
                RestoreReadable(metallicPath, metallicReadable);
                RestoreReadable(roughnessPath, roughnessReadable);
            }
        }

        private static void CreateMaterial(
            string name,
            Texture2D baseColor,
            Texture2D normal,
            Texture2D metallicSmoothness,
            Texture2D ambientOcclusion,
            Color fallbackColor,
            float smoothness,
            bool transparent,
            float metallic = 0f)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("Could not find a Lit shader. Install or enable a Unity render pipeline first.");
                return;
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            SetTexture(material, baseColor, "_BaseMap", "_MainTex");
            SetTexture(material, normal, "_BumpMap");
            SetTexture(material, metallicSmoothness, "_MetallicGlossMap");
            SetTexture(material, ambientOcclusion, "_OcclusionMap");
            SetTexture(material, baseColor, "_BaseMap", "_MainTex");
            SetTexture(material, normal, "_BumpMap");
            material.SetColor("_BaseColor", fallbackColor);
            material.SetColor("_Color", fallbackColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Glossiness", smoothness);
            material.EnableKeyword("_NORMALMAP");

            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Mode", 3f);
                material.SetFloat("_Alpha", fallbackColor.a);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            EditorUtility.SetDirty(material);
        }

        private static void SetTexture(Material material, Texture2D texture, params string[] propertyNames)
        {
            if (texture == null)
            {
                return;
            }

            foreach (string propertyName in propertyNames)
            {
                if (material.HasProperty(propertyName))
                {
                    material.SetTexture(propertyName, texture);
                    return;
                }
            }
        }

        private static Texture2D FindTexture(params string[] nameTokens)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PbrFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                foreach (string token in nameTokens)
                {
                    if (fileName.Contains(token.ToLowerInvariant()))
                    {
                        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
            }

            return null;
        }

        private static bool MakeReadable(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.isReadable)
            {
                return importer != null && importer.isReadable;
            }

            importer.isReadable = true;
            importer.SaveAndReimport();
            return false;
        }

        private static void RestoreReadable(string path, bool wasReadable)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || wasReadable)
            {
                return;
            }

            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModelGame.Vehicle.Editor
{
    public static class FortunerPbrMaterialMenu
    {
        private const string TextureFolder = "Assets/Vehicle/FortunerProcedural/Textures";
        private const string MaterialFolder = "Assets/Vehicle/FortunerProcedural/Materials/PBR";

        [MenuItem("Tools/Fortuner/Create Body PBR Material From Textures")]
        public static void CreateBodyPbrMaterial()
        {
            EnsureFolder("Assets/Vehicle");
            EnsureFolder("Assets/Vehicle/FortunerProcedural");
            EnsureFolder("Assets/Vehicle/FortunerProcedural/Materials");
            EnsureFolder(MaterialFolder);

            Texture2D baseColor = FindTexture("basecolor", "albedo", "diffuse");
            Texture2D normal = FindTexture("normal");
            Texture2D metallic = FindTexture("metallic", "metalness");
            Texture2D ambientOcclusion = FindTexture("ambientocclusion", "_ao", "ao");

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            string materialPath = MaterialFolder + "/Fortuner_Body_PBR.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "Fortuner_Body_PBR"
                };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.shader = shader;
            }

            SetColor(material, "_BaseColor", new Color(0.86f, 0.87f, 0.88f, 1f));
            SetColor(material, "_Color", new Color(0.86f, 0.87f, 0.88f, 1f));
            SetTexture(material, "_BaseMap", baseColor);
            SetTexture(material, "_MainTex", baseColor);
            SetTexture(material, "_BumpMap", normal);
            SetTexture(material, "_MetallicGlossMap", metallic);
            SetTexture(material, "_OcclusionMap", ambientOcclusion);
            SetFloat(material, "_Metallic", metallic == null ? 0.05f : 1f);
            SetFloat(material, "_Smoothness", 0.72f);
            SetFloat(material, "_OcclusionStrength", 1f);
            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = material;

            Debug.Log("Fortuner PBR material created/updated at " + materialPath + ". " +
                      "Roughness should be inverted and packed into MetallicGloss alpha for production URP use.");
        }

        private static Texture2D FindTexture(params string[] tokens)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (fileName.Contains(tokens[i]))
                    {
                        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
            }
            return null;
        }

        private static void SetColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (texture != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif

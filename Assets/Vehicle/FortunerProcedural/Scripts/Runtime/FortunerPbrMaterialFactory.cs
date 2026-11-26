using UnityEngine;

namespace ModelGame.Vehicle
{
    [System.Serializable]
    public class FortunerPbrTextureSet
    {
        public Texture2D baseColor;
        public Texture2D normal;
        public Texture2D roughness;
        public Texture2D metallic;
        public Texture2D ambientOcclusion;
        public Texture2D height;
    }

    /// <summary>
    /// Creates a URP Lit material and wires the texture slots that Unity exposes.
    /// Roughness is converted to a scalar smoothness value because URP Lit stores
    /// smoothness in the alpha channel of the metallic/smoothness map.
    /// </summary>
    public static class FortunerPbrMaterialFactory
    {
        public static Material Create(
            string materialName,
            Color baseColor,
            float metallic,
            float roughness,
            FortunerPbrTextureSet textures = null,
            bool emission = false,
            Color emissionColor = default(Color))
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = materialName
            };

            SetColor(material, "_BaseColor", baseColor);
            SetColor(material, "_Color", baseColor);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Smoothness", Mathf.Clamp01(1f - roughness));

            if (textures != null)
            {
                SetTexture(material, "_BaseMap", textures.baseColor);
                SetTexture(material, "_MainTex", textures.baseColor);

                if (textures.normal != null)
                {
                    SetTexture(material, "_BumpMap", textures.normal);
                    SetFloat(material, "_BumpScale", 1f);
                    material.EnableKeyword("_NORMALMAP");
                }

                SetTexture(material, "_MetallicGlossMap", textures.metallic);
                SetTexture(material, "_OcclusionMap", textures.ambientOcclusion);
                SetFloat(material, "_OcclusionStrength", 1f);
                SetTexture(material, "_ParallaxMap", textures.height);
                SetFloat(material, "_Parallax", 0.02f);
            }

            if (emission)
            {
                Color finalEmission = emissionColor == default(Color) ? baseColor : emissionColor;
                SetColor(material, "_EmissionColor", finalEmission);
                material.EnableKeyword("_EMISSION");
            }

            return material;
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
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only importer and asset generator for the two supplied reference sheets.
/// The default rectangles match the layout of Image #2 and can be adjusted for
/// another atlas without changing the runtime builder.
/// </summary>
public sealed class FortunerUltraGeneratorWindow : EditorWindow
{
    private Texture2D referenceBoard;
    private string outputFolder = "Assets/FortunerUltra/Generated";
    private float length = 4.795f;
    private float width = 1.855f;
    private float height = 1.835f;
    private bool generateColliders;
    private bool generateWheelColliders;
    private bool createPrefab = true;
    private bool extractMapsBeforeGenerate = true;

    // Normalized top-left rectangles for Image #2. Borders and title strips are
    // intentionally excluded. These are editable in the window if the board is cropped.
    private Rect baseColorRect = new Rect(0.412f, 0.050f, 0.300f, 0.340f);
    private Rect normalRect = new Rect(0.713f, 0.050f, 0.287f, 0.340f);
    private Rect roughnessRect = new Rect(0.412f, 0.435f, 0.198f, 0.205f);
    private Rect metallicRect = new Rect(0.612f, 0.435f, 0.200f, 0.205f);
    private Rect aoRect = new Rect(0.813f, 0.435f, 0.187f, 0.205f);

    [MenuItem("Tools/AYUTTHAYA/Generate Fortuner Ultra PBR")]
    public static void OpenWindow()
    {
        GetWindow<FortunerUltraGeneratorWindow>("Fortuner Ultra PBR");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Fortuner Ultra PBR Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "The board is a reference atlas, not the original mesh. This tool extracts usable map panels and builds a procedural modular SUV with its own compatible UVs.",
            MessageType.Info);

        referenceBoard = (Texture2D)EditorGUILayout.ObjectField("Reference texture board", referenceBoard, typeof(Texture2D), false);
        outputFolder = EditorGUILayout.TextField("Output folder", outputFolder);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Real-world dimensions (meters)", EditorStyles.boldLabel);
        length = EditorGUILayout.FloatField("Length", length);
        width = EditorGUILayout.FloatField("Width", width);
        height = EditorGUILayout.FloatField("Height", height);

        EditorGUILayout.Space(6f);
        generateColliders = EditorGUILayout.Toggle("Generate collision proxies", generateColliders);
        generateWheelColliders = EditorGUILayout.Toggle("Generate WheelColliders", generateWheelColliders);
        extractMapsBeforeGenerate = EditorGUILayout.Toggle("Extract PBR maps first", extractMapsBeforeGenerate);
        createPrefab = EditorGUILayout.Toggle("Save prefab", createPrefab);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Map rectangles (normalized, top-left origin)", EditorStyles.boldLabel);
        baseColorRect = EditorGUILayout.RectField("Base Color", baseColorRect);
        normalRect = EditorGUILayout.RectField("Normal", normalRect);
        roughnessRect = EditorGUILayout.RectField("Roughness", roughnessRect);
        metallicRect = EditorGUILayout.RectField("Metallic", metallicRect);
        aoRect = EditorGUILayout.RectField("AO", aoRect);

        EditorGUILayout.Space(8f);
        using (new EditorGUI.DisabledScope(referenceBoard == null))
        {
            if (GUILayout.Button("Extract PBR PNGs from reference board", GUILayout.Height(28f)))
            {
                ExtractMapsAndCreateMaterial();
            }
        }

        if (GUILayout.Button("Generate procedural Fortuner in current scene", GUILayout.Height(32f)))
        {
            GenerateInScene();
        }
    }

    private void ExtractMapsAndCreateMaterial()
    {
        if (referenceBoard == null)
        {
            EditorUtility.DisplayDialog("Fortuner Ultra", "Assign Image #2 first.", "OK");
            return;
        }

        if (!ConfirmOverwriteIfNeeded()) return;

        EnsureAssetFolder(outputFolder);
        EnsureReadable(referenceBoard);

        Texture2D baseColor = CropTopLeft(referenceBoard, baseColorRect, TextureFormat.RGBA32);
        Texture2D normal = CropTopLeft(referenceBoard, normalRect, TextureFormat.RGBA32);
        Texture2D roughness = CropTopLeft(referenceBoard, roughnessRect, TextureFormat.RGBA32);
        Texture2D metallic = CropTopLeft(referenceBoard, metallicRect, TextureFormat.RGBA32);
        Texture2D ao = CropTopLeft(referenceBoard, aoRect, TextureFormat.RGBA32);
        Texture2D packed = PackMetallicSmoothness(metallic, roughness);

        Texture2D baseAsset = SaveTexture(baseColor, "Fortuner_BaseColor.png", true, false);
        Texture2D normalAsset = SaveTexture(normal, "Fortuner_Normal.png", false, true);
        Texture2D roughnessAsset = SaveTexture(roughness, "Fortuner_Roughness.png", false, false);
        Texture2D metallicAsset = SaveTexture(metallic, "Fortuner_Metallic.png", false, false);
        Texture2D packedAsset = SaveTexture(packed, "Fortuner_MetallicSmoothness.png", false, false);
        Texture2D aoAsset = SaveTexture(ao, "Fortuner_AO.png", false, false);

        CreateMaterialAsset(baseAsset, normalAsset, metallicAsset, packedAsset, aoAsset);

        DestroyImmediate(baseColor);
        DestroyImmediate(normal);
        DestroyImmediate(roughness);
        DestroyImmediate(metallic);
        DestroyImmediate(ao);
        DestroyImmediate(packed);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Fortuner Ultra", "PBR PNGs and a Unity material were created. The source sheet resolution is preserved; it is not native 4K unless the source is 4K.", "OK");
    }

    private void GenerateInScene()
    {
        EnsureAssetFolder(outputFolder);

        Texture2D baseColor = LoadGeneratedTexture("Fortuner_BaseColor.png");
        Texture2D normal = LoadGeneratedTexture("Fortuner_Normal.png");
        Texture2D roughness = LoadGeneratedTexture("Fortuner_Roughness.png");
        Texture2D metallic = LoadGeneratedTexture("Fortuner_Metallic.png");
        Texture2D packed = LoadGeneratedTexture("Fortuner_MetallicSmoothness.png");
        Texture2D ao = LoadGeneratedTexture("Fortuner_AO.png");

        if (extractMapsBeforeGenerate && referenceBoard != null && baseColor == null)
        {
            ExtractMapsAndCreateMaterial();
            AssetDatabase.Refresh();
            baseColor = LoadGeneratedTexture("Fortuner_BaseColor.png");
            normal = LoadGeneratedTexture("Fortuner_Normal.png");
            roughness = LoadGeneratedTexture("Fortuner_Roughness.png");
            metallic = LoadGeneratedTexture("Fortuner_Metallic.png");
            packed = LoadGeneratedTexture("Fortuner_MetallicSmoothness.png");
            ao = LoadGeneratedTexture("Fortuner_AO.png");
        }

        GameObject root = new GameObject("FortunerUltra");
        Undo.RegisterCreatedObjectUndo(root, "Create Fortuner Ultra");
        Selection.activeGameObject = root;

        FortunerProceduralBuilder builder = root.AddComponent<FortunerProceduralBuilder>();
        builder.Configure(length, width, height, generateColliders, generateWheelColliders,
            null, baseColor, normal, roughness, metallic, packed, ao, null);
        builder.GenerateFortuner();

        if (createPrefab)
        {
            SavePrefabAndGeneratedAssets(root);
        }

        EditorUtility.DisplayDialog("Fortuner Ultra", "Procedural model generated. Check the Console for the known limitations and asset paths.", "OK");
    }

    private void SavePrefabAndGeneratedAssets(GameObject root)
    {
        string meshFolder = outputFolder + "/Meshes";
        string materialFolder = outputFolder + "/Materials";
        string prefabFolder = outputFolder + "/Prefabs";
        EnsureAssetFolder(meshFolder);
        EnsureAssetFolder(materialFolder);
        EnsureAssetFolder(prefabFolder);

        Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material source = renderer.sharedMaterial;
            if (source == null || AssetDatabase.Contains(source)) continue;

            Material saved;
            if (!materialMap.TryGetValue(source, out saved))
            {
                saved = new Material(source) { name = source.name };
                string path = AssetDatabase.GenerateUniqueAssetPath(materialFolder + "/" + Sanitize(source.name) + ".mat");
                AssetDatabase.CreateAsset(saved, path);
                materialMap.Add(source, saved);
            }
            renderer.sharedMaterial = saved;
        }

        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh source = filter.sharedMesh;
            if (source == null || AssetDatabase.Contains(source)) continue;
            if (!source.name.Contains("Fortuner Ultra") && !source.name.EndsWith(" Mesh")) continue;

            Mesh saved = UnityEngine.Object.Instantiate(source);
            saved.name = source.name;
            string path = AssetDatabase.GenerateUniqueAssetPath(meshFolder + "/" + Sanitize(source.name) + ".asset");
            AssetDatabase.CreateAsset(saved, path);
            filter.sharedMesh = saved;
        }

        string prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabFolder + "/FortunerUltra.prefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.UserAction);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Fortuner Ultra saved to " + prefabPath + ". The procedural geometry is an approximation because the original production mesh was not supplied.");
    }

    private Texture2D LoadGeneratedTexture(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(outputFolder + "/" + fileName);
    }

    private Texture2D CropTopLeft(Texture2D source, Rect normalized, TextureFormat format)
    {
        Rect r = ClampRect(normalized);
        int x = Mathf.Clamp(Mathf.RoundToInt(r.x * source.width), 0, source.width - 1);
        int yTop = Mathf.Clamp(Mathf.RoundToInt(r.y * source.height), 0, source.height - 1);
        int w = Mathf.Clamp(Mathf.RoundToInt(r.width * source.width), 1, source.width - x);
        int h = Mathf.Clamp(Mathf.RoundToInt(r.height * source.height), 1, source.height - yTop);
        int yBottom = source.height - yTop - h;

        Texture2D result = new Texture2D(w, h, format, true, !format.Equals(TextureFormat.RGBA32));
        result.name = source.name + " Crop";
        result.SetPixels(source.GetPixels(x, yBottom, w, h));
        result.Apply(true, false);
        return result;
    }

    private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness)
    {
        int width = Mathf.Min(metallic.width, roughness.width);
        int height = Mathf.Min(metallic.height, roughness.height);
        Texture2D packed = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
        Color[] metallicPixels = metallic.GetPixels();
        Color[] roughnessPixels = roughness.GetPixels();
        Color[] output = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sourceIndex = y * metallic.width + x;
                int roughnessIndex = y * roughness.width + x;
                int outputIndex = y * width + x;
                float metallicValue = metallicPixels[sourceIndex].grayscale;
                float smoothness = 1f - roughnessPixels[roughnessIndex].grayscale;
                output[outputIndex] = new Color(metallicValue, metallicValue, metallicValue, smoothness);
            }
        }

        packed.SetPixels(output);
        packed.Apply(true, false);
        packed.name = "Fortuner MetallicSmoothness";
        return packed;
    }

    private Texture2D SaveTexture(Texture2D texture, string fileName, bool sRgb, bool normalMap)
    {
        string assetPath = outputFolder + "/" + fileName;
        string absolutePath = Path.GetFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = sRgb;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 8;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
    }

    private bool ConfirmOverwriteIfNeeded()
    {
        string[] fileNames =
        {
            "Fortuner_BaseColor.png",
            "Fortuner_Normal.png",
            "Fortuner_Roughness.png",
            "Fortuner_Metallic.png",
            "Fortuner_MetallicSmoothness.png",
            "Fortuner_AO.png"
        };

        foreach (string fileName in fileNames)
        {
            if (File.Exists(Path.GetFullPath(outputFolder + "/" + fileName)))
            {
                return EditorUtility.DisplayDialog(
                    "Fortuner Ultra",
                    "One or more output maps already exist. Overwrite them?",
                    "Overwrite",
                    "Cancel");
            }
        }

        return true;
    }

    private void CreateMaterialAsset(Texture2D baseColor, Texture2D normal, Texture2D metallic,
        Texture2D packed, Texture2D ao)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null)
        {
            Debug.LogError("No URP/Lit or Standard shader was found. The material was not created.");
            return;
        }

        Material material = new Material(shader) { name = "Fortuner Ultra PBR" };
        SetTexture(material, "_BaseMap", baseColor);
        SetTexture(material, "_MainTex", baseColor);
        SetTexture(material, "_BumpMap", normal);
        SetTexture(material, "_MetallicGlossMap", packed != null ? packed : metallic);
        SetTexture(material, "_OcclusionMap", ao);
        material.EnableKeyword("_NORMALMAP");
        material.SetFloat("_Metallic", 1f);
        material.SetFloat("_Smoothness", 0.72f);

        string path = AssetDatabase.GenerateUniqueAssetPath(outputFolder + "/FortunerUltraPBR.mat");
        AssetDatabase.CreateAsset(material, path);
    }

    private static void SetTexture(Material material, string property, Texture texture)
    {
        if (texture != null && material.HasProperty(property)) material.SetTexture(property, texture);
    }

    private static void EnsureReadable(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }

    private static Rect ClampRect(Rect rect)
    {
        float x = Mathf.Clamp01(rect.x);
        float y = Mathf.Clamp01(rect.y);
        float right = Mathf.Clamp01(rect.xMax);
        float bottom = Mathf.Clamp01(rect.yMax);
        return Rect.MinMaxRect(x, y, Mathf.Max(x + 0.001f, right), Mathf.Max(y + 0.001f, bottom));
    }

    private static void EnsureAssetFolder(string assetFolder)
    {
        string[] parts = assetFolder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static string Sanitize(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid.ToString(), "_");
        return value.Replace("/", "_").Replace("\\", "_");
    }
}
#endif

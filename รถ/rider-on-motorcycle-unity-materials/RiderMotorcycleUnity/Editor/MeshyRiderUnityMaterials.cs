using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Creates/updates only the two supplied materials, then remaps only the two original FBX material slots.
internal static class MeshyRiderUnityMaterials
{
    internal const string MaterialMenu = "Tools/Meshy Rider/Set Up Materials for Current Pipeline";
    internal const string RemapMenu = "Tools/Meshy Rider/Apply Materials to Selected FBX";

    [MenuItem(MaterialMenu)]
    private static void SetUpMenu() => RunMenu(() => SetUp());

    [MenuItem(RemapMenu, true)]
    private static bool CanApplyMenu() => AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) is ModelImporter;

    [MenuItem(RemapMenu)]
    private static void ApplyMenu() => RunMenu(() => Apply(AssetDatabase.GetAssetPath(Selection.activeObject)));

    private static void RunMenu(Action action)
    {
        try { action(); }
        catch (Exception e) { Debug.LogException(e); EditorUtility.DisplayDialog("Meshy Rider", e.Message, "OK"); }
    }

    internal static Material[] SetUp()
    {
        string root = PackageRoot();
        var asset = GraphicsSettings.currentRenderPipeline;
        string pipeline = asset == null ? "Built-in" : DetectPipeline(asset);
        string shaderName = pipeline == "Built-in" ? "Standard" : pipeline == "URP" ? "Universal Render Pipeline/Lit" : "HDRP/Lit";
        Shader shader = Shader.Find(shaderName);
        if (shader == null) throw new InvalidOperationException("Shader not found: " + shaderName + ". Check that your active render pipeline is installed.");
        if (!AssetDatabase.IsValidFolder(root + "/Materials")) AssetDatabase.CreateFolder(root, "Materials");
        Material bike = MakeMaterial(root, shader, pipeline, "Motorcycle", ".jpg");
        Material rider = MakeMaterial(root, shader, pipeline, "Rider", ".png");
        AssetDatabase.SaveAssets();
        Debug.Log("Meshy Rider: materials ready for " + pipeline + " (" + shaderName + ").");
        return new[] { bike, rider };
    }

    private static string DetectPipeline(RenderPipelineAsset asset)
    {
        for (Type t = asset.GetType(); t != null; t = t.BaseType)
        {
            if (t.FullName == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset") return "URP";
            if (t.FullName == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset") return "HDRP";
        }
        throw new InvalidOperationException("Unsupported render pipeline: " + asset.GetType().FullName);
    }

    private static string PackageRoot()
    {
        var path = AssetDatabase.FindAssets("MeshyRiderUnityMaterials t:MonoScript")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => p.EndsWith("/Editor/MeshyRiderUnityMaterials.cs", StringComparison.Ordinal) && AssetDatabase.IsValidFolder(p.Substring(0, p.LastIndexOf("/Editor/", StringComparison.Ordinal)) + "/Textures"));
        if (path == null) throw new InvalidOperationException("Keep the Editor and Textures folders together inside Assets.");
        return path.Substring(0, path.LastIndexOf("/Editor/", StringComparison.Ordinal));
    }

    private static Texture2D GetMap(string path, bool normal, bool data)
    {
        if (!(AssetImporter.GetAtPath(path) is TextureImporter i)) throw new FileNotFoundException("Missing texture asset: " + path);
        var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        var alpha = normal ? TextureImporterAlphaSource.None : TextureImporterAlphaSource.FromInput;
        var compression = data ? TextureImporterCompression.Uncompressed : TextureImporterCompression.CompressedHQ;
        if (i.textureType != type || i.sRGBTexture != !(normal || data) ||
            i.alphaSource != alpha || i.alphaIsTransparency || i.convertToNormalmap || i.textureCompression != compression || i.maxTextureSize != 4096)
        {
            i.textureType = type;
            i.sRGBTexture = !(normal || data);
            i.alphaSource = alpha;
            i.alphaIsTransparency = false;
            i.convertToNormalmap = false;
            i.textureCompression = compression;
            i.maxTextureSize = 4096;
            i.SaveAndReimport();
        }
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) throw new FileNotFoundException("Unity could not load: " + path);
        return texture;
    }

    private static Material MakeMaterial(string root, Shader shader, string pipeline, string part, string extension)
    {
        string from = root + "/Textures/" + part;
        var color = GetMap(from + "_BaseColor" + extension, false, false);
        var normal = GetMap(from + "_Normal.png", true, false);
        var metal = GetMap(from + "_MetallicSmoothness.png", false, true);
        var setup = new Material(shader) { name = part };
        if (pipeline == "Built-in")
        {
            setup.SetTexture("_MainTex", color); setup.SetTexture("_BumpMap", normal); setup.SetTexture("_MetallicGlossMap", metal);
            setup.SetFloat("_Metallic", 1f); setup.SetFloat("_Glossiness", 1f); setup.SetFloat("_GlossMapScale", 1f);
            setup.SetFloat("_BumpScale", 1f); setup.SetFloat("_SmoothnessTextureChannel", 0f);
            setup.EnableKeyword("_NORMALMAP"); setup.EnableKeyword("_METALLICGLOSSMAP");
        }
        else if (pipeline == "URP")
        {
            setup.SetTexture("_BaseMap", color); setup.SetTexture("_BumpMap", normal); setup.SetTexture("_MetallicGlossMap", metal);
            setup.SetFloat("_WorkflowMode", 1f); setup.SetFloat("_Metallic", 1f); setup.SetFloat("_Smoothness", 1f);
            setup.SetFloat("_BumpScale", 1f); setup.SetFloat("_SmoothnessTextureChannel", 0f); setup.SetFloat("_Cull", 0f);
            setup.EnableKeyword("_NORMALMAP"); setup.EnableKeyword("_METALLICSPECGLOSSMAP"); setup.doubleSidedGI = true;
        }
        else
        {
            setup.SetTexture("_BaseColorMap", color); setup.SetTexture("_NormalMap", normal); setup.SetTexture("_MaskMap", metal);
            setup.SetFloat("_Metallic", 1f); setup.SetFloat("_Smoothness", 1f); setup.SetFloat("_NormalScale", 1f);
            setup.SetFloat("_DoubleSidedEnable", 1f); setup.doubleSidedGI = true;
            setup.EnableKeyword("_NORMALMAP"); setup.EnableKeyword("_NORMALMAP_TANGENT_SPACE"); setup.EnableKeyword("_MASKMAP"); setup.EnableKeyword("_DOUBLESIDED_ON");
            var hd = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UnityEngine.Rendering.HighDefinition.HDMaterial", false)).FirstOrDefault(a => a != null);
            hd?.GetMethod("ValidateMaterial", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Material) }, null)?.Invoke(null, new object[] { setup });
        }
        string path = root + "/Materials/" + part + ".mat";
        var result = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (result == null) { result = setup; AssetDatabase.CreateAsset(result, path); }
        else { EditorUtility.CopySerialized(setup, result); UnityEngine.Object.DestroyImmediate(setup); EditorUtility.SetDirty(result); }
        return result;
    }

    internal static void Apply(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (importer == null || model == null || !path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Select rider-on-motorcycle-v4.fbx in the Unity Project window, then run this command again.");
        var names = model.GetComponentsInChildren<Renderer>(true).Select(r => r.gameObject.name).ToArray();
        if (!(new[] { "Motorcycle", "Rider", "Front wheel", "Rear wheel" }).All(n => names.Contains(n)))
            throw new InvalidOperationException("This FBX is not the supplied rider-on-motorcycle model. Select the correct model in the Project window.");
        var assets = SetUp();
        if (importer.materialImportMode == ModelImporterMaterialImportMode.None) importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Material_0"), assets[0]);
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "BakedMaterial"), assets[1]);
        importer.SaveAndReimport();
        model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var matches = model.GetComponentsInChildren<Renderer>(true).Where(r => names.Contains(r.gameObject.name)).ToArray();
        if (matches.Length != 4 || matches.Any(r => r.sharedMaterials.Length != 1 || r.sharedMaterial != (r.gameObject.name == "Rider" ? assets[1] : assets[0])))
            throw new InvalidOperationException("The reimport did not bind all four meshes. Assign Motorcycle.mat to Motorcycle and both wheels, and Rider.mat to Rider.");
        Debug.Log("Meshy Rider: both material slots remapped successfully. The FBX file and its animation were not edited.", model);
    }
}

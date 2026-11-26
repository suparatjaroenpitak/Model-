// Unity 6 editor-only. Reads prebuilt mesh data; no runtime scripts or external packages.
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

public static class ThaiShophouseV5Generator
{
    const string DefaultRoot = "Assets/ThaiShophouseV5";
    static readonly float[] Widths = {5.4f,5.3f,3.7f,3.7f,4.4f,4.4f};
    static readonly float[] Heights = {13.6f,13.8f,11.9f,11.9f,13.2f,13.2f};
    static readonly string[] Labels = {"GoldShop_Red","GoldShop_Crest","GableShop_A","GableShop_B","OpticalShop","AirConditioningShop","StreetAndUtilities"};
    static readonly string[] Maps = {"BaseColor","Normal","Roughness","Metallic","AO","Height","MetallicSmoothness"};
    const float Total = 26.9f;
    const float Depth = 12f;

    [MenuItem("Tools/AYUTTHAYA/Generate Shophouse V5 PBR")]
    static void GeneratePbr() { Generate(false); }
    [MenuItem("Tools/AYUTTHAYA/Generate Shophouse V5 Clay")]
    static void GenerateClay() { Generate(true); }
    [MenuItem("Tools/AYUTTHAYA/Shophouse V5 Force LOD0 (Selected)")]
    static void ForceLod() { SetLod(0); }
    [MenuItem("Tools/AYUTTHAYA/Shophouse V5 Restore Automatic LOD (Selected)")]
    static void AutoLod() { SetLod(-1); }
    static void SetLod(int n)
    {
        if (!Selection.activeGameObject) return;
        foreach (var g in Selection.activeGameObject.GetComponentsInChildren<LODGroup>(true)) g.ForceLOD(n);
        SceneView.RepaintAll();
    }
    static string SourceRoot()
    {
        if (File.Exists(DefaultRoot+"/MeshData/Unit1_LOD0.bytes")) return DefaultRoot;
        foreach (string guid in AssetDatabase.FindAssets("ThaiShophouseV5Generator t:MonoScript"))
        {
            string p=AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(p)!="ThaiShophouseV5Generator.cs") continue;
            string root=Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\','/');
            if (File.Exists(root+"/MeshData/Unit1_LOD0.bytes")) return root;
        }
        throw new FileNotFoundException("Copy the COMPLETE ThaiShophouseV5 folder (Editor, MeshData, Textures) into Assets. Do not copy only the C# script.");
    }
    static string MakeFolder(string parent,string name)
    {
        string path=parent+"/"+name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            string guid=AssetDatabase.CreateFolder(parent,name);
            if (string.IsNullOrEmpty(guid) || !AssetDatabase.IsValidFolder(path)) throw new IOException("Cannot create asset folder: "+path);
        }
        return path;
    }
    static Texture2D ImportTexture(string root,string name)
    {
        string path=root+"/Textures/"+name+".png";
        var imp=AssetImporter.GetAtPath(path) as TextureImporter;
        if (!imp) throw new IOException("Texture importer not found: "+path);
        imp.textureType=name=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
        imp.sRGBTexture=name=="BaseColor";
        imp.maxTextureSize=4096;imp.mipmapEnabled=true;imp.anisoLevel=8;
        imp.wrapMode=TextureWrapMode.Clamp;imp.textureCompression=TextureImporterCompression.CompressedHQ;
        imp.alphaSource=name=="MetallicSmoothness"?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
        imp.alphaIsTransparency=false;
        imp.SaveAndReimport();
        var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (!tex) throw new IOException("Cannot load texture: "+path);
        if (tex.width!=4096 || tex.height!=4096) Debug.LogWarning("Texture resolution is overridden: "+path+" ("+tex.width+"x"+tex.height+")");
        return tex;
    }
    static void ValidateFiles(string root,bool clay)
    {
        for(int i=0;i<7;i++) for(int j=0;j<4;j++)
            if(!File.Exists(DataPath(root,i,j)))throw new FileNotFoundException("Missing MeshData file: "+DataPath(root,i,j));
        if(!clay)foreach(string n in Maps)if(!File.Exists(root+"/Textures/"+n+".png"))throw new FileNotFoundException("Missing texture: "+n+".png");
    }
    static string DataPath(string root,int i,int lod) => root+"/MeshData/"+(i==6?"Street":"Unit"+(i+1))+"_LOD"+lod+".bytes";
    static Material MakeMaterial(string root,string path,bool clay)
    {
        bool srp=GraphicsSettings.currentRenderPipeline!=null;
        Shader shader=Shader.Find(srp?"Universal Render Pipeline/Lit":"Standard");
        if(!shader)throw new InvalidOperationException("URP/Lit or Standard shader not found. HDRP is not supported by this package.");
        var m=new Material(shader){name=clay?"Shophouse_Clay":"Shophouse_PBR_Atlas",enableInstancing=true};
        Color color=clay?new Color(.66f,.66f,.66f):Color.white;
        if(m.HasProperty("_Color"))m.SetColor("_Color",color);
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
        if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0);
        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.25f);
        if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.25f);
        if(!clay)
        {
            var textures=new Dictionary<string,Texture2D>();foreach(var map in Maps)textures.Add(map,ImportTexture(root,map));
            if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",textures["BaseColor"]);
            if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",textures["BaseColor"]);
            m.SetTexture("_BumpMap",textures["Normal"]);m.EnableKeyword("_NORMALMAP");
            if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",.65f);
            m.SetTexture("_MetallicGlossMap",textures["MetallicSmoothness"]);
            m.EnableKeyword(srp?"_METALLICSPECGLOSSMAP":"_METALLICGLOSSMAP");
            if(m.HasProperty("_WorkflowMode"))m.SetFloat("_WorkflowMode",1);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",1);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1);
            if(m.HasProperty("_GlossMapScale"))m.SetFloat("_GlossMapScale",1);
            if(m.HasProperty("_SmoothnessTextureChannel"))m.SetFloat("_SmoothnessTextureChannel",0);
            m.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            if(m.HasProperty("_OcclusionMap")){m.SetTexture("_OcclusionMap",textures["AO"]);m.EnableKeyword("_OCCLUSIONMAP");}
            if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1);
        }
        AssetDatabase.CreateAsset(m,path);
        if(!AssetDatabase.LoadAssetAtPath<Material>(path))throw new IOException("Material save failed: "+path);
        return m;
    }
    static float CenterX(int i)
    {
        if(i==6)return 0;float x=-Total/2;for(int j=0;j<i;j++)x+=Widths[j];return x+Widths[i]/2;
    }
    static Mesh ReadMesh(string path,string name,float xOffset)
    {
        using(var f=File.OpenRead(path))using(var r=new BinaryReader(f))
        {
            if(new string(r.ReadChars(4))!="SHP5")throw new InvalidDataException("Bad mesh header: "+path);
            int nv=r.ReadInt32(),ni=r.ReadInt32();
            if(nv<3||nv>2000000||ni<3||ni%3!=0||ni>12000000||f.Length!=12L+32L*nv+4L*ni)throw new InvalidDataException("Bad mesh size: "+path);
            var v=new Vector3[nv];var n=new Vector3[nv];var uv=new Vector2[nv];var t=new int[ni];
            for(int i=0;i<nv;i++)v[i]=new Vector3(r.ReadSingle()-xOffset,r.ReadSingle(),r.ReadSingle());
            for(int i=0;i<nv;i++)n[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());
            for(int i=0;i<nv;i++)uv[i]=new Vector2(r.ReadSingle(),r.ReadSingle());
            for(int i=0;i<ni;i++){t[i]=r.ReadInt32();if(t[i]<0||t[i]>=nv)throw new InvalidDataException("Bad triangle index: "+path);}
            for(int i=0;i<nv;i++)
            {
                if(!Finite(v[i].x)||!Finite(v[i].y)||!Finite(v[i].z)||!Finite(n[i].x)||!Finite(n[i].y)||!Finite(n[i].z)||!Finite(uv[i].x)||!Finite(uv[i].y)||uv[i].x<0||uv[i].x>1||uv[i].y<0||uv[i].y>1)throw new InvalidDataException("Invalid vertex/UV: "+path);
            }
            var mesh=new Mesh{name=name,indexFormat=nv>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.triangles=t;mesh.RecalculateBounds();mesh.RecalculateTangents();return mesh;
        }
    }
    static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f);
    static void AddBoxCollider(GameObject root,string name,Vector3 center,Vector3 size)
    {
        var child=new GameObject(name);child.transform.SetParent(root.transform,false);var c=child.AddComponent<BoxCollider>();c.center=center;c.size=size;
    }
    static void Generate(bool clay)
    {
        string output=null;GameObject sceneRoot=null;GameObject temporary=null;
        try
        {
            AssetDatabase.Refresh();string root=SourceRoot();ValidateFiles(root,clay);
            string parent=MakeFolder("Assets","GeneratedThaiShophouseV5");
            string unique=AssetDatabase.GenerateUniqueAssetPath(parent+(clay?"/Clay":"/PBR"));
            output=MakeFolder(parent,Path.GetFileName(unique));
            string meshDir=MakeFolder(output,"Meshes"),matDir=MakeFolder(output,"Materials"),prefabDir=MakeFolder(output,"Prefabs");
            Material mat=MakeMaterial(root,matDir+"/Shophouse.mat",clay);
            sceneRoot=new GameObject(clay?"ThaiShophouse_Row_Clay":"ThaiShophouse_Row_PBR");
            var counts=new int[4];var vertexCounts=new int[4];
            for(int i=0;i<7;i++)
            {
                temporary=new GameObject(Labels[i]);float offset=CenterX(i);
                var lods=new LOD[4];float[] heights={.55f,.25f,.09f,.015f};
                for(int j=0;j<4;j++)
                {
                    string name=Labels[i]+"_LOD"+j;Mesh mesh=ReadMesh(DataPath(root,i,j),name,offset);
                    string path=meshDir+"/"+name+".asset";AssetDatabase.CreateAsset(mesh,path);
                    if(!AssetDatabase.LoadAssetAtPath<Mesh>(path))throw new IOException("Mesh save failed: "+path);
                    var child=new GameObject(name);child.transform.SetParent(temporary.transform,false);child.isStatic=true;
                    child.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=child.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;
                    lods[j]=new LOD(heights[j],new Renderer[]{renderer});counts[j]+=(int)(mesh.GetIndexCount(0)/3);vertexCounts[j]+=mesh.vertexCount;
                }
                var group=temporary.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
                if(i<6)AddBoxCollider(temporary,"ExteriorCollision",new Vector3(0,Heights[i]/2,Depth/2),new Vector3(Widths[i],Heights[i],Depth));
                else
                {
                    AddBoxCollider(temporary,"PavementCollision",new Vector3(0,-.10f,-2.15f),new Vector3(Total+2,.20f,4.30f));
                    AddBoxCollider(temporary,"RoadCollision",new Vector3(0,-.19f,-7.4f),new Vector3(Total+2,.18f,6.20f));
                }
                string partPath=prefabDir+"/"+Labels[i]+".prefab";
                var part=PrefabUtility.SaveAsPrefabAsset(temporary,partPath);if(!part)throw new IOException("Prefab save failed: "+partPath);
                Object.DestroyImmediate(temporary);temporary=null;
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(part,sceneRoot.transform);instance.transform.localPosition=new Vector3(offset,0,0);
            }
            string mainPath=prefabDir+"/ThaiShophouse_Row.prefab";
            if(!PrefabUtility.SaveAsPrefabAssetAndConnect(sceneRoot,mainPath,InteractionMode.UserAction))throw new IOException("Row prefab save failed: "+mainPath);
            var lines=new List<string>{"Thai Shophouse V5 mesh report","Measured from imported meshes; not the reference image's stated polycount."};
            for(int i=0;i<4;i++)lines.Add("LOD"+i+": "+counts[i]+" triangles, "+vertexCounts[i]+" vertices");
            File.WriteAllLines(output+"/GenerationReport.txt",lines);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Undo.RegisterCreatedObjectUndo(sceneRoot,"Create Thai Shophouse V5");EditorSceneManager.MarkSceneDirty(sceneRoot.scene);Selection.activeGameObject=sceneRoot;
            Debug.Log(string.Join("\n",lines)+"\nPrefab: "+mainPath,sceneRoot);
        }
        catch(Exception e)
        {
            if(temporary)Object.DestroyImmediate(temporary);if(sceneRoot)Object.DestroyImmediate(sceneRoot);
            // Only remove assets from the fresh output folder created by this run.
            if(!string.IsNullOrEmpty(output)&&AssetDatabase.IsValidFolder(output))AssetDatabase.DeleteAsset(output);
            Debug.LogException(e);EditorUtility.DisplayDialog("Shophouse generation failed",e.Message,"OK");
        }
    }
}

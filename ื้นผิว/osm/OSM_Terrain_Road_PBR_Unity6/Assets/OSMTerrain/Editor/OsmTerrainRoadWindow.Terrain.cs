using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public partial class OsmTerrainRoadWindow {
    TerrainData terrainData;
    Vector3 terrainOrigin;
    int resolution=1025, textureSize=1024;
    float elevationRange=50, roadLift=.08f, roadStep=1;
    string rawPath="";
    bool flipRaw=false, bigEndian=false;
    void TerrainGUI(){
        EditorGUILayout.Space();EditorGUILayout.LabelField("Terrain / PBR",EditorStyles.boldLabel);
        resolution=EditorGUILayout.IntPopup("Heightmap resolution",resolution,new[]{"513","1025","2049"},new[]{513,1025,2049});
        textureSize=EditorGUILayout.IntPopup("PBR texture size",textureSize,new[]{"512","1024","2048"},new[]{512,1024,2048});
        EditorGUILayout.BeginHorizontal();rawPath=EditorGUILayout.TextField("Optional RAW16",rawPath);if(GUILayout.Button("Browse",GUILayout.Width(70))){var p=EditorUtility.OpenFilePanel("RAW heightmap","","raw");if(p.Length>0)rawPath=p;}EditorGUILayout.EndHorizontal();
        elevationRange=EditorGUILayout.FloatField("Height range (m)",elevationRange);
        flipRaw=EditorGUILayout.Toggle("Flip RAW rows",flipRaw);bigEndian=EditorGUILayout.Toggle("RAW big endian",bigEndian);
        roadLift=EditorGUILayout.FloatField("Road lift (m)",roadLift);roadStep=EditorGUILayout.Slider("Road mesh step (m)",roadStep,.25f,4f);
        EditorGUILayout.HelpBox("No RAW = flat terrain. RAW must cover exactly the generated terrain bounds. Existing road snap is ignored in this combined generator. Do not overlap old road/terrain models.",MessageType.Info);
    }
    void CreateTerrain(string folder,Transform root){
        var points=nodes.Values.Select(Project).ToArray();
        float minX=points.Min(p=>p.x)-20*scale,maxX=points.Max(p=>p.x)+20*scale,minZ=points.Min(p=>p.z)-20*scale,maxZ=points.Max(p=>p.z)+20*scale;
        if(elevationRange<=0)throw new Exception("Height range must be positive");
        terrainOrigin=new Vector3(minX,baseY,minZ);
        terrainData=new TerrainData{heightmapResolution=resolution,size=new Vector3(maxX-minX,elevationRange*scale,maxZ-minZ)};
        var h=new float[resolution,resolution];
        if(!string.IsNullOrWhiteSpace(rawPath)){
            var bytes=File.ReadAllBytes(rawPath);if(bytes.Length!=resolution*resolution*2)throw new Exception("RAW16 size does not match selected resolution");
            for(int z=0;z<resolution;z++)for(int x=0;x<resolution;x++){int i=((flipRaw?resolution-1-z:z)*resolution+x)*2;int value=bigEndian?(bytes[i]<<8)|bytes[i+1]:bytes[i]|(bytes[i+1]<<8);h[z,x]=value/65535f;}
        }
        terrainData.SetHeights(0,0,h);
        var grass=Pbr(folder,"Grass",new Color(.24f,.34f,.16f),.35f,.5f);
        var layer=new TerrainLayer{diffuseTexture=grass.mainTexture as Texture2D,normalMapTexture=grass.GetTexture("_BumpMap") as Texture2D,tileSize=new Vector2(6*scale,6*scale),metallic=0,smoothness=.15f};
        AssetDatabase.CreateAsset(layer,folder+"/Grass.terrainlayer");terrainData.terrainLayers=new[]{layer};terrainData.alphamapResolution=128;var alpha=new float[128,128,1];for(int y=0;y<128;y++)for(int x=0;x<128;x++)alpha[y,x,0]=1;terrainData.SetAlphamaps(0,0,alpha);
        AssetDatabase.CreateAsset(terrainData,folder+"/TerrainData.asset");var go=Terrain.CreateTerrainGameObject(terrainData);go.name="Terrain";go.transform.SetParent(root,false);go.transform.localPosition=terrainOrigin;
        Debug.Log($"RAW bounds local X {minX} to {maxX}, Z {minZ} to {maxZ}; origin lat/lon {originLat}, {originLon}; scale {scale}");
    }
    float Ground(Vector3 p){return terrainOrigin.y+terrainData.GetInterpolatedHeight(Mathf.Clamp01((p.x-terrainOrigin.x)/terrainData.size.x),Mathf.Clamp01((p.z-terrainOrigin.z)/terrainData.size.z));}
    void RoadSurface(List<Vector3> p,List<float> s,float width,string surface){
        string kind=surface=="gravel"||surface=="dirt"||surface=="ground"||surface=="unpaved"||surface=="sand"?"Dirt":"Asphalt";
        float len=s[s.Count-1];int across=Mathf.Clamp(Mathf.CeilToInt(width/(scale*1.5f)),2,24);
        for(float a=0;a<len-.0001f;){int ix=s.BinarySearch(a+.0001f);if(ix<0)ix=~ix;float b=Mathf.Min(len,a+roadStep*scale,ix<s.Count?s[ix]:len);if(b<=a+.00001f)break;
            var x=At(p,s,a,out var n);var y=At(p,s,b,out var m);
            for(int j=0;j<across;j++){float u=width*(j/(float)across-.5f),v=width*((j+1)/(float)across-.5f);RoadQuad(x+n*u,x+n*v,y+m*v,y+m*u,kind);}a=b;
        }
    }
    void RoadQuad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,string kind){var center=(a+b+c+d)*.25f;string key=kind+"_"+Mathf.FloorToInt(center.x/(128*scale))+"_"+Mathf.FloorToInt(center.z/(128*scale));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new Batch();int i=batch.v.Count;foreach(var q in new[]{a,b,c,d}){var p=q;p.y=Ground(p)+roadLift*scale;batch.v.Add(p);batch.uv.Add(new Vector2(p.x,p.z)/(scale*1.5f));}batch.tris.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}
    Texture2D SaveMap(string folder,string name,Color[] pixels,bool normal=false,bool linear=false){
        var t=new Texture2D(textureSize,textureSize,TextureFormat.RGBA32,false,linear);t.SetPixels(pixels);t.Apply();string path=folder+"/"+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());DestroyImmediate(t);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;imp.sRGBTexture=!linear&&!normal;imp.wrapMode=TextureWrapMode.Repeat;imp.mipmapEnabled=true;imp.maxTextureSize=textureSize;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    Material Pbr(string folder,string name,Color color,float variation,float roughness){
        int n=textureSize,N=n*n;var heights=new float[N];var rng=new System.Random(55);for(int y=0;y<n;y++)for(int x=0;x<n;x++){float u=x/(float)n,v=y/(float)n;float wave=Mathf.Sin(u*2*Mathf.PI*7)*Mathf.Cos(v*2*Mathf.PI*9);heights[y*n+x]=Mathf.Clamp01(.5f+wave*.13f+((float)rng.NextDouble()-.5f)*.55f);}
        var albedo=new Color[N];var normal=new Color[N];var rough=new Color[N];var ao=new Color[N];var heightMap=new Color[N];var metal=new Color[N];
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){int i=y*n+x;float h=heights[i];float dx=heights[y*n+(x+1)%n]-heights[y*n+(x+n-1)%n],dy=heights[((y+1)%n)*n+x]-heights[((y+n-1)%n)*n+x];var v=new Vector3(-dx*.6f,-dy*.6f,1).normalized;normal[i]=new Color(v.x*.5f+.5f,v.y*.5f+.5f,v.z*.5f+.5f,1);float factor=1+(h-.5f)*variation*2;if((name=="White"||name=="Yellow")&&h<.22f)factor*=.48f;albedo[i]=new Color(color.r*factor,color.g*factor,color.b*factor,1);float r=Mathf.Clamp01(roughness+.45f+(h-.5f)*.2f);rough[i]=new Color(r,r,r,1);float o=.75f+h*.25f;ao[i]=new Color(o,o,o,1);heightMap[i]=new Color(h,h,h,1);metal[i]=new Color(0,0,0,1-r);}
        var a=SaveMap(folder,name+"_Albedo",albedo);var b=SaveMap(folder,name+"_Normal",normal,true,true);SaveMap(folder,name+"_Roughness",rough,false,true);var oMap=SaveMap(folder,name+"_AO",ao,false,true);SaveMap(folder,name+"_Height",heightMap,false,true);var m=SaveMap(folder,name+"_MetallicSmoothness",metal,false,true);
        var shader=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline!=null?Shader.Find("Universal Render Pipeline/Lit"):Shader.Find("Standard");if(shader==null)throw new Exception("URP Lit or Standard shader required (HDRP not supported)");var mat=new Material(shader);mat.mainTexture=a;mat.SetTexture("_BumpMap",b);mat.EnableKeyword("_NORMALMAP");mat.SetTexture("_MetallicGlossMap",m);mat.EnableKeyword("_METALLICGLOSSMAP");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",1);if(mat.HasProperty("_GlossMapScale"))mat.SetFloat("_GlossMapScale",1);mat.SetTexture("_OcclusionMap",oMap);mat.EnableKeyword("_OCCLUSIONMAP");AssetDatabase.CreateAsset(mat,folder+"/"+name+".mat");return mat;
    }
}

// Unity 6 Editor generator. 1 unit = 1 metre. +Z forward. URP Lit / Built-in Standard.
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

public static class FortunerUltraV2Generator
{
    const string Input = "Assets/FortunerUltraV2/Textures";
    const string Output = "Assets/GeneratedFortunerUltraV2";
    static readonly string[] Maps = {"BaseColor", "Normal", "Roughness", "Metallic", "AO", "MetallicSmoothness"};
    const int Paint=0, Roof=1, Door=2, Glass=3, Head=4, Tail=5, Grille=6, Chrome=7, Tread=8, Rubber=9, Alloy=10, Leather=11, Trim=12, Badge=13, Disc=14, Under=15;
    const float Axle=1.3725f, WheelY=.395f, WheelX=.83f, Nose=2.395f, Rear=-2.40f;
    static string folder;
    static int meshNo, quality;
    static Material mat, glassMat;
    static int[] triangles = new int[4];

    [MenuItem("Tools/AYUTTHAYA/Generate Fortuner Ultra V2 PBR")]
    public static void Generate()
    {
        GameObject root=null;
        folder=null;
        try {
            if (!PrepareTextures()) return;
            var rp=GraphicsSettings.currentRenderPipeline;
            string shaderName=rp==null?"Standard":"Universal Render Pipeline/Lit";
            if(rp!=null && !rp.GetType().Name.Contains("Universal")) throw new InvalidOperationException("Use URP or Built-in (HDRP is not supported).");
            Shader shader=Shader.Find(shaderName);
            if(shader==null) throw new InvalidOperationException("Shader missing: "+shaderName);
            EnsureFolder(Output);
            string requested=AssetDatabase.GenerateUniqueAssetPath(Output+"/Fortuner");
            folder=EnsureFolder(requested); meshNo=0;
            mat=MaterialAsset(shader,"Fortuner_PBR_Atlas",1f);
            glassMat=MaterialAsset(shader,"Fortuner_Tinted_Glass",.18f);
            root=new GameObject("Toyota_Fortuner_Ultra_V2");
            var lods=new LOD[4];
            float[] transitions={.60f,.30f,.12f,.018f};
            for(quality=0;quality<4;quality++) {
                var level=Group(root.transform,"LOD"+quality);
                level.localScale=new Vector3(.965f,.951f,1f);
                Build(level);
                if(quality>0) {
                    foreach(Transform part in level) {
                        if(part.name!="Wheels") MergeStaticPart(part);
                        else foreach(Transform wheel in part) {var spin=wheel.Find("Spin_Axis_X");if(spin!=null)MergeStaticPart(spin);}
                    }
                }
                var rr=level.GetComponentsInChildren<Renderer>(true);
                lods[quality]=new LOD(transitions[quality],rr);
                triangles[quality]=ValidateLOD(level);
            }
            var group=root.AddComponent<LODGroup>(); group.SetLODs(lods); group.RecalculateBounds();
            AddBoxCollider(root.transform,"Body_Collider",new Vector3(0,.856f,0),new Vector3(1.73f,.827f,4.59f));
            AddBoxCollider(root.transform,"Cabin_Collider",new Vector3(0,1.484f,-.40f),new Vector3(1.525f,.561f,2.81f));
            var saved=PrefabUtility.SaveAsPrefabAssetAndConnect(root,folder+"/Fortuner_Ultra_V2.prefab",InteractionMode.UserAction,out bool success);
            if(!success||saved==null) throw new IOException("Prefab creation failed: "+folder);
            var hero=root.transform.Find("LOD0").GetComponentsInChildren<Renderer>(true);
            Bounds bounds=hero[0].bounds;foreach(var r in hero)bounds.Encapsulate(r.bounds);
            string report="Fortuner Ultra V2 - runtime generated mesh metrics\n"+"LOD0="+triangles[0]+"\nLOD1="+triangles[1]+"\nLOD2="+triangles[2]+"\nLOD3="+triangles[3]+"\nRenderer bounds (including mirrors/rails): "+bounds.size+" metres\nReference target: L 4.795, W 1.855 (body), H 1.835 metres (approx).\nUV/index/channel checks passed. This is not visual validation.\n";
            File.WriteAllText(folder+"/GenerationReport.txt",report);AssetDatabase.ImportAsset(folder+"/GenerationReport.txt");
            AssetDatabase.SaveAssets(); Undo.RegisterCreatedObjectUndo(root,"Generate Fortuner Ultra V2");
            EditorSceneManager.MarkSceneDirty(root.scene); Selection.activeGameObject=root;
            Debug.Log($"Fortuner created: {folder}/Fortuner_Ultra_V2.prefab | triangles LOD0={triangles[0]:N0}, LOD1={triangles[1]:N0}, LOD2={triangles[2]:N0}, LOD3={triangles[3]:N0}",root);
        } catch(Exception e) { if(root!=null) Object.DestroyImmediate(root); if(!string.IsNullOrEmpty(folder)&&folder.StartsWith(Output+"/",StringComparison.Ordinal)&&AssetDatabase.IsValidFolder(folder)) AssetDatabase.DeleteAsset(folder); Debug.LogException(e); EditorUtility.DisplayDialog("Fortuner generation failed",e.Message,"OK"); }
    }
    static string EnsureFolder(string path)
    {
        path=path.Replace('\\','/'); if(AssetDatabase.IsValidFolder(path)) return path;
        int i=path.LastIndexOf('/'); if(i<=0)throw new IOException("Invalid asset folder: "+path);
        string parent=EnsureFolder(path.Substring(0,i));
        string guid=AssetDatabase.CreateFolder(parent,path.Substring(i+1)); string actual=AssetDatabase.GUIDToAssetPath(guid);
        if(string.IsNullOrEmpty(actual)||!AssetDatabase.IsValidFolder(actual)||actual!=path)throw new IOException("Cannot create folder: "+path);
        return actual;
    }
    static bool Complete(string p) { foreach(string n in Maps) if(!File.Exists(Path.Combine(p,n+".png")))return false; return true; }
    static bool PrepareTextures()
    {
        if(!Complete(Input)) {
            string source=EditorUtility.OpenFolderPanel("Select FortunerUltraV2/Textures from extracted ZIP",Application.dataPath,"");
            if(string.IsNullOrEmpty(source))return false;
            if(!Complete(source))throw new IOException("Selected folder must contain all six Fortuner Ultra PNG maps: "+string.Join(", ",Maps));
            if(!EditorUtility.DisplayDialog("Import Fortuner V2 textures","Copy the six maps into "+Input+"? Existing files with the same names will be replaced.","Copy","Cancel"))return false;
            EnsureFolder(Input);
            foreach(string n in Maps){string src=Path.GetFullPath(Path.Combine(source,n+".png"));string dst=Path.GetFullPath(Input+"/"+n+".png");if(!string.Equals(src,dst,StringComparison.OrdinalIgnoreCase)) File.Copy(src,dst,true);}
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string n in Maps) {
            string path=Input+"/"+n+".png"; var imp=AssetImporter.GetAtPath(path) as TextureImporter;
            if(imp==null)throw new IOException("Cannot import texture: "+path);
            imp.textureType=n=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
            imp.sRGBTexture=n=="BaseColor"; imp.maxTextureSize=4096; imp.npotScale=TextureImporterNPOTScale.None;
            imp.mipmapEnabled=true; imp.wrapMode=TextureWrapMode.Clamp; imp.filterMode=FilterMode.Trilinear; imp.anisoLevel=8;
            imp.textureCompression=TextureImporterCompression.CompressedHQ;
            imp.alphaSource=n=="MetallicSmoothness"?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
            imp.alphaIsTransparency=false; imp.isReadable=false;
            imp.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture==null)throw new IOException("Texture load failed: "+path);if(texture.width!=4096||texture.height!=4096)Debug.LogWarning("Expected 4096x4096: "+path+" (imported "+texture.width+"x"+texture.height+")");
        }
        return true;
    }
    static void Save(Object o,string path)
    {
        if(!AssetDatabase.IsValidFolder(Path.GetDirectoryName(path).Replace('\\','/')))throw new IOException("Missing output folder: "+path);
        AssetDatabase.CreateAsset(o,path);
        if(!AssetDatabase.Contains(o))throw new IOException("Cannot save asset: "+path);
    }
    static Material MaterialAsset(Shader shader,string name,float bump)
    {
        var m=new Material(shader){name=name,enableInstancing=true};
        void Set(string prop,string map){if(m.HasProperty(prop))m.SetTexture(prop,AssetDatabase.LoadAssetAtPath<Texture2D>(Input+"/"+map+".png"));}
        void F(string prop,float v){if(m.HasProperty(prop))m.SetFloat(prop,v);}
        Set("_BaseMap","BaseColor");Set("_MainTex","BaseColor");Set("_BumpMap","Normal");Set("_MetallicGlossMap","MetallicSmoothness");Set("_OcclusionMap","AO");
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.white); if(m.HasProperty("_Color"))m.SetColor("_Color",Color.white);
        F("_WorkflowMode",1);F("_Metallic",1);F("_Smoothness",1);F("_Glossiness",1);F("_GlossMapScale",1);F("_SmoothnessTextureChannel",0);F("_BumpScale",bump);F("_OcclusionStrength",1);F("_Surface",0);
        m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_METALLICGLOSSMAP");m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");m.DisableKeyword("_SPECULAR_SETUP");m.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
        Save(m,folder+"/"+name+".mat");return m;
    }
    // Combine static low-LOD surfaces by material, keeping wheel pivots separate.
    static void MergeStaticPart(Transform root)
    {
        var groups=new Dictionary<Material,List<CombineInstance>>(); var old=new List<MeshFilter>();
        foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)) {
            var r=f.GetComponent<MeshRenderer>();if(r==null||f.sharedMesh==null||r.sharedMaterial==null)continue;
            if(!groups.TryGetValue(r.sharedMaterial,out var list)){list=new List<CombineInstance>();groups.Add(r.sharedMaterial,list);}
            list.Add(new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});old.Add(f);
        }
        if(old.Count<2)return;
        foreach(var kv in groups){var m=new Mesh{name=root.name+"_Merged",indexFormat=IndexFormat.UInt32};m.CombineMeshes(kv.Value.ToArray(),true,true,false);m.RecalculateBounds();Save(m,folder+"/Mesh_"+(meshNo++).ToString("D4")+".asset");var t=Group(root,"Merged_"+kv.Key.name);t.gameObject.AddComponent<MeshFilter>().sharedMesh=m;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=kv.Key;}
        foreach(var f in old){string path=AssetDatabase.GetAssetPath(f.sharedMesh);Object.DestroyImmediate(f.gameObject);if(path.StartsWith(folder+"/Mesh_",StringComparison.Ordinal))AssetDatabase.DeleteAsset(path);}
    }
    static Transform Group(Transform p,string name){var t=new GameObject(name).transform;t.SetParent(p,false);return t;}
    static void AddBoxCollider(Transform p,string name,Vector3 pos,Vector3 size){var t=Group(p,name);t.localPosition=pos;t.gameObject.AddComponent<BoxCollider>().size=size;}
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    static float L(float a,float b,float t)=>Mathf.Lerp(a,b,t);
    static float Sin(float t)=>Mathf.Sin(t*Mathf.PI);
    static int Res(int n)=>Mathf.Max(2,quality==0?Mathf.CeilToInt(n*1.25f):quality==1?Mathf.CeilToInt(n*.80f):quality==2?Mathf.CeilToInt(n*.40f):Mathf.CeilToInt(n*.20f));
    static Vector2 UV(int tile,float u,float v){const float inset=17f/4096f, span=990f/4096f;return new Vector2(tile%4*.25f+inset+Mathf.Clamp01(u)*span,1-(tile/4+1)*.25f+inset+Mathf.Clamp01(v)*span);}
    // Continuous grids with stable UVs, shared vertices and a separately selected outward normal.
    static Transform Surface(Transform p,string name,int nx,int ny,int tile,Func<float,float,Vector3> shape,Func<float,float,Vector3> outward,Func<float,float,Vector2> coords=null,bool seam=false)
    {
        nx=Res(nx);ny=Res(ny);var verts=new List<Vector3>((nx+1)*(ny+1));var uv=new List<Vector2>();var ids=new List<int>();
        for(int j=0;j<=ny;j++)for(int i=0;i<=nx;i++){float u=i/(float)nx,v=j/(float)ny;verts.Add(shape(u,v));Vector2 q=coords==null?new Vector2(u,v):coords(u,v);uv.Add(UV(tile,q.x,q.y));}
        for(int j=0;j<ny;j++)for(int i=0;i<nx;i++){
            int a=j*(nx+1)+i,b=a+1,d=a+nx+1,c=d+1;
            Vector3 n=outward((i+.5f)/nx,(j+.5f)/ny);
            if(Vector3.Dot(Vector3.Cross(verts[b]-verts[a],verts[c]-verts[a]),n)>=0){ids.Add(a);ids.Add(b);ids.Add(c);ids.Add(a);ids.Add(c);ids.Add(d);}
            else {ids.Add(a);ids.Add(c);ids.Add(b);ids.Add(a);ids.Add(d);ids.Add(c);}
        }
        RemoveDegenerate(verts,ids);
        var mesh=new Mesh{name=name,indexFormat=verts.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(ids,0);mesh.RecalculateNormals();
        if(seam){var n=mesh.normals;for(int i=0;i<=nx;i++){int a=i,b=ny*(nx+1)+i;Vector3 q=(n[a]+n[b]).normalized;n[a]=q;n[b]=q;}mesh.normals=n;}
        mesh.RecalculateTangents();mesh.RecalculateBounds();Save(mesh,folder+"/Mesh_"+(meshNo++).ToString("D4")+".asset");
        var t=Group(p,name);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=tile==Glass?glassMat:mat;return t;
    }
    static Transform Quad(Transform p,string name,int tile,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal,int n=8)
    {return Surface(p,name,n,n,tile,(u,v)=>Vector3.Lerp(Vector3.Lerp(a,b,u),Vector3.Lerp(d,c,u),v),(u,v)=>normal);}
    // Rounded box: six independent curved faces, shared seams in geometry.
    static Transform Box(Transform p,string name,int tile,Vector3 center,Vector3 size,float bevel)
    {
        Vector3 half=size*.5f;float r=Mathf.Clamp(bevel,.0001f,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.95f);
        Vector3 inner=half-Vector3.one*r;
        var verts=new List<Vector3>();var norms=new List<Vector3>();var uv=new List<Vector2>();var ids=new List<int>();int n=Res(8);
        // Nonuniform grid: include the bevel boundary and several samples across the radius.
        float Coord(float t,float h){float q=Mathf.Abs(t*2-1);return Mathf.Sign(t*2-1)*(q<.5f?L(0,h-r,q*2):L(h-r,h,(q-.5f)*2));}
        for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2){int a=axis,b=(a+1)%3,c=(a+2)%3,start=verts.Count;Vector3 norm=Vector3.zero;norm[a]=sign;
            for(int j=0;j<=n;j++)for(int i=0;i<=n;i++) {float u=i/(float)n,v=j/(float)n;Vector3 raw=Vector3.zero;raw[a]=half[a]*sign;raw[b]=Coord(u,half[b]);raw[c]=Coord(v,half[c]);Vector3 cl=V(Mathf.Clamp(raw.x,-inner.x,inner.x),Mathf.Clamp(raw.y,-inner.y,inner.y),Mathf.Clamp(raw.z,-inner.z,inner.z));Vector3 normal=(raw-cl).normalized;verts.Add(cl+normal*r);norms.Add(normal);uv.Add(UV(tile,u,v));}
            for(int j=0;j<n;j++)for(int i=0;i<n;i++){int q=start+j*(n+1)+i,e=q+1,d=q+n+1,f=d+1;if(Vector3.Dot(Vector3.Cross(verts[e]-verts[q],verts[f]-verts[q]),norm)>=0)ids.AddRange(new[]{q,e,f,q,f,d});else ids.AddRange(new[]{q,f,e,q,d,f});}
        }
        RemoveDegenerate(verts,ids);
        var mesh=new Mesh{name=name};mesh.SetVertices(verts);mesh.SetNormals(norms);mesh.SetUVs(0,uv);mesh.SetTriangles(ids,0);mesh.RecalculateTangents();mesh.RecalculateBounds();Save(mesh,folder+"/Mesh_"+(meshNo++).ToString("D4")+".asset");
        var t=Group(p,name);t.localPosition=center;t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=tile==Glass?glassMat:mat;return t;
    }
    // Beam using rounded box only for visible structural details.
    static void Beam(Transform p,string name,int tile,Vector3 a,Vector3 b,float w,float h){var t=Box(p,name,tile,(a+b)*.5f,V(w,h,Vector3.Distance(a,b)),Mathf.Min(w,h)*.22f);t.localRotation=Quaternion.FromToRotation(Vector3.forward,b-a);}
    static float Arch(float z){float dz=Mathf.Min(Mathf.Abs(z-Axle),Mathf.Abs(z+Axle));const float r=.443f;return dz<r?Mathf.Max(.52f,WheelY+Mathf.Sqrt(r*r-dz*dz)):.52f;}
    static float Belt(float z)=>1.27f+.035f*Mathf.Exp(-Mathf.Pow((z+1.4f)*1.1f,2))-.09f*Mathf.Pow(Mathf.Clamp01((z-1.45f)/.95f),2);
    static float SideX(float z,float y){float crease=.014f*Mathf.Exp(-Mathf.Pow((y-1.13f)*30,2))-.012f*Mathf.Exp(-Mathf.Pow((y-.72f)*22,2));float bulge=.039f*Mathf.Exp(-Mathf.Pow((Mathf.Abs(z)-Axle)*3,2));return .895f+bulge+crease+.025f*Mathf.Sin(Mathf.Clamp01((y-.52f)/.78f)*Mathf.PI)-.045f*Mathf.Pow(Mathf.Clamp01((.68f-y)/.16f),2)-.055f*Mathf.Pow(Mathf.Clamp01((Mathf.Abs(z)-1.95f)/.45f),2);}
    static void SidePanel(Transform p,string name,int s,float z0,float z1,int tile){Surface(p,name,48,20,tile,(u,v)=>{float z=L(z0,z1,u),y=L(Arch(z),Belt(z),v);return V(s*SideX(z,y),y,z);},(u,v)=>Vector3.right*s,(u,v)=>new Vector2(u,v));}
    static void ArchLip(Transform p,int s,float z){Surface(p,"Wheel_Arch_Lip",64,6,Paint,(u,v)=>{float a=L(.29f,Mathf.PI-.29f,u),r=L(.443f,.475f,v),zz=z+r*Mathf.Cos(a),yy=WheelY+r*Mathf.Sin(a);return V(s*(SideX(zz,yy)+.009f*Sin(v)),yy,zz);},(u,v)=>Vector3.right*s);}
    static void RoofSkin(Transform p){Surface(p,"Roof_Skin",48,72,Roof,(u,v)=>{float z=L(-2.18f,.62f,v),x=(u*2-1)*L(.74f,.77f,Sin(v));return V(x,1.78f+.10f*(1-Mathf.Pow(Mathf.Abs(2*u-1),2))+.025f*Sin(v),z);},(u,v)=>Vector3.up);}
    static void HoodSkin(Transform p){Surface(p,"Hood_Sculpted",64,40,Paint,(u,v)=>{float a=u*2-1,z=L(2.27f,1.02f,v),w=L(.84f,.89f,v);float y=L(1.16f,1.30f,v)+.045f*(1-a*a)+.018f*Mathf.Exp(-Mathf.Pow((Mathf.Abs(a)-.62f)*11,2))*Sin(v);return V(a*w,y,z);},(u,v)=>Vector3.up);}
    static Vector3 RoofEdge(int s,float z)=>V(s*.76f,1.79f,z);
    static void SideGlass(Transform p,int s,string name,float lowZ0,float lowZ1,float highZ0,float highZ1)
    {Surface(p,name,24,18,Glass,(u,v)=>{float z=L(L(lowZ0,lowZ1,u),L(highZ0,highZ1,u),v);return V(s*(L(SideX(z,Belt(z)),.76f,v)+.009f*Sin(v)),L(Belt(z)+.045f,1.765f,v),z);},(u,v)=>Vector3.right*s);}
    static void Pillar(Transform p,int s,string name,float low,float high,float width){Beam(p,name,Paint,V(s*(SideX(low,Belt(low))-.012f),Belt(low)+.02f,low),RoofEdge(s,high),width,width);}
    static float FrontZ(float x,float y)=>Nose-.10f*Mathf.Pow(x/.87f,2)-.035f*(1-Mathf.Clamp01((y-.48f)/.70f))+.023f*Mathf.Exp(-Mathf.Pow((y-.85f)*15,2))-.012f*Mathf.Exp(-Mathf.Pow((y-.66f)*12,2))*Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.69f)*12,2));
    // Remove zero-area triangles before saving. Reusable material atlas UVs may intentionally overlap.
    static void RemoveDegenerate(List<Vector3> v,List<int> ids)
    {
        for(int i=ids.Count-3;i>=0;i-=3) if(Vector3.Cross(v[ids[i+1]]-v[ids[i]],v[ids[i+2]]-v[ids[i]]).sqrMagnitude<1e-19f) ids.RemoveRange(i,3);
    }
    static int ValidateLOD(Transform level)
    {
        int tris=0,vertices=0,meshes=0;
        foreach(var f in level.GetComponentsInChildren<MeshFilter>(true)) {
            Mesh m=f.sharedMesh; if(m==null||m.vertexCount==0)throw new InvalidOperationException("Empty mesh: "+f.name);
            var uv=m.uv; if(uv.Length!=m.vertexCount||m.normals.Length!=m.vertexCount||m.tangents.Length!=m.vertexCount)throw new InvalidOperationException("Missing vertex channels: "+f.name);
            foreach(var q in uv) if(float.IsNaN(q.x)||float.IsNaN(q.y)||q.x<0||q.x>1||q.y<0||q.y>1)throw new InvalidOperationException("Invalid atlas UV: "+f.name);
            var r=f.GetComponent<MeshRenderer>(); if(r==null||r.sharedMaterial==null)throw new InvalidOperationException("Missing material: "+f.name);
            for(int sub=0;sub<m.subMeshCount;sub++) { var ids=m.GetIndices(sub); foreach(int index in ids)if(index<0||index>=m.vertexCount)throw new InvalidOperationException("Invalid index: "+f.name); tris+=ids.Length/3; }
            vertices+=m.vertexCount;meshes++;
        }
        Debug.Log($"{level.name}: {tris:N0} triangles, {vertices:N0} vertices, {meshes:N0} mesh renderers; UV/index/channel checks passed.");return tris;
    }
    // Extruded elliptical trim, with local curved tube cross-section and continuous normals.
    static void Oval(Transform p,string name,int tile,Vector3 center,float rx,float ry,float tube,int direction)
    {
        Surface(p,name,64,10,tile,(u,v)=>{float a=u*2*Mathf.PI,b=v*2*Mathf.PI;return center+V((rx+tube*Mathf.Cos(b))*Mathf.Cos(a),(ry+tube*Mathf.Cos(b))*Mathf.Sin(a),direction*tube*Mathf.Sin(b));},(u,v)=>{float a=u*2*Mathf.PI,b=v*2*Mathf.PI;return V(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(a)*Mathf.Cos(b),direction*Mathf.Sin(b));},null,true);
    }
    static void Projector(Transform p,string name,int side,float x,float y,float radius)
    {
        float z=FrontZ(x,y)+.031f;
        Oval(p,name+"_Bezel",Chrome,V(side*x,y,z),radius,radius,.004f,1);
        Surface(p,name+"_Lens",20,20,Glass,(u,v)=>{float a=(2*u-1)*Mathf.PI*.5f,b=(2*v-1)*Mathf.PI*.5f;return V(side*x+radius*.89f*Mathf.Sin(a)*Mathf.Cos(b),y+radius*.89f*Mathf.Sin(b),z+.010f*Mathf.Cos(a)*Mathf.Cos(b));},(u,v)=>Vector3.forward);
    }
    static float RearZ(float x,float y)=>Rear+.075f*Mathf.Pow(x/.87f,2)+.035f*(1-Mathf.Clamp01((y-.62f)/.69f));
    static void FrontRear(Transform p)
    {
        Surface(p,"Front_Bumper_Curved",64,32,Paint,(u,v)=>{float a=2*u-1,w=L(.83f,.87f,Sin(v));float x=a*w,y=L(.48f,1.18f,v);return V(x,y,FrontZ(x,y));},(u,v)=>Vector3.forward);
        Surface(p,"Upper_Grille_Inset",40,20,Trim,(u,v)=>{float a=2*u-1;float x=a*L(.48f,.60f,v),y=L(.91f,1.16f,v);return V(x,y,FrontZ(x,y)+.012f);},(u,v)=>Vector3.forward);
        for(int j=0;j<4;j++){float y=.93f+j*.064f;Surface(p,"Upper_Grille_Chrome_"+j,36,5,Chrome,(u,v)=>{float x=(2*u-1)*L(.49f,.58f,j/3f),yy=y+v*.019f+.013f*Mathf.Pow(x/.6f,2);return V(x,yy,FrontZ(x,yy)+.026f);},(u,v)=>Vector3.forward);}
        if(quality<3){float z=FrontZ(0,1.05f)+.043f;Oval(p,"Toyota_Emblem_Outer",Chrome,V(0,1.05f,z),.111f,.067f,.006f,1);Oval(p,"Toyota_Emblem_Vertical",Chrome,V(0,1.05f,z+.002f),.035f,.064f,.004f,1);Oval(p,"Toyota_Emblem_Horizontal",Chrome,V(0,1.067f,z+.004f),.104f,.026f,.004f,1);}
        Surface(p,"Lower_Intake",32,14,Trim,(u,v)=>{float a=2*u-1;float x=a*L(.45f,.53f,v),y=L(.58f,.78f,v);return V(x,y,FrontZ(x,y)+.012f);},(u,v)=>Vector3.forward);
        for(int j=0;j<3;j++){float y=.595f+j*.073f;Surface(p,"Lower_Grille_Slat_"+j,28,3,Chrome,(u,v)=>{float a=2*u-1;float yy=y+v*.012f;return V(a*.47f,yy,FrontZ(a*.47f,yy)+.021f);},(u,v)=>Vector3.forward);}
        for(int s=-1;s<=1;s+=2){int side=s;
            Surface(p,"Headlight_"+s,36,18,Head,(u,v)=>{float a=L(.56f,.855f,u);float y=L(.965f+.053f*u,1.125f+.046f*u,v);return V(side*a,y,FrontZ(a,y)+.019f);},(u,v)=>V(side*.4f,0,1),(u,v)=>new Vector2(u,v));
            if(quality<2){Projector(p,"Projector_"+s,s,.725f,1.076f,.037f);Projector(p,"Inner_Optic_"+s,s,.646f,1.061f,.024f);}
            Surface(p,"Headlight_Chrome_Brow_"+s,40,4,Chrome,(u,v)=>{float x=L(.56f,.855f,u),y=1.126f+.046f*u+v*.009f;return V(side*x,y,FrontZ(x,y)+.025f);},(u,v)=>Vector3.forward);
            Surface(p,"Fog_Housing_"+s,18,14,Trim,(u,v)=>V(side*L(.60f,.79f,u),L(.57f,.79f,v),FrontZ(L(.60f,.79f,u),L(.57f,.79f,v))+.012f),(u,v)=>Vector3.forward);
            if(quality<3)Projector(p,"Fog_Lamp_"+s,s,.70f,.68f,.040f);
        }
        Surface(p,"Rear_Hatch_Curved",60,40,Paint,(u,v)=>{float a=2*u-1;float x=a*L(.85f,.87f,Sin(v)),y=L(.62f,1.31f,v);return V(x,y,RearZ(x,y));},(u,v)=>Vector3.back);
        Surface(p,"Rear_Bumper_Curved",48,16,Paint,(u,v)=>{float a=2*u-1;return V(a*.85f,L(.46f,.65f,v),Rear+.05f*a*a);},(u,v)=>Vector3.back);
        Surface(p,"Rear_Fortuner_Badge",48,8,Badge,(u,v)=>{float a=2*u-1;float x=-a*.49f,y=L(1.09f,1.20f,v);return V(x,y,RearZ(x,y)-.013f);},(u,v)=>Vector3.back);
        for(int s=-1;s<=1;s+=2){int side=s;Surface(p,"Tail_Lamp_"+s,32,16,Tail,(u,v)=>{float x=L(.48f,.85f,u);float y=L(1.16f,1.34f,v);return V(side*x,y,RearZ(x,y)-.018f);},(u,v)=>Vector3.back,(u,v)=>new Vector2(u,v));}
        for(int s=-1;s<=1;s+=2){int side=s;
            Surface(p,"Rear_Reflector_"+s,24,5,Tail,(u,v)=>{float x=L(.56f,.78f,u),y=L(.51f,.55f,v);return V(side*x,y,Rear+.05f*Mathf.Pow(x/.85f,2)-.008f);},(u,v)=>Vector3.back,(u,v)=>new Vector2(u,L(.66f,.85f,v)));
            if(quality<2)Surface(p,"Rear_Light_Chrome_Trim_"+s,32,3,Chrome,(u,v)=>{float x=L(.48f,.85f,u),y=1.16f-v*.008f;return V(side*x,y,RearZ(x,y)-.023f);},(u,v)=>Vector3.back);
        }
        if(quality<2){float z=RearZ(0,1.265f)-.023f;Oval(p,"Rear_Emblem_Outer",Chrome,V(0,1.265f,z),.072f,.043f,.004f,-1);Oval(p,"Rear_Emblem_Vertical",Chrome,V(0,1.265f,z-.002f),.022f,.041f,.003f,-1);Oval(p,"Rear_Emblem_Horizontal",Chrome,V(0,1.276f,z-.004f),.067f,.017f,.003f,-1);}
        Box(p,"Rear_Valance",Trim,V(0,.52f,Rear+.01f),V(1.34f,.13f,.07f),.025f);
    }
    static void Build(Transform root)
    {
        var body=Group(root,"Bodywork");var cabin=Group(root,"Cabin");var doors=Group(root,"Doors");var wheels=Group(root,"Wheels");var detail=Group(root,"Details");var under=Group(root,"Underbody");
        // Hidden chassis is kept inside the wheel envelopes; no solid block fills the arches.
        Box(under,"Floorpan",Under,V(0,.67f,0),V(1.33f,.20f,4.49f),.07f);
        Box(under,"Engine",Under,V(0,.94f,1.68f),V(1.18f,.38f,1.19f),.06f);
        HoodSkin(body);RoofSkin(cabin);FrontRear(body);
        Surface(body,"Hood_Nose_Return",48,8,Paint,(u,v)=>{float a=2*u-1,x=a*.84f,y=1.16f+.045f*(1-a*a);return V(x,L(1.15f,y,v),L(FrontZ(x,1.15f),2.27f,v));},(u,v)=>V(0,.5f,1));
        Surface(body,"Rear_Hatch_Upper_Return",48,8,Paint,(u,v)=>{float a=2*u-1,x=a*.85f;return V(x,L(1.31f,1.32f,v),L(RearZ(x,1.31f),-2.35f,v));},(u,v)=>V(0,.4f,-1));
        for(int s=-1;s<=1;s+=2){string side=s<0?"Left":"Right";
            int ss=s;
            Surface(body,side+"_Front_Corner",20,24,Paint,(u,v)=>{float y=L(.52f,1.17f,v),x=L(SideX(2.29f,y),.84f,u);return V(ss*x,y,L(2.29f,FrontZ(x,y),u));},(u,v)=>V(ss,0,.7f));
            Surface(body,side+"_Rear_Corner",20,24,Paint,(u,v)=>{float y=L(.52f,1.29f,v),x=L(SideX(-2.31f,y),.85f,u);return V(ss*x,y,L(-2.31f,RearZ(x,y),u));},(u,v)=>V(ss,0,-.7f));

            SidePanel(body,side+"_Front_Fender",s,1.003f,2.29f,Paint);
            SidePanel(doors,side+"_Front_Door",s,-.10f,.994f,Door);
            SidePanel(doors,side+"_Rear_Door",s,-1.12f,-.109f,Door);
            SidePanel(body,side+"_Rear_Quarter",s,-2.31f,-1.129f,Paint);
            ArchLip(body,s,Axle);ArchLip(body,s,-Axle);
            foreach(float axle in new[]{Axle,-Axle}){float zz0=axle;Surface(body,"Wheel_Well_Return",64,8,Trim,(u,v)=>{float a=L(.29f,Mathf.PI-.29f,u),z=zz0+.443f*Mathf.Cos(a),y=WheelY+.443f*Mathf.Sin(a);return V(ss*L(SideX(z,y),.68f,v),y,z);},(u,v)=>V(0,-Mathf.Sin(L(.29f,Mathf.PI-.29f,u)),-Mathf.Cos(L(.29f,Mathf.PI-.29f,u))));}
            Surface(body,"Rocker_Underside",40,6,Paint,(u,v)=>{float z=L(-.92f,.92f,u);return V(s*L(.665f,SideX(z,.52f),v),.52f,z);},(u,v)=>Vector3.down);

            SideGlass(cabin,s,side+"_Front_Window",-.08f,.98f,-.08f,.58f);
            SideGlass(cabin,s,side+"_Rear_Window",-1.10f,-.14f,-1.10f,-.14f);
            SideGlass(cabin,s,side+"_Quarter_Window",-2.25f,-1.16f,-2.14f,-1.16f);
            Pillar(cabin,s,side+"_A_Pillar",1.01f,.62f,.075f);Pillar(cabin,s,side+"_B_Pillar",-.11f,-.11f,.08f);Pillar(cabin,s,side+"_C_Pillar",-1.13f,-1.13f,.085f);Pillar(cabin,s,side+"_D_Pillar",-2.31f,-2.18f,.11f);
            Beam(detail,side+"_Belt_Trim",Chrome,V(s*.91f,1.30f,1.00f),V(s*.92f,1.33f,-2.24f),.022f,.022f);
            Box(detail,side+"_Running_Board",Trim,V(s*.96f,.54f,-.10f),V(.23f,.10f,2.02f),.035f);
            Box(detail,side+"_Rear_Handle",Chrome,V(s*.947f,1.17f,-.94f),V(.042f,.045f,.14f),.017f);
            Box(detail,side+"_Front_Handle",Chrome,V(s*.946f,1.15f,.09f),V(.042f,.045f,.145f),.017f);
            Box(detail,side+"_Mirror_Stem",Trim,V(s*.94f,1.40f,.85f),V(.19f,.07f,.12f),.025f);
            Box(detail,side+"_Mirror_Housing",Paint,V(s*1.055f,1.44f,.85f),V(.22f,.17f,.24f),.075f);
            Box(detail,side+"_Mirror_Glass",Glass,V(s*1.055f,1.44f,.728f),V(.17f,.12f,.012f),.005f);
            Beam(detail,side+"_Roof_Rail",Trim,V(s*.58f,1.91f,-1.99f),V(s*.58f,1.91f,.38f),.048f,.045f);
            Wheel(wheels,s,Axle);Wheel(wheels,s,-Axle);
        }
        Surface(cabin,"Windshield",52,32,Glass,(u,v)=>{float a=2*u-1;return V(a*L(.88f,.76f,v),L(1.30f,1.79f,v)+.016f*(1-a*a)*Sin(v),L(1.02f,.62f,v)+.034f*(1-a*a)*Sin(v));},(u,v)=>V(0,.5f,1));
        Surface(cabin,"Rear_Glass",44,28,Glass,(u,v)=>{float a=2*u-1;return V(a*L(.86f,.75f,v),L(1.32f,1.79f,v),L(-2.35f,-2.18f,v)-.025f*(1-a*a)*Sin(v));},(u,v)=>V(0,.3f,-1));
        Box(cabin,"Rear_Spoiler",Paint,V(0,1.82f,-2.29f),V(1.55f,.07f,.30f),.03f);
        if(quality<2){
            Beam(detail,"Rear_Wiper",Trim,V(-.02f,1.39f,-2.344f),V(.46f,1.43f,-2.331f),.022f,.019f);
            for(int s=-1;s<=1;s+=2){for(int j=0;j<3;j++)Box(detail,"Roof_Rail_Mount",Trim,V(s*.58f,1.885f,-1.90f+j*1.10f),V(.075f,.055f,.115f),.015f);
                for(int j=0;j<12;j++)Box(detail,"Step_Grip",Rubber,V(s*.96f,.596f,-1.0f+j*.16f),V(.17f,.012f,.045f),.004f);}
        }
        if(quality<2){Beam(detail,"Wiper_L",Trim,V(-.75f,1.32f,1.01f),V(-.05f,1.37f,.966f),.02f,.02f);Beam(detail,"Wiper_R",Trim,V(.02f,1.32f,1.01f),V(.73f,1.37f,.966f),.02f,.02f);}
        if(quality==0){var inside=Group(root,"Interior_Prototype");for(int s=-1;s<=1;s+=2){Box(inside,"Seat_Base",Leather,V(s*.40f,.90f,.28f),V(.48f,.17f,.50f),.07f);Box(inside,"Seat_Back",Leather,V(s*.40f,1.14f,.06f),V(.46f,.52f,.15f),.065f);}Box(inside,"Rear_Bench",Leather,V(0,.92f,-.85f),V(1.40f,.19f,.52f),.07f);Box(inside,"Dashboard",Trim,V(0,1.19f,.88f),V(1.58f,.21f,.30f),.06f);}
    }
    // Axial ring, v is angle; seam normals are averaged and tangent UV remains split.
    static void Ring(Transform p,string name,int tile,float a0,float a1,float r0,float r1,int s,int steps,Vector3 normalHint,bool tread=false)
    {
        Surface(p,name,tread?24:8,steps,tile,(u,v)=>{float a=L(a0,a1,u),r=L(r0,r1,u),th=v*Mathf.PI*2;
            if(tread && quality==0) {float groove=Mathf.Pow(Mathf.Abs(Mathf.Sin(u*Mathf.PI*5)),16);float cross=Mathf.Pow(Mathf.Abs(Mathf.Sin(th*24+u*2)),10);r-=.0025f*groove+.0018f*cross;}
            return V(a*s,r*Mathf.Cos(th),r*Mathf.Sin(th));},(u,v)=>{float th=v*Mathf.PI*2;return V(normalHint.x*s,normalHint.y*Mathf.Cos(th),normalHint.y*Mathf.Sin(th));},null,true);
    }
    static void Wheel(Transform p,int s,float z)
    {
        var pivot=Group(p,(z>0?"Front_":"Rear_")+(s<0?"Left":"Right"));pivot.localPosition=V(s*WheelX,WheelY,z);
        var spin=Group(pivot,"Spin_Axis_X");int steps=192; // Res also reduces for lower LODs.
        Ring(spin,"Tire_Tread",Tread,-.105f,.105f,.395f,.395f,s,steps,V(0,1,0),true);
        Ring(spin,"Outer_Shoulder",Rubber,.105f,.142f,.395f,.359f,s,steps,V(.7f,.7f,0));
        Ring(spin,"Outer_Sidewall",Rubber,.142f,.135f,.359f,.253f,s,steps,V(1,0,0));
        Ring(spin,"Inner_Shoulder",Rubber,-.105f,-.142f,.395f,.359f,s,steps,V(-.7f,.7f,0));
        Ring(spin,"Inner_Sidewall",Rubber,-.142f,-.135f,.359f,.253f,s,steps,V(-1,0,0));
        Ring(spin,"Rim_Barrel",Alloy,-.13f,.136f,.253f,.253f,s,steps,V(0,-1,0));
        Ring(spin,"Rim_Outer_Lip",Chrome,.136f,.143f,.253f,.230f,s,steps,V(1,0,0));
        Ring(spin,"Brake_Disc",Disc,.046f,.046f,.055f,.194f,s,steps,V(1,0,0));
        Ring(spin,"Hub_Face",Alloy,.142f,.150f,.008f,.073f,s,steps,V(1,0,0));
        // Twelve sculpted spokes with width and depth varying from hub to rim.
        for(int i=0;i<12;i++){float th=i*Mathf.PI*2/12;Vector3 Shape(float u,float v,float depth){float r=L(.065f,.237f,u),a=th+.10f*Sin(u)+(v-.5f)*L(.42f,.12f,u);return V(s*(.139f-.027f*Sin(u)+.005f*Sin(v)-depth),r*Mathf.Cos(a),r*Mathf.Sin(a));}
            Surface(spin,"Spoke_"+i,16,6,Alloy,(u,v)=>Shape(u,v,0),(u,v)=>Vector3.right*s);
            if(quality<2){Surface(spin,"Spoke_Back_"+i,12,4,Alloy,(u,v)=>Shape(u,v,.022f),(u,v)=>Vector3.left*s);
                for(int edge=0;edge<2;edge++){int e=edge;Surface(spin,"Spoke_Edge_"+i+"_"+e,12,3,Alloy,(u,v)=>Shape(u,e,.022f*v),(u,v)=>V(0,-Mathf.Sin(th)*(e==0?-1:1),Mathf.Cos(th)*(e==0?-1:1)));}}
        }
        if(quality<2)for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Box(spin,"Lug_"+i,Chrome,V(s*.151f,.047f*Mathf.Cos(a),.047f*Mathf.Sin(a)),V(.018f,.017f,.017f),.004f);}
        if(quality<3)Box(pivot,"Caliper",Under,V(s*.078f,.08f,-.16f),V(.075f,.13f,.09f),.02f);
    }
}

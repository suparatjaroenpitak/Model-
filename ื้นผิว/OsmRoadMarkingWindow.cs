using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

public class OsmRoadMarkingWindow : EditorWindow {
    string left="", right="";
    double originLat=14.3409, originLon=100.54353;
    float scale=1, height=0.025f, baseY=0, laneWidth=3.25f, dash=3, gap=6, lineWidth=.15f;
    bool snap=true, minor=false, yellowEdge=true;
    LayerMask roadLayers=~0;
    Transform anchor;
    static readonly CultureInfo CI=CultureInfo.InvariantCulture;
    static readonly HashSet<string> Roads=new HashSet<string>{"primary","secondary","tertiary","unclassified","residential","service","track","primary_link","secondary_link","tertiary_link"};
    class Way { public List<long> nodes=new List<long>(); public Dictionary<string,string> tags=new Dictionary<string,string>(); public string Get(string k,string d="")=>tags.TryGetValue(k,out var v)?v:d; }
    struct Geo { public double lat,lon; public Geo(double a,double b){lat=a;lon=b;} }
    class Batch { public List<Vector3> v=new List<Vector3>(); public List<Vector2> uv=new List<Vector2>(); public List<int> tris=new List<int>(); }
    Dictionary<long,Geo> nodes=new Dictionary<long,Geo>(); Dictionary<long,Way> ways=new Dictionary<long,Way>();
    Dictionary<string,Batch> batches=new Dictionary<string,Batch>();
    [MenuItem("Tools/OSM/Road Markings")] static void Open()=>GetWindow<OsmRoadMarkingWindow>("OSM Markings");
    void OnGUI(){
        EditorGUILayout.HelpBox("Use exactly the same origin, scale and alignment as your existing road mesh. Select only road surface layers for raycasts.",MessageType.Info);
        FileField("Left OSM",ref left); FileField("Right OSM",ref right);
        originLat=EditorGUILayout.DoubleField("Origin latitude",originLat); originLon=EditorGUILayout.DoubleField("Origin longitude",originLon);
        scale=EditorGUILayout.FloatField("Unity units per metre",scale); anchor=(Transform)EditorGUILayout.ObjectField("Map origin transform",anchor,typeof(Transform),true);
        baseY=EditorGUILayout.FloatField("Flat local Y",baseY); height=EditorGUILayout.FloatField("Paint lift (metres)",height);
        laneWidth=EditorGUILayout.FloatField("Default lane width (m)",laneWidth); lineWidth=EditorGUILayout.FloatField("Paint width (m)",lineWidth);
        dash=EditorGUILayout.FloatField("Dash length (m)",dash); gap=EditorGUILayout.FloatField("Gap length (m)",gap);
        yellowEdge=EditorGUILayout.Toggle("Yellow left edge",yellowEdge); minor=EditorGUILayout.Toggle("Mark minor roads too",minor);
        snap=EditorGUILayout.Toggle("Snap to road colliders",snap);
        if(snap){ roadLayers.value=EditorGUILayout.MaskField("Road layer mask",roadLayers.value,Enumerable.Range(0,32).Select(i=>string.IsNullOrEmpty(LayerMask.LayerToName(i))?"Layer "+i:LayerMask.LayerToName(i)).ToArray()); }
        if(GUILayout.Button("Generate model + prefab")) { try{Generate();}catch(Exception e){Debug.LogException(e);EditorUtility.DisplayDialog("Error",e.Message,"OK");}finally{EditorUtility.ClearProgressBar();} }
    }
    void FileField(string label,ref string path){ EditorGUILayout.BeginHorizontal(); path=EditorGUILayout.TextField(label,path); if(GUILayout.Button("Browse",GUILayout.Width(70))){var p=EditorUtility.OpenFilePanel(label,"","osm");if(p.Length>0)path=p;} EditorGUILayout.EndHorizontal(); }
    void Read(string path){ var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null}; using(var reader=XmlReader.Create(path,settings)){var doc=new XmlDocument{XmlResolver=null};doc.Load(reader);foreach(XmlElement n in doc.SelectNodes("/osm/node")){nodes[long.Parse(n.GetAttribute("id"))]=new Geo(double.Parse(n.GetAttribute("lat"),CI),double.Parse(n.GetAttribute("lon"),CI));} foreach(XmlElement w in doc.SelectNodes("/osm/way")){var way=new Way();foreach(XmlElement n in w.SelectNodes("nd"))way.nodes.Add(long.Parse(n.GetAttribute("ref")));foreach(XmlElement t in w.SelectNodes("tag"))way.tags[t.GetAttribute("k")]=t.GetAttribute("v");if(Roads.Contains(way.Get("highway")))ways[long.Parse(w.GetAttribute("id"))]=way;}} }
    Vector3 Project(Geo g){return new Vector3((float)((g.lon-originLon)*111195*Math.Cos(originLat*Math.PI/180))*scale,baseY,(float)((g.lat-originLat)*111195)*scale);}
    Vector3 Surface(Vector3 p){if(snap){Vector3 world=anchor?anchor.TransformPoint(p):p; if(Physics.Raycast(world+Vector3.up*500,Vector3.down,out var hit,1000,roadLayers,QueryTriggerInteraction.Ignore)){world=hit.point+Vector3.up*height*scale;return anchor?anchor.InverseTransformPoint(world):world;}}p.y+=height*scale;return p;}
    Vector3 At(List<Vector3> p,List<float> s,float d,out Vector3 normal){int i=s.BinarySearch(d);if(i<0)i=~i;i=Mathf.Clamp(i,1,p.Count-1);float t=Mathf.Clamp01((d-s[i-1])/Mathf.Max(.0001f,s[i]-s[i-1]));Vector3 dir=(p[i]-p[i-1]).normalized;normal=new Vector3(-dir.z,0,dir.x); // left in geographic road direction
        Vector3 a=normal,b=normal;if(i>1){var prev=(p[i-1]-p[i-2]).normalized;a=Miter(new Vector3(-prev.z,0,prev.x),normal);}if(i<p.Count-1){var next=(p[i+1]-p[i]).normalized;b=Miter(normal,new Vector3(-next.z,0,next.x));}normal=Vector3.Lerp(a,b,t);return Vector3.Lerp(p[i-1],p[i],t);}
    Vector3 Miter(Vector3 a,Vector3 b){var n=(a+b).normalized;float dot=Vector3.Dot(n,b);return dot>.4f?n/Mathf.Max(.5f,dot):b;}
    void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,bool yellow){var center=(a+b+c+d)*.25f;string key=(yellow?"Yellow_":"White_")+Mathf.FloorToInt(center.x/(128*scale))+"_"+Mathf.FloorToInt(center.z/(128*scale));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new Batch();int i=batch.v.Count;foreach(var p in new[]{a,b,c,d}){batch.v.Add(Surface(p));batch.uv.Add(new Vector2(p.x,p.z)/(scale*1.5f));}batch.tris.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}
    void Strip(List<Vector3> p,List<float> s,float start,float end,float offset,float width,bool yellow){float a=start;while(a<end-.0001f){int idx=s.BinarySearch(a+.0001f);if(idx<0)idx=~idx;float b=Mathf.Min(end,a+scale,idx<s.Count?s[idx]:end);if(b<=a+.00001f)break;var x=At(p,s,a,out var n);var y=At(p,s,b,out var m);Quad(x+n*(offset-width/2),x+n*(offset+width/2),y+m*(offset+width/2),y+m*(offset-width/2),yellow);a=b;}}
    void Line(List<Vector3> p,List<float> s,float offset,bool solid,bool yellow){float len=s[s.Count-1];if(solid){Strip(p,s,0,len,offset,lineWidth*scale,yellow);return;}for(float a=0;a<len;a+=(dash+gap)*scale)Strip(p,s,a,Mathf.Min(len,a+dash*scale),offset,lineWidth*scale,yellow);}
    void Arrow(List<Vector3> p,List<float> s,float at,float off,string type){var pos=At(p,s,at,out var n);n=n.normalized;var f=new Vector3(n.z,0,-n.x);pos+=n*off;float k=scale; // arrow head represented as two triangles in a quad, shaft as strip
        if(type=="none"||type=="reverse"||type.StartsWith("merge"))return;
        Action<Vector3,Vector3> stroke=(a,b)=>{var v=(b-a).normalized;var side=new Vector3(-v.z,0,v.x)*(.12f*k);Quad(a-side,a+side,b+side,b-side,false);};
        var tail=pos-f*1.2f*k;var joint=pos+f*.15f*k;stroke(tail,joint);
        foreach(var turn in type.Split(';')){Vector3 dir=turn.Contains("left")?n:turn.Contains("right")?-n:f;if(turn.Contains("slight"))dir=(dir+f).normalized;if(turn!="through"&&!turn.Contains("left")&&!turn.Contains("right"))continue;var tip=joint+dir*1.1f*k;stroke(joint,tip);var side=new Vector3(-dir.z,0,dir.x);var back=tip-dir*.55f*k;Quad(back-side*.32f*k,back+side*.32f*k,tip,tip,false);}}
    void Build(Way w){var p=new List<Vector3>();foreach(var id in w.nodes){if(!nodes.TryGetValue(id,out var g))return;var v=Project(g);if(p.Count==0||(p[p.Count-1]-v).sqrMagnitude>.000001f)p.Add(v);}if(p.Count<2)return;if(w.Get("oneway")=="-1")p.Reverse();var s=new List<float>{0};for(int i=1;i<p.Count;i++)s.Add(s[i-1]+Vector3.Distance(p[i-1],p[i]));float len=s[s.Count-1];string h=w.Get("highway");bool major=h=="primary"||h=="secondary"||h=="tertiary"||h.EndsWith("_link");bool one=w.Get("oneway")=="yes"||w.Get("oneway")=="1"||w.Get("oneway")=="-1"||w.Get("junction")=="roundabout";if(!major&&!minor)return;int lanes;if(!int.TryParse(w.Get("lanes"),out lanes))lanes=one?1:2;lanes=Mathf.Clamp(lanes,1,12);float width=laneWidth*lanes;if(float.TryParse(w.Get("width").Replace(" m",""),NumberStyles.Float,CI,out var parsed)&&parsed>1&&parsed<60)width=parsed;float lw=width/lanes*scale;int forward=lanes/2;if(int.TryParse(w.Get("lanes:forward"),out var f))forward=Mathf.Clamp(f,1,Mathf.Max(1,lanes-1));for(int i=1;i<lanes;i++){bool center=!one&&i==forward;Line(p,s,(i-lanes*.5f)*lw,center,center);}if(major){float edge=width*.5f*scale-.2f*scale;Line(p,s,edge,true,yellowEdge);Line(p,s,-edge,true,false);}string turns=w.Get("turn:lanes");if(one&&turns.Length>0&&len>8*scale){var ts=turns.Split('|');if(ts.Length==lanes)for(int i=0;i<lanes;i++)Arrow(p,s,len-4*scale,(lanes*.5f-i-.5f)*lw,ts[i]);}}
    Material Paint(string folder,string name,Color color){var tex=new Texture2D(256,256,TextureFormat.RGBA32,true);var rng=new System.Random(55);var pixels=new Color[256*256];for(int i=0;i<pixels.Length;i++){float n=(float)rng.NextDouble();pixels[i]=Color.Lerp(color,new Color(.24f,.24f,.23f),n<.065f?.65f:n*.20f);}tex.SetPixels(pixels);tex.Apply();tex.wrapMode=TextureWrapMode.Repeat;AssetDatabase.CreateAsset(tex,folder+"/"+name+"_Albedo.asset");var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");var mat=new Material(shader);mat.mainTexture=tex;if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",.16f);if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",.16f);if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",0);AssetDatabase.CreateAsset(mat,folder+"/"+name+".mat");return mat;}
    void Generate(){if(!File.Exists(left)||!File.Exists(right))throw new Exception("Select both OSM files");if(scale<=0||dash<=0||gap<0||laneWidth<=0||lineWidth<=0)throw new Exception("Invalid dimensions");nodes.Clear();ways.Clear();batches.Clear();Read(left);Read(right);int i=0;foreach(var w in ways.Values){if(i++%100==0)EditorUtility.DisplayProgressBar("OSM","Generating road markings",(float)i/ways.Count);Build(w);}string folder="Assets/OSM_Markings_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");AssetDatabase.CreateFolder("Assets",Path.GetFileName(folder));var white=Paint(folder,"White",new Color(.88f,.87f,.78f));var yellow=Paint(folder,"Yellow",new Color(.95f,.67f,.15f));var root=new GameObject("OSM_RoadMarkings");if(anchor){root.transform.SetParent(anchor,false);}foreach(var pair in batches){var b=pair.Value;var mesh=new Mesh{name=pair.Key,indexFormat=IndexFormat.UInt32}; // ensure upward winding
        for(int j=0;j<b.tris.Count;j+=3){var a=b.v[b.tris[j]];var c=b.v[b.tris[j+1]];var d=b.v[b.tris[j+2]];if(Vector3.Cross(c-a,d-a).y<0){int t=b.tris[j+1];b.tris[j+1]=b.tris[j+2];b.tris[j+2]=t;}}
        mesh.SetVertices(b.v);mesh.SetUVs(0,b.uv);mesh.SetTriangles(b.tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,folder+"/"+pair.Key+".asset");var child=new GameObject(pair.Key);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=child.AddComponent<MeshRenderer>();mr.sharedMaterial=pair.Key.StartsWith("Yellow")?yellow:white;mr.shadowCastingMode=ShadowCastingMode.Off;}
        PrefabUtility.SaveAsPrefabAsset(root,folder+"/OSM_RoadMarkings.prefab");Undo.RegisterCreatedObjectUndo(root,"OSM Markings");AssetDatabase.SaveAssets();Selection.activeGameObject=root;Debug.Log($"OSM marking model: {ways.Count} road ways read, {batches.Count} mesh chunks. {folder}");}
}

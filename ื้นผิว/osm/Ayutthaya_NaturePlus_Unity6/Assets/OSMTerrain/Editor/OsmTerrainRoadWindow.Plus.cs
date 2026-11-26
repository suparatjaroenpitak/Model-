using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public partial class OsmTerrainRoadWindow {
    readonly Dictionary<long,Way> waterWays=new Dictionary<long,Way>();
    readonly HashSet<long> treeIds=new HashSet<long>();
    bool addTrees=true, addRocks=true, addWater=true;
    int extraTrees=1800, rockCount=700;
    byte[,] wet;
    const int WN=1024;
    void PlusGUI(){
        addTrees=EditorGUILayout.Toggle("3D trees + exposed roots",addTrees);
        extraTrees=EditorGUILayout.IntSlider("Additional trees",extraTrees,0,6000);
        addRocks=EditorGUILayout.Toggle("3D weathered rocks",addRocks);
        rockCount=EditorGUILayout.IntSlider("Rock count",rockCount,0,3000);
        addWater=EditorGUILayout.Toggle("OSM water surfaces",addWater);
    }
    bool Inside(Vector3 p)=>p.x>=terrainOrigin.x&&p.z>=terrainOrigin.z&&p.x<terrainOrigin.x+terrainData.size.x&&p.z<terrainOrigin.z+terrainData.size.z;
    Vector2 WaterGrid(Vector3 p)=>new Vector2((p.x-terrainOrigin.x)/terrainData.size.x*WN,(p.z-terrainOrigin.z)/terrainData.size.z*WN);
    bool IsWetCell(int x,int z)=>wet!=null&&wet[Mathf.Clamp(z*WN/MaskSize,0,WN-1),Mathf.Clamp(x*WN/MaskSize,0,WN-1)]!=0;
    bool WetAt(Vector3 p){var g=WaterGrid(p);return wet!=null&&Inside(p)&&wet[(int)g.y,(int)g.x]!=0;}
    List<Vector3> WayPoints(Way w){var p=new List<Vector3>();foreach(var id in w.nodes){if(!nodes.TryGetValue(id,out var g))return new List<Vector3>();p.Add(Project(g));}return p;}
    void PrepareWater(){
        wet=new byte[WN,WN];
        foreach(var w in waterWays.Values){var pp=WayPoints(w);if(pp.Count<2)continue;var p=pp.Select(WaterGrid).ToArray();
            bool polygon=w.nodes.Count>3&&w.nodes[0]==w.nodes[w.nodes.Count-1]&&(w.Get("natural")=="water"||w.Get("waterway")=="riverbank");
            if(polygon){int lo=Mathf.Clamp(Mathf.FloorToInt(p.Min(v=>v.y)),0,WN),hi=Mathf.Clamp(Mathf.CeilToInt(p.Max(v=>v.y)),0,WN);for(int z=lo;z<hi;z++){var xs=new List<float>();float y=z+.5f;for(int i=1;i<p.Length;i++){var a=p[i-1];var b=p[i];if((a.y>y)!=(b.y>y))xs.Add(a.x+(y-a.y)*(b.x-a.x)/(b.y-a.y));}xs.Sort();for(int j=1;j<xs.Count;j+=2)for(int x=Mathf.Clamp(Mathf.CeilToInt(xs[j-1]-.5f),0,WN);x<Mathf.Clamp(Mathf.CeilToInt(xs[j]-.5f),0,WN);x++)wet[z,x]=1;}}
            else {if(w.Get("waterway")==""||w.Get("waterway")=="riverbank")continue;float width=w.Get("waterway")=="river"?55:w.Get("waterway")=="canal"?8:3;if(float.TryParse(w.Get("width").Replace(" m",""),System.Globalization.NumberStyles.Float,CI,out var v)&&v>0)width=Mathf.Min(v,300);float dx=terrainData.size.x/WN,dz=terrainData.size.z/WN,r=width*scale*.5f;for(int i=1;i<p.Length;i++){var a=p[i-1];var b=p[i];int loX=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,b.x)-r/dx),0,WN),hiX=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,b.x)+r/dx),0,WN);int loZ=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,b.y)-r/dz),0,WN),hiZ=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,b.y)+r/dz),0,WN);var d=new Vector2((b.x-a.x)*dx,(b.y-a.y)*dz);for(int z=loZ;z<hiZ;z++)for(int x=loX;x<hiX;x++){var q=new Vector2((x+.5f-a.x)*dx,(z+.5f-a.y)*dz);float t=Mathf.Clamp01(Vector2.Dot(q,d)/Mathf.Max(.00001f,d.sqrMagnitude));if((q-d*t).sqrMagnitude<=r*r)wet[z,x]=1;}}}
        }
    }
    class GeoMesh {
        public List<Vector3> v=new List<Vector3>();public List<Vector2> uv=new List<Vector2>();public List<int> t=new List<int>();
        public void Tube(Vector3[] p,float[] radius,int sides){int start=v.Count;for(int j=0;j<p.Length;j++){Vector3 dir=(p[Mathf.Min(j+1,p.Length-1)]-p[Mathf.Max(0,j-1)]).normalized;Vector3 a=Vector3.Cross(dir,Mathf.Abs(dir.y)>.9f?Vector3.right:Vector3.up).normalized,b=Vector3.Cross(dir,a);for(int k=0;k<=sides;k++){float f=k/(float)sides,ang=f*Mathf.PI*2;float r=radius[j]*(1+.12f*Mathf.Sin(ang*5+j*.7f));v.Add(p[j]+r*(a*Mathf.Cos(ang)+b*Mathf.Sin(ang)));uv.Add(new Vector2(f,j*.65f));}}for(int j=0;j<p.Length-1;j++)for(int k=0;k<sides;k++){int a=start+j*(sides+1)+k,b=a+sides+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}}
        public void Blob(Vector3 center,Vector3 size,int rings,int sides,int seed){int start=v.Count;for(int j=0;j<=rings;j++){float phi=Mathf.PI*j/rings;for(int k=0;k<=sides;k++){float a=2*Mathf.PI*k/sides;var n=new Vector3(Mathf.Sin(phi)*Mathf.Cos(a),Mathf.Cos(phi),Mathf.Sin(phi)*Mathf.Sin(a));float r=1+.22f*Mathf.Sin(n.x*11+seed)*Mathf.Cos(n.z*8+n.y*5);v.Add(center+Vector3.Scale(n,size)*r);uv.Add(new Vector2(k/(float)sides,j/(float)rings));}}for(int j=0;j<rings;j++)for(int k=0;k<sides;k++){int a=start+j*(sides+1)+k,b=a+sides+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}}
        public Mesh Save(string path){var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;}
    }
    GameObject MeshObject(string name,Transform parent,Mesh mesh,Material mat){var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;}
    GameObject MakeTree(string folder,Material bark,Material leaf,int variant){var root=new GameObject("Tree_"+variant);var rng=new System.Random(700+variant);var renders=new List<Renderer[]>();for(int lod=0;lod<2;lod++){var wood=new GeoMesh();var crown=new GeoMesh();float h=6+variant*.65f;var top=new Vector3(.35f,h,0);wood.Tube(new[]{Vector3.zero,new Vector3(.08f,h*.3f,0),new Vector3(-.12f,h*.65f,.15f),top},new[]{.39f,.27f,.16f,.035f},lod==0?14:7);for(int i=0;i<6;i++){float a=i*2.39996f+variant;var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var start=Vector3.up*(h*(.45f+i*.055f));var end=start+dir*(2.2f+(float)rng.NextDouble())+Vector3.up*1.65f;wood.Tube(new[]{start,start+dir*.9f+Vector3.up*.5f,end},new[]{.14f,.085f,.018f},lod==0?9:5);for(int j=0;j<(lod==0?5:2);j++){float b=j*2.4f;var c=end+new Vector3(Mathf.Cos(b)*.9f,j*.17f,Mathf.Sin(b)*.9f);crown.Blob(c,new Vector3(1.25f,.85f,1.15f),lod==0?7:4,lod==0?10:6,variant+i+j);}if(lod==0)wood.Tube(new[]{Vector3.up*.4f,dir*.55f+Vector3.up*.18f,dir*1.1f+Vector3.up*.08f,dir*1.7f-Vector3.up*.09f},new[]{.2f,.14f,.08f,.015f},9);}
            var w=MeshObject("Bark_Roots_LOD"+lod,root.transform,wood.Save(folder+"/Tree"+variant+"_Wood"+lod+".asset"),bark);var l=MeshObject("Canopy_LOD"+lod,root.transform,crown.Save(folder+"/Tree"+variant+"_Leaf"+lod+".asset"),leaf);renders.Add(new[]{w.GetComponent<Renderer>(),l.GetComponent<Renderer>()});}
        var group=root.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.12f,renders[0]),new LOD(.012f,renders[1])});group.RecalculateBounds();var col=root.AddComponent<CapsuleCollider>();col.radius=.35f;col.height=5;col.center=Vector3.up*2.5f;var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/Tree_"+variant+".prefab");DestroyImmediate(root);return prefab;
    }
    Vector3 CellPoint(int x,int z){return terrainOrigin+new Vector3(x/(float)WN*terrainData.size.x,0,z/(float)WN*terrainData.size.z);}
    void CreateNatureObjects(string folder,Transform root){
        var nature=new GameObject("Nature_3D").transform;nature.SetParent(root,false);var rng=new System.Random(2026);var area=AreaMask();
        bool Allowed(Vector3 p,bool planted){if(!Inside(p)||WetAt(p))return false;var g=Grid(p);int x=Mathf.Clamp((int)g.x,0,MaskSize-1),z=Mathf.Clamp((int)g.y,0,MaskSize-1);return roadMask[z,x]==0&&(planted||area[z,x]<2);}
        Vector3 RandomPoint()=>terrainOrigin+new Vector3((float)rng.NextDouble()*terrainData.size.x,0,(float)rng.NextDouble()*terrainData.size.z);
        if(addTrees){var bark=Pbr(folder,"OldBark",new Color(.25f,.19f,.12f),.8f,.48f);var leaf=Pbr(folder,"TropicalLeaves",new Color(.24f,.32f,.105f),.85f,.37f);bark.enableInstancing=leaf.enableInstancing=true;var prefabs=Enumerable.Range(0,4).Select(i=>MakeTree(folder,bark,leaf,i)).ToArray();var instances=new List<TreeInstance>();terrainData.treePrototypes=prefabs.Select(p=>new TreePrototype{prefab=p}).ToArray();
            void Plant(Vector3 p){float s=.65f+(float)rng.NextDouble()*.65f;instances.Add(new TreeInstance{position=new Vector3((p.x-terrainOrigin.x)/terrainData.size.x,(Ground(p)-terrainOrigin.y)/terrainData.size.y,(p.z-terrainOrigin.z)/terrainData.size.z),prototypeIndex=rng.Next(prefabs.Length),widthScale=s*scale,heightScale=s*scale,rotation=(float)rng.NextDouble()*Mathf.PI*2,color=Color.white,lightmapColor=Color.white});}
            foreach(var id in treeIds)if(nodes.TryGetValue(id,out var g)){var p=Project(g);if(Allowed(p,true))Plant(p);}for(int i=0,placed=0;i<extraTrees*12&&placed<extraTrees;i++){var p=RandomPoint();if(!Allowed(p,false))continue;var g=Grid(p);if(area[(int)g.y,(int)g.x]!=1&&rng.NextDouble()>.18)continue;Plant(p);placed++;}terrainData.SetTreeInstances(instances.ToArray(),true);Debug.Log("Nature trees: "+instances.Count);}
        if(addRocks){var mat=Pbr(folder,"WeatheredRock",new Color(.36f,.34f,.29f),.8f,.52f);mat.enableInstancing=true;var meshes=new List<Mesh>();for(int j=0;j<6;j++){var m=new GeoMesh();m.Blob(Vector3.zero,new Vector3(.7f,.43f,.55f),12,18,j+70);meshes.Add(m.Save(folder+"/Rock_"+j+".asset"));}for(int i=0,n=0;i<rockCount*10&&n<rockCount;i++){var p=RandomPoint();if(!Allowed(p,false))continue;float s=(.35f+(float)rng.NextDouble()*1.4f)*scale;p.y=Ground(p)+s*.12f;var go=MeshObject("Rock_"+n++,nature,meshes[rng.Next(meshes.Count)],mat);go.transform.localPosition=p;go.transform.localScale=Vector3.one*s;go.transform.localRotation=Quaternion.Euler(0,rng.Next(360),0);if(s>scale){var c=go.AddComponent<MeshCollider>();c.sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;}}}
        if(addWater){var mat=Pbr(folder,"RiverWater",new Color(.20f,.29f,.24f),.35f,.03f);mat.SetFloat("_Smoothness",.86f);mat.DisableKeyword("_METALLICGLOSSMAP");mat.DisableKeyword("_METALLICSPECGLOSSMAP");mat.SetTexture("_MetallicGlossMap",null);mat.SetTextureScale("_BumpMap",new Vector2(.4f,.4f));
            // Surface follows the DEM locally; no fabricated absolute water level or excavation.
            for(int z0=0;z0<WN;z0+=64)for(int x0=0;x0<WN;x0+=64){var m=new GeoMesh();for(int z=z0;z<Mathf.Min(z0+64,WN);z++)for(int x=x0;x<Mathf.Min(x0+64,WN);x++){if(wet[z,x]==0)continue;int k=m.v.Count;foreach(var p0 in new[]{CellPoint(x,z),CellPoint(x,z+1),CellPoint(x+1,z+1),CellPoint(x+1,z)}){var p=p0;p.y=Ground(p)+.04f*scale;m.v.Add(p);m.uv.Add(new Vector2(p.x,p.z)/(scale*5));}m.t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}if(m.v.Count>0)MeshObject("Water_"+x0+"_"+z0,nature,m.Save(folder+"/Water_"+x0+"_"+z0+".asset"),mat);}}
        EditorUtility.SetDirty(terrainData);
    }
}

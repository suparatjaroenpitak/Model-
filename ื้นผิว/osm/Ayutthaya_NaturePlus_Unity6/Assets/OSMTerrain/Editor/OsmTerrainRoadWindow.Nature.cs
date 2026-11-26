using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public partial class OsmTerrainRoadWindow {
    bool naturalRelief=true, vegetation=true;
    float relief=0.8f;
    const int MaskSize=1024;
    byte[,] roadMask;
    List<Way> naturalAreas=new List<Way>();
    void NatureGUI(){
        naturalRelief=EditorGUILayout.Toggle("Procedural low relief (no DEM)",naturalRelief);
        relief=EditorGUILayout.Slider("Relief amplitude (m)",relief,0,2);
        vegetation=EditorGUILayout.Toggle("Roadside grass",vegetation);
        PlusGUI();
    }
    float Noise(float x,float z,float frequency,int seed){return Mathf.PerlinNoise(x*frequency+seed*13.17f,z*frequency+seed*7.19f);}
    void AddRelief(float[,] h){if(!naturalRelief||!string.IsNullOrWhiteSpace(rawPath))return;for(int z=0;z<resolution;z++)for(int x=0;x<resolution;x++){float wx=x/(float)(resolution-1)*terrainData.size.x/scale,wz=z/(float)(resolution-1)*terrainData.size.z/scale;h[z,x]=Mathf.Clamp01(relief*(.75f*Noise(wx,wz,.003f,1)+.25f*Noise(wx,wz,.025f,2))/elevationRange);}}
    Vector2 Grid(Vector3 p){return new Vector2((p.x-terrainOrigin.x)/terrainData.size.x*MaskSize,(p.z-terrainOrigin.z)/terrainData.size.z*MaskSize);}
    void MakeRoadMask(){roadMask=new byte[MaskSize,MaskSize];float cellX=terrainData.size.x/MaskSize,cellZ=terrainData.size.z/MaskSize;foreach(var w in ways.Values){int lanes;if(!int.TryParse(w.Get("lanes"),out lanes))lanes=w.Get("oneway")=="yes"?1:2;float width=laneWidth*Mathf.Clamp(lanes,1,12);if(float.TryParse(w.Get("width").Replace(" m",""),System.Globalization.NumberStyles.Float,CI,out var parsed)&&parsed>1&&parsed<60)width=parsed;float radius=(width*.5f+2.5f)*scale;for(int j=1;j<w.nodes.Count;j++){if(!nodes.TryGetValue(w.nodes[j-1],out var a)||!nodes.TryGetValue(w.nodes[j],out var b))continue;var p=Project(a);var q=Project(b);int steps=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(p,q)/Mathf.Min(cellX,cellZ)));for(int k=0;k<=steps;k++){var g=Grid(Vector3.Lerp(p,q,k/(float)steps));int rx=Mathf.CeilToInt(radius/cellX),rz=Mathf.CeilToInt(radius/cellZ);for(int y=Mathf.Max(0,Mathf.FloorToInt(g.y)-rz);y<Mathf.Min(MaskSize,Mathf.FloorToInt(g.y)+rz+1);y++)for(int x=Mathf.Max(0,Mathf.FloorToInt(g.x)-rx);x<Mathf.Min(MaskSize,Mathf.FloorToInt(g.x)+rx+1);x++)if(Mathf.Pow((x+.5f-g.x)*cellX,2)+Mathf.Pow((y+.5f-g.y)*cellZ,2)<=radius*radius)roadMask[y,x]=1;}}}}
    byte[,] AreaMask(){var map=new byte[MaskSize,MaskSize];foreach(var w in naturalAreas){if(w.nodes.Count<4||w.nodes[0]!=w.nodes[w.nodes.Count-1])continue;var p=new List<Vector2>();foreach(long id in w.nodes)if(nodes.TryGetValue(id,out var g))p.Add(Grid(Project(g)));if(p.Count!=w.nodes.Count)continue;byte type=(byte)((w.Get("natural")=="water"||w.Get("natural")=="wetland")?3:(w.Get("landuse")=="residential"||w.Get("landuse")=="retail"||w.Get("landuse")=="commercial")?2:1);float lo=MaskSize,hi=0;foreach(var v in p){lo=Mathf.Min(lo,v.y);hi=Mathf.Max(hi,v.y);}for(int y=Mathf.Max(0,Mathf.FloorToInt(lo));y<Mathf.Min(MaskSize,Mathf.CeilToInt(hi));y++){var xs=new List<float>();for(int i=1;i<p.Count;i++){var a=p[i-1];var b=p[i];float yy=y+.5f;if((a.y>yy)!=(b.y>yy))xs.Add(a.x+(yy-a.y)*(b.x-a.x)/(b.y-a.y));}xs.Sort();for(int i=1;i<xs.Count;i+=2)for(int x=Mathf.Max(0,Mathf.CeilToInt(xs[i-1]-.5f));x<Mathf.Min(MaskSize,Mathf.CeilToInt(xs[i]-.5f));x++)map[y,x]=Math.Max(map[y,x],type);}}return map;}
    void NaturalLayers(string folder){
        MakeRoadMask();var area=AreaMask();
        string[] names={"OldGrass","DryGrass","WeatheredSoil","LeafLitter","DampEarth"};
        Color[] colors={new Color(.27f,.32f,.15f),new Color(.43f,.39f,.22f),new Color(.38f,.28f,.19f),new Color(.29f,.23f,.13f),new Color(.22f,.19f,.14f)};
        var layers=new TerrainLayer[names.Length];
        for(int i=0;i<layers.Length;i++){var m=Pbr(folder,names[i],colors[i],.75f,.4f);var layer=new TerrainLayer{diffuseTexture=m.mainTexture as Texture2D,normalMapTexture=m.GetTexture("_BumpMap") as Texture2D,tileSize=Vector2.one*(i==2?3:4)*scale,metallic=0,smoothness=.08f,normalScale=1};AssetDatabase.CreateAsset(layer,folder+"/"+names[i]+".terrainlayer");layers[i]=layer;}
        terrainData.terrainLayers=layers;terrainData.alphamapResolution=MaskSize;var alpha=new float[MaskSize,MaskSize,5];
        for(int y=0;y<MaskSize;y++)for(int x=0;x<MaskSize;x++){float wx=x/(float)MaskSize*terrainData.size.x/scale,wz=y/(float)MaskSize*terrainData.size.z/scale;float a=Noise(wx,wz,.015f,5),b=Noise(wx,wz,.07f,7),c=Noise(wx,wz,.004f,8);float[] weights={.15f+a*a*2,.1f+(1-a)*b, .1f+Mathf.Pow(1-b,3)*2,.05f+c*c,.04f+Mathf.Pow(1-c,4)};if(area[y,x]==1){weights[0]*=2;weights[3]*=2;}if(area[y,x]==2||roadMask[y,x]!=0){weights[2]*=6;weights[0]*=.15f;weights[3]*=.25f;}if(area[y,x]==3){weights[4]*=12;weights[0]*=.1f;}float sum=0;foreach(float v in weights)sum+=v;for(int i=0;i<5;i++)alpha[y,x,i]=weights[i]/sum;}
        terrainData.SetAlphamaps(0,0,alpha);
        if(!vegetation)return;
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[64*64];var rng=new System.Random(123);for(int blade=0;blade<11;blade++){float bx=8+(float)rng.NextDouble()*48,lean=(float)rng.NextDouble()*24-12;int tall=rng.Next(28,63);for(int y=0;y<tall;y++){float t=y/(float)tall;int cx=Mathf.RoundToInt(bx+lean*t*t);int half=Mathf.Max(0,Mathf.RoundToInt(2*(1-t)));for(int x=Mathf.Max(0,cx-half);x<=Mathf.Min(63,cx+half);x++)pixels[y*64+x]=Color.Lerp(new Color(.22f,.28f,.10f,1),new Color(.60f,.57f,.28f,1),t);}}tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,folder+"/GrassBlade.asset");terrainData.detailPrototypes=new[]{new DetailPrototype{prototypeTexture=tex,renderMode=DetailRenderMode.GrassBillboard,minHeight=.25f*scale,maxHeight=.75f*scale,minWidth=.25f*scale,maxWidth=.65f*scale,healthyColor=new Color(.65f,.72f,.43f),dryColor=new Color(.72f,.61f,.36f)}};
        terrainData.SetDetailResolution(MaskSize,32);var detail=new int[MaskSize,MaskSize];for(int y=0;y<MaskSize;y++)for(int x=0;x<MaskSize;x++)if(roadMask[y,x]==0&&area[y,x]<2&&!IsWetCell(x,y)){float n=Noise(x,y,.045f,12);detail[y,x]=n>.48f?Mathf.RoundToInt(n*(area[y,x]==1?8:3)):0;}terrainData.SetDetailLayer(0,0,0,detail);
    }
}

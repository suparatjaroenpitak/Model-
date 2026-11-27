using UnityEngine;
namespace AyutthayaBuildings {
public sealed class BuildingPart : MonoBehaviour {
 public float health=100; public Vector3 size; public Material debrisMaterial; public bool structural; bool broken;
 public void TakeDamage(float amount){if(broken||amount<=0)return;health-=amount;if(health<=0)Break();}
 [ContextMenu("Break this part")] public void Break(){if(broken)return;broken=true; var owner=GetComponentInParent<BuildingStructure>(); var r=GetComponent<Renderer>(); if(r)r.enabled=false; foreach(var c in GetComponents<Collider>())c.enabled=false; if(Application.isPlaying){int nx=Mathf.Clamp(Mathf.CeilToInt(size.x/.45f),1,5), ny=Mathf.Clamp(Mathf.CeilToInt(size.y/.45f),1,5),nz=Mathf.Clamp(Mathf.CeilToInt(size.z/.45f),1,5); Vector3 cell=new Vector3(size.x/nx,size.y/ny,size.z/nz); for(int x=0;x<nx;x++)for(int y=0;y<ny;y++)for(int z=0;z<nz;z++){var d=GameObject.CreatePrimitive(PrimitiveType.Cube);d.name="Building debris";d.transform.position=transform.TransformPoint(-size*.5f+Vector3.Scale(cell,new Vector3(x+.5f,y+.5f,z+.5f)));d.transform.rotation=transform.rotation;d.transform.localScale=Vector3.Scale(cell*.92f,transform.lossyScale);d.GetComponent<Renderer>().sharedMaterial=debrisMaterial;var rb=d.AddComponent<Rigidbody>();rb.mass=Mathf.Max(.1f,cell.x*cell.y*cell.z*80);Destroy(d,12);}} if(owner)owner.Notify(this); }
}
}

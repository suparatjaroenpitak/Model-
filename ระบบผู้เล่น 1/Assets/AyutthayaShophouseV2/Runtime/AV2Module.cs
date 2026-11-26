using UnityEngine;
namespace Ayutthaya.ShophouseV2 {
public class AV2Module : MonoBehaviour {
 public int level=-1; public string kind;
 public void Release(){
  if(kind=="fall"){
   // Replace concave collision with a simple proxy before adding a dynamic rigidbody.
   var mf=GetComponent<MeshFilter>(); if(!mf||!mf.sharedMesh)return;
   foreach(var c in GetComponents<Collider>())c.enabled=false;
   var box=gameObject.AddComponent<BoxCollider>();box.center=mf.sharedMesh.bounds.center;box.size=mf.sharedMesh.bounds.size;
   transform.SetParent(null,true);var rb=gameObject.AddComponent<Rigidbody>();rb.mass=Mathf.Clamp(box.size.x*box.size.y*box.size.z*45,8,250);rb.AddTorque(new Vector3(.18f,.04f,.1f),ForceMode.Impulse);Destroy(gameObject,60f);
  } else if(kind=="break"||kind=="glassbreak") {var b=GetComponent<AV2Breakable>();if(b)b.Break(transform.position);}
  else if(level>=0)gameObject.SetActive(false);
 }
}
}

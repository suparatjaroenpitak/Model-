using UnityEngine;
namespace BankKit {
public class BankBreakable : MonoBehaviour {
 public float health=100,debrisLifetime=20; bool broken;
 public void ApplyDamage(float damage){ApplyDamage(damage,transform.position,2);}
 [ContextMenu("Test/Break Panel (Play Mode)")] public void TestBreak(){if(Application.isPlaying)ApplyDamage(Mathf.Max(health,1),transform.position,3);}
 public void ApplyDamage(float damage,Vector3 point,float impulse){if(!Application.isPlaying||broken||damage<=0)return;health-=damage;if(health>0)return;broken=true;
 var pieces=new Transform[transform.childCount];for(int i=0;i<pieces.Length;i++)pieces[i]=transform.GetChild(i);
 foreach(var p in pieces){p.SetParent(null,true);var mf=p.GetComponent<MeshFilter>();var mc=p.GetComponent<MeshCollider>();if(mc)mc.convex=true;var col=p.GetComponent<Collider>();if(!col){var b=p.gameObject.AddComponent<BoxCollider>();if(mf&&mf.sharedMesh){b.center=mf.sharedMesh.bounds.center;b.size=mf.sharedMesh.bounds.size;}}var rb=p.GetComponent<Rigidbody>();if(!rb)rb=p.gameObject.AddComponent<Rigidbody>();rb.isKinematic=false;rb.useGravity=true;rb.mass=4;rb.AddExplosionForce(impulse*50,point,5,.2f,ForceMode.Impulse);Destroy(p.gameObject,debrisLifetime);}Destroy(gameObject);}
}}

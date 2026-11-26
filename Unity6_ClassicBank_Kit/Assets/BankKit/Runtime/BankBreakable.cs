using UnityEngine;
namespace BankKit {
public class BankBreakable : MonoBehaviour {
 public float health=100; public float debrisLifetime=20; bool broken;
 public void ApplyDamage(float damage){ApplyDamage(damage,transform.position,2);}
 public void ApplyDamage(float damage,Vector3 point,float impulse){if(broken||damage<=0)return; health-=damage;if(health>0)return;broken=true; var pieces=new Transform[transform.childCount];for(int i=0;i<pieces.Length;i++)pieces[i]=transform.GetChild(i);foreach(var p in pieces){p.SetParent(null,true);var col=p.GetComponent<Collider>();if(!col){var b=p.gameObject.AddComponent<BoxCollider>();var mf=p.GetComponent<MeshFilter>();if(mf){b.center=mf.sharedMesh.bounds.center;b.size=mf.sharedMesh.bounds.size;}}var rb=p.gameObject.AddComponent<Rigidbody>();rb.mass=4;rb.AddExplosionForce(impulse*50,point,5,0.2f,ForceMode.Impulse);Destroy(p.gameObject,debrisLifetime);}Destroy(gameObject);}
}}

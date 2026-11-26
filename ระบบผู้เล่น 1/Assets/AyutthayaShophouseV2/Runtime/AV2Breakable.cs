using UnityEngine;
namespace Ayutthaya.ShophouseV2 {
public class AV2Breakable : MonoBehaviour {
 public float health=40; public bool support; public int level;
 public GameObject[] fragments; public AV2Building building;
 public bool Broken {get;private set;}
 public void Damage(float amount,Vector3 point){if(Broken||amount<=0)return; health-=amount;if(health<=0)Break(point);}
 public void Break(Vector3 point){if(Broken)return;Broken=true;
  foreach(var c in GetComponents<Collider>())c.enabled=false;
  foreach(var r in GetComponents<Renderer>())r.enabled=false;
  foreach(var f in fragments){if(!f)continue;f.transform.SetParent(null,true);f.SetActive(true);var body=f.GetComponent<Rigidbody>(); if(body){body.isKinematic=false;body.AddExplosionForce(1.8f,point,4f,.05f,ForceMode.Impulse);} Destroy(f,35f);}
  if(support && building) building.SupportLost(level);
 }
}
}

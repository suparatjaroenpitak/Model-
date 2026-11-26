using UnityEngine;
public class BankDamage : MonoBehaviour {
 public float health=100f, explosionForce=2.5f, debrisLifetime=20f, collisionThreshold=8f;
 bool broken;
 public void ApplyDamage(float amount){ApplyDamage(amount,transform.position);}
 public void ApplyDamage(float amount,Vector3 point){if(broken||amount<=0)return;health-=amount;if(health<=0)BreakAt(point);}
 public void Break(){BreakAt(transform.position);}
 void BreakAt(Vector3 point){if(broken)return;broken=true;
  var parts=GetComponentsInChildren<BoxCollider>();
  foreach(var c in parts){if(c==null)continue;var g=c.gameObject;if(g==gameObject)continue;
   g.transform.SetParent(null,true);var rb=g.GetComponent<Rigidbody>();if(rb==null)rb=g.AddComponent<Rigidbody>();rb.isKinematic=false;rb.mass=8f;rb.AddExplosionForce(explosionForce*rb.mass,point,4f,.4f,ForceMode.Impulse);Destroy(g,debrisLifetime);
  }
 }
 void OnCollisionEnter(Collision c){if(c.rigidbody!=null&&c.relativeVelocity.magnitude>=collisionThreshold)ApplyDamage(c.relativeVelocity.magnitude*5f,c.GetContact(0).point);}
}

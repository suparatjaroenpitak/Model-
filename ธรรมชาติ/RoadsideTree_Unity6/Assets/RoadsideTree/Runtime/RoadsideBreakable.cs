using UnityEngine;
public class RoadsideBreakable : MonoBehaviour {
 public float health=100; public bool broken; public void TakeDamage(float damage){if(broken||damage<=0)return;health-=damage;if(health>0)return;broken=true;var body=GetComponent<Rigidbody>();if(!body)body=gameObject.AddComponent<Rigidbody>();body.mass=80;body.AddTorque(transform.forward*300,ForceMode.Impulse);}
}

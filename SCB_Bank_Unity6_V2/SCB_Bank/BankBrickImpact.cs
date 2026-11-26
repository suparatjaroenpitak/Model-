using UnityEngine;
public class BankBrickImpact : MonoBehaviour {
 void OnCollisionEnter(Collision c){var wall=GetComponentInParent<BankDamage>();if(wall!=null&&c.rigidbody!=null&&c.relativeVelocity.magnitude>=wall.collisionThreshold)wall.ApplyDamage(c.relativeVelocity.magnitude*5f,c.GetContact(0).point);}
}

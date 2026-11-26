using UnityEngine;
namespace SoldierKit {
public class SoldierFire : MonoBehaviour {
 public Transform muzzle; public Animator animator; public ParticleSystem flash; public float range=80, damage=20, interval=.18f; public LayerMask hitMask=~0; float next;
 public bool Fire(Vector3 direction) { if(!muzzle || Time.time<next || direction.sqrMagnitude<.001f)return false; next=Time.time+interval; direction.Normalize(); if(flash)flash.Play(); if(animator) foreach(var p in animator.parameters) if(p.name=="Fire" && p.type==AnimatorControllerParameterType.Trigger){animator.SetTrigger("Fire");break;} foreach(var hit in SortedHits(direction)){ if(hit.collider.transform.IsChildOf(transform))continue; hit.collider.SendMessageUpwards("TakeDamage",damage,SendMessageOptions.DontRequireReceiver); Debug.DrawLine(muzzle.position,hit.point,Color.yellow,.3f); return true;} Debug.DrawRay(muzzle.position,direction*range,Color.yellow,.3f);return true; }
 RaycastHit[] SortedHits(Vector3 d){var h=Physics.RaycastAll(muzzle.position,d,range,hitMask,QueryTriggerInteraction.Ignore); System.Array.Sort(h,(a,b)=>a.distance.CompareTo(b.distance));return h;}
 public void FireForward(){if(muzzle)Fire(muzzle.forward);}
}
}

using UnityEngine;
namespace BankKit { public class BankDemoDamage:MonoBehaviour { public Camera view; public float range=30; public void Hit(){var c=view?view:Camera.main;if(c&&Physics.Raycast(c.transform.position,c.transform.forward,out var hit,range)){var b=hit.collider.GetComponentInParent<BankBreakable>();if(b)b.ApplyDamage(100,hit.point,3);}} }}

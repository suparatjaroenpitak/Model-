using UnityEngine;
namespace AyutthayaBuildings {
public sealed class BuildingDebug:MonoBehaviour {
 public Camera view; public float range=20;
 public void Interact(){if(Hit(out var h)){var d=h.collider.GetComponentInParent<BuildingDoor>();if(d)d.Interact();else Debug.Log("No door on hit: "+h.collider.name);}}
 public void Damage(){if(Hit(out var h)){var p=h.collider.GetComponentInParent<BuildingPart>();if(p)p.TakeDamage(120);else Debug.Log("No building part on hit: "+h.collider.name);}}
 bool Hit(out RaycastHit h){h=default;var c=view?view:Camera.main;if(!c){Debug.LogWarning("Assign View camera");return false;}var hits=Physics.RaycastAll(c.transform.position,c.transform.forward,Mathf.Max(1,range),~0,QueryTriggerInteraction.Ignore);System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));foreach(var hit in hits){if(hit.collider.transform.IsChildOf(transform))continue;h=hit;return true;}Debug.Log("No hit in range");return false;}
 void Update(){
#if ENABLE_INPUT_SYSTEM
 var k=UnityEngine.InputSystem.Keyboard.current;if(k!=null){if(k.eKey.wasPressedThisFrame)Interact();if(k.kKey.wasPressedThisFrame)Damage();}
#elif ENABLE_LEGACY_INPUT_MANAGER
 if(Input.GetKeyDown(KeyCode.E))Interact();if(Input.GetKeyDown(KeyCode.K))Damage();
#endif
 }
}
}

using UnityEngine;
namespace AyutthayaBuildings {
public sealed class BuildingDebug:MonoBehaviour {
 public Camera view; public float range=8;
 public void Interact(){if(Hit(out var h)){var d=h.collider.GetComponentInParent<BuildingDoor>();if(d)d.Interact();}}
 public void Damage(){if(Hit(out var h)){var p=h.collider.GetComponentInParent<BuildingPart>();if(p)p.TakeDamage(120);}}
 bool Hit(out RaycastHit h){var c=view?view:Camera.main;h=default;return c&&Physics.Raycast(c.transform.position,c.transform.forward,out h,range,~0,QueryTriggerInteraction.Ignore);}
 void Update(){
#if ENABLE_INPUT_SYSTEM
 var k=UnityEngine.InputSystem.Keyboard.current;if(k!=null){if(k.eKey.wasPressedThisFrame)Interact();if(k.kKey.wasPressedThisFrame)Damage();}
#elif ENABLE_LEGACY_INPUT_MANAGER
 if(Input.GetKeyDown(KeyCode.E))Interact();if(Input.GetKeyDown(KeyCode.K))Damage();
#endif
 }
}
}

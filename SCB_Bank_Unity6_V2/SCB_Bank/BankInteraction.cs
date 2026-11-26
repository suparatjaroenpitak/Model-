using UnityEngine;
public class BankInteraction : MonoBehaviour {
 public Camera playerCamera;
 public float doorRange=4f, damageRange=30f, testDamage=100f;
 public bool enableTestInput=true;
 void Update(){
  if(!enableTestInput)return;
  bool open=false, damage=false;
#if ENABLE_INPUT_SYSTEM
  var k=UnityEngine.InputSystem.Keyboard.current;
  if(k!=null){open=k.eKey.wasPressedThisFrame;damage=k.fKey.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
  open=Input.GetKeyDown(KeyCode.E);damage=Input.GetKeyDown(KeyCode.F);
#endif
  if(!open&&!damage)return;
  Camera cam=playerCamera!=null?playerCamera:Camera.main;
  if(cam==null){Debug.LogWarning("Bank: assign Player Camera or tag it MainCamera",this);return;}
  Ray ray=cam.ViewportPointToRay(new Vector3(.5f,.5f,0));
  if(!Physics.Raycast(ray,out RaycastHit hit,damage?damageRange:doorRange,~0,QueryTriggerInteraction.Ignore)){return;}
  if(!hit.collider.transform.IsChildOf(transform))return;
  if(open){var pair=hit.collider.GetComponentInParent<BankDoorGroup>();if(pair!=null)pair.Interact();else {var door=hit.collider.GetComponentInParent<BankDoor>();if(door!=null)door.Interact();}}
  if(damage){var wall=hit.collider.GetComponentInParent<BankDamage>();if(wall!=null)wall.ApplyDamage(testDamage,hit.point);else Debug.Log("Bank: this part has no damage component: "+hit.collider.name,hit.collider);}
 }
}

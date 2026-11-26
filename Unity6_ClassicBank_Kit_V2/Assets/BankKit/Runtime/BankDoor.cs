using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace BankKit {
public class BankDoor : MonoBehaviour {
 public Transform left,right,player; public float angle=95,speed=120,radius=3.5f; public bool open,automatic=true;
 public bool manualKey=true; public float closeDelay=1.5f; float lastNear=-100, manualUntil=-100, nextSearch; bool warned;
 public void Interact(){open=!open;manualUntil=Time.time+4f;}
 public void SetOpen(bool value){open=value;manualUntil=Time.time+4f;}
 [ContextMenu("Test/Open Door (Play Mode)")] void TestOpen(){SetOpen(true);}
 [ContextMenu("Test/Close Door (Play Mode)")] void TestClose(){SetOpen(false);}
 bool Near(Vector3 position){Vector3 d=position-transform.position;return new Vector2(d.x,d.z).sqrMagnitude<=radius*radius && Mathf.Abs(d.y)<3.5f;}
 void Update(){
  if(!left||!right){if(!warned){Debug.LogWarning("BankDoor: missing hinge references",this);warned=true;}return;}
  if(!player && Time.unscaledTime>=nextSearch){nextSearch=Time.unscaledTime+1;var p=GameObject.FindWithTag("Player");if(p)player=p.transform;else {var cc=FindObjectsByType<CharacterController>(FindObjectsSortMode.None);if(cc.Length==1)player=cc[0].transform;}}
  bool near=player && Near(player.position);
  if(!near){foreach(var c in Physics.OverlapBox(transform.position+Vector3.up, new Vector3(radius,1.7f,radius), Quaternion.identity,~0,QueryTriggerInteraction.Ignore)){if(c.GetComponentInParent<CharacterController>() || c.CompareTag("Player")){near=true;break;}}}
  if(!player && Camera.main)near|=Near(Camera.main.transform.position);
  if(near)lastNear=Time.time;
  bool pressed=false;
#if ENABLE_INPUT_SYSTEM
  pressed=Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
  pressed=Input.GetKeyDown(KeyCode.E);
#endif
  if(manualKey && near && pressed)Interact();
  if(automatic && Time.time>=manualUntil)open=Time.time-lastNear<closeDelay;
  left.localRotation=Quaternion.RotateTowards(left.localRotation,Quaternion.Euler(0,open?-angle:0,0),speed*Time.deltaTime);
  right.localRotation=Quaternion.RotateTowards(right.localRotation,Quaternion.Euler(0,open?angle:0,0),speed*Time.deltaTime);
 }
}}

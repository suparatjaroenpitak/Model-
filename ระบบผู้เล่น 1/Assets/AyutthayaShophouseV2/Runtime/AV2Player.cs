using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Ayutthaya.ShophouseV2 {
[RequireComponent(typeof(CharacterController))]
public class AV2Player : MonoBehaviour {
 public Camera view; public GameObject avatar; public AV2Building testBuilding;
 public float walkSpeed=3.5f,runSpeed=6f,jumpHeight=1.15f,mouseSensitivity=.12f,interactRange=3.5f;
 public bool firstPerson=false; public bool allowCollapseTest=true;
 CharacterController cc; float yaw,pitch=12,vertical; Vector3 spawn; string hint=""; bool locked;
 void Start(){cc=GetComponent<CharacterController>();spawn=transform.position;yaw=transform.eulerAngles.y;Lock(true);}
 void Lock(bool state){locked=state;Cursor.lockState=state?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!state;}
 void Update(){
  Vector2 move=Vector2.zero,look=Vector2.zero;bool sprint=false,jump=false,interact=false,damage=false,collapse=false,toggle=false,escape=false,click=false,reset=false;
#if ENABLE_INPUT_SYSTEM
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null){move=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));sprint=k.leftShiftKey.isPressed;jump=k.spaceKey.wasPressedThisFrame;interact=k.eKey.wasPressedThisFrame;damage=k.fKey.wasPressedThisFrame;collapse=k.kKey.wasPressedThisFrame;toggle=k.vKey.wasPressedThisFrame;escape=k.escapeKey.wasPressedThisFrame;reset=k.rKey.wasPressedThisFrame;}
  if(m!=null){look=m.delta.ReadValue();click=m.leftButton.wasPressedThisFrame;}
#elif ENABLE_LEGACY_INPUT_MANAGER
  move=new Vector2((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));look=new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"))*15f;sprint=Input.GetKey(KeyCode.LeftShift);jump=Input.GetKeyDown(KeyCode.Space);interact=Input.GetKeyDown(KeyCode.E);damage=Input.GetKeyDown(KeyCode.F);collapse=Input.GetKeyDown(KeyCode.K);toggle=Input.GetKeyDown(KeyCode.V);escape=Input.GetKeyDown(KeyCode.Escape);click=Input.GetMouseButtonDown(0);reset=Input.GetKeyDown(KeyCode.R);
#endif
  if(escape)Lock(false);else if(click&&!locked)Lock(true);
  if(!locked){move=Vector2.zero;look=Vector2.zero;jump=interact=damage=collapse=toggle=false;}
  yaw+=look.x*mouseSensitivity;pitch=Mathf.Clamp(pitch-look.y*mouseSensitivity,-65,75);transform.rotation=Quaternion.Euler(0,yaw,0);
  if(toggle){firstPerson=!firstPerson;if(avatar)avatar.SetActive(!firstPerson);}
  if(cc.isGrounded&&vertical<0)vertical=-2f;if(cc.isGrounded&&jump)vertical=Mathf.Sqrt(2*9.81f*jumpHeight);vertical-=9.81f*Time.deltaTime;
  Vector3 dir=transform.right*move.x+transform.forward*move.y;dir=Vector3.ClampMagnitude(dir,1);cc.Move((dir*(sprint?runSpeed:walkSpeed)+Vector3.up*vertical)*Time.deltaTime);
  if(reset||transform.position.y< -12){cc.enabled=false;transform.position=spawn;vertical=0;cc.enabled=true;}
  PositionCamera();hint="";
  if(view && locked && FindTarget(out RaycastHit hit)){
   var door=hit.collider.GetComponentInParent<AV2Door>();var part=hit.collider.GetComponentInParent<AV2Breakable>();
   if(door){hint=door.IsOpen?"E  Close":"E  Open";if(interact)door.Toggle();}
   if(part&&!part.Broken){hint="F  Damage test";if(damage)part.Damage(45,hit.point);}
  }
  if(collapse&&allowCollapseTest&&testBuilding)testBuilding.CollapseAll();
 }
 void PositionCamera(){if(!view)return;Vector3 focus=transform.position+Vector3.up*1.62f;Quaternion q=Quaternion.Euler(pitch,yaw,0);Vector3 back=-(q*Vector3.forward);float distance=firstPerson?0:3.3f;
  if(!firstPerson){foreach(var hit in Physics.SphereCastAll(focus,.16f,back,distance,~0,QueryTriggerInteraction.Ignore)){if(hit.collider.transform.IsChildOf(transform))continue;distance=Mathf.Min(distance,Mathf.Max(.15f,hit.distance-.12f));}}
  view.transform.SetPositionAndRotation(focus+back*distance,q);
  if(avatar)avatar.SetActive(!firstPerson&&distance>.55f);
 }
 bool FindTarget(out RaycastHit target){target=default;float best=float.PositiveInfinity;foreach(var hit in Physics.RaycastAll(view.transform.position,view.transform.forward,8,~0,QueryTriggerInteraction.Ignore)){if(hit.collider.transform.IsChildOf(transform))continue;if(hit.distance<best){best=hit.distance;target=hit;}}return best<float.PositiveInfinity&&Vector3.Distance(transform.position+Vector3.up*1.1f,target.point)<=interactRange;}
 void OnGUI(){GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,20,25),"+");GUI.Box(new Rect(12,12,310,94),"AYUTTHAYA | Interactive Shophouse V2\nWASD / Shift / Space / V (view)\nE (door) / F (damage test) / K (collapse)\nR (respawn) / Esc (cursor)");if(!string.IsNullOrEmpty(hint))GUI.Box(new Rect(Screen.width/2-100,Screen.height/2+20,200,30),hint);}
}
}

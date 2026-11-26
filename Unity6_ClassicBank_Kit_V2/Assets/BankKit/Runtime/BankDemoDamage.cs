using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace BankKit {
public class BankDemoDamage : MonoBehaviour {
 public Camera view; public float range=50,damage=100,impulse=3; public bool testInput=true; public LayerMask hitMask=~0;
 void Update(){if(!testInput)return;bool pressed=false;
#if ENABLE_INPUT_SYSTEM
 pressed=(Keyboard.current!=null && Keyboard.current.kKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
 pressed=Input.GetKeyDown(KeyCode.K);
#endif
 if(pressed)Hit();}
 [ContextMenu("Test/Damage Aim (Play Mode)")]
 public void Hit(){var c=view?view:Camera.main;if(!c){Debug.LogWarning("BankDemoDamage: assign View camera or MainCamera tag",this);return;}if(Physics.Raycast(c.transform.position,c.transform.forward,out var hit,range,hitMask,QueryTriggerInteraction.Ignore)){var b=hit.collider.GetComponentInParent<BankBreakable>();if(b){b.ApplyDamage(damage,hit.point,impulse);Debug.Log("Bank damage hit: "+hit.collider.name,this);}else Debug.Log("Bank damage: blocked by "+hit.collider.name+" (not breakable)",this);}else Debug.Log("Bank damage: no collider in aim",this);}
}}

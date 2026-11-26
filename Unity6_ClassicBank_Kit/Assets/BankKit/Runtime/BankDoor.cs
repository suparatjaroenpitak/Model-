using UnityEngine;
namespace BankKit {
public class BankDoor : MonoBehaviour {
 public Transform left,right; public float angle=95,speed=120; public bool open; public bool automatic=true; public float radius=2.7f; public Transform player;
 public void Interact(){open=!open;}
 void Update(){if(automatic){if(!player){var p=GameObject.FindWithTag("Player"); if(p)player=p.transform;} if(player)open=Vector3.Distance(player.position,transform.position)<radius; } if(left)left.localRotation=Quaternion.RotateTowards(left.localRotation,Quaternion.Euler(0,open?-angle:0,0),speed*Time.deltaTime); if(right)right.localRotation=Quaternion.RotateTowards(right.localRotation,Quaternion.Euler(0,open?angle:0,0),speed*Time.deltaTime);}
}}

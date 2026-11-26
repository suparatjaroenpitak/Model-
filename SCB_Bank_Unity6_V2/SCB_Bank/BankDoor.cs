using UnityEngine;
public class BankDoor : MonoBehaviour {
 public float angle=100, speed=120; public bool locked; bool opened; Quaternion closed;
 void Awake(){closed=transform.localRotation;}
 public void Interact(){if(!locked) opened=!opened;}
 void Update(){transform.localRotation=Quaternion.RotateTowards(transform.localRotation,closed*Quaternion.Euler(0,opened?angle:0,0),speed*Time.deltaTime);}
}

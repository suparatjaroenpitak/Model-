using UnityEngine;
public class RoadsideDoor : MonoBehaviour {
 public float angle=105, speed=180; public bool opened; Quaternion closed;
 void Awake(){closed=transform.localRotation;}
 public void Interact(){opened=!opened;}
 void Update(){transform.localRotation=Quaternion.RotateTowards(transform.localRotation,closed*Quaternion.Euler(0,opened?angle:0,0),speed*Time.deltaTime);}
}

using UnityEngine;
namespace AyutthayaBuildings {
public sealed class BuildingDoor:MonoBehaviour {
 public Transform hinge; public float angle=100; public float speed=120; bool opened;
 public void Interact(){opened=!opened;}
 void Update(){if(hinge)hinge.localRotation=Quaternion.RotateTowards(hinge.localRotation,Quaternion.Euler(0,opened?angle:0,0),speed*Time.deltaTime);}
}
}

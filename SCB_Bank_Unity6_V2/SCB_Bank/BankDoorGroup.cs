using UnityEngine;
public class BankDoorGroup : MonoBehaviour {
 public void Interact(){ foreach(var door in GetComponentsInChildren<BankDoor>()) door.Interact(); }
}

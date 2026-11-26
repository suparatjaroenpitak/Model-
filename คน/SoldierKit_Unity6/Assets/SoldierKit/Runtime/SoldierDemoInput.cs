using UnityEngine;
namespace SoldierKit {
public class SoldierDemoInput:MonoBehaviour {
 public SoldierFire weapon; public Camera aimCamera;
 void Update(){bool pressed=false;
#if ENABLE_INPUT_SYSTEM
 pressed=UnityEngine.InputSystem.Mouse.current!=null && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
 pressed=Input.GetMouseButton(0);
#endif
 if(pressed && weapon && weapon.muzzle){var c=aimCamera?aimCamera:Camera.main; if(c){var ray=c.ViewportPointToRay(new Vector3(.5f,.5f)); weapon.Fire(ray.GetPoint(weapon.range)-weapon.muzzle.position);}else weapon.FireForward();}}
}
}

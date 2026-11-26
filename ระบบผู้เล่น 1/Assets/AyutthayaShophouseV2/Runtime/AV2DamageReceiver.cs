using UnityEngine;
namespace Ayutthaya.ShophouseV2 {
// Optional bridge for your existing player or gameplay systems.
public static class AV2DamageReceiver {
 public static bool Apply(Collider collider,float amount,Vector3 point){if(!collider)return false;var p=collider.GetComponentInParent<AV2Breakable>();if(!p)return false;p.Damage(amount,point);return true;}
 public static bool Interact(Collider collider){if(!collider)return false;var d=collider.GetComponentInParent<AV2Door>();if(!d)return false;d.Toggle();return true;}
}
}

using UnityEngine;
namespace AyutthayaBuildings {
public sealed class BuildingStructure:MonoBehaviour {
 public BuildingPart[] supports; public BuildingPart[] upperParts; public float collapseFraction=.65f; int lost; bool collapsed;
 public void Notify(BuildingPart part){if(collapsed||!part.structural)return;lost++;if(lost>=Mathf.Max(1,Mathf.CeilToInt(supports.Length*collapseFraction))){collapsed=true;foreach(var p in upperParts)if(p)p.Break();}}
 public void TakeDamage(float damage){foreach(var p in GetComponentsInChildren<BuildingPart>())p.TakeDamage(damage);}
}
}

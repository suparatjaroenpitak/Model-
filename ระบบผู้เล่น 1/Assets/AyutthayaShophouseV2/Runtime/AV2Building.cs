using UnityEngine;
namespace Ayutthaya.ShophouseV2 {
public class AV2Building : MonoBehaviour {
 public AV2Module[] modules; public int supportsToFail=2; readonly int[] lost=new int[3]; bool collapsing; int collapsedFrom=4;
 public void SupportLost(int level){if(level<0||level>=3||collapsing)return;if(++lost[level]>=supportsToFail) CollapseFrom(level);}
 public void CollapseFrom(int level){if(collapsing||level>=collapsedFrom)return;level=Mathf.Clamp(level,0,2);collapsing=true;int previous=collapsedFrom;collapsedFrom=level;
  foreach(var m in modules)if(m&&m.level>=level&&m.level<previous&&m.gameObject.activeSelf)m.Release();
  collapsing=false;
 }
 [ContextMenu("Collapse All (Play Mode)")] public void CollapseAll(){if(Application.isPlaying)CollapseFrom(0);}
}
}

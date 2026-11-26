using UnityEngine;
namespace Ayutthaya.ShophouseV2 {
public class AV2Door : MonoBehaviour {
 public Transform[] leaves; public float[] angles; public bool shutter; public float speed=3f;
 public bool IsOpen {get;private set;} float progress;
 public void Toggle(){IsOpen=!IsOpen;}
 void Update(){progress=Mathf.MoveTowards(progress,IsOpen?1:0,Time.deltaTime*speed); float t=Mathf.SmoothStep(0,1,progress);
  if(shutter) foreach(var leaf in leaves) if(leaf) leaf.localScale=new Vector3(1,Mathf.Lerp(1,.035f,t),1);
  else for(int i=0;i<leaves.Length;i++) if(leaves[i]) leaves[i].localRotation=Quaternion.Euler(0,angles[i]*t,0);
 }
}
}

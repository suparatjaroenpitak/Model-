#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ModelGame.Vehicle.Editor
{
    public static class FortunerVehicleMenu
    {
        [MenuItem("Tools/Fortuner/Create Procedural Vehicle Generator")]
        public static void CreateGenerator()
        {
            GameObject vehicle = new GameObject("Fortuner_Procedural");
            Undo.RegisterCreatedObjectUndo(vehicle, "Create Fortuner Vehicle Generator");
            vehicle.AddComponent<FortunerVehicleGenerator>();
            Selection.activeGameObject = vehicle;
        }
    }
}
#endif

using System;
using UnityEngine;

namespace ToyotaFortuner
{
    public enum VehicleAxis
    {
        X,
        Y,
        Z
    }

    [Serializable]
    public class ToyotaFortunerLodDefinition
    {
        public string label = "LOD0";
        public int triangles;
        public int vertices;
        [Range(0.001f, 1f)]
        public float screenRelativeTransitionHeight = 0.5f;
    }

    [CreateAssetMenu(
        fileName = "ToyotaFortunerAssetProfile",
        menuName = "Vehicles/Toyota Fortuner/Asset Profile")]
    public class ToyotaFortunerAssetProfile : ScriptableObject
    {
        [Header("Real-world reference dimensions (metres)")]
        [Tooltip("Unity vehicle axis order is X = length, Y = height, Z = width.")]
        public Vector3 dimensions = new Vector3(4.795f, 1.835f, 1.855f);

        public VehicleAxis lengthAxis = VehicleAxis.X;
        public VehicleAxis heightAxis = VehicleAxis.Y;
        public VehicleAxis widthAxis = VehicleAxis.Z;

        [Tooltip("Additional uniform scale applied after the model is fitted to the reference dimensions.")]
        public float uniformScale = 1f;

        [Header("Game-ready LOD targets from the supplied reference sheet")]
        public ToyotaFortunerLodDefinition[] lods =
        {
            new ToyotaFortunerLodDefinition
            {
                label = "LOD0",
                triangles = 312864,
                vertices = 169845,
                screenRelativeTransitionHeight = 0.60f
            },
            new ToyotaFortunerLodDefinition
            {
                label = "LOD1",
                triangles = 168412,
                vertices = 91205,
                screenRelativeTransitionHeight = 0.30f
            },
            new ToyotaFortunerLodDefinition
            {
                label = "LOD2",
                triangles = 72640,
                vertices = 40518,
                screenRelativeTransitionHeight = 0.12f
            },
            new ToyotaFortunerLodDefinition
            {
                label = "LOD3",
                triangles = 28396,
                vertices = 15637,
                screenRelativeTransitionHeight = 0.03f
            }
        };

        [Header("Generated PBR materials")]
        public Material bodyPaint;
        public Material glass;
        public Material rubber;
        public Material chrome;
        public Material lights;
        public Material interior;
        public Material decal;
    }
}

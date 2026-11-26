using System.Collections.Generic;
using UnityEngine;

namespace ToyotaFortuner
{
    [DisallowMultipleComponent]
    public class ToyotaFortunerRuntimeSetup : MonoBehaviour
    {
        [SerializeField] private ToyotaFortunerAssetProfile profile;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool fitToReferenceDimensions = true;
        [SerializeField] private bool useUniformScale = true;
        [SerializeField] private bool applyMaterials = true;
        [SerializeField] private bool buildLodGroup = true;

        private bool dimensionsApplied;

        private void Reset()
        {
            modelRoot = transform;
        }

        private void Awake()
        {
            if (applyOnAwake)
            {
                ApplySetup();
            }
        }

        [ContextMenu("Apply Toyota Fortuner Setup")]
        public void ApplySetup()
        {
            if (modelRoot == null)
            {
                modelRoot = transform;
            }

            if (profile == null)
            {
                Debug.LogWarning("Toyota Fortuner setup skipped: no Asset Profile is assigned.", this);
                return;
            }

            if (fitToReferenceDimensions && !dimensionsApplied)
            {
                FitToReferenceDimensions();
                dimensionsApplied = true;
            }

            if (applyMaterials)
            {
                ApplyMaterialsByPartName();
            }

            if (buildLodGroup)
            {
                BuildLodGroupFromChildNames();
            }
        }

        private void FitToReferenceDimensions()
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning("Toyota Fortuner setup could not find any Renderer components to measure.", this);
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 current = bounds.size;
            Vector3 target = profile.dimensions;
            if (current.x <= Mathf.Epsilon || current.y <= Mathf.Epsilon || current.z <= Mathf.Epsilon)
            {
                Debug.LogWarning("Toyota Fortuner setup found an invalid model bounds size.", this);
                return;
            }

            Vector3 ratios = new Vector3(
                target.x / current.x,
                target.y / current.y,
                target.z / current.z);

            if (useUniformScale)
            {
                float uniformRatio = Mathf.Pow(ratios.x * ratios.y * ratios.z, 1f / 3f);
                modelRoot.localScale *= uniformRatio * profile.uniformScale;
            }
            else
            {
                modelRoot.localScale = Vector3.Scale(
                    modelRoot.localScale,
                    ratios * profile.uniformScale);
            }
        }

        private void ApplyMaterialsByPartName()
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material material = ResolveMaterial(renderer.transform);
                if (material == null)
                {
                    continue;
                }

                Material[] slots = renderer.sharedMaterials;
                if (slots == null || slots.Length == 0)
                {
                    renderer.sharedMaterial = material;
                    continue;
                }

                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = material;
                }

                renderer.sharedMaterials = slots;
            }
        }

        private Material ResolveMaterial(Transform part)
        {
            string partName = part.name.ToLowerInvariant();

            if (ContainsAny(partName, "glass", "window", "windshield")) return profile.glass;
            if (ContainsAny(partName, "tire", "tyre", "rubber", "wheel_tread")) return profile.rubber;
            if (ContainsAny(partName, "chrome", "grille", "grill", "emblem", "rim")) return profile.chrome;
            if (ContainsAny(partName, "headlight", "taillight", "tail_light", "lamp", "light")) return profile.lights;
            if (ContainsAny(partName, "interior", "seat", "dashboard", "dash", "steering")) return profile.interior;
            if (ContainsAny(partName, "decal", "badge", "logo", "sticker")) return profile.decal;

            return profile.bodyPaint;
        }

        private void BuildLodGroupFromChildNames()
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            List<Renderer>[] groups = new List<Renderer>[4];
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new List<Renderer>();
            }

            foreach (Renderer renderer in renderers)
            {
                int lodIndex = FindLodIndex(renderer.transform);
                if (lodIndex >= 0)
                {
                    groups[lodIndex].Add(renderer);
                }
            }

            bool hasAtLeastOneLod = false;
            for (int i = 0; i < groups.Length; i++)
            {
                hasAtLeastOneLod |= groups[i].Count > 0;
            }

            if (!hasAtLeastOneLod)
            {
                Debug.LogWarning(
                    "No child names containing LOD0, LOD1, LOD2 or LOD3 were found. " +
                    "Keep the imported LOD meshes in separately named groups.", this);
                return;
            }

            LODGroup lodGroup = modelRoot.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = modelRoot.gameObject.AddComponent<LODGroup>();
            }

            int lodCount = Mathf.Min(profile.lods.Length, groups.Length);
            List<LOD> lods = new List<LOD>(lodCount);
            for (int i = 0; i < lodCount; i++)
            {
                if (groups[i].Count == 0)
                {
                    continue;
                }

                float height = profile.lods[i].screenRelativeTransitionHeight;
                lods.Add(new LOD(height, groups[i].ToArray()));
            }

            if (lods.Count > 0)
            {
                lodGroup.SetLODs(lods.ToArray());
                lodGroup.RecalculateBounds();
            }
        }

        private static int FindLodIndex(Transform current)
        {
            while (current != null)
            {
                string name = current.name.ToUpperInvariant();
                for (int i = 0; i < 4; i++)
                {
                    if (name.Contains("LOD" + i))
                    {
                        return i;
                    }
                }

                current = current.parent;
            }

            return -1;
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (value.Contains(token))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

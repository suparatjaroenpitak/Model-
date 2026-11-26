using System.Collections.Generic;
using UnityEngine;

namespace ModelGame.Vehicle
{
    /// <summary>
    /// Procedural, modular SUV proxy based on the supplied Fortuner reference sheets.
    /// The generated object is deliberately assembled from named parts so production
    /// meshes can replace individual pieces without changing the generator API.
    /// </summary>
    [ExecuteAlways]
    public class FortunerVehicleGenerator : MonoBehaviour
    {
        [Header("Reference dimensions (metres)")]
        [Min(0.1f)] public float overallLength = 4.795f;
        [Min(0.1f)] public float overallWidth = 1.855f;
        [Min(0.1f)] public float overallHeight = 1.835f;
        [Min(0.01f)] public float modelScale = 1f;

        [Header("Body look")]
        public Color bodyColor = new Color(0.86f, 0.87f, 0.88f, 1f);
        public Color glassColor = new Color(0.015f, 0.025f, 0.035f, 1f);
        public Color tireColor = new Color(0.008f, 0.008f, 0.008f, 1f);
        public Color rimColor = new Color(0.45f, 0.47f, 0.5f, 1f);
        public Color chromeColor = new Color(0.72f, 0.75f, 0.78f, 1f);
        public Color headlightColor = new Color(0.7f, 0.9f, 1f, 1f);
        public Color taillightColor = new Color(1f, 0.03f, 0.01f, 1f);
        public FortunerPbrTextureSet pbrTextures = new FortunerPbrTextureSet();

        [Header("Generation")]
        public bool createLodGroup = true;
        public bool generateOnEnable;
        public bool includeDetailedTrim = true;

        [Header("Production LOD targets from reference sheet")]
        public int lod0TargetTriangles = 312864;
        public int lod1TargetTriangles = 168412;
        public int lod2TargetTriangles = 72640;
        public int lod3TargetTriangles = 28396;

        [SerializeField] private Transform generatedRoot;

        private readonly List<Material> generatedMaterials = new List<Material>();
        private Material bodyMaterial;
        private Material glassMaterial;
        private Material tireMaterial;
        private Material rimMaterial;
        private Material chromeMaterial;
        private Material headlightMaterial;
        private Material taillightMaterial;
        private Material blackTrimMaterial;

        public Transform GeneratedRoot => generatedRoot;

        private void OnEnable()
        {
            if (generateOnEnable && generatedRoot == null)
            {
                GenerateModel();
            }
        }

        [ContextMenu("Generate Fortuner Procedural Model")]
        public void GenerateModel()
        {
            ClearGeneratedModel();
            CreateMaterials();

            generatedRoot = new GameObject("Fortuner_Generated").transform;
            generatedRoot.SetParent(transform, false);
            generatedRoot.localScale = Vector3.one * modelScale;

            Transform lod0 = CreateChild(generatedRoot, "LOD0_Hero");
            Transform lod1 = CreateChild(generatedRoot, "LOD1_Mid");
            Transform lod2 = CreateChild(generatedRoot, "LOD2_Low");
            Transform lod3 = CreateChild(generatedRoot, "LOD3_ImpostorProxy");

            List<Renderer> lod0Renderers = new List<Renderer>();
            List<Renderer> lod1Renderers = new List<Renderer>();
            List<Renderer> lod2Renderers = new List<Renderer>();
            List<Renderer> lod3Renderers = new List<Renderer>();

            BuildLod0(lod0, lod0Renderers);
            BuildLod1(lod1, lod1Renderers);
            BuildLod2(lod2, lod2Renderers);
            BuildLod3(lod3, lod3Renderers);

            if (createLodGroup)
            {
                LODGroup lodGroup = generatedRoot.gameObject.AddComponent<LODGroup>();
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.60f, lod0Renderers.ToArray()),
                    new LOD(0.34f, lod1Renderers.ToArray()),
                    new LOD(0.16f, lod2Renderers.ToArray()),
                    new LOD(0.04f, lod3Renderers.ToArray())
                });
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.RecalculateBounds();
            }
            else
            {
                lod1.gameObject.SetActive(false);
                lod2.gameObject.SetActive(false);
                lod3.gameObject.SetActive(false);
            }
        }

        [ContextMenu("Clear Generated Model")]
        public void ClearGeneratedModel()
        {
            if (generatedRoot != null)
            {
                DestroyObject(generatedRoot.gameObject);
                generatedRoot = null;
            }

            for (int i = generatedMaterials.Count - 1; i >= 0; i--)
            {
                if (generatedMaterials[i] != null)
                {
                    DestroyObject(generatedMaterials[i]);
                }
            }
            generatedMaterials.Clear();
        }

        private void CreateMaterials()
        {
            bodyMaterial = CreateMaterial("Fortuner_Body_PBR", bodyColor, 0.05f, 0.28f, pbrTextures);
            glassMaterial = CreateMaterial("Fortuner_Glass", glassColor, 0.1f, 0.08f);
            tireMaterial = CreateMaterial("Fortuner_Tire", tireColor, 0f, 0.62f);
            rimMaterial = CreateMaterial("Fortuner_Rim", rimColor, 0.88f, 0.2f);
            chromeMaterial = CreateMaterial("Fortuner_Chrome", chromeColor, 0.95f, 0.12f);
            headlightMaterial = CreateMaterial("Fortuner_Headlight", headlightColor, 0.05f, 0.12f, null, true, headlightColor * 2f);
            taillightMaterial = CreateMaterial("Fortuner_Taillight", taillightColor, 0.05f, 0.18f, null, true, taillightColor * 1.5f);
            blackTrimMaterial = CreateMaterial("Fortuner_BlackTrim", new Color(0.012f, 0.014f, 0.016f, 1f), 0.1f, 0.34f);
        }

        private Material CreateMaterial(
            string materialName,
            Color color,
            float metallic,
            float roughness,
            FortunerPbrTextureSet textures = null,
            bool emission = false,
            Color emissionColor = default(Color))
        {
            Material material = FortunerPbrMaterialFactory.Create(
                materialName,
                color,
                metallic,
                roughness,
                textures,
                emission,
                emissionColor);
            generatedMaterials.Add(material);
            return material;
        }

        private void BuildLod0(Transform parent, List<Renderer> renderers)
        {
            float length = overallLength;
            float width = overallWidth;
            float height = overallHeight;
            float wheelRadius = Mathf.Clamp(height * 0.205f, 0.28f, 0.42f);
            float wheelY = wheelRadius + 0.08f;
            float axleX = length * 0.31f;

            renderers.Add(CreateHull(parent, "BodyShell", new[]
            {
                new HullSection(-length * 0.50f, width * 0.34f, width * 0.38f, 0.44f, 0.98f),
                new HullSection(-length * 0.39f, width * 0.47f, width * 0.49f, 0.38f, 1.13f),
                new HullSection(0f, width * 0.49f, width * 0.50f, 0.36f, 1.12f),
                new HullSection(length * 0.32f, width * 0.45f, width * 0.47f, 0.41f, 1.16f),
                new HullSection(length * 0.50f, width * 0.34f, width * 0.38f, 0.48f, 0.98f)
            }, bodyMaterial));

            renderers.Add(CreateBox(parent, "Hood", new Vector3(length * 0.29f, 1.20f, 0f), new Vector3(length * 0.34f, 0.16f, width * 0.84f), bodyMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "Roof", new Vector3(-length * 0.10f, 1.67f, 0f), new Vector3(length * 0.48f, 0.14f, width * 0.82f), bodyMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RearRoofSpoiler", new Vector3(-length * 0.46f, 1.66f, 0f), new Vector3(0.12f, 0.06f, width * 0.76f), blackTrimMaterial, Quaternion.identity));

            renderers.Add(CreateBox(parent, "FrontBumper", new Vector3(length * 0.505f, 0.67f, 0f), new Vector3(0.15f, 0.30f, width * 0.88f), blackTrimMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RearBumper", new Vector3(-length * 0.505f, 0.67f, 0f), new Vector3(0.15f, 0.25f, width * 0.88f), blackTrimMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "FrontSkidPlate", new Vector3(length * 0.54f, 0.52f, 0f), new Vector3(0.06f, 0.12f, width * 0.58f), chromeMaterial, Quaternion.identity));

            if (includeDetailedTrim)
            {
                renderers.Add(CreateBox(parent, "FrontGrille", new Vector3(length * 0.535f, 0.93f, 0f), new Vector3(0.04f, 0.28f, width * 0.54f), chromeMaterial, Quaternion.identity));
                renderers.Add(CreateBox(parent, "GrilleInset", new Vector3(length * 0.558f, 0.93f, 0f), new Vector3(0.025f, 0.20f, width * 0.43f), blackTrimMaterial, Quaternion.identity));
                renderers.Add(CreateBox(parent, "FrontBadge", new Vector3(length * 0.575f, 1.04f, 0f), new Vector3(0.02f, 0.10f, 0.14f), chromeMaterial, Quaternion.identity));
                renderers.Add(CreateBox(parent, "LicensePlate", new Vector3(length * 0.582f, 0.73f, 0f), new Vector3(0.015f, 0.10f, 0.38f), blackTrimMaterial, Quaternion.identity));
            }

            renderers.Add(CreateBox(parent, "Windshield", new Vector3(length * 0.075f, 1.51f, 0f), new Vector3(0.08f, 0.56f, width * 0.78f), glassMaterial, Quaternion.Euler(0f, 0f, -18f)));
            renderers.Add(CreateBox(parent, "RearGlass", new Vector3(-length * 0.31f, 1.52f, 0f), new Vector3(0.08f, 0.54f, width * 0.78f), glassMaterial, Quaternion.Euler(0f, 0f, 18f)));
            renderers.Add(CreateBox(parent, "LeftSideGlass", new Vector3(-length * 0.10f, 1.56f, width * 0.415f), new Vector3(length * 0.34f, 0.42f, 0.035f), glassMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RightSideGlass", new Vector3(-length * 0.10f, 1.56f, -width * 0.415f), new Vector3(length * 0.34f, 0.42f, 0.035f), glassMaterial, Quaternion.identity));

            renderers.Add(CreateBox(parent, "LeftSideStep", new Vector3(-length * 0.02f, 0.49f, width * 0.535f), new Vector3(length * 0.55f, 0.10f, 0.12f), blackTrimMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RightSideStep", new Vector3(-length * 0.02f, 0.49f, -width * 0.535f), new Vector3(length * 0.55f, 0.10f, 0.12f), blackTrimMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "LeftRoofRail", new Vector3(-length * 0.08f, 1.82f, width * 0.37f), new Vector3(length * 0.47f, 0.045f, 0.045f), chromeMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RightRoofRail", new Vector3(-length * 0.08f, 1.82f, -width * 0.37f), new Vector3(length * 0.47f, 0.045f, 0.045f), chromeMaterial, Quaternion.identity));

            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * (width * 0.52f);
                renderers.Add(CreateSphere(parent, side < 0 ? "LeftMirror" : "RightMirror", new Vector3(length * 0.07f, 1.40f, z), new Vector3(0.16f, 0.10f, 0.22f), bodyMaterial));
                renderers.Add(CreateBox(parent, side < 0 ? "LeftFenderTrim" : "RightFenderTrim", new Vector3(length * 0.28f, 0.80f, side * width * 0.475f), new Vector3(length * 0.25f, 0.06f, 0.06f), blackTrimMaterial, Quaternion.identity));
            }

            if (includeDetailedTrim)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float z = side * (width * 0.515f);
                    renderers.Add(CreateBox(parent, side < 0 ? "LeftFrontHandle" : "RightFrontHandle", new Vector3(-length * 0.02f, 1.17f, z), new Vector3(0.18f, 0.035f, 0.035f), chromeMaterial, Quaternion.identity));
                    renderers.Add(CreateBox(parent, side < 0 ? "LeftRearHandle" : "RightRearHandle", new Vector3(-length * 0.28f, 1.17f, z), new Vector3(0.18f, 0.035f, 0.035f), chromeMaterial, Quaternion.identity));
                }
            }

            renderers.Add(CreateBox(parent, "LeftHeadlight", new Vector3(length * 0.535f, 1.10f, width * 0.30f), new Vector3(0.05f, 0.14f, width * 0.20f), headlightMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RightHeadlight", new Vector3(length * 0.535f, 1.10f, -width * 0.30f), new Vector3(0.05f, 0.14f, width * 0.20f), headlightMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "LeftTaillight", new Vector3(-length * 0.535f, 1.03f, width * 0.33f), new Vector3(0.05f, 0.16f, width * 0.17f), taillightMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "RightTaillight", new Vector3(-length * 0.535f, 1.03f, -width * 0.33f), new Vector3(0.05f, 0.16f, width * 0.17f), taillightMaterial, Quaternion.identity));

            CreateWheelSet(parent, renderers, axleX, wheelY, width * 0.54f, wheelRadius, true);
        }

        private void BuildLod1(Transform parent, List<Renderer> renderers)
        {
            float wheelRadius = Mathf.Clamp(overallHeight * 0.205f, 0.28f, 0.42f);
            renderers.Add(CreateHull(parent, "BodyShell_Mid", new[]
            {
                new HullSection(-overallLength * 0.50f, overallWidth * 0.34f, overallWidth * 0.38f, 0.44f, 0.98f),
                new HullSection(-overallLength * 0.36f, overallWidth * 0.47f, overallWidth * 0.49f, 0.38f, 1.12f),
                new HullSection(overallLength * 0.32f, overallWidth * 0.45f, overallWidth * 0.47f, 0.41f, 1.14f),
                new HullSection(overallLength * 0.50f, overallWidth * 0.34f, overallWidth * 0.38f, 0.48f, 0.98f)
            }, bodyMaterial));
            renderers.Add(CreateBox(parent, "Cabin_Mid", new Vector3(-overallLength * 0.10f, 1.52f, 0f), new Vector3(overallLength * 0.48f, 0.65f, overallWidth * 0.82f), bodyMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "GlassBand_Mid", new Vector3(-overallLength * 0.10f, 1.65f, 0f), new Vector3(overallLength * 0.43f, 0.28f, overallWidth * 0.84f), glassMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "FrontBumper_Mid", new Vector3(overallLength * 0.505f, 0.67f, 0f), new Vector3(0.15f, 0.25f, overallWidth * 0.86f), blackTrimMaterial, Quaternion.identity));
            CreateWheelSet(parent, renderers, overallLength * 0.31f, wheelRadius + 0.08f, overallWidth * 0.54f, wheelRadius, false);
        }

        private void BuildLod2(Transform parent, List<Renderer> renderers)
        {
            float wheelRadius = Mathf.Clamp(overallHeight * 0.205f, 0.28f, 0.42f);
            renderers.Add(CreateBox(parent, "Body_Low", new Vector3(0f, 0.86f, 0f), new Vector3(overallLength, 0.92f, overallWidth * 0.92f), bodyMaterial, Quaternion.identity));
            renderers.Add(CreateBox(parent, "Cabin_Low", new Vector3(-overallLength * 0.10f, 1.54f, 0f), new Vector3(overallLength * 0.48f, 0.62f, overallWidth * 0.80f), bodyMaterial, Quaternion.identity));
            CreateWheelSet(parent, renderers, overallLength * 0.31f, wheelRadius + 0.08f, overallWidth * 0.54f, wheelRadius, false);
        }

        private void BuildLod3(Transform parent, List<Renderer> renderers)
        {
            renderers.Add(CreateBox(parent, "VehicleProxy", new Vector3(0f, 0.95f, 0f), new Vector3(overallLength, 1.22f, overallWidth * 0.90f), bodyMaterial, Quaternion.identity));
        }

        private void CreateWheelSet(Transform parent, List<Renderer> renderers, float axleX, float wheelY, float wheelZ, float radius, bool detailed)
        {
            for (int axle = -1; axle <= 1; axle += 2)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string axleName = axle > 0 ? "Front" : "Rear";
                    string sideName = side > 0 ? "Left" : "Right";
                    Vector3 position = new Vector3(axle * axleX, wheelY, side * wheelZ);
                    renderers.Add(CreateCylinder(parent, axleName + sideName + "Tire", position, radius, 0.22f, tireMaterial));
                    renderers.Add(CreateCylinder(parent, axleName + sideName + "Rim", position + new Vector3(0f, 0f, side * 0.12f), radius * 0.55f, 0.235f, rimMaterial));
                    if (detailed)
                    {
                        renderers.Add(CreateCylinder(parent, axleName + sideName + "Hub", position + new Vector3(0f, 0f, side * 0.145f), radius * 0.18f, 0.25f, chromeMaterial));
                    }
                }
            }
        }

        private Renderer CreateBox(Transform parent, string partName, Vector3 position, Vector3 scale, Material material, Quaternion rotation)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            RemoveCollider(part);
            return ApplyMaterial(part, material);
        }

        private Renderer CreateSphere(Transform parent, string partName, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            RemoveCollider(part);
            return ApplyMaterial(part, material);
        }

        private Renderer CreateCylinder(Transform parent, string partName, Vector3 position, float radius, float width, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            part.transform.localScale = new Vector3(radius * 2f, width * 0.5f, radius * 2f);
            RemoveCollider(part);
            return ApplyMaterial(part, material);
        }

        private Renderer CreateHull(Transform parent, string partName, HullSection[] sections, Material material)
        {
            GameObject part = new GameObject(partName);
            part.transform.SetParent(parent, false);
            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = BuildHullMesh(partName + "Mesh", sections);
            meshRenderer.sharedMaterial = material;
            return meshRenderer;
        }

        private Mesh BuildHullMesh(string meshName, HullSection[] sections)
        {
            const int ringSize = 8;
            List<Vector3> vertices = new List<Vector3>(sections.Length * ringSize + 2);
            List<Vector2> uvs = new List<Vector2>(sections.Length * ringSize + 2);
            List<int> triangles = new List<int>();

            for (int sectionIndex = 0; sectionIndex < sections.Length; sectionIndex++)
            {
                HullSection section = sections[sectionIndex];
                float bevel = Mathf.Min(0.06f, (section.topY - section.bottomY) * 0.18f);
                Vector3[] ring =
                {
                    new Vector3(section.x, section.bottomY + bevel, -section.bottomHalfWidth),
                    new Vector3(section.x, section.topY - bevel, -section.topHalfWidth),
                    new Vector3(section.x, section.topY, -section.topHalfWidth * 0.72f),
                    new Vector3(section.x, section.topY, section.topHalfWidth * 0.72f),
                    new Vector3(section.x, section.topY - bevel, section.topHalfWidth),
                    new Vector3(section.x, section.bottomY + bevel, section.bottomHalfWidth),
                    new Vector3(section.x, section.bottomY, section.bottomHalfWidth * 0.72f),
                    new Vector3(section.x, section.bottomY, -section.bottomHalfWidth * 0.72f)
                };

                for (int ringIndex = 0; ringIndex < ring.Length; ringIndex++)
                {
                    vertices.Add(ring[ringIndex]);
                    uvs.Add(new Vector2(sectionIndex / Mathf.Max(1f, sections.Length - 1f), ringIndex / (float)(ringSize - 1)));
                }
            }

            for (int sectionIndex = 0; sectionIndex < sections.Length - 1; sectionIndex++)
            {
                int current = sectionIndex * ringSize;
                int next = (sectionIndex + 1) * ringSize;
                for (int ringIndex = 0; ringIndex < ringSize; ringIndex++)
                {
                    int nextRing = (ringIndex + 1) % ringSize;
                    triangles.Add(current + ringIndex);
                    triangles.Add(next + ringIndex);
                    triangles.Add(next + nextRing);
                    triangles.Add(current + ringIndex);
                    triangles.Add(next + nextRing);
                    triangles.Add(current + nextRing);
                }
            }

            int frontCenter = vertices.Count;
            vertices.Add(new Vector3(sections[0].x, (sections[0].bottomY + sections[0].topY) * 0.5f, 0f));
            uvs.Add(new Vector2(0f, 0.5f));
            int rearCenter = vertices.Count;
            HullSection last = sections[sections.Length - 1];
            vertices.Add(new Vector3(last.x, (last.bottomY + last.topY) * 0.5f, 0f));
            uvs.Add(new Vector2(1f, 0.5f));

            for (int ringIndex = 0; ringIndex < ringSize; ringIndex++)
            {
                int nextRing = (ringIndex + 1) % ringSize;
                triangles.Add(frontCenter);
                triangles.Add(nextRing);
                triangles.Add(ringIndex);

                int rearStart = (sections.Length - 1) * ringSize;
                triangles.Add(rearCenter);
                triangles.Add(rearStart + ringIndex);
                triangles.Add(rearStart + nextRing);
            }

            Mesh mesh = new Mesh
            {
                name = meshName
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Renderer ApplyMaterial(GameObject part, Material material)
        {
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        private Transform CreateChild(Transform parent, string childName)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private void RemoveCollider(GameObject part)
        {
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyObject(collider);
            }
        }

        private void DestroyObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private struct HullSection
        {
            public readonly float x;
            public readonly float bottomHalfWidth;
            public readonly float topHalfWidth;
            public readonly float bottomY;
            public readonly float topY;

            public HullSection(float x, float bottomHalfWidth, float topHalfWidth, float bottomY, float topY)
            {
                this.x = x;
                this.bottomHalfWidth = bottomHalfWidth;
                this.topHalfWidth = topHalfWidth;
                this.bottomY = bottomY;
                this.topY = topY;
            }
        }
    }
}

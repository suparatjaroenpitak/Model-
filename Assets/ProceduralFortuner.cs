using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a stylized, modular Toyota Fortuner-like SUV from Unity primitives
/// and a small custom lofted body mesh.
///
/// Usage:
/// 1. Add this component to an empty GameObject.
/// 2. Use the component context menu: Generate Fortuner.
/// 3. Optionally assign your body/glass/etc. materials and texture maps.
///
/// This is a procedural blockout, not a 1:1 reconstruction of the reference
/// images. Exact matching requires the original mesh and matching UV layout.
/// </summary>
public class ProceduralFortuner : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool addMeshColliders = false;
    [SerializeField] private bool addWheelColliders = false;
    [SerializeField, Min(0.1f)] private float vehicleLength = 4.795f;
    [SerializeField, Min(0.1f)] private float vehicleWidth = 1.855f;
    [SerializeField, Min(0.1f)] private float vehicleHeight = 1.835f;

    [Header("Materials (optional)")]
    [SerializeField] private Material bodyMaterial;
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material rubberMaterial;
    [SerializeField] private Material chromeMaterial;
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material tailLightMaterial;
    [SerializeField] private Material interiorMaterial;

    [Header("Texture maps (optional)")]
    [Tooltip("The supplied base-color sheet can be assigned here, but it will not line up with this generated mesh unless the UVs are recreated to match the source model.")]
    [SerializeField] private Texture2D baseColorMap;
    [SerializeField] private Texture2D normalMap;
    [SerializeField] private Texture2D roughnessMap;
    [SerializeField] private Texture2D metallicMap;
    [SerializeField] private Texture2D ambientOcclusionMap;

    [Header("Appearance")]
    [SerializeField] private Color bodyColor = new Color(0.92f, 0.92f, 0.90f, 1f);
    [SerializeField] private Color glassColor = new Color(0.035f, 0.08f, 0.11f, 1f);
    [SerializeField] private Color rubberColor = new Color(0.015f, 0.015f, 0.015f, 1f);
    [SerializeField] private Color chromeColor = new Color(0.72f, 0.76f, 0.78f, 1f);

    private const string GeneratedRootName = "__ProceduralFortuner";
    private Transform generatedRoot;

    private Material bodyMat;
    private Material glassMat;
    private Material rubberMat;
    private Material chromeMat;
    private Material lightMat;
    private Material tailLightMat;
    private Material interiorMat;

    private float HalfWidth => vehicleWidth * 0.5f;
    private float HalfLength => vehicleLength * 0.5f;

    private void Start()
    {
        if (generateOnStart && transform.Find(GeneratedRootName) == null)
        {
            GenerateFortuner();
        }
    }

    [ContextMenu("Generate Fortuner")]
    public void GenerateFortuner()
    {
        ClearGenerated();

        generatedRoot = new GameObject(GeneratedRootName).transform;
        generatedRoot.SetParent(transform, false);

        CreateMaterials();
        BuildBody();
        BuildWindows();
        BuildExteriorDetails();
        BuildWheels();
    }

    [ContextMenu("Clear Generated Model")]
    public void ClearGenerated()
    {
        Transform oldRoot = transform.Find(GeneratedRootName);
        if (oldRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(oldRoot.gameObject);
        }
        else
        {
            DestroyImmediate(oldRoot.gameObject);
        }

        generatedRoot = null;
    }

    private void CreateMaterials()
    {
        bodyMat = MakeMaterial("Fortuner Body", bodyMaterial, bodyColor, 0.05f, 0.72f);
        glassMat = MakeGlassMaterial("Fortuner Glass", glassMaterial, glassColor);
        rubberMat = MakeMaterial("Fortuner Rubber", rubberMaterial, rubberColor, 0f, 0.35f);
        chromeMat = MakeMaterial("Fortuner Chrome", chromeMaterial, chromeColor, 0.9f, 0.82f);
        lightMat = MakeEmissionMaterial("Fortuner Headlight", lightMaterial, new Color(0.75f, 0.9f, 1f, 1f));
        tailLightMat = MakeEmissionMaterial("Fortuner Tail Light", tailLightMaterial, new Color(1f, 0.015f, 0.005f, 1f));
        interiorMat = MakeMaterial("Fortuner Interior", interiorMaterial, new Color(0.035f, 0.04f, 0.045f, 1f), 0f, 0.3f);

        ApplyTextureMaps(bodyMat);
    }

    private void BuildBody()
    {
        Mesh bodyMesh = CreateLoftedBodyMesh();
        CreateMeshObject("Body", bodyMesh, bodyMat);

        // Hood, roof and lower sill provide readable panel breaks in the blockout.
        CreateBox("Hood", new Vector3(0f, 1.18f, HalfLength * 0.61f),
            new Vector3(vehicleWidth * 0.78f, 0.10f, vehicleLength * 0.27f), bodyMat);
        CreateBox("Roof", new Vector3(0f, 1.39f, -0.05f),
            new Vector3(vehicleWidth * 0.77f, 0.08f, vehicleLength * 0.38f), bodyMat);

        CreateBox("Left Side Sill", new Vector3(-HalfWidth * 0.97f, 0.38f, -0.05f),
            new Vector3(0.08f, 0.14f, vehicleLength * 0.53f), chromeMat);
        CreateBox("Right Side Sill", new Vector3(HalfWidth * 0.97f, 0.38f, -0.05f),
            new Vector3(0.08f, 0.14f, vehicleLength * 0.53f), chromeMat);

        // A dark interior volume makes the glazing read correctly from game distance.
        CreateBox("Interior", new Vector3(0f, 1.05f, -0.05f),
            new Vector3(vehicleWidth * 0.73f, 0.48f, vehicleLength * 0.32f), interiorMat);
    }

    private void BuildWindows()
    {
        float sideX = HalfWidth * 0.975f;
        float rearZ = -vehicleLength * 0.23f;
        float frontZ = vehicleLength * 0.27f;

        for (int side = -1; side <= 1; side += 2)
        {
            float x = sideX * side;

            CreateDoubleSidedQuad(
                side == -1 ? "Left Front Window" : "Right Front Window",
                new Vector3(x, 0.83f, 0.36f),
                new Vector3(x, 0.83f, frontZ),
                new Vector3(x, 1.28f, frontZ - 0.06f),
                new Vector3(x, 1.31f, 0.46f),
                glassMat);

            CreateDoubleSidedQuad(
                side == -1 ? "Left Rear Window" : "Right Rear Window",
                new Vector3(x, 0.83f, rearZ),
                new Vector3(x, 0.83f, 0.27f),
                new Vector3(x, 1.29f, 0.38f),
                new Vector3(x, 1.29f, rearZ - 0.03f),
                glassMat);
        }

        // Front and rear glass are slightly angled to suggest the SUV profile.
        CreateDoubleSidedQuad(
            "Windshield",
            new Vector3(-HalfWidth * 0.74f, 0.85f, HalfLength * 0.73f),
            new Vector3(HalfWidth * 0.74f, 0.85f, HalfLength * 0.73f),
            new Vector3(HalfWidth * 0.70f, 1.30f, HalfLength * 0.54f),
            new Vector3(-HalfWidth * 0.70f, 1.30f, HalfLength * 0.54f),
            glassMat);

        CreateDoubleSidedQuad(
            "Rear Glass",
            new Vector3(HalfWidth * 0.68f, 0.85f, -HalfLength * 0.67f),
            new Vector3(-HalfWidth * 0.68f, 0.85f, -HalfLength * 0.67f),
            new Vector3(-HalfWidth * 0.66f, 1.26f, -HalfLength * 0.55f),
            new Vector3(HalfWidth * 0.66f, 1.26f, -HalfLength * 0.55f),
            glassMat);

        // Roof rails.
        for (int side = -1; side <= 1; side += 2)
        {
            CreateCylinder(side == -1 ? "Left Roof Rail" : "Right Roof Rail",
                new Vector3(side * HalfWidth * 0.63f, 1.49f, -0.06f),
                0.025f, vehicleLength * 0.34f, chromeMat, Quaternion.Euler(90f, 0f, 0f));
        }
    }

    private void BuildExteriorDetails()
    {
        float frontZ = HalfLength + 0.035f;
        float rearZ = -HalfLength - 0.035f;

        // Front bumper and grille.
        CreateBox("Front Bumper", new Vector3(0f, 0.48f, frontZ),
            new Vector3(vehicleWidth * 0.84f, 0.22f, 0.12f), chromeMat);
        CreateBox("Front Grille", new Vector3(0f, 0.77f, frontZ + 0.02f),
            new Vector3(vehicleWidth * 0.48f, 0.28f, 0.06f), rubberMat);

        for (int i = -2; i <= 2; i++)
        {
            CreateBox("Grille Bar " + i,
                new Vector3(i * 0.16f, 0.77f, frontZ + 0.055f),
                new Vector3(0.025f, 0.24f, 0.018f), chromeMat);
        }

        CreateBox("Toyota Emblem", new Vector3(0f, 0.78f, frontZ + 0.09f),
            new Vector3(0.18f, 0.13f, 0.025f), chromeMat);

        // Headlights and fog lights.
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBox(side == -1 ? "Left Headlight" : "Right Headlight",
                new Vector3(side * vehicleWidth * 0.32f, 0.94f, frontZ + 0.035f),
                new Vector3(vehicleWidth * 0.24f, 0.17f, 0.055f), lightMat,
                Quaternion.Euler(0f, side * -8f, 0f));

            CreateCylinder(side == -1 ? "Left Fog Light" : "Right Fog Light",
                new Vector3(side * vehicleWidth * 0.37f, 0.57f, frontZ + 0.045f),
                0.065f, 0.045f, lightMat, Quaternion.Euler(90f, 0f, 0f));

            CreateBox(side == -1 ? "Left Tail Light" : "Right Tail Light",
                new Vector3(side * vehicleWidth * 0.35f, 0.91f, rearZ - 0.035f),
                new Vector3(vehicleWidth * 0.27f, 0.18f, 0.06f), tailLightMat,
                Quaternion.Euler(0f, side * 8f, 0f));

            // Mirrors.
            CreateSphere(side == -1 ? "Left Mirror" : "Right Mirror",
                new Vector3(side * (HalfWidth + 0.10f), 1.12f, 0.50f),
                new Vector3(0.16f, 0.10f, 0.22f), bodyMat);
        }

        // Rear bumper and license plate.
        CreateBox("Rear Bumper", new Vector3(0f, 0.45f, rearZ),
            new Vector3(vehicleWidth * 0.82f, 0.20f, 0.12f), chromeMat);
        CreateBox("License Plate", new Vector3(0f, 0.68f, rearZ - 0.07f),
            new Vector3(0.48f, 0.18f, 0.025f), rubberMat);

        // Door handles and a simple center seam for each side.
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * (HalfWidth + 0.015f);
            CreateBox("Front Door Handle",
                new Vector3(x, 0.96f, 0.55f), new Vector3(0.025f, 0.045f, 0.13f), chromeMat);
            CreateBox("Rear Door Handle",
                new Vector3(x, 0.96f, -0.58f), new Vector3(0.025f, 0.045f, 0.13f), chromeMat);
            CreateBox("Door Belt Trim",
                new Vector3(x, 0.76f, -0.03f), new Vector3(0.018f, 0.025f, vehicleLength * 0.46f), chromeMat);
        }
    }

    private void BuildWheels()
    {
        float wheelRadius = vehicleHeight * 0.215f;
        float wheelWidth = vehicleWidth * 0.16f;
        float wheelY = wheelRadius + 0.04f;
        float wheelZ = vehicleLength * 0.325f;

        for (int side = -1; side <= 1; side += 2)
        {
            for (int axle = -1; axle <= 1; axle += 2)
            {
                string sideName = side == -1 ? "Left" : "Right";
                string axleName = axle == 1 ? "Front" : "Rear";
                string wheelName = sideName + " " + axleName + " Wheel";
                Vector3 position = new Vector3(side * (HalfWidth + wheelWidth * 0.30f), wheelY, axle * wheelZ);
                Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);

                CreateCylinder(wheelName + " Tire", position, wheelRadius, wheelWidth, rubberMat, rotation);

                Vector3 rimPosition = position + new Vector3(side * (wheelWidth * 0.53f), 0f, 0f);
                CreateCylinder(wheelName + " Rim", rimPosition, wheelRadius * 0.62f,
                    wheelWidth * 0.13f, chromeMat, rotation);
                CreateCylinder(wheelName + " Hub", rimPosition + new Vector3(side * 0.025f, 0f, 0f),
                    wheelRadius * 0.15f, wheelWidth * 0.18f, rubberMat, rotation);

                for (int spoke = 0; spoke < 5; spoke++)
                {
                    float angle = spoke * 72f * Mathf.Deg2Rad;
                    Vector3 spokePos = rimPosition + new Vector3(side * (wheelRadius * 0.28f),
                        Mathf.Sin(angle) * wheelRadius * 0.25f,
                        Mathf.Cos(angle) * wheelRadius * 0.25f);
                    CreateBox(wheelName + " Spoke " + spoke, spokePos,
                        new Vector3(wheelWidth * 0.06f, wheelRadius * 0.08f, wheelRadius * 0.42f), chromeMat,
                        Quaternion.Euler(0f, spoke * 72f, 0f));
                }

                if (addWheelColliders)
                {
                    WheelCollider wheelCollider = new GameObject(wheelName + " Collider").AddComponent<WheelCollider>();
                    wheelCollider.transform.SetParent(generatedRoot, false);
                    wheelCollider.transform.localPosition = position;
                    wheelCollider.radius = wheelRadius;
                    wheelCollider.suspensionDistance = 0.12f;
                }
            }
        }
    }

    private Mesh CreateLoftedBodyMesh()
    {
        // Cross-sections run from rear (-Z) to front (+Z).
        float[] z =
        {
            -HalfLength,
            -HalfLength * 0.90f,
            -HalfLength * 0.40f,
             HalfLength * 0.34f,
             HalfLength * 0.82f,
             HalfLength
        };

        float[] width =
        {
            HalfWidth * 0.72f,
            HalfWidth * 0.92f,
            HalfWidth,
            HalfWidth,
            HalfWidth * 0.91f,
            HalfWidth * 0.78f
        };

        float[] bottom = { 0.38f, 0.27f, 0.24f, 0.25f, 0.34f, 0.49f };
        float[] top = { 1.03f, 1.21f, 1.34f, 1.34f, 1.16f, 0.78f };
        float[] corner = { 0.10f, 0.13f, 0.15f, 0.15f, 0.12f, 0.09f };

        const int pointsPerRing = 8;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int ring = 0; ring < z.Length; ring++)
        {
            float w = width[ring];
            float b = bottom[ring];
            float t = top[ring];
            float c = Mathf.Min(corner[ring], Mathf.Min(w * 0.45f, (t - b) * 0.45f));

            Vector3[] profile =
            {
                new Vector3(-w + c, b, z[ring]),
                new Vector3( w - c, b, z[ring]),
                new Vector3( w, b + c, z[ring]),
                new Vector3( w, t - c, z[ring]),
                new Vector3( w - c, t, z[ring]),
                new Vector3(-w + c, t, z[ring]),
                new Vector3(-w, t - c, z[ring]),
                new Vector3(-w, b + c, z[ring])
            };

            for (int i = 0; i < pointsPerRing; i++)
            {
                vertices.Add(profile[i]);
                uvs.Add(new Vector2((profile[i].x / (HalfWidth * 2f)) + 0.5f,
                    Mathf.InverseLerp(0f, vehicleHeight, profile[i].y)));
            }
        }

        for (int ring = 0; ring < z.Length - 1; ring++)
        {
            int current = ring * pointsPerRing;
            int next = (ring + 1) * pointsPerRing;

            for (int point = 0; point < pointsPerRing; point++)
            {
                int a = current + point;
                int b = current + ((point + 1) % pointsPerRing);
                int c = next + ((point + 1) % pointsPerRing);
                int d = next + point;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        // End caps.
        int rearCenter = vertices.Count;
        vertices.Add(new Vector3(0f, 0.70f, z[0]));
        uvs.Add(new Vector2(0.5f, 0.5f));
        int frontCenter = vertices.Count;
        vertices.Add(new Vector3(0f, 0.65f, z[z.Length - 1]));
        uvs.Add(new Vector2(0.5f, 0.5f));

        for (int point = 0; point < pointsPerRing; point++)
        {
            int nextPoint = (point + 1) % pointsPerRing;

            triangles.Add(rearCenter);
            triangles.Add(nextPoint);
            triangles.Add(point);

            int front = (z.Length - 1) * pointsPerRing;
            triangles.Add(frontCenter);
            triangles.Add(front + point);
            triangles.Add(front + nextPoint);
        }

        Mesh mesh = new Mesh
        {
            name = "Fortuner Procedural Body"
        };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material,
        Quaternion rotation = default(Quaternion))
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SetupObject(go, name, position, scale, rotation, material);
        return go;
    }

    private GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SetupObject(go, name, position, scale, Quaternion.identity, material);
        return go;
    }

    private GameObject CreateCylinder(string name, Vector3 position, float radius, float depth,
        Material material, Quaternion rotation = default(Quaternion))
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SetupObject(go, name, position, new Vector3(radius * 2f, depth * 0.5f, radius * 2f), rotation, material);
        return go;
    }

    private GameObject CreateMeshObject(string name, Mesh mesh, Material material)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(generatedRoot, false);
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        if (addMeshColliders)
        {
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        return go;
    }

    private GameObject CreateDoubleSidedQuad(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
        Material material)
    {
        Mesh mesh = new Mesh { name = name + " Mesh" };
        mesh.vertices = new[] { a, b, c, d, d, c, b, a };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f)
        };
        mesh.triangles = new[]
        {
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7
        };
        mesh.RecalculateNormals();
        return CreateMeshObject(name, mesh, material);
    }

    private void SetupObject(GameObject go, string name, Vector3 position, Vector3 scale,
        Quaternion rotation, Material material)
    {
        go.name = name;
        go.transform.SetParent(generatedRoot, false);
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        go.transform.localScale = scale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        if (!addMeshColliders)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }
        }
    }

    private Material MakeMaterial(string materialName, Material source, Color color,
        float metallic, float smoothness)
    {
        Shader shader = source != null ? source.shader : FindDefaultShader();
        Material material = new Material(shader) { name = materialName };
        if (source != null)
        {
            material.CopyPropertiesFromMaterial(source);
        }

        SetMaterialColor(material, color);
        SetMaterialFloat(material, "_Metallic", metallic);
        SetMaterialFloat(material, "_Smoothness", smoothness);
        SetMaterialFloat(material, "_Glossiness", smoothness);
        return material;
    }

    private static Shader FindDefaultShader()
    {
        // Prefer URP when the project uses it, then fall back to Built-in Render Pipeline.
        return Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("HDRP/Lit");
    }

    private Material MakeGlassMaterial(string materialName, Material source, Color color)
    {
        Material material = MakeMaterial(materialName, source, color, 0.05f, 0.82f);

        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
        }

        SetMaterialFloat(material, "_Cull", 0f);
        SetMaterialColor(material, new Color(color.r, color.g, color.b, 0.58f));
        return material;
    }

    private Material MakeEmissionMaterial(string materialName, Material source, Color color)
    {
        Material material = MakeMaterial(materialName, source, color, 0f, 0.65f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.5f);
        }

        return material;
    }

    private void ApplyTextureMaps(Material material)
    {
        if (material == null)
        {
            return;
        }

        SetTexture(material, "_MainTex", baseColorMap);
        SetTexture(material, "_BaseMap", baseColorMap);

        if (normalMap != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
        }

        SetTexture(material, "_MetallicGlossMap", metallicMap);
        SetTexture(material, "_OcclusionMap", ambientOcclusionMap);

        // Unity's Standard shader expects smoothness, while the supplied map is roughness.
        // Assign a converted smoothness map in a DCC for physically correct results.
        if (roughnessMap != null && material.HasProperty("_RoughnessMap"))
        {
            material.SetTexture("_RoughnessMap", roughnessMap);
        }
    }

    private static void SetTexture(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}

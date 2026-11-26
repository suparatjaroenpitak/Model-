using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Unity 6 procedural Fortuner-like SUV builder.
///
/// The supplied reference sheets contain a reference UV layout and PBR atlas,
/// but do not contain the original mesh. This component therefore creates a
/// compatible, modular approximation with its own 0-1 UV layout. The source
/// atlas can be assigned for look development, but it cannot be expected to
/// line up with the original vehicle UV islands without the original FBX/GLB.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class FortunerProceduralBuilder : MonoBehaviour
{
    public const string GeneratedRootName = "__FortunerUltraGenerated";

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool addMeshColliders;
    [SerializeField] private bool addWheelColliders;
    [SerializeField, Min(0.1f)] private float vehicleLength = 4.795f;
    [SerializeField, Min(0.1f)] private float vehicleWidth = 1.855f;
    [SerializeField, Min(0.1f)] private float vehicleHeight = 1.835f;

    [Header("Source materials (optional)")]
    [SerializeField] private Material bodyMaterial;
    [SerializeField] private Material glassMaterial;
    [SerializeField] private Material rubberMaterial;
    [SerializeField] private Material chromeMaterial;
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material tailLightMaterial;
    [SerializeField] private Material interiorMaterial;

    [Header("PBR maps")]
    [SerializeField] private Texture2D baseColorMap;
    [SerializeField] private Texture2D normalMap;
    [SerializeField] private Texture2D roughnessMap;
    [SerializeField] private Texture2D metallicMap;
    [SerializeField] private Texture2D metallicSmoothnessMap;
    [SerializeField] private Texture2D ambientOcclusionMap;
    [SerializeField] private Texture2D heightMap;

    [Header("Appearance")]
    [SerializeField] private Color bodyColor = new Color(0.92f, 0.92f, 0.90f, 1f);
    [SerializeField] private Color glassColor = new Color(0.025f, 0.07f, 0.095f, 1f);
    [SerializeField] private Color rubberColor = new Color(0.012f, 0.012f, 0.012f, 1f);
    [SerializeField] private Color chromeColor = new Color(0.72f, 0.76f, 0.80f, 1f);

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
        if (Application.isPlaying && generateOnStart && transform.Find(GeneratedRootName) == null)
        {
            GenerateFortuner();
        }
    }

    /// <summary>Used by the Editor generator without exposing implementation fields.</summary>
    public void Configure(
        float length,
        float width,
        float height,
        bool generateColliders,
        bool generateWheelColliders,
        Material bodySource,
        Texture2D baseColor,
        Texture2D normal,
        Texture2D roughness,
        Texture2D metallic,
        Texture2D packedMetallicSmoothness,
        Texture2D ao,
        Texture2D heightTexture)
    {
        vehicleLength = Mathf.Max(0.1f, length);
        vehicleWidth = Mathf.Max(0.1f, width);
        vehicleHeight = Mathf.Max(0.1f, height);
        addMeshColliders = generateColliders;
        addWheelColliders = generateWheelColliders;
        bodyMaterial = bodySource;
        baseColorMap = baseColor;
        normalMap = normal;
        roughnessMap = roughness;
        metallicMap = metallic;
        metallicSmoothnessMap = packedMetallicSmoothness;
        ambientOcclusionMap = ao;
        heightMap = heightTexture;
    }

    [ContextMenu("Generate Fortuner Ultra")]
    public void GenerateFortuner()
    {
        ClearGenerated();

        generatedRoot = new GameObject(GeneratedRootName).transform;
        generatedRoot.SetParent(transform, false);

        CreateMaterials();
        BuildChassisAndBody();
        BuildGlazingAndRoofRails();
        BuildExteriorDetails();
        BuildInteriorHint();
        BuildWheelsAndBrakes();
        BuildCollisionProxies();
    }

    [ContextMenu("Reapply PBR Maps")]
    public void ReapplyPbrMaps()
    {
        if (bodyMat == null)
        {
            CreateMaterials();
        }

        ApplyPbrMaps(bodyMat);
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
        bodyMat = MakeMaterial("Fortuner Ultra Body", bodyMaterial, bodyColor, 0.02f, 0.72f);
        glassMat = MakeGlassMaterial("Fortuner Ultra Glass", glassMaterial, glassColor);
        rubberMat = MakeMaterial("Fortuner Ultra Rubber", rubberMaterial, rubberColor, 0.0f, 0.34f);
        chromeMat = MakeMaterial("Fortuner Ultra Chrome", chromeMaterial, chromeColor, 0.90f, 0.82f);
        lightMat = MakeEmissionMaterial("Fortuner Ultra Headlight", lightMaterial, new Color(0.72f, 0.90f, 1f, 1f));
        tailLightMat = MakeEmissionMaterial("Fortuner Ultra Tail Light", tailLightMaterial, new Color(1f, 0.015f, 0.005f, 1f));
        interiorMat = MakeMaterial("Fortuner Ultra Interior", interiorMaterial, new Color(0.025f, 0.03f, 0.035f, 1f), 0f, 0.28f);

        ApplyPbrMaps(bodyMat);
    }

    private void BuildChassisAndBody()
    {
        CreateMeshObject("Body Shell", CreateLoftedBodyMesh(), bodyMat);

        CreateBox("Chassis", new Vector3(0f, 0.39f, -0.04f),
            new Vector3(vehicleWidth * 0.75f, 0.20f, vehicleLength * 0.70f), interiorMat);
        CreateBox("Hood", new Vector3(0f, 1.19f, HalfLength * 0.63f),
            new Vector3(vehicleWidth * 0.78f, 0.10f, vehicleLength * 0.26f), bodyMat);
        CreateBox("Roof Panel", new Vector3(0f, 1.40f, -0.05f),
            new Vector3(vehicleWidth * 0.75f, 0.075f, vehicleLength * 0.38f), bodyMat);

        for (int side = -1; side <= 1; side += 2)
        {
            string sideName = side < 0 ? "Left" : "Right";
            CreateBox(sideName + " Side Sill", new Vector3(side * HalfWidth * 0.97f, 0.38f, -0.04f),
                new Vector3(0.075f, 0.14f, vehicleLength * 0.54f), chromeMat);
            CreateBox(sideName + " Belt Trim", new Vector3(side * (HalfWidth + 0.012f), 0.76f, -0.03f),
                new Vector3(0.020f, 0.025f, vehicleLength * 0.46f), chromeMat);
        }
    }

    private void BuildGlazingAndRoofRails()
    {
        float sideX = HalfWidth * 0.978f;
        float rearZ = -vehicleLength * 0.23f;
        float frontZ = vehicleLength * 0.27f;

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * sideX;
            string sideName = side < 0 ? "Left" : "Right";

            CreateDoubleSidedQuad(sideName + " Front Window",
                new Vector3(x, 0.84f, 0.36f), new Vector3(x, 0.84f, frontZ),
                new Vector3(x, 1.29f, frontZ - 0.06f), new Vector3(x, 1.31f, 0.45f), glassMat);

            CreateDoubleSidedQuad(sideName + " Rear Window",
                new Vector3(x, 0.84f, rearZ), new Vector3(x, 0.84f, 0.28f),
                new Vector3(x, 1.29f, 0.39f), new Vector3(x, 1.29f, rearZ - 0.03f), glassMat);

            CreateBox(sideName + " A Pillar", new Vector3(x, 1.11f, 0.39f),
                new Vector3(0.045f, 0.48f, 0.055f), rubberMat, Quaternion.Euler(0f, side * -18f, 0f));
            CreateBox(sideName + " B Pillar", new Vector3(x, 1.10f, -0.18f),
                new Vector3(0.045f, 0.49f, 0.055f), rubberMat);
            CreateBox(sideName + " C Pillar", new Vector3(x, 1.10f, -0.70f),
                new Vector3(0.045f, 0.46f, 0.055f), rubberMat, Quaternion.Euler(0f, side * 14f, 0f));

            CreateCylinder(sideName + " Roof Rail", new Vector3(side * HalfWidth * 0.63f, 1.49f, -0.06f),
                0.024f, vehicleLength * 0.34f, chromeMat, Quaternion.Euler(90f, 0f, 0f));
        }

        CreateDoubleSidedQuad("Windshield",
            new Vector3(-HalfWidth * 0.74f, 0.86f, HalfLength * 0.73f),
            new Vector3(HalfWidth * 0.74f, 0.86f, HalfLength * 0.73f),
            new Vector3(HalfWidth * 0.70f, 1.30f, HalfLength * 0.54f),
            new Vector3(-HalfWidth * 0.70f, 1.30f, HalfLength * 0.54f), glassMat);

        CreateDoubleSidedQuad("Rear Glass",
            new Vector3(HalfWidth * 0.68f, 0.86f, -HalfLength * 0.67f),
            new Vector3(-HalfWidth * 0.68f, 0.86f, -HalfLength * 0.67f),
            new Vector3(-HalfWidth * 0.66f, 1.27f, -HalfLength * 0.55f),
            new Vector3(HalfWidth * 0.66f, 1.27f, -HalfLength * 0.55f), glassMat);
    }

    private void BuildExteriorDetails()
    {
        float frontZ = HalfLength + 0.04f;
        float rearZ = -HalfLength - 0.04f;

        CreateBox("Front Bumper", new Vector3(0f, 0.49f, frontZ),
            new Vector3(vehicleWidth * 0.85f, 0.22f, 0.13f), chromeMat);
        CreateBox("Front Grille", new Vector3(0f, 0.79f, frontZ + 0.025f),
            new Vector3(vehicleWidth * 0.49f, 0.28f, 0.065f), rubberMat);

        for (int i = -2; i <= 2; i++)
        {
            CreateBox("Grille Bar " + i, new Vector3(i * 0.16f, 0.79f, frontZ + 0.065f),
                new Vector3(0.025f, 0.235f, 0.018f), chromeMat);
        }

        CreateBox("Front Emblem", new Vector3(0f, 0.80f, frontZ + 0.10f),
            new Vector3(0.18f, 0.13f, 0.025f), chromeMat);

        for (int side = -1; side <= 1; side += 2)
        {
            string sideName = side < 0 ? "Left" : "Right";
            CreateBox(sideName + " Headlight", new Vector3(side * vehicleWidth * 0.32f, 0.95f, frontZ + 0.038f),
                new Vector3(vehicleWidth * 0.24f, 0.17f, 0.055f), lightMat,
                Quaternion.Euler(0f, side * -8f, 0f));
            CreateCylinder(sideName + " Fog Light", new Vector3(side * vehicleWidth * 0.37f, 0.58f, frontZ + 0.05f),
                0.065f, 0.045f, lightMat, Quaternion.Euler(90f, 0f, 0f));
            CreateBox(sideName + " Tail Light", new Vector3(side * vehicleWidth * 0.35f, 0.91f, rearZ - 0.04f),
                new Vector3(vehicleWidth * 0.27f, 0.18f, 0.06f), tailLightMat,
                Quaternion.Euler(0f, side * 8f, 0f));
            CreateSphere(sideName + " Mirror", new Vector3(side * (HalfWidth + 0.10f), 1.12f, 0.50f),
                new Vector3(0.16f, 0.10f, 0.22f), bodyMat);
            CreateBox(sideName + " Front Door Handle", new Vector3(side * (HalfWidth + 0.016f), 0.97f, 0.55f),
                new Vector3(0.025f, 0.045f, 0.13f), chromeMat);
            CreateBox(sideName + " Rear Door Handle", new Vector3(side * (HalfWidth + 0.016f), 0.97f, -0.58f),
                new Vector3(0.025f, 0.045f, 0.13f), chromeMat);
        }

        CreateBox("Rear Bumper", new Vector3(0f, 0.45f, rearZ),
            new Vector3(vehicleWidth * 0.83f, 0.20f, 0.12f), chromeMat);
        CreateBox("Rear License Plate", new Vector3(0f, 0.68f, rearZ - 0.07f),
            new Vector3(0.48f, 0.18f, 0.025f), rubberMat);
        CreateBox("Rear Spoiler", new Vector3(0f, 1.31f, rearZ + 0.09f),
            new Vector3(vehicleWidth * 0.55f, 0.055f, 0.16f), bodyMat);
    }

    private void BuildInteriorHint()
    {
        CreateBox("Interior Volume", new Vector3(0f, 1.05f, -0.05f),
            new Vector3(vehicleWidth * 0.71f, 0.47f, vehicleLength * 0.32f), interiorMat);
        CreateBox("Dashboard", new Vector3(0f, 1.02f, HalfLength * 0.54f),
            new Vector3(vehicleWidth * 0.68f, 0.10f, 0.12f), interiorMat);
        CreateCylinder("Steering Wheel", new Vector3(-0.30f, 1.12f, HalfLength * 0.43f),
            0.15f, 0.025f, rubberMat, Quaternion.Euler(90f, 0f, 0f));
    }

    private void BuildWheelsAndBrakes()
    {
        float wheelRadius = vehicleHeight * 0.215f;
        float wheelWidth = vehicleWidth * 0.16f;
        float wheelY = wheelRadius + 0.04f;
        float wheelZ = vehicleLength * 0.325f;

        for (int side = -1; side <= 1; side += 2)
        {
            for (int axle = -1; axle <= 1; axle += 2)
            {
                string sideName = side < 0 ? "Left" : "Right";
                string axleName = axle > 0 ? "Front" : "Rear";
                string wheelName = sideName + " " + axleName + " Wheel";
                Vector3 wheelPosition = new Vector3(side * (HalfWidth + wheelWidth * 0.30f), wheelY, axle * wheelZ);
                Quaternion wheelRotation = Quaternion.Euler(0f, 0f, 90f);

                CreateCylinder(wheelName + " Tire", wheelPosition, wheelRadius, wheelWidth, rubberMat, wheelRotation);
                Vector3 rimPosition = wheelPosition + new Vector3(side * wheelWidth * 0.53f, 0f, 0f);
                CreateCylinder(wheelName + " Alloy Rim", rimPosition, wheelRadius * 0.62f,
                    wheelWidth * 0.13f, chromeMat, wheelRotation);
                CreateCylinder(wheelName + " Hub", rimPosition + new Vector3(side * 0.025f, 0f, 0f),
                    wheelRadius * 0.15f, wheelWidth * 0.18f, rubberMat, wheelRotation);
                CreateCylinder(wheelName + " Brake Disc", rimPosition + new Vector3(side * 0.012f, 0f, 0f),
                    wheelRadius * 0.47f, wheelWidth * 0.08f, chromeMat, wheelRotation);
                CreateBox(wheelName + " Brake Caliper", rimPosition + new Vector3(side * 0.035f, wheelRadius * 0.27f, 0f),
                    new Vector3(wheelWidth * 0.08f, wheelRadius * 0.17f, wheelRadius * 0.20f), tailLightMat);

                for (int spoke = 0; spoke < 5; spoke++)
                {
                    float angle = spoke * 72f * Mathf.Deg2Rad;
                    Vector3 spokePosition = rimPosition + new Vector3(side * wheelWidth * 0.10f,
                        Mathf.Sin(angle) * wheelRadius * 0.25f,
                        Mathf.Cos(angle) * wheelRadius * 0.25f);
                    CreateBox(wheelName + " Spoke " + spoke, spokePosition,
                        new Vector3(wheelWidth * 0.06f, wheelRadius * 0.08f, wheelRadius * 0.42f), chromeMat,
                        Quaternion.Euler(0f, spoke * 72f, 0f));
                }

                if (addWheelColliders)
                {
                    GameObject colliderObject = new GameObject(wheelName + " WheelCollider");
                    colliderObject.transform.SetParent(generatedRoot, false);
                    colliderObject.transform.localPosition = wheelPosition;
                    WheelCollider wheelCollider = colliderObject.AddComponent<WheelCollider>();
                    wheelCollider.radius = wheelRadius;
                    wheelCollider.suspensionDistance = 0.12f;
                }
            }
        }
    }

    private void BuildCollisionProxies()
    {
        if (!addMeshColliders || generatedRoot == null)
        {
            return;
        }

        GameObject proxy = new GameObject("Collision Proxies");
        proxy.transform.SetParent(generatedRoot, false);
        BoxCollider bodyCollider = proxy.AddComponent<BoxCollider>();
        bodyCollider.center = new Vector3(0f, 0.83f, 0f);
        bodyCollider.size = new Vector3(vehicleWidth * 0.88f, 1.10f, vehicleLength * 0.84f);

        BoxCollider roofCollider = proxy.AddComponent<BoxCollider>();
        roofCollider.center = new Vector3(0f, 1.35f, -0.05f);
        roofCollider.size = new Vector3(vehicleWidth * 0.70f, 0.24f, vehicleLength * 0.42f);
    }

    private Mesh CreateLoftedBodyMesh()
    {
        float[] z =
        {
            -HalfLength,
            -HalfLength * 0.92f,
            -HalfLength * 0.48f,
            -HalfLength * 0.05f,
             HalfLength * 0.36f,
             HalfLength * 0.78f,
             HalfLength
        };
        float[] width =
        {
            HalfWidth * 0.72f,
            HalfWidth * 0.91f,
            HalfWidth,
            HalfWidth,
            HalfWidth * 0.98f,
            HalfWidth * 0.90f,
            HalfWidth * 0.76f
        };
        float[] bottom = { 0.39f, 0.28f, 0.24f, 0.24f, 0.25f, 0.34f, 0.49f };
        float[] top = { 1.04f, 1.20f, 1.34f, 1.35f, 1.34f, 1.17f, 0.78f };

        Vector2[] profile =
        {
            new Vector2(-0.72f, 0.00f), new Vector2(-0.24f, 0.00f), new Vector2(0.24f, 0.00f),
            new Vector2(0.72f, 0.00f), new Vector2(1.00f, 0.22f), new Vector2(1.00f, 0.55f),
            new Vector2(0.84f, 0.88f), new Vector2(0.45f, 1.00f), new Vector2(0.00f, 1.00f),
            new Vector2(-0.45f, 1.00f), new Vector2(-0.84f, 0.88f), new Vector2(-1.00f, 0.55f),
            new Vector2(-1.00f, 0.22f)
        };

        const int pointsPerRing = 13;
        List<Vector3> vertices = new List<Vector3>(z.Length * pointsPerRing + 2);
        List<Vector2> uvs = new List<Vector2>(z.Length * pointsPerRing + 2);
        List<int> triangles = new List<int>();

        for (int ring = 0; ring < z.Length; ring++)
        {
            for (int point = 0; point < pointsPerRing; point++)
            {
                Vector2 p = profile[point];
                vertices.Add(new Vector3(p.x * width[ring], Mathf.Lerp(bottom[ring], top[ring], p.y), z[ring]));
                uvs.Add(new Vector2(point / (float)pointsPerRing,
                    Mathf.InverseLerp(-HalfLength, HalfLength, z[ring])));
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
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }
        }

        int rearCenter = vertices.Count;
        vertices.Add(new Vector3(0f, 0.70f, z[0]));
        uvs.Add(new Vector2(0.5f, 0f));
        int frontCenter = vertices.Count;
        vertices.Add(new Vector3(0f, 0.70f, z[z.Length - 1]));
        uvs.Add(new Vector2(0.5f, 1f));

        int frontStart = (z.Length - 1) * pointsPerRing;
        for (int point = 0; point < pointsPerRing; point++)
        {
            int nextPoint = (point + 1) % pointsPerRing;
            triangles.Add(rearCenter); triangles.Add(nextPoint); triangles.Add(point);
            triangles.Add(frontCenter); triangles.Add(frontStart + point); triangles.Add(frontStart + nextPoint);
        }

        Mesh mesh = new Mesh { name = "Fortuner Ultra Body Shell Mesh" };
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0, true);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }

    private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material,
        Quaternion rotation = default)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SetupObject(go, objectName, position, scale, rotation, material);
        return go;
    }

    private GameObject CreateSphere(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SetupObject(go, objectName, position, scale, Quaternion.identity, material);
        return go;
    }

    private GameObject CreateCylinder(string objectName, Vector3 position, float radius, float depth,
        Material material, Quaternion rotation = default)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SetupObject(go, objectName, position, new Vector3(radius * 2f, depth * 0.5f, radius * 2f), rotation, material);
        return go;
    }

    private GameObject CreateMeshObject(string objectName, Mesh mesh, Material material)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(generatedRoot, false);
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        if (addMeshColliders)
        {
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = false;
        }

        return go;
    }

    private GameObject CreateDoubleSidedQuad(string objectName, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
        Material material)
    {
        Mesh mesh = new Mesh { name = objectName + " Mesh" };
        mesh.vertices = new[] { a, b, c, d, a, d, c, b };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 };
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return CreateMeshObject(objectName, mesh, material);
    }

    private void SetupObject(GameObject go, string objectName, Vector3 position, Vector3 scale,
        Quaternion rotation, Material material)
    {
        go.name = objectName;
        go.transform.SetParent(generatedRoot, false);
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        go.transform.localScale = scale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        // Unity's built-in sphere/cylinder meshes can contain tiny negative UV
        // seam values. Clone and clamp those meshes so generated assets obey
        // the declared 0-1 UV contract without mutating Unity's shared meshes.
        MeshFilter primitiveFilter = go.GetComponent<MeshFilter>();
        if (primitiveFilter != null && primitiveFilter.sharedMesh != null)
        {
            Mesh normalizedMesh = Instantiate(primitiveFilter.sharedMesh);
            normalizedMesh.name = objectName + " Mesh";
            NormalizeMeshUvs(normalizedMesh);
            normalizedMesh.RecalculateTangents();
            primitiveFilter.sharedMesh = normalizedMesh;
        }

        if (!addMeshColliders)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Destroy(collider);
                else DestroyImmediate(collider);
            }
        }
    }

    private Material MakeMaterial(string materialName, Material source, Color color, float metallic, float smoothness)
    {
        Shader shader = source != null ? source.shader : FindPbrShader();
        Material material = new Material(shader) { name = materialName };
        if (source != null) material.CopyPropertiesFromMaterial(source);
        SetMaterialColor(material, color);
        SetMaterialFloat(material, "_Metallic", metallic);
        SetMaterialFloat(material, "_Smoothness", smoothness);
        SetMaterialFloat(material, "_Glossiness", smoothness);
        return material;
    }

    private Material MakeGlassMaterial(string materialName, Material source, Color color)
    {
        Material material = MakeMaterial(materialName, source, color, 0.05f, 0.82f);
        SetMaterialFloat(material, "_Surface", 1f);
        SetMaterialFloat(material, "_Mode", 3f);
        SetMaterialFloat(material, "_Cull", 0f);
        SetMaterialColor(material, new Color(color.r, color.g, color.b, 0.58f));
        material.renderQueue = 3000;
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
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

    private void ApplyPbrMaps(Material material)
    {
        if (material == null) return;

        SetTexture(material, "_MainTex", baseColorMap);
        SetTexture(material, "_BaseMap", baseColorMap);

        if (normalMap != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
        }

        Texture2D packed = metallicSmoothnessMap != null ? metallicSmoothnessMap : metallicMap;
        SetTexture(material, "_MetallicGlossMap", packed);
        SetTexture(material, "_OcclusionMap", ambientOcclusionMap);
        SetTexture(material, "_ParallaxMap", heightMap);
        SetTexture(material, "_RoughnessMap", roughnessMap);
        SetMaterialFloat(material, "_GlossMapScale", 1f);
        SetMaterialFloat(material, "_OcclusionStrength", 1f);
    }

    private static Shader FindPbrShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("HDRP/Lit")
            ?? Shader.Find("Standard");
    }

    private static void SetTexture(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName)) material.SetTexture(propertyName, texture);
    }

    private static void NormalizeMeshUvs(Mesh mesh)
    {
        Vector2[] uvs = mesh.uv;
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(Mathf.Clamp01(uvs[i].x), Mathf.Clamp01(uvs[i].y));
        }
        mesh.uv = uvs;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName)) material.SetFloat(propertyName, value);
    }
}

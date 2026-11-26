using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AyutthayaBiome
{
    public class AyutthayaBiomeScatterWindow : EditorWindow
    {
        private Terrain terrain;
        private int seed = 42;

        [Header("Texture / Terrain")]
        private int textureSize = 1024;
        private int alphamapResolution = 512;
        private int detailResolution = 1024;
        private bool rebuildTerrainTextures = true;
        private float oldLookStrength = 0.65f;
        private float macroNoiseScale = 0.0016f;
        private float microNoiseScale = 0.025f;
        private float roadsideWearWidth = 8f;

        [Header("Trees")]
        private GameObject[] treePrefabs = new GameObject[0];
        private int treeCount = 2200;
        private Vector2 treeScaleRange = new Vector2(0.85f, 1.35f);
        private float minTreeSpacing = 7.5f;
        private float treeSlopeLimit = 26f;
        private float treeWaterAvoidDistance = 5f;

        [Header("Rocks")]
        private GameObject[] rockPrefabs = new GameObject[0];
        private int rockCount = 900;
        private Vector2 rockScaleRange = new Vector2(0.7f, 2.2f);
        private float rockSlopeMin = 2f;
        private float rockSlopeMax = 38f;

        [Header("Grass")]
        private Texture2D grassBillboardTexture;
        private int grassDensity = 18;
        private int grassHealthy = 1;
        private int grassDry = 1;
        private float grassMinHeight = 0.25f;
        private float grassMaxHeight = 0.8f;
        private float grassNoiseScale = 0.005f;

        [Header("Masks / Optional")]
        private Transform roadParent;
        private Transform waterParent;
        private float avoidRoadDistance = 4f;
        private float avoidWaterDistance = 3.5f;
        private bool clearOldGeneratedObjects = true;

        private Vector2 scroll;

        [MenuItem("Tools/AYUTTHAYA/Biome Scatter Generator")]
        public static void Open()
        {
            var window = GetWindow<AyutthayaBiomeScatterWindow>("Biome Scatter");
            window.minSize = new Vector2(540, 700);
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ayutthaya Biome Scatter Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("สร้าง Texture ธรรมชาติแบบเก่าๆ ให้ Terrain และสุ่มวางต้นไม้ หญ้า หินอัตโนมัติ โดยพยายามหลบถนนและน้ำ", MessageType.Info);

            terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
            seed = EditorGUILayout.IntField("Seed", seed);

            DrawTerrainSection();
            DrawPrefabSection();
            DrawGrassSection();
            DrawMasksSection();

            GUI.enabled = terrain != null;
            if (GUILayout.Button("Generate Pretty Textures + Random Trees/Grass/Rocks", GUILayout.Height(40)))
            {
                Generate();
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void DrawTerrainSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("1) Terrain & Textures", EditorStyles.boldLabel);
            rebuildTerrainTextures = EditorGUILayout.Toggle("Rebuild Terrain Textures", rebuildTerrainTextures);
            textureSize = EditorGUILayout.IntPopup("Texture Size", textureSize, new[] {"512", "1024", "2048"}, new[] {512, 1024, 2048});
            alphamapResolution = EditorGUILayout.IntPopup("Alphamap Resolution", alphamapResolution, new[] {"256", "512", "1024"}, new[] {256, 512, 1024});
            detailResolution = EditorGUILayout.IntPopup("Detail Resolution", detailResolution, new[] {"512", "1024", "2048"}, new[] {512, 1024, 2048});
            oldLookStrength = EditorGUILayout.Slider("Old / Aged Look", oldLookStrength, 0f, 1f);
            macroNoiseScale = EditorGUILayout.Slider("Macro Noise Scale", macroNoiseScale, 0.0004f, 0.005f);
            microNoiseScale = EditorGUILayout.Slider("Micro Noise Scale", microNoiseScale, 0.005f, 0.08f);
            roadsideWearWidth = EditorGUILayout.Slider("Roadside Wear Width", roadsideWearWidth, 2f, 25f);
        }

        private void DrawPrefabSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2) Trees & Rocks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("นำ Prefab ต้นไม้และหินใส่เข้ามาได้หลายตัว ระบบจะสุ่มเลือกให้", MessageType.None);

            DrawObjectArray(ref treePrefabs, typeof(GameObject), "Tree Prefabs");
            treeCount = EditorGUILayout.IntField("Tree Count", treeCount);
            treeScaleRange = EditorGUILayout.Vector2Field("Tree Scale Range", treeScaleRange);
            minTreeSpacing = EditorGUILayout.Slider("Min Tree Spacing", minTreeSpacing, 2f, 20f);
            treeSlopeLimit = EditorGUILayout.Slider("Tree Slope Limit", treeSlopeLimit, 0f, 40f);
            treeWaterAvoidDistance = EditorGUILayout.Slider("Tree Water Avoid Distance", treeWaterAvoidDistance, 0f, 15f);

            EditorGUILayout.Space(4);
            DrawObjectArray(ref rockPrefabs, typeof(GameObject), "Rock Prefabs");
            rockCount = EditorGUILayout.IntField("Rock Count", rockCount);
            rockScaleRange = EditorGUILayout.Vector2Field("Rock Scale Range", rockScaleRange);
            rockSlopeMin = EditorGUILayout.Slider("Rock Min Slope", rockSlopeMin, 0f, 40f);
            rockSlopeMax = EditorGUILayout.Slider("Rock Max Slope", rockSlopeMax, 1f, 50f);
        }

        private void DrawGrassSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("3) Grass", EditorStyles.boldLabel);
            grassBillboardTexture = (Texture2D)EditorGUILayout.ObjectField("Grass Billboard (optional)", grassBillboardTexture, typeof(Texture2D), false);
            grassDensity = EditorGUILayout.IntSlider("Grass Density", grassDensity, 1, 32);
            grassHealthy = EditorGUILayout.IntSlider("Healthy Color", grassHealthy, 0, 1);
            grassDry = EditorGUILayout.IntSlider("Dry Color", grassDry, 0, 1);
            grassMinHeight = EditorGUILayout.Slider("Grass Min Height", grassMinHeight, 0.05f, 1.2f);
            grassMaxHeight = EditorGUILayout.Slider("Grass Max Height", grassMaxHeight, 0.1f, 1.6f);
            grassNoiseScale = EditorGUILayout.Slider("Grass Noise Scale", grassNoiseScale, 0.001f, 0.03f);
        }

        private void DrawMasksSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("4) Optional Road / Water Mask", EditorStyles.boldLabel);
            roadParent = (Transform)EditorGUILayout.ObjectField("Road Parent", roadParent, typeof(Transform), true);
            waterParent = (Transform)EditorGUILayout.ObjectField("Water Parent", waterParent, typeof(Transform), true);
            avoidRoadDistance = EditorGUILayout.Slider("Avoid Road Distance", avoidRoadDistance, 0f, 20f);
            avoidWaterDistance = EditorGUILayout.Slider("Avoid Water Distance", avoidWaterDistance, 0f, 20f);
            clearOldGeneratedObjects = EditorGUILayout.Toggle("Clear Old Generated Objects", clearOldGeneratedObjects);
        }

        private void DrawObjectArray(ref GameObject[] array, Type type, string label)
        {
            int size = Mathf.Max(0, EditorGUILayout.IntField(label + " Size", array?.Length ?? 0));
            if (array == null || array.Length != size)
            {
                Array.Resize(ref array, size);
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (GameObject)EditorGUILayout.ObjectField($"{label} [{i}]", array[i], type, false);
            }
            EditorGUI.indentLevel--;
        }

        private void Generate()
        {
            try
            {
                Undo.IncrementCurrentGroup();
                UnityEngine.Random.InitState(seed);

                string root = EnsureGeneratedFolder();
                var td = terrain.terrainData;

                if (rebuildTerrainTextures)
                {
                    td.alphamapResolution = alphamapResolution;
                    td.SetDetailResolution(detailResolution, 16);
                    BuildTerrainLayers(root, td);
                    PaintTerrain(td);
                    CreateGrassDetails(root, td);
                }

                if (clearOldGeneratedObjects)
                {
                    DeleteChildIfExists("_GeneratedTrees");
                    DeleteChildIfExists("_GeneratedRocks");
                }

                ScatterTrees();
                ScatterRocks();

                EditorUtility.SetDirty(td);
                EditorUtility.SetDirty(terrain.gameObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[AyutthayaBiome] Generate complete.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[AyutthayaBiome] Generation failed: " + ex);
            }
        }

        private string EnsureGeneratedFolder()
        {
            const string baseFolder = "Assets/AyutthayaBiome/Generated";
            if (!AssetDatabase.IsValidFolder("Assets/AyutthayaBiome"))
                AssetDatabase.CreateFolder("Assets", "AyutthayaBiome");
            if (!AssetDatabase.IsValidFolder(baseFolder))
                AssetDatabase.CreateFolder("Assets/AyutthayaBiome", "Generated");

            string dated = baseFolder + "/Run_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            AssetDatabase.CreateFolder(baseFolder, Path.GetFileName(dated));
            return dated;
        }

        private void BuildTerrainLayers(string root, TerrainData td)
        {
            List<TerrainLayer> layers = new List<TerrainLayer>();
            layers.Add(CreateTerrainLayer(root, "OldGrass", GenerateGroundTexture(textureSize, new Color(0.34f, 0.42f, 0.22f), new Color(0.28f, 0.33f, 0.16f), 0.08f, 0.22f), GenerateNormalMap(textureSize), new Vector2(7, 7)));
            layers.Add(CreateTerrainLayer(root, "DryGrass", GenerateGroundTexture(textureSize, new Color(0.57f, 0.50f, 0.24f), new Color(0.42f, 0.36f, 0.17f), 0.10f, 0.18f), GenerateNormalMap(textureSize), new Vector2(7, 7)));
            layers.Add(CreateTerrainLayer(root, "Soil", GenerateGroundTexture(textureSize, new Color(0.33f, 0.25f, 0.16f), new Color(0.24f, 0.17f, 0.10f), 0.12f, 0.22f), GenerateNormalMap(textureSize), new Vector2(6, 6)));
            layers.Add(CreateTerrainLayer(root, "WetSoil", GenerateGroundTexture(textureSize, new Color(0.22f, 0.19f, 0.16f), new Color(0.16f, 0.14f, 0.12f), 0.08f, 0.14f), GenerateNormalMap(textureSize), new Vector2(6, 6)));
            layers.Add(CreateTerrainLayer(root, "Rocky", GenerateGroundTexture(textureSize, new Color(0.45f, 0.44f, 0.40f), new Color(0.30f, 0.29f, 0.27f), 0.17f, 0.30f), GenerateNormalMap(textureSize), new Vector2(5, 5)));
            td.terrainLayers = layers.ToArray();
        }

        private TerrainLayer CreateTerrainLayer(string root, string layerName, Texture2D diffuse, Texture2D normal, Vector2 tileSize)
        {
            string diffusePath = root + $"/{layerName}_Albedo.asset";
            string normalPath = root + $"/{layerName}_Normal.asset";
            AssetDatabase.CreateAsset(diffuse, diffusePath);
            AssetDatabase.CreateAsset(normal, normalPath);

            var layer = new TerrainLayer
            {
                diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath),
                normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath),
                tileSize = tileSize,
                tileOffset = Vector2.zero
            };

            string layerPath = root + $"/{layerName}.terrainlayer";
            AssetDatabase.CreateAsset(layer, layerPath);
            return AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
        }

        private Texture2D GenerateGroundTexture(int size, Color baseA, Color baseB, float speckleStrength, float mossStrength)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float n1 = Mathf.PerlinNoise(u * 4.5f + seed * 0.0131f, v * 4.5f + seed * 0.0087f);
                    float n2 = Mathf.PerlinNoise(u * 18f + 23.17f, v * 18f + 91.73f);
                    float n3 = Mathf.PerlinNoise(u * 54f + 11.2f, v * 54f + 15.4f);
                    float cracks = Mathf.PerlinNoise(u * 100f + 7.0f, v * 100f + 19.0f);
                    float moss = Mathf.PerlinNoise(u * 9f + 40f, v * 9f + 16f);

                    Color c = Color.Lerp(baseA, baseB, n1);
                    c *= 0.92f + n2 * speckleStrength;
                    c = Color.Lerp(c, c * 0.7f, Mathf.SmoothStep(0.65f, 1f, cracks) * 0.12f);
                    c = Color.Lerp(c, new Color(c.r * 0.75f, c.g * 1.1f, c.b * 0.78f), moss * mossStrength * oldLookStrength);
                    c = Color.Lerp(c, c * 0.85f, n3 * 0.10f * oldLookStrength);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply(true, false);
            return tex;
        }

        private Texture2D GenerateNormalMap(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float hL = SampleNoiseHeight(u - 1f / size, v);
                    float hR = SampleNoiseHeight(u + 1f / size, v);
                    float hD = SampleNoiseHeight(u, v - 1f / size);
                    float hU = SampleNoiseHeight(u, v + 1f / size);
                    Vector3 n = new Vector3(hL - hR, 0.5f, hD - hU).normalized;
                    Color c = new Color(n.x * 0.5f + 0.5f, n.z * 0.5f + 0.5f, n.y * 0.5f + 0.5f, 1f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply(true, false);
            return tex;
        }

        private float SampleNoiseHeight(float u, float v)
        {
            float n1 = Mathf.PerlinNoise(u * 10f + 5f, v * 10f + 17f);
            float n2 = Mathf.PerlinNoise(u * 36f + 21f, v * 36f + 9f) * 0.35f;
            return n1 + n2;
        }

        private void PaintTerrain(TerrainData td)
        {
            int aw = td.alphamapWidth;
            int ah = td.alphamapHeight;
            int layers = td.terrainLayers.Length;
            float[,,] map = new float[ah, aw, layers];
            var origin = terrain.transform.position;
            var size = td.size;

            for (int y = 0; y < ah; y++)
            {
                for (int x = 0; x < aw; x++)
                {
                    float nx = (float)x / (aw - 1);
                    float ny = (float)y / (ah - 1);
                    float worldX = origin.x + nx * size.x;
                    float worldZ = origin.z + ny * size.z;
                    float normX = worldX / Mathf.Max(1f, size.x);
                    float normZ = worldZ / Mathf.Max(1f, size.z);

                    float height01 = td.GetInterpolatedHeight(nx, ny) / Mathf.Max(1f, td.size.y);
                    float slope01 = td.GetSteepness(nx, ny) / 90f;
                    float macro = Mathf.PerlinNoise(worldX * macroNoiseScale + seed * 0.01f, worldZ * macroNoiseScale + seed * 0.02f);
                    float micro = Mathf.PerlinNoise(worldX * microNoiseScale + 31.8f, worldZ * microNoiseScale + 11.6f);
                    float wetNoise = Mathf.PerlinNoise(worldX * 0.0023f + 77f, worldZ * 0.0023f + 33f);
                    float roadMask = DistanceMaskToChildren(roadParent, new Vector3(worldX, 0f, worldZ), roadsideWearWidth);
                    float waterMask = DistanceMaskToChildren(waterParent, new Vector3(worldX, 0f, worldZ), 10f);

                    float oldGrass = Mathf.Clamp01(0.55f + macro * 0.35f - slope01 * 0.25f);
                    float dryGrass = Mathf.Clamp01(0.30f + (1f - macro) * 0.25f + micro * 0.15f + roadMask * 0.20f);
                    float soil = Mathf.Clamp01(0.15f + slope01 * 0.40f + roadMask * 0.85f + (1f - micro) * 0.10f);
                    float wetSoil = Mathf.Clamp01((1f - height01) * 0.35f + waterMask * 0.80f + wetNoise * 0.15f);
                    float rocky = Mathf.Clamp01(slope01 * 0.75f + Mathf.SmoothStep(0.62f, 1f, micro) * 0.20f);

                    // Age / neglected look
                    soil += oldLookStrength * 0.10f;
                    dryGrass += oldLookStrength * 0.08f;
                    oldGrass -= oldLookStrength * 0.04f;

                    float total = oldGrass + dryGrass + soil + wetSoil + rocky;
                    if (total <= 0.0001f) total = 1f;

                    map[y, x, 0] = oldGrass / total;
                    map[y, x, 1] = dryGrass / total;
                    map[y, x, 2] = soil / total;
                    map[y, x, 3] = wetSoil / total;
                    map[y, x, 4] = rocky / total;
                }
            }

            td.SetAlphamaps(0, 0, map);
        }

        private void CreateGrassDetails(string root, TerrainData td)
        {
            Texture2D grassTex = grassBillboardTexture != null ? grassBillboardTexture : GenerateGrassBillboard(256);
            if (grassBillboardTexture == null)
            {
                AssetDatabase.CreateAsset(grassTex, root + "/GrassBillboard.asset");
                grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(root + "/GrassBillboard.asset");
            }

            DetailPrototype prototype = new DetailPrototype
            {
                prototypeTexture = grassTex,
                renderMode = DetailRenderMode.GrassBillboard,
                minWidth = grassMinHeight * 0.30f,
                maxWidth = grassMaxHeight * 0.38f,
                minHeight = grassMinHeight,
                maxHeight = grassMaxHeight,
                healthyColor = grassHealthy == 1 ? new Color(0.38f, 0.55f, 0.22f) : new Color(0.50f, 0.58f, 0.28f),
                dryColor = grassDry == 1 ? new Color(0.55f, 0.49f, 0.24f) : new Color(0.49f, 0.44f, 0.19f),
                noiseSpread = 0.2f,
                usePrototypeMesh = false
            };

            td.detailPrototypes = new[] { prototype };
            int res = td.detailResolution;
            int[,] layer = new int[res, res];
            var size = td.size;
            var origin = terrain.transform.position;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float nx = (float)x / (res - 1);
                    float ny = (float)y / (res - 1);
                    float worldX = origin.x + nx * size.x;
                    float worldZ = origin.z + ny * size.z;
                    float slope = td.GetSteepness(nx, ny);
                    float noise = Mathf.PerlinNoise(worldX * grassNoiseScale + 17.3f, worldZ * grassNoiseScale + 83.9f);
                    float roadMask = DistanceMaskToChildren(roadParent, new Vector3(worldX, 0f, worldZ), avoidRoadDistance + 1.0f);
                    float waterMask = DistanceMaskToChildren(waterParent, new Vector3(worldX, 0f, worldZ), avoidWaterDistance + 0.5f);

                    if (slope < 32f && roadMask < 0.45f && waterMask < 0.6f)
                    {
                        float density = Mathf.Clamp01(noise * 1.15f - slope / 70f);
                        layer[y, x] = Mathf.RoundToInt(density * grassDensity);
                    }
                    else
                    {
                        layer[y, x] = 0;
                    }
                }
            }

            td.SetDetailLayer(0, 0, 0, layer);
        }

        private Texture2D GenerateGrassBillboard(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

            int blades = 10;
            for (int b = 0; b < blades; b++)
            {
                float center = Mathf.Lerp(0.2f, 0.8f, b / (float)(blades - 1)) + UnityEngine.Random.Range(-0.04f, 0.04f);
                float bend = UnityEngine.Random.Range(-0.1f, 0.1f);
                float width = UnityEngine.Random.Range(2f, 5f);
                float height = UnityEngine.Random.Range(size * 0.45f, size * 0.9f);
                Color bladeColor = Color.Lerp(new Color(0.36f, 0.52f, 0.22f), new Color(0.61f, 0.57f, 0.30f), UnityEngine.Random.value * oldLookStrength);

                for (int yy = 0; yy < height; yy++)
                {
                    float t = yy / height;
                    float xPos = center + Mathf.Sin(t * 2.8f) * bend * (1f - t);
                    int px = Mathf.RoundToInt(xPos * size);
                    int half = Mathf.RoundToInt(width * (1f - t) + 1f);
                    for (int xx = -half; xx <= half; xx++)
                    {
                        int sx = px + xx;
                        int sy = yy;
                        if (sx < 0 || sx >= size || sy < 0 || sy >= size) continue;
                        float alpha = Mathf.Clamp01(1f - Mathf.Abs(xx) / (float)(half + 1));
                        tex.SetPixel(sx, sy, new Color(bladeColor.r, bladeColor.g, bladeColor.b, alpha));
                    }
                }
            }

            tex.Apply(true, false);
            return tex;
        }

        private void ScatterTrees()
        {
            if (treePrefabs == null || treePrefabs.Length == 0) return;
            Transform parent = GetOrCreateChild("_GeneratedTrees");
            TerrainData td = terrain.terrainData;
            List<Vector3> accepted = new List<Vector3>();

            int spawned = 0;
            int attempts = treeCount * 25;
            for (int i = 0; i < attempts && spawned < treeCount; i++)
            {
                Vector3 pos = SampleTerrainPosition(td);
                float slope = td.GetSteepness(NormX(pos), NormZ(pos));
                if (slope > treeSlopeLimit) continue;
                if (DistanceMaskToChildren(roadParent, pos, avoidRoadDistance) > 0.5f) continue;
                if (DistanceMaskToChildren(waterParent, pos, treeWaterAvoidDistance) > 0.5f) continue;
                if (!IsFarEnough(pos, accepted, minTreeSpacing)) continue;

                GameObject prefab = treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length)];
                if (prefab == null) continue;

                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(go, "Scatter Tree");
                float scale = UnityEngine.Random.Range(treeScaleRange.x, treeScaleRange.y);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);
                go.transform.localScale = Vector3.one * scale;

                accepted.Add(pos);
                spawned++;
            }

            Debug.Log($"[AyutthayaBiome] Spawned trees: {spawned}/{treeCount}");
        }

        private void ScatterRocks()
        {
            if (rockPrefabs == null || rockPrefabs.Length == 0) return;
            Transform parent = GetOrCreateChild("_GeneratedRocks");
            TerrainData td = terrain.terrainData;

            int spawned = 0;
            int attempts = rockCount * 15;
            for (int i = 0; i < attempts && spawned < rockCount; i++)
            {
                Vector3 pos = SampleTerrainPosition(td);
                float nx = NormX(pos);
                float nz = NormZ(pos);
                float slope = td.GetSteepness(nx, nz);
                if (slope < rockSlopeMin || slope > rockSlopeMax) continue;
                if (DistanceMaskToChildren(roadParent, pos, avoidRoadDistance) > 0.5f) continue;
                if (DistanceMaskToChildren(waterParent, pos, avoidWaterDistance) > 0.6f) continue;

                GameObject prefab = rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Length)];
                if (prefab == null) continue;

                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(go, "Scatter Rock");
                float scale = UnityEngine.Random.Range(rockScaleRange.x, rockScaleRange.y);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(-8f, 8f));
                go.transform.localScale = Vector3.one * scale;
                spawned++;
            }

            Debug.Log($"[AyutthayaBiome] Spawned rocks: {spawned}/{rockCount}");
        }

        private Vector3 SampleTerrainPosition(TerrainData td)
        {
            Vector3 origin = terrain.transform.position;
            float x = UnityEngine.Random.Range(0f, td.size.x);
            float z = UnityEngine.Random.Range(0f, td.size.z);
            float y = terrain.SampleHeight(new Vector3(origin.x + x, 0f, origin.z + z)) + origin.y;
            return new Vector3(origin.x + x, y, origin.z + z);
        }

        private float NormX(Vector3 pos) => Mathf.InverseLerp(terrain.transform.position.x, terrain.transform.position.x + terrain.terrainData.size.x, pos.x);
        private float NormZ(Vector3 pos) => Mathf.InverseLerp(terrain.transform.position.z, terrain.transform.position.z + terrain.terrainData.size.z, pos.z);

        private bool IsFarEnough(Vector3 candidate, List<Vector3> accepted, float minDistance)
        {
            float sqr = minDistance * minDistance;
            for (int i = 0; i < accepted.Count; i++)
            {
                if ((accepted[i] - candidate).sqrMagnitude < sqr)
                    return false;
            }
            return true;
        }

        private Transform GetOrCreateChild(string name)
        {
            Transform t = terrain.transform.Find(name);
            if (t != null) return t;
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create generated parent");
            go.transform.SetParent(terrain.transform);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        private void DeleteChildIfExists(string name)
        {
            Transform t = terrain.transform.Find(name);
            if (t != null)
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        private float DistanceMaskToChildren(Transform root, Vector3 worldPos, float radius)
        {
            if (root == null || radius <= 0.01f) return 0f;
            float nearest = float.MaxValue;
            bool found = false;
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                if (child == root) continue;
                Vector3 p = child.position;
                float d = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(p.x, p.z));
                if (d < nearest)
                {
                    nearest = d;
                    found = true;
                }
            }
            if (!found) return 0f;
            return 1f - Mathf.Clamp01(nearest / radius);
        }
    }
}

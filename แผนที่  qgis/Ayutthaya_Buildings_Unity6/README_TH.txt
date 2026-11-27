Unity 6 Building Generator V1 (Built-in / URP)
Copy Assets into existing project. Tools > AYUTTHAYA > Building Generator V1.
Generate prefabs only creates six variants. Generate + Scatter requires existing Terrain.
For first test choose player, local test mode, radius 100, count 5, texture 512.
Avoid layers: road/water/existing building colliders only; DO NOT include Terrain layer.
Add BuildingDebug to existing player; assign camera. Play: aim at door and E, aim at wall and K.
Existing damage integration: hit.collider.GetComponentInParent<AyutthayaBuildings.BuildingPart>()?.TakeDamage(25f);
Each panel has health and collider. Roof collapses after threshold of supports; debris expires after 12 seconds. Not engineering simulation.
No new scene or player; no DEM edits. Scatter uses random sites, not OSM building footprints. Six variants are single-storey shells with rooms, no windows or furniture. Beveled geometry and procedural maps, not AAA photogrammetry. No LOD; use small counts initially. Heightmap not flattened, possible gaps on uneven ground. HDRP unsupported. Unity compilation/runtime not tested here.

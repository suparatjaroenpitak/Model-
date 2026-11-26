THAI SHOPHOUSE V5 — MESH FIRST
Unity 6 | URP/Lit or Built-in Standard | no FBX, Python, Blender or third-party runtime required.

1. แตก ZIP แล้วคัดลอกโฟลเดอร์ Assets/ThaiShophouseV5 ไปยัง Assets ของโปรเจกต์ Unity โดยไม่สร้าง Assets ซ้อนกัน
2. ตรวจ Console ให้ไม่มี Compile Error จากสคริปต์อื่น
3. Tools > AYUTTHAYA > Generate Shophouse V5 Clay เพื่อดู Geometry โดยไม่มี texture ก่อน
4. เลือก root ใน Hierarchy กด F; Tools > AYUTTHAYA > Shophouse V5 Force LOD0 (Selected) เพื่อดู LOD0
5. เมื่อพอใจรูปทรง ให้ปิดรถ/อาคาร Clay ใน Hierarchy แล้วใช้ Tools > AYUTTHAYA > Generate Shophouse V5 PBR
6. ผลลัพธ์ใน Assets/GeneratedThaiShophouseV5/PBR/Prefabs (อาจเป็น PBR 1 ฯลฯ เพื่อไม่เขียนทับของเดิม)
   ThaiShophouse_Row.prefab คือทั้งฉาก; มี prefab อาคารแต่ละคูหาและถนนแยกกัน
7. ใช้ Restore Automatic LOD (Selected) เมื่อเสร็จการตรวจรูปทรง

MeshData: binary little-endian SHP5 header, int32 vertex count, int32 index count,
float32 positions xyz, float32 normals xyz, float32 UV xy, int32 indices.
28 files = 7 modules × 4 LOD levels. All geometry exists before Unity import.
1 unit = 1 metre. 6 units, width 26.9m, depth 12m, heights approx 11.9–13.8m + roof/parapet. Assumed dimensions, not surveyed.

Texture atlas: 4096 x 4096, 8x8 grid, 32 used cells, 16px padding per cell.
BaseColor includes selected crops from source image, not original 4K details. Normal/Height derived from color luminance (approximation); roughness/metallic estimated; AO neutral white (not baked).
Atlas UV overlaps intentionally for reusable materials; no individual unique UV unwrap.
Height is included for editing, not wired to shader. Glass is opaque/tinted; no transparent interior.
Collision proxies block building volumes; intended as exterior scenery. No walkable interiors.
References are included for editing, not runtime textures. Screenshots are software rasterizer previews of actual geometry, not Unity renders or PBR lighting.

Validation: binary file length, index bounds, UV ranges, finite positions/normals, PNG size and zip integrity checked outside Unity. Unity compilation and rendering have not been tested. The model is a stylized-to-realistic exterior reconstruction, not a certified AAA model or exact survey.

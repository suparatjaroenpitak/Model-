# Fortuner Ultra Procedural Asset for Unity 6

This package creates a modular, procedural Fortuner-like SUV from the dimensions and visual references supplied by the user.

## Important limitation

The supplied images are presentation sheets: they contain a wireframe illustration, an example UV layout, and a PBR atlas. They do not contain the original vertex/index buffer or the original mesh-to-UV correspondence. Therefore the generator creates a new 0-1 UV layout and a compatible procedural approximation. It cannot reproduce the stated 254k/312k triangle production mesh exactly from images alone.

The PBR extraction tool crops the map panels from Image #2. It preserves the available source resolution; it does not create true 4096x4096 detail from a smaller board. For a production asset, replace the extracted maps with original 4K maps that were baked from the final mesh.

## Installation

1. Copy `Assets/FortunerUltra` into a Unity 6 project.
2. Put Image #2 in the project, for example `Assets/FortunerUltra/Textures/FortunerReferenceBoard.png`.
3. Open `Tools > AYUTTHAYA > Generate Fortuner Ultra PBR`.
4. Assign the board and press `Extract PBR PNGs from reference board`.
5. Press `Generate procedural Fortuner in current scene`.
6. Save the generated prefab under `Assets/FortunerUltra/Generated/Prefabs`.

This workspace already contains the two supplied reference sheets and deterministic crops in `Assets/FortunerUltra/Textures`. The Editor tool asks before overwriting existing extracted maps.

## วิธีใช้งานภาษาไทย

1. นำโฟลเดอร์ `Assets/FortunerUltra` ไปวางในโปรเจกต์ Unity 6 ที่ใช้ URP หรือ Built-in.
2. เปิด `Tools > AYUTTHAYA > Generate Fortuner Ultra PBR`.
3. ลาก `Fortuner_PBRReferenceBoard.png` ใส่ช่อง Reference texture board.
4. กด Extract เพื่อสร้าง PNG แผนที่วัสดุ และกด Generate เพื่อสร้างรถใน Scene.
5. เปิด Generate collision proxies และ Generate WheelColliders เมื่อจะนำไปต่อกับระบบรถ.
6. ตรวจ Console และบันทึก Prefab จากโฟลเดอร์ Generated.

สคริปต์จะถามก่อนเขียนทับ texture เดิม และแยก Runtime/Editor ให้ชัดเจน จึงไม่ต้องใส่ไฟล์ Editor ลงใน build player.

## Generated maps

- `Fortuner_BaseColor.png`: sRGB color map.
- `Fortuner_Normal.png`: Unity tangent-space normal map.
- `Fortuner_Roughness.png`: linear roughness source.
- `Fortuner_Metallic.png`: linear metallic source.
- `Fortuner_AO.png`: linear ambient occlusion source.
- `Fortuner_MetallicSmoothness.png`: Unity packed map, RGB = metallic and A = `1 - roughness`.
- `Fortuner_UVReference.png`: UV layout preview from the supplied board.

The default crop rectangles are normalized to Image #2. If the board is re-cropped, adjust the rectangles in the Editor window.

## Runtime usage

Add `FortunerProceduralBuilder` to an empty GameObject and use the component context menu `Generate Fortuner Ultra`. The public `Configure` method is provided for editor tooling and build pipelines.

The builder creates named modular parts including body shell, chassis, glazing, pillars, bumpers, grille, lights, mirrors, handles, interior hint geometry, tires, alloy rims, brake discs, brake calipers, collision proxies, and optional WheelColliders.

## PBR notes

The material uses URP/Lit when available and falls back to Built-in/Standard. Unity's metallic workflow expects smoothness, not roughness, so the editor tool packs `smoothness = 1 - roughness` into the alpha channel of `Fortuner_MetallicSmoothness.png`.

## Validation status

The source was compiled with Unity 6.0.6f1 in a temporary validation project. A generator smoke test produced 88 mesh filters, 88 renderers, 4,118 triangles, and UVs within 0-1. This verifies script execution and data contracts, not visual parity with the unavailable original production mesh. Treat scene lighting, material tuning, collider behavior, and human visual review as required final validation.

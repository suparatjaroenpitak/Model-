UNITY 6 CLASSIC BANK KIT (C#)
1. Copy Assets/BankKit into your existing project's Assets folder. No scene is supplied or created.
2. Tools > Bank Kit > Generate Bank. Start at 1024 texture size, 64 segments. Generate model prefab.
3. Drag Assets/GeneratedBank/Classic_Bank.prefab into your existing scene. Keep scale (1,1,1).
4. Tag your existing player Player, or assign its Transform to BankDoor.player. Automatic door opens in Play Mode. For manual use disable automatic and call Interact() from your own interaction system.
5. Damage: call hit.collider.GetComponentInParent<BankKit.BankBreakable>()?.ApplyDamage(100, hit.point, 3f) from your existing game hit logic. BankDemoDamage.Hit() is an optional camera raycast method; connect it to your existing input or UI. It does not bind any input itself.
6. Walls are segmented into masonry pieces; each panel detaches its pieces as rigidbodies when health is exhausted. Debris expires after 20 seconds. Roof and structural floors do not automatically collapse; no structural support solver. No actual physics validation performed.
7. Meshes have UV0, normals and tangents. Textures are procedural, not extracted from the supplied reference sheets. No UV atlas or lightmap UV2; per-piece UVs. PBR height and roughness PNGs are exported; height is not applied as displacement. Glass uses approximate shader transparency.
8. Generator writes actual instance triangle count into MeshStatistics.txt. No LOD is generated. Many brick/tile objects may be expensive; use the brick toggle to reduce wall parts. 4K textures are memory intensive; 1K recommended first.
9. To regenerate, rename or remove Assets/GeneratedBank yourself. Generator refuses to overwrite existing assets. A failed generation may leave partial assets.
10. Reference chosen: two-storey classical facade with red brick, stone trim, curved windows, columns, roof, and interpreted lobby/stair/office. Simplified gable roof and blank sign; no exact emblem, furniture set, vault or rear window detail. Not compiled/tested in Unity in this environment.

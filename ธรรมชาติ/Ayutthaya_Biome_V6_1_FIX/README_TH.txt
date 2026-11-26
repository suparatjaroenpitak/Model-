AYUTTHAYA BIOME AUTO PREFABS – Unity 6
แทนที่ Assets/AyutthayaBiome/Editor/AyutthayaBiomeScatterWindow.cs เดิม (อย่าเก็บคลาสซ้ำ)
คัดลอก Assets จาก ZIP เข้าโปรเจกต์ แล้วเปิด Tools > AYUTTHAYA > Biome Scatter Generator
เลือก Terrain เดิม; เปิด Auto generate tree and rock prefabs; ตั้ง Seed, จำนวนต้นไม้/หิน, Grass Density, Texture Size; เลือก Road Parent / Water Parent ถ้ามี แล้วกด Generate
สร้างต้นไม้ 4 แบบ (กิ่ง ราก ใบแยกชิ้น LOD 3 ระดับ), หิน 6 แบบ, วัสดุ PBR พร้อม PNG และ grass billboard อัตโนมัติ
Generated assets อยู่ใน Assets/AyutthayaBiome/Generated/Run_...; ต้นไม้และหินที่สุ่มเป็นลูกของ Terrain
สำรอง TerrainData ก่อน เพราะระบบแทนที่ terrain layers และ grass detail layers เดิม; ไม่แก้ heightmap หรือ Player
แนะนำเริ่ม 1024 texture, 3000 leaves, 500 trees, 200 rocks, detail resolution 512, grass density 8
ข้อจำกัด: การหลบถนน/น้ำใช้กรอบ Renderer XZ อาจกันพื้นที่มากเกินไป โดยเฉพาะ mesh chunks ขนาดใหญ่; ไม่หลบอาคารโดยอัตโนมัติ; ไม่สร้างน้ำใหม่; ไม่ใช่ photogrammetry; ไม่ได้ compile/run ใน Unity จริง; สำหรับ URP/Built-in ไม่รับรอง HDRP. หินไม่มี LOD; ใบไม้สองด้านใช้ geometry เพิ่ม; ต้นไม้เป็น GameObject ไม่ใช่ Terrain Tree Instances.

FORTUNER ULTRA V2 — UNITY 6

ติดตั้ง
1. แตก ZIP แล้วนำ Assets/FortunerUltraV2 ไปใส่ใน Assets ของ Unity Project (อย่าสร้าง Assets ซ้อน)
2. ใช้ URP หรือ Built-in; รอ Unity Import/Compile ให้เสร็จ
3. Tools > AYUTTHAYA > Generate Fortuner Ultra V2 PBR (ไม่ต้องกด Play)
4. หาก Texture ไม่ครบ เลือกโฟลเดอร์ FortunerUltraV2/Textures จาก ZIP ที่แตกแล้ว ยืนยันการคัดลอก
5. เลือกรถใน Hierarchy แล้วกด F; Prefab อยู่ใน Assets/GeneratedFortunerUltraV2/Fortuner/ หรือชื่อโฟลเดอร์ต่อท้ายเลข
6. ตรวจ GenerationReport.txt และ Console สำหรับ Triangle จริงและขนาด Renderer bounds
7. เปิด Linear Color Space และใช้ Reflection Probe/Skybox; ตรวจ platform texture overrides
8. หากมี CS0111 จาก IndustrialEstateGenerator.cs ต้องแก้ก่อน เพราะ Unity compile สคริปต์ทั้งโปรเจกต์

การปรับปรุง V2
- สร้าง LOD0–LOD3 จากสมการจริง โดยลดจำนวน subdivisions และรวม mesh ตามวัสดุใน LOD ที่ต่ำลง
- เพิ่มวงแหวนตรา Toyota แบบ mesh, ขอบโครเมียมไฟหน้า, เลนส์ projector/fog แบบ geometry, แถบสะท้อนท้าย, ที่ปัดน้ำฝนท้าย, ฐานราวหลังคาและพื้นกันลื่นบันได
- เพิ่มความละเอียด LOD0 และตรวจ mesh UV/indices/normal/tangent channel; ลบ triangle พื้นที่ใกล้ศูนย์ก่อนบันทึก
- ปรับ scale ระดับ LOD ให้ใกล้สัดส่วนรถในภาพ (เป็นการปรับโดยประมาณ ไม่ใช่ค่าขนาดจริงที่วัดจาก CAD; รวมกระจกแล้วกว้างขึ้น)
- คงล้อแยก pivot และ static colliders; ไม่ใช่ระบบขับขี่/ประตูเปิดปิด
- Atlas 4K ทั้ง 6 แผ่นคงจาก Ultra V1 เพราะภาพ PBR ใบที่สองตรงกับแหล่งเดิม; tile UV 4x4 และ padding เดิม ใช้พื้นที่ซ้ำโดยตั้งใจ ไม่ใช่ UV unwrap แบบไม่ซ้อน
- แผ่น AO สีขาว neutral; PBR/Normal ประมาณจากภาพและ procedural ไม่ได้ bake จาก mesh; ภาพต้นทาง 1536x1024 ไม่สามารถกู้รายละเอียด 4K แท้ได้
- ภาพอ้างอิงระบุ 312864/168412/72640/28396 tris เป็นเป้าหมายอ้างอิง ไม่ใช่จำนวนจริงของงานนี้; Unity รายงานจำนวนจริงเมื่อสร้าง
- กระจกสีเข้มทึบและไฟไม่มี optical transmission หรือ emission ที่ทำงานจริง; ภายในพื้นฐาน
- ตรวจ ZIP/PNG และตรวจโครงสร้างชื่อเมธอด static เบื้องต้นแล้ว ยังไม่ได้ compile หรือ render ใน Unity จริง และไม่ได้ยืนยัน likeness หรือ topology ที่เหมือนต้นฉบับ

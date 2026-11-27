V2 FIX — Unity 6 URP / Built-in
สำรองโปรเจกต์ แล้วแทนที่ไฟล์ C# เดิมทั้ง 5 ไฟล์ (อย่าวางซ้ำต่างโฟลเดอร์)
ออกจาก Play Mode > Tools > AYUTTHAYA > Building Generator V2 FIX
เลือก Terrain จาก Hierarchy (ระบบเลือก active terrain ให้ถ้ามี)
Quick test: create one building — ไม่ใช้ Avoid mask; อาจทับถนน ใช้ทดสอบเท่านั้น
Generate + Scatter: ตั้ง Local test mode OFF เพื่อสุ่มทั่ว Terrain ที่เลือก
Avoid layers: ถนน/น้ำต้องแยก Layer จาก Terrain และมี Collider
Player: Add Component BuildingDebug, ใส่ Main Camera, Play, เล็งประตู E, เล็งผนัง K
Damage API: BuildingPart.TakeDamage(float); ทั้งอาคารใช้ BuildingStructure.DamageWholeBuilding(float)
PBR แบบ procedural, 6 แบบอาคารชั้นเดียว, ไม่ใช่ AAA ที่ผ่านการ render
ยังไม่ได้ Compile/Play ใน Unity จริง; หากมี error ให้ส่ง Console error แรกพร้อมไฟล์และบรรทัด

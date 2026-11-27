Texture + Material สำหรับ rider-on-motorcycle-v4.fbx
=====================================================

วิธีติดตั้ง
1. แตก ZIP แล้วคัดลอกโฟลเดอร์ RiderMotorcycleUnity ทั้งโฟลเดอร์ไปไว้ใน Assets ของโปรเจกต์ Unity ของคุณ
   เก็บไฟล์ .meta ที่อยู่ข้าง Texture และ Material ไว้ด้วย เพื่อให้ Material ยังอ้างอิง Texture ได้
2. ในหน้าต่าง Project ของ Unity เลือกไฟล์ rider-on-motorcycle-v4.fbx ที่นำเข้าไว้แล้ว
3. เลือก Tools > Meshy Rider > Apply Materials to Selected FBX
   คำสั่งนี้เลือก Shader ให้ตรงกับ Render Pipeline ที่ใช้อยู่ แล้วผูก Material ให้รถ ล้อทั้งสอง และคนขี่ โดยไม่แก้ไฟล์ FBX หรือการตั้งค่า Animation

ไฟล์ในแพ็ก
- Materials/Motorcycle.mat: ใช้กับตัวรถ ล้อหน้า และล้อหลัง
- Materials/Rider.mat: ใช้กับคนขี่
- Textures: ภาพสี (BaseColor), Normal Map และ Metallic/Smoothness สำหรับรถและคนขี่ รวม 6 ไฟล์
- Editor: เมนูช่วยตั้ง Material และเชื่อมช่อง Material ของ FBX

การใช้โดยไม่ผูกกับ FBX อัตโนมัติ
- ไฟล์ .mat ที่ติดมาใช้ Standard ของ Built-in เป็นค่าเริ่มต้น หากใช้ URP หรือ HDRP ให้รัน Tools > Meshy Rider > Set Up Materials for Current Pipeline ก่อนใช้ Material
- กำหนด Motorcycle.mat ให้กับ Motorcycle, Front wheel, Rear wheel และกำหนด Rider.mat ให้กับ Rider
- ชุด MetallicSmoothness ใช้ R = Metallic, A = Smoothness, G = AO เป็นกลาง และ B = Detail Mask เป็นกลาง โดยไม่เพิ่มรายละเอียด AO ที่ไม่มีในต้นฉบับ

ทดสอบใน Unity 6000.6.5f1 แบบ Built-in แล้ว: ทั้ง 4 ชิ้นอ้างอิง Material และ Texture ภายนอกครบ และคลิปเดิมยาว 2 วินาที การตั้งค่า URP/HDRP มีในเมนู แต่ยังไม่ได้ทดสอบในโปรเจกต์จริงที่ใช้สอง Pipeline นี้

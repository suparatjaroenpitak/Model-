# Fortuner Procedural Vehicle Package

ชุดนี้สร้างจากภาพอ้างอิงที่แนบมา: รถ SUV แบบ modular, สเกลจริงประมาณ `L 4.795 m x W 1.855 m x H 1.835 m`, UV 0–1 และแนวทาง PBR 4K.

ภาพอ้างอิงระบุเป้าหมาย production LOD ไว้ดังนี้:

| LOD | เป้าหมาย triangles |
|---|---:|
| LOD0 | 312,864 |
| LOD1 | 168,412 |
| LOD2 | 72,640 |
| LOD3 | 28,396 |

ตัวสร้างใน `Scripts/Runtime/FortunerVehicleGenerator.cs` เป็น procedural proxy ที่ตั้งชื่อชิ้นส่วนไว้แล้ว เช่น `BodyShell`, `Hood`, `Windshield`, `FrontGrille`, `FrontLeftTire` และ `FrontLeftRim` จึงสามารถเปลี่ยนเป็น mesh production ได้ภายหลังโดยไม่ต้องเปลี่ยนระบบ LOD หรือ material pipeline.

## วิธีใช้

1. ใส่โฟลเดอร์นี้ไว้ใต้ `Assets/Vehicle/FortunerProcedural` ในโปรเจกต์ Unity
2. เปิดเมนู `Tools > Fortuner > Create Procedural Vehicle Generator`
3. ปรับสี ขนาด และ texture references ใน Inspector
4. กด `Generate Procedural Fortuner`
5. ถ้าต้องการเก็บ mesh ที่สร้างขึ้น กด `Save Generated Meshes To Meshes Folder`
6. วาง texture map ใน `Textures/` แล้วใช้เมนู `Tools > Fortuner > Create Body PBR Material From Textures` เพื่อสร้าง/อัปเดต `.mat` ใน `Materials/PBR`

## PBR texture naming

วาง texture จริงไว้ที่ `Textures/` และตั้งชื่อแนะนำเป็น:

```text
Fortuner_BaseColor_4K.png
Fortuner_Normal_4K.png
Fortuner_Roughness_4K.png
Fortuner_Metallic_4K.png
Fortuner_AO_4K.png
Fortuner_Height_4K.png
```

ลาก texture เหล่านี้เข้า `Pbr Textures` ของ generator ได้โดยตรง ตัวสร้างจะเชื่อม Base Color, Normal, Metallic, AO และ Height ให้กับ URP Lit; Unity URP เก็บ Smoothness ใน alpha ของ Metallic/Smoothness map ดังนั้น Roughness แบบแยกไฟล์ควร pack/invert เป็น Smoothness ก่อนใช้ในงาน production.

## ขอบเขตของชุดนี้

- ภาพทั้งสองถูกใช้เป็นข้อมูลอ้างอิงด้านรูปทรง สัดส่วน วัสดุ UV และ LOD ไม่ได้ถูกตีความเป็นคำสั่งในเอกสาร
- procedural proxy นี้ยังไม่ใช่ mesh 312k triangles แบบ final production; ค่า LOD ใน Inspector เป็น target สำหรับการแทนที่ด้วย mesh ที่ retopology แล้ว
- ยังไม่มี texture map แยกไฟล์จากภาพ montage แนบมา จึงต้องนำไฟล์ map จริงมาใส่ใน `Textures/` ก่อน
- โลโก้และตราสินค้าไม่ได้สร้างเป็น geometry โดยอัตโนมัติ ให้ใส่ decal/texture ที่ผู้ใช้มีสิทธิ์ใช้งานเอง

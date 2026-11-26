# Toyota Fortuner — Unity Game-Ready Asset Setup

ชุดนี้สร้างจาก reference sheet ที่แนบมา โดยเตรียม Unity integration สำหรับโมเดล Toyota Fortuner แบบ modular/game-ready

## สเปกที่ใช้

- ขนาดอ้างอิง: ยาว 4.795 m, กว้าง 1.855 m, สูง 1.835 m
- แกน Unity ที่คาดหวัง: X = ความยาว, Y = ความสูง, Z = ความกว้าง
- UV: single UV 0–1, non-overlap
- Material: PBR — Base Color, Normal, Roughness, Metallic, AO และ Detail/Decal แบบ alpha
- LOD target จาก wireframe sheet:
  - LOD0: 312,864 tris / 169,845 verts
  - LOD1: 168,412 tris / 91,205 verts
  - LOD2: 72,640 tris / 40,518 verts
  - LOD3: 28,396 tris / 15,637 verts
- Format ที่รองรับ: FBX หรือ GLB ที่ import เข้า Unity ได้

> หมายเหตุ: reference sheet แรกแสดงตัวเลข LOD อีกชุดหนึ่ง จึงใช้ตัวเลขจาก sheet wireframe เป็นค่าหลักใน Asset Profile และใช้ transition height เป็นค่าปรับแต่งใน Unity

## โครงสร้างโฟลเดอร์

```text
Assets/Vehicle/ToyotaFortuner/
├── Runtime/              # MonoBehaviour และ ScriptableObject
├── Editor/               # เครื่องมือสร้าง material, pack texture และติดตั้ง setup
├── Models/               # FBX/GLB ต้นฉบับหรือไฟล์ import
├── Meshes/               # mesh ที่แยก LOD0–LOD3 หรือชิ้นส่วน modular
├── Textures/
│   ├── PBR/              # BaseColor, Normal, Roughness, Metallic, AO
│   ├── DetailDecal/      # PNG decal และ alpha
│   └── Generated/        # MetallicSmoothness ที่ pack สำหรับ Lit/URP
├── Materials/            # Unity .mat และ Asset Profile
└── Prefabs/              # prefab ที่ setup แล้ว
```

## ชื่อไฟล์ texture ที่แนะนำ

```text
ToyotaFortuner_BaseColor_4K.png
ToyotaFortuner_Normal_4K.png
ToyotaFortuner_Roughness_4K.png
ToyotaFortuner_Metallic_4K.png
ToyotaFortuner_AO_4K.png
ToyotaFortuner_DetailDecal_4K.png
```

## วิธีใช้ใน Unity

1. วาง FBX/GLB ไว้ใน `Models/` และ mesh LOD ที่แยกแล้วไว้ใน `Meshes/` โดยตั้งชื่อกลุ่มเป็น `LOD0`, `LOD1`, `LOD2`, `LOD3`
2. วาง texture 4K ใน `Textures/PBR/`
3. เปิดเมนู `Tools > Toyota Fortuner > Create Folder Structure`
4. หากใช้ URP/Built-in Lit ให้เรียก `Pack Metallic + Roughness For URP` เพื่อแปลง Metallic เป็น RGB และ Roughness เป็น Smoothness ใน Alpha
5. เรียก `Create PBR Material Set`
6. เลือก root ของโมเดล แล้วเรียก `Add Runtime Setup To Selected`
7. เปิด Asset Profile แล้วลาก material ที่สร้างไปยังช่อง Body Paint, Glass, Rubber, Chrome, Lights, Interior และ Decal
8. กด `Apply Toyota Fortuner Setup` บน component เพื่อปรับ scale, จับคู่ material และสร้าง LODGroup

## การจับคู่ชิ้นส่วนแบบ modular

ชื่อชิ้นส่วนควรมีคำสำคัญ เช่น `Body`, `Glass`, `Tire`, `Chrome`, `Grille`, `Headlight`, `TailLight`, `Interior`, `Seat`, `Decal` หรือ `Badge` เพื่อให้ runtime script เลือก material ให้โดยอัตโนมัติ

## ข้อจำกัดของชุดนี้

ภาพที่แนบมาเป็นข้อมูลอ้างอิงและ texture layout ไม่ใช่ไฟล์ FBX/GLB หรือ texture map ที่ import ได้จริง ดังนั้น script นี้ทำหน้าที่เป็น asset configurator/import pipeline เมื่อใส่ mesh และ texture จริงแล้ว หากต้องการสร้าง geometry จริงจากภาพ ต้องมีไฟล์ mesh หรือใช้ขั้นตอนสร้าง 3D แยกต่างหากก่อนนำเข้า Unity

AYUTTHAYA SIMULATOR — Interactive Destructible Shophouse V2
Unity 6 / URP or Built-in / C# / Binary Mesh / Windows keyboard & mouse

ติดตั้ง
1. แตก ZIP และนำ Assets/AyutthayaShophouseV2 ไปวางใน Assets ของโปรเจกต์เดิมทั้งโฟลเดอร์ (อย่าให้ Assets ซ้อน Assets)
2. รอ Import และ Compile ให้เสร็จ ถ้า Console มี Error จาก Script รุ่นอื่น ให้แก้ Error นั้นก่อน
3. เมนู Tools > AYUTTHAYA > Shophouse V2 > 1 Create Clay Prefab สร้าง Prefab สีเทาเพื่อตรวจรูปทรง (ลากจาก Project ลง Scene ได้)
4. เมนู 2 Create PBR Prefabs สร้าง Prefab ตัวอาคารพร้อมวัสดุ และตัวละครทดสอบแยกต่างหาก ไม่เปลี่ยน Scene ปัจจุบัน
5. เมนู 3 Create Playable Demo Scene สร้าง Prefab และ Scene ใหม่ (Unity จะถามบันทึก Scene ที่เปิดอยู่ก่อนเปลี่ยน) เปิด PlayableDemo.unity ที่สร้างแล้วกด Play
6. ไฟล์สร้างอัตโนมัติอยู่ Assets/GeneratedAyutthayaShophouseV2/PBR*/Prefabs และ PlayableDemo.unity ภายในโฟลเดอร์ PBR* (หมายเลขเพิ่มเมื่อสร้างซ้ำ)

ควบคุมผู้เล่น
W A S D เดิน, Shift วิ่ง, Space กระโดด, Mouse มองรอบตัว, V สลับ TPS/FPS
E เล็งที่ประตูกระจกหรือประตูม้วนแล้วเปิด/ปิด (สังเกตข้อความตรงกลางจอ ระยะจากผู้เล่นไม่เกิน 3.5 ม.)
F เล็งที่ส่วนคอนกรีตด้านหน้า/แผ่นกระจกคงที่ แล้วกดเพื่อทดสอบ damage (กระจกครั้งเดียว, ผนังทั่วไปครั้งเดียว, เสารับน้ำหนักสองครั้ง)
K ทดสอบพังทั้งอาคาร; เมื่อเสาหน้าชั้นเดียวกันเสียหายครบสองต้น ระบบสั่งพังชั้นนั้นและส่วนบนโดยอัตโนมัติ
R กลับจุดเริ่มต้น (ไม่คืนสภาพอาคาร); Esc ปลดล็อกเมาส์ คลิก Game View เพื่อล็อกอีกครั้ง
ออก Play Mode แล้วกด Play ใหม่เพื่อคืนสภาพอาคาร

ทดสอบเดินเข้าตึก
- เริ่มที่หน้าตึก หันเข้าประตูกลาง ใช้ E เปิดประตูคู่และเดินเข้า
- ทางด้านในขวา (x ประมาณ 3.88, z ประมาณ 7.75) เป็นบันไดขึ้นชั้น 2
- เดินตามทางลาด Collider ที่ซ่อนอยู่ใต้ขั้นบันไดไปยังชานพักด้านหลัง z=14.25
- ขยับไปช่องบันไดข้างกัน (x ประมาณ 5.12) แล้วขึ้นย้อนกลับมาทางด้านหน้า ไปชั้น 3
- โหมด FPS อาจเดินในที่แคบได้สะดวกกว่า TPS

การใช้ Player หรือ CharacterController เดิม (Somchai)
ใช้เฉพาะ ShophouseV2.prefab ใน Scene เดิม; ไม่ต้องเพิ่ม DemoPlayer หรือกล้องใหม่
เรียก Ayutthaya.ShophouseV2.AV2DamageReceiver.Interact(hit.collider) จาก interaction raycast ของคุณ
เรียก Ayutthaya.ShophouseV2.AV2DamageReceiver.Apply(hit.collider, damage, hit.point) จากระบบ gameplay damage
ถ้าต้องการใช้ DemoPlayer ใน Scene ของคุณ ให้ปิดกล้อง/AudioListener เดิมที่ซ้ำกัน และกำหนด testBuilding ใน Inspector เมื่อจะทดสอบปุ่ม K
ตั้งค่า Active Input Handling เป็น Input System Package (New) หรือ Input Manager (Old) หรือ Both ตามโปรเจกต์ (รองรับ compile symbols ของ Unity) ไม่ต้องสร้าง InputAction asset

ระบบสร้าง Mesh
MeshData/*.bytes เป็นข้อมูล AYV2 little-endian: magic 4 bytes, vertex count int32, index count int32, positions (3 float32 per vertex), normals (3 float32), UV (2 float32), triangle indices int32
Manifest.json ระบุตำแหน่งและชนิดโมดูล, ชิ้นส่วน fracture, ความสัมพันธ์กับประตูและชั้น
Unity คำนวณ Tangents/Bounds, สร้าง Mesh Asset และ Prefab โดยไม่ต้องใช้ Blender หรือ Python บนเครื่องผู้ใช้
ขนาดตึกสมมติ 12 x 16 ม. สูงชั้นละ 3.3 ม. หลังคาดาดฟ้าราบ (เลือกอิงภาพอาคารพังและภาพโมดูล; ภาพแรกเป็นหลังคาจั่ว)
ห้องชั้น 1 ร้านค้า, ชั้น 2 สำนักงาน, ชั้น 3 ห้องพักแบบเปิด มีเฟอร์นิเจอร์ต้นแบบ ไม่มีผนังกั้นห้องน้ำหรือประตูห้องภายใน
พื้นชั้นบนแยกชิ้น มีช่องบันไดจริง, ไม่ใช้กล่อง collider ขนาดทั้งอาคาร
กระจกหน้าต่างเป็นวัสดุโปร่งใสใน Unity; ภาพ software preview แสดงสีทึบ (ไม่จำลอง transparency/PBR)
PBR Atlas 4K จำนวน 8 แผ่น ใช้ UV 16 พื้นที่วัสดุร่วมกัน มี padding; ใช้ BaseColor, Normal, MetallicSmoothness, AO และ Emissive โดย Roughness, Metallic, Height เก็บไว้เป็นข้อมูลเสริม
ใช้ material tiling atlas; ไม่ได้ unique unwrap ทั้งตึก. Texture 4K สร้างจากตัวอย่างภาพเดิมผสม procedural detail; AO ขาว neutral และ Normal ไม่ได้ bake high-poly

ระบบพังทลาย
27 ชิ้นคอนกรีต (เสา คาน ผนังใต้หน้าต่าง) + 30 กระจกหน้าต่างคงที่ มีชิ้นแตกเตรียมไว้ 282 ชิ้น (Voronoi prism/triangular prism)
แต่ละชิ้นมี Rigidbody + convex MeshCollider เมื่อแตก; ไม่ใช้ลูกบาศก์สุ่มแทนเศษผนัง
การพังเป็นระบบเกมแบบ scripted: เมื่อเสาเสียหายตามเงื่อนไข จะปล่อยพื้น/หลังคา/ระเบียงและโมดูลที่กำหนดให้ตกด้วยฟิสิกส์; ผนังด้านข้างและด้านหลังบางส่วนถูกซ่อน ไม่ได้แตกทุกชิ้น
ไม่ใช่การจำลองวิศวกรรมโครงสร้างจริงหรือระบบ fracture ต่อเนื่อง; เศษซากลบหลัง 35 วินาที และส่วนตกบางส่วนหลัง 60 วินาที
LODGroup ใช้คัดทิ้งเฉพาะรายละเอียดตกแต่งระยะไกล; ยังไม่มี LOD0-LOD3 ของอาคารทั้งหลัง เพื่อไม่ให้สถานะประตู/ความเสียหายและ Collider ซ้ำข้าม LOD
ตัวละครเป็น placeholder ไม่มี rig หรือเดิน animation; สามารถเปลี่ยนเป็นตัวละคร Somchai ของคุณได้

ตรวจสอบ
อ่าน Validation.txt สำหรับผลตรวจ binary mesh, triangle, UV, PBR map dimensions, manifest, C# bracket balance. ยังไม่ได้ Compile หรือ Play Mode ใน Unity จริง. ภาพ ClayPreview.jpg, TexturedPreview.jpg, InteriorSection.jpg สร้างจาก Mesh จริงผ่าน software renderer ไม่ใช่ Unity screenshot. ภาพ section ตัดหลังคาและผนังด้านหนึ่งเพื่อให้เห็นภายใน (ไม่ใช่ state พังจริง). ไม่รับรอง AAA.

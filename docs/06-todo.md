# 06 งานที่ต้องทำต่อ

[← กลับหน้าหลัก](../README.md)

## ต่อจากนี้ (พัฒนาเว็บ)

- [ ] ออกแบบ Document Type ของเว็บ (หน้าแรก, หน้าชุดข้อมูล, หน้ารายการ)
- [ ] ผูก composition / เพิ่ม `publishToCkan` ตาม [04](04-ckan-document-types.md)
- [ ] ตั้ง Allowed child node types ให้สร้างหน้ารายการใต้หน้าชุดข้อมูลได้
- [ ] ขอ API token และ URL ของ CKAN แล้วกรอก CKAN Settings ตาม [05](05-ckan-settings-and-testing.md)
- [ ] ทดสอบ sync ครบ 4 ขั้น และ Sync Logs ไม่มี failed

## ความปลอดภัย (ควรทำเร็ว)

- [ ] **ย้าย `Imaging:HMACSecretKey` ออกจาก `appsettings.json`** — Umbraco เขียน key นี้ให้เองตอนเปิดเว็บครั้งแรก
      และถูก commit ขึ้น GitHub แล้ว ถ้า repo เป็น public ให้ย้ายไป `appsettings.Local.json` / ตัวแปรสภาพแวดล้อม
      แล้วสร้าง key ใหม่ (key เดิมถือว่ารั่วแล้ว)
- [ ] **Revoke GitLab token ที่เคยแชร์ไว้** แล้วสร้างใหม่ → `dotnet nuget update source GitLab ...` (ดู [03](03-setup-new-machine.md))
- [ ] เปลี่ยนรหัสผ่านแอดมินเป็นรหัสที่เดายาก (ใน Umbraco: Users → เลือก user → Change password)

## ก่อนขึ้น production

- [ ] **ตั้ง Data Protection key ring** ใน [Program.cs](../Program.cs) — ApiKey ของ CKAN ถูกเข้ารหัสด้วย key นี้
      ถ้า key หาย (ย้ายเครื่อง, container ใหม่, หลาย instance ใช้ key ไม่ตรงกัน) sync จะล้มจนกว่าจะกรอก ApiKey ใหม่ทุกที่

  ```csharp
  builder.Services.AddDataProtection()
      .SetApplicationName("modumbraco")                       // ต้องเหมือนกันทุก instance
      .PersistKeysToFileSystem(new DirectoryInfo(@"D:\keys")); // ที่คงทน มี backup แชร์กันได้
  ```

- [ ] เปลี่ยนฐานข้อมูลจาก SQLite เป็น SQL Server (connection string `umbracoDbDSN` ใน appsettings ของ production)
- [ ] ตั้ง `Umbraco:CMS:WebRouting:UmbracoApplicationUrl` — ไม่ตั้งแล้วลิงก์ในอีเมล (เชิญผู้ใช้ / รีเซ็ตรหัส) จะใช้ไม่ได้
- [ ] ตั้ง SMTP (`Umbraco:CMS:Global:Smtp`) ถ้าจะใช้อีเมลแจ้งเตือน sync ล้มเหลวของ CKAN
- [ ] ตรวจ `Ckan:AcceptInvalidCertificate` ต้องเป็น `false`
- [ ] ปิด unattended install (`InstallUnattended`) ใน config ของ production
- [ ] server build/deploy ต้องเข้าถึง GitLab NuGet (`10.16.1.64`) ได้ และมี token ของตัวเอง

## รอจากภายนอก

- [ ] `Pkg.Umbraco.CKAN` ยังเป็นรุ่นพัฒนา (0.1.0) — ติดตามเวอร์ชันใหม่จาก GitLab
- [ ] แจ้งคนดูแลแพ็กเกจเรื่องส่ง body แบบ chunked (`CkanClient.PostAsync` ใช้ `PostAsJsonAsync`) — แก้แล้วลบ handler ของเว็บออกได้ ดู [07](07-ckan-chunked-fix.md)
- [ ] อัปเกรดเป็น Umbraco 18 ได้เมื่อแพ็กเกจออกรุ่นที่รองรับ 18 เท่านั้น
- [ ] GitLab ภายในเป็น `http://` (ไม่เข้ารหัส) — ถ้าเปิด https ได้ ให้เปลี่ยน URL ของ source และเอา `--allow-insecure-connections` ออก

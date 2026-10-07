# 02 สิ่งที่ติดตั้ง

[← กลับหน้าหลัก](../README.md)

## แพ็กเกจ NuGet

เวอร์ชันทั้งหมดอยู่ใน [Directory.Packages.props](../Directory.Packages.props) (Central Package Management) —
ใน [MODumbraco.csproj](../MODumbraco.csproj) มีแค่ชื่อแพ็กเกจ ไม่มีเลขเวอร์ชัน

| แพ็กเกจ | เวอร์ชัน | มาจาก | หน้าที่ |
|---|---|---|---|
| `Umbraco.Cms` | 17.7.0 | nuget.org | ตัว Umbraco CMS |
| `Umbraco.Cms.DevelopmentMode.Backoffice` | 17.7.0 | nuget.org | ให้แก้ template/โค้ดผ่านหลังบ้านได้ตอน dev |
| `Microsoft.ICU.ICU4C.Runtime` | 72.1.0.3 | nuget.org | ทำให้การจัดการภาษา/วันที่เหมือนกันทุกเครื่อง |
| `Pkg.Umbraco.CKAN` | 0.1.0 | GitLab ภายใน | ส่งข้อมูลจาก Umbraco ขึ้น CKAN |

> **อัปเกรด Umbraco:** อย่าขึ้น 18.x จนกว่า `Pkg.Umbraco.CKAN` จะออกเวอร์ชันที่รองรับ — อัปเกรดใน 17.x ได้ (17.6.0 ขึ้นไป)

## เครื่องมือบนเครื่อง dev

| | ใช้ |
|---|---|
| .NET SDK | 10.x (โปรเจค target `net10.0`) |
| Umbraco templates | `Umbraco.Templates` (ติดตั้งด้วย `dotnet new install Umbraco.Templates`) |
| NuGet source `GitLab` | `http://10.16.1.64/api/v4/projects/505/packages/nuget/index.json` (ตั้งต่อเครื่อง ไม่อยู่ใน repo) |

## ไฟล์ config

| ไฟล์ | อยู่ใน git | เก็บอะไร |
|---|---|---|
| [appsettings.json](../appsettings.json) | ✅ | ค่าทั่วไปของ Umbraco |
| [appsettings.Development.json](../appsettings.Development.json) | ✅ | ค่าตอน dev: connection string SQLite, เปิด unattended install, log ออก console |
| `appsettings.Local.json` | ❌ (gitignore) | ชื่อ/อีเมล/รหัสผ่านของแอดมินที่ใช้ตอนติดตั้งครั้งแรก — แต่ละเครื่องสร้างเอง |
| [Properties/launchSettings.json](../Properties/launchSettings.json) | ✅ | พอร์ต 9797 (http) / 44358 (https) |
| `%AppData%\NuGet\NuGet.Config` | ❌ (นอกโปรเจค) | NuGet source `GitLab` + token (เข้ารหัสด้วย Windows) |

`appsettings.Local.json` ถูกโหลดเฉพาะตอน build แบบ Debug (ดู [Program.cs](../Program.cs))

## ไฟล์ที่ไม่อยู่ใน git

| path | คืออะไร |
|---|---|
| `umbraco/Data/Umbraco.sqlite.db` | ฐานข้อมูล SQLite ของเครื่องนั้น |
| `umbraco/Logs/` | log ของ Umbraco |
| `bin/`, `obj/` | ผลการ build |
| `appsettings-schema*.json`, `umbraco-package-schema.json` | schema ที่ Umbraco สร้างให้ตอน build |

## สิ่งที่ `Pkg.Umbraco.CKAN` สร้างในฐานข้อมูล

สร้างเองตอนเว็บบูตครั้งแรกหลังติดตั้งแพ็กเกจ (package migration)

| ชนิด | ชื่อ |
|---|---|
| ตาราง | `pkgCkanSyncLog` (log การ sync), `pkgCkanSyncMap` (จับคู่ node ↔ ชื่อบน CKAN) |
| โฟลเดอร์ | `Packages` > `Pkg.CKAN` ทั้งใน Document Types และ Data Types |
| Composition | `pkgCkanRootSettings` (สำหรับหน้าแรก), `pkgCkanDatasetSettings` (สำหรับหน้าชุดข้อมูล) |
| Data type | `CKAN - Textstring`, `CKAN - True/False (Default: True)`, `CKAN - True/False (Default: False)`, `CKAN - Tags`, `CKAN - ประเภทข้อมูล`, `CKAN - วัตถุประสงค์`, `CKAN - หน่วยความถี่…`, `CKAN - ขอบเขตเชิงภูมิศาสตร์…`, `CKAN - รูปแบบการเก็บข้อมูล`, `CKAN - หมวดหมู่ข้อมูล…`, `CKAN - สัญญาอนุญาตให้ใช้ข้อมูล`, `CKAN - ApiKey (ซ่อนค่า)`, `CKAN - อีเมลแจ้งเตือน` |
| เมนูหลังบ้าน | Settings > **CKAN** > CKAN Settings / Sync Logs |
| API | `/api/open-data/...` (สาธารณะ), `/api/ckan-integration/...` (ต้องมี key) |

> ของที่แพ็กเกจสร้าง **ถูกล็อก** ไม่ให้ลบหรือเปลี่ยน alias ผ่านหลังบ้าน — เปลี่ยนชื่อที่แสดง / description / ลำดับได้

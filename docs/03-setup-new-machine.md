# 03 ตั้งค่าเครื่องใหม่

[← กลับหน้าหลัก](../README.md)

ทำตามลำดับ ครั้งเดียวต่อเครื่อง

## 1. ติดตั้งเครื่องมือ

- [.NET SDK 10](https://dotnet.microsoft.com/download) — ตรวจด้วย `dotnet --list-sdks` ต้องมี `10.x`
- Git

## 2. Clone โปรเจค

```powershell
git clone https://github.com/thanachot531070021/MODumbraco.git
cd MODumbraco
```

## 3. เพิ่ม NuGet source ของ GitLab

แพ็กเกจ `Pkg.Umbraco.CKAN` อยู่ใน GitLab ภายใน — ถ้าไม่ทำขั้นนี้ restore/build จะไม่ผ่าน

### 3.1 สร้าง Personal Access Token

1. เข้า http://10.16.1.64 แล้วล็อกอิน
2. รูปโปรไฟล์ (มุมขวาบน) → **Edit profile** → **Access Tokens**
3. ตั้งชื่อ (เช่น `nuget`) ติ๊ก scope **`read_api`** แล้วกด Create
4. copy token เก็บไว้ (ขึ้นต้นด้วย `glpat-`, แสดงครั้งเดียว)

> ต้องเป็น **token** — รหัสผ่านที่ใช้ล็อกอินหน้าเว็บใช้ไม่ได้ (ได้ 401)
> บัญชีต้องมีสิทธิ์อย่างน้อย Reporter ใน project `umbraco_package/pkg.umbraco.ckan`

### 3.2 เพิ่ม source

```powershell
dotnet nuget add source "http://10.16.1.64/api/v4/projects/505/packages/nuget/index.json" `
  --name GitLab `
  --username <gitlab-username> `
  --password <token> `
  --allow-insecure-connections
```

- `<gitlab-username>` = ชื่อผู้ใช้ GitLab (ดูที่หน้าโปรไฟล์) **ไม่ใช่อีเมล**
- `--allow-insecure-connections` จำเป็นเพราะ server เป็น `http://` (.NET 9 ขึ้นไปบล็อก http โดยค่าเริ่มต้น)
- token ถูกเก็บแบบเข้ารหัสใน `%AppData%\NuGet\NuGet.Config` ของเครื่องนั้น ไม่อยู่ในโปรเจค

ตรวจว่าใช้ได้:

```powershell
dotnet package search Pkg.Umbraco.CKAN --source GitLab --exact-match
```

ต้องเห็นเวอร์ชัน `0.1.0` (ถ้าได้ 401 = token ผิด/หมดอายุ/ไม่มีสิทธิ์)

เปลี่ยน token ภายหลัง:

```powershell
dotnet nuget update source GitLab --username <gitlab-username> --password <token-ใหม่>
```

## 4. สร้างไฟล์แอดมิน `appsettings.Local.json`

สร้างไฟล์ที่ root ของโปรเจค (ไฟล์นี้อยู่ใน `.gitignore` — ห้าม commit)

```json
{
  "Umbraco": {
    "CMS": {
      "Unattended": {
        "UnattendedUserName": "ชื่อที่แสดง",
        "UnattendedUserEmail": "you@example.com",
        "UnattendedUserPassword": "รหัสผ่านอย่างน้อย 10 ตัวอักษร"
      }
    }
  }
}
```

ตอนเปิดเว็บครั้งแรก Umbraco จะสร้างฐานข้อมูล SQLite และบัญชีแอดมินจากค่านี้ให้เอง
(ถ้าไม่สร้างไฟล์นี้ จะเจอหน้า install wizard ให้กรอกเองแทน)

## 5. รันเว็บ

```powershell
dotnet run
```

ครั้งแรกจะช้ากว่าปกติ (restore แพ็กเกจ + สร้างฐานข้อมูล + migration ของ CKAN) — รอจนเห็น
`Application started` ใน console

| | URL |
|---|---|
| เว็บ | http://localhost:9797 |
| หลังบ้าน | http://localhost:9797/umbraco |

หน้าเว็บขึ้น "No Published Content" ถือว่าปกติ (ยังไม่มี content)

## เรื่องที่ต้องระวัง

### Windows ที่ตั้ง format เป็นภาษาไทย (พุทธศักราช)

Umbraco บน SQLite แปลงวันที่ตาม culture ของเครื่อง ถ้าเป็นปฏิทินพุทธ ปีจะหายไป 543 ทุกรอบจนเว็บบูตไม่ขึ้น
ตรวจด้วย:

```powershell
(Get-Culture).Calendar.GetType().Name   # ต้องเป็น GregorianCalendar
```

ถ้าเป็น `ThaiBuddhistCalendar` ให้เพิ่มที่บรรทัดแรกของ [Program.cs](../Program.cs):

```csharp
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
```

### build error "file is locked by MODumbraco"

มีเว็บตัวเก่ารันค้างอยู่ — ปิดก่อน (Ctrl+C ใน console ที่รันอยู่ หรือ `Stop-Process -Name MODumbraco`) แล้ว build ใหม่

### warning NU1507

ขึ้นเมื่อเครื่องมี NuGet source หลายตัว เป็นแค่คำเตือน ไม่กระทบการทำงาน

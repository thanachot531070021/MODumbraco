# 05 ตั้งค่าและทดสอบ CKAN

[← กลับหน้าหลัก](../README.md)

ทำหลังสร้าง Document Type ตาม [04](04-ckan-document-types.md) แล้ว

## ค่าอยู่ 3 ที่

| ที่ | ใช้กับ | เปลี่ยนแล้วต้อง |
|---|---|---|
| Settings > **CKAN** > **CKAN Settings** (หลังบ้าน) | ค่าการเชื่อมต่อกลางของทุกเว็บ | บันทึก (มีผลภายใน 30 วินาที) |
| แท็บ **CKAN** ของหน้าแรก | ค่าเฉพาะเว็บนั้น (ทับค่ากลางทีละช่อง) | **Publish** หน้าแรก |
| `appsettings.json` section `Ckan` | พฤติกรรมของแพ็กเกจ | restart เว็บ |

> ค่าการเชื่อมต่อ (BaseUrl, ApiKey) **ห้ามใส่ใน appsettings** — กรอกในหลังบ้านเท่านั้น (ApiKey ถูกเข้ารหัสเก็บ)

## 1. ออก API token บน CKAN

ให้ผู้ดูแล CKAN รันบนเครื่อง CKAN (หรือออกจากหน้าโปรไฟล์ของ CKAN):

```
ckan user token add <sysadmin> umbraco
```

## 2. กรอกค่ากลาง

หลังบ้าน → **Settings** → **CKAN** → **CKAN Settings**

| ช่อง | ตัวอย่าง |
|---|---|
| BaseUrl | `https://data.example.go.th` |
| OrganizationSlug | `my-organization` |
| OpenDataBaseUrl | URL ของเว็บนี้ที่ CKAN เรียกกลับมาได้ (ใช้ทำลิงก์ JSON) |
| ApiKey | token จากข้อ 1 |

ตารางท้ายหน้าบอกว่าแต่ละหน้าแรกใช้ค่าจากไหนจริง

ถ้ามีหลายหน้าแรกที่ส่งไปคนละ CKAN/องค์กร → กรอกแท็บ CKAN ของหน้าแรกนั้นเฉพาะช่องที่ต่าง แล้ว **Publish หน้าแรก**

## 3. ค่าใน `appsettings.json` (ไม่ใส่ = ใช้ค่าเริ่มต้น)

ตอนนี้โปรเจค **ยังไม่ได้ใส่** section นี้ ถ้าต้องการให้เพิ่มใต้ root ของ [appsettings.json](../appsettings.json):

```json
"Ckan": {
  "AcceptInvalidCertificate": false,
  "TimeoutSeconds": 30,
  "SyncOnPublish": true
}
```

| ค่า | ค่าเริ่มต้น | ความหมาย |
|---|---|---|
| `AcceptInvalidCertificate` | `false` | `true` เฉพาะตอน dev ที่ CKAN ใช้ self-signed cert — **ห้ามเปิดใน production** |
| `TimeoutSeconds` | `30` | timeout ต่อ request ที่เรียก CKAN |
| `SyncOnPublish` | `true` | `false` = ไม่ส่งอะไรขึ้น CKAN (ทดสอบเว็บโดยไม่แตะ CKAN) |

อีเมลแจ้งเตือนเมื่อ sync ล้มเหลว (`Ckan:FailureEmail`) ต้องตั้ง SMTP ของ Umbraco ด้วย — ดู `docs/settings.md` ในแพ็กเกจ

## 4. ทดสอบ

| ขั้น | ทำ | ผลที่ต้องได้ |
|---|---|---|
| 1 | Publish หน้าแรก | organization บน CKAN |
| 2 | หน้าชุดข้อมูล: ติ๊ก **CKAN Enabled DataSet**, กรอก **DatasetSlugs** (อังกฤษ) → Publish | dataset พร้อม resource JSON + CSV |
| 3 | หน้ารายการข้างใต้: ติ๊ก **Publish to CKAN** → Publish | แถวขึ้นใน CSV และใน `/api/open-data/{guid-ของ-dataset}/items.json` |
| 4 | Settings > CKAN > **Sync Logs** | ไม่มีแถว `failed` |

> CSV เป็นสำเนา ณ เวลาที่ sync — แก้แถวแล้วไม่ Publish จะไม่อัปเดต ส่วน JSON เป็นลิงก์สดอัปเดตเอง

## API ที่ใช้ตรวจได้

| Route | ใช้ทำอะไร |
|---|---|
| `GET /api/open-data` | รายการแถวที่เผยแพร่แล้ว (`?dataset={guid}`) |
| `GET /api/open-data/{id}/items.json` | รายการของ dataset (ลิงก์ที่ส่งขึ้น CKAN) |
| `GET /api/open-data/{id}/items.csv` | แบบ CSV |
| `GET /api/ckan-integration/status` | สถานะการเชื่อมต่อทุก root (ต้องส่ง header `X-Integration-Key` = ApiKey) |

## ปัญหาที่พบบ่อย

| อาการ | ตรวจ |
|---|---|
| กด Save / Publish แล้วไม่มีอะไรเกิดขึ้น, Network ว่าง, Console ขึ้น `All variants must have a name ... invariant_` | **ช่องชื่อหน้าด้านบนสุด (ข้างไอคอน) ว่าง** — ต้องกรอก (ไม่ใช่ช่อง Page Title) ถ้าพิมพ์ไทยให้กด Tab ออกจากช่องก่อน Save |
| กด Save แล้วเด้งไปหน้าล็อกอิน / 401 | บัญชีเดียวกันถูกล็อกอินจากที่อื่น (`AllowConcurrentLogins: false` จะเตะ session เดิมออก) — ล็อกอินใหม่ |
| ทุกอย่างเป็น `skipped` | หน้าแรกผูก `pkgCkanRootSettings` หรือยัง / หน้าชุดข้อมูลติ๊ก CKAN Enabled DataSet หรือยัง / `Ckan:SyncOnPublish` |
| `failed` "ยังไม่ได้ตั้ง BaseUrl/ApiKey" | กรอก CKAN Settings หรือแท็บ CKAN ของหน้าแรก (แล้ว Publish หน้าแรก) |
| แถวข้อมูลเป็น `skipped` | node อยู่ใต้หน้าที่ติ๊ก CKAN Enabled DataSet ไหม / หน้าชุดข้อมูล Publish แล้วหรือยัง |
| แถวไม่ขึ้นเลย ไม่มี error | alias ของช่องเป็น `publishToCkan` ตรงทุกตัวอักษรไหม |
| ชื่อ dataset เป็น `umbraco-{guid}` | ชื่อหน้าเป็นภาษาไทยล้วน — กรอก DatasetSlugs |
| `failed` "... Missing value" (เช่น `organization_create ... name: Missing value`) | CKAN อ่าน request แบบ chunked ไม่ได้ — ดู [07](07-ckan-chunked-fix.md) (เว็บนี้แก้ไว้แล้ว ถ้ายังเจอ ให้เช็กว่า restart เว็บหลังแก้โค้ดหรือยัง) |
| BaseUrl / Open Data Base Url ไม่มี `http://` | แพ็กเกจไม่เติมให้ — ต้องพิมพ์ URL เต็ม (`http://...`) หรือเว้นว่างให้ใช้ค่าจาก Settings |
| 409 ตอนสร้าง | ชื่อซ้ำ (ระบบกู้ให้เอง) หรือข้อมูลผิดรูปแบบ เช่นอีเมล — ดูข้อความใน Sync Logs |
| หน้า Settings ขึ้น "ตั้งไว้แต่ถอดรหัสไม่ได้" | Data Protection key หาย (ย้ายเครื่อง/สร้าง container ใหม่) — กรอก ApiKey ใหม่ และดู [06](06-todo.md) |

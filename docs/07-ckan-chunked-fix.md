# 07 แก้ปัญหา CKAN ตอบ "Missing value" (chunked request body)

[← กลับหน้าหลัก](../README.md)

## สรุป

`Pkg.Umbraco.CKAN 0.1.0` ส่งคำสั่งแบบ POST ไป CKAN โดย **ไม่บอกขนาดของ body** (`Transfer-Encoding: chunked`)
CKAN บางเครื่องอ่าน body แบบนี้ไม่ได้ เลยเห็นเป็น body ว่าง และตอบกลับว่าขาดข้อมูล เช่น `name: Missing value`

เว็บนี้แก้จากฝั่งเว็บเองโดย **ไม่แก้แพ็กเกจ**: เพิ่ม handler ที่รวม body ให้เป็นก้อนเดียวพร้อมบอกขนาด (`Content-Length`) ก่อนส่งออกไป

| ไฟล์ | ทำอะไร |
|---|---|
| [Ckan/BufferedRequestContentHandler.cs](../Ckan/BufferedRequestContentHandler.cs) | handler ที่แปลง body แบบ chunked ให้เป็นก้อนเดียวพร้อม `Content-Length` |
| [Program.cs](../Program.cs) | ผูก handler นี้เข้ากับ HttpClient ชื่อ `"ckan"` ที่แพ็กเกจใช้ (3 บรรทัด) |

---

## อาการที่เห็น

- Publish แล้วใน **Settings → CKAN → Sync Logs** ขึ้น `failed` พร้อมข้อความประมาณ
  - `CKAN action 'organization_create' ...: name: Missing value`
  - `CKAN action 'package_create' ...: name: Missing value`
  - `CKAN action 'package_patch' / 'organization_patch' / 'package_delete' ...: id: Missing value`
- ไม่มีอะไรถูกสร้างหรืออัปเดตบน CKAN
- **แต่** ส่วนที่เป็นการอ่าน (เช่นปุ่มทดสอบการเชื่อมต่อ, `organization_show`, `package_show`) ใช้ได้ปกติ — ทำให้ดูเหมือนตั้งค่าถูกแล้ว
- ทดสอบด้วย Postman / curl / Python แล้วผ่าน — ทำให้ดูเหมือนแพ็กเกจผิดเอง (จริง ๆ เป็นที่การจับคู่ "วิธีส่ง" กับ "ตัวรับ")

> ข้อความ "Missing value" ไม่ได้แปลว่าข้อมูลใน Umbraco ว่าง — แพ็กเกจส่ง `name` ไปครบ แต่ CKAN อ่าน body ไม่ได้

## ทำไมถึงเกิด

ต้องเข้าเงื่อนไข **ทั้งสองฝั่งพร้อมกัน** ถึงจะเจอ

### ฝั่งผู้ส่ง: แพ็กเกจส่ง body แบบ chunked

แพ็กเกจเรียก CKAN ด้วย `HttpClient.PostAsJsonAsync(...)` ซึ่ง .NET แปลง object เป็น JSON แบบ stream
ระหว่างส่ง จึงไม่รู้ขนาดล่วงหน้า และส่งด้วย `Transfer-Encoding: chunked` แทน `Content-Length`

| คำสั่งของแพ็กเกจ | HTTP | โดนผลกระทบ |
|---|---|---|
| `status_show`, `organization_show`, `organization_list`, `package_show`, `package_search`, `resource_show` | GET (ไม่มี body) | ❌ ไม่โดน |
| `organization_create`, `organization_patch`, `package_create`, `package_patch`, `package_update`, `package_delete`, `resource_create`, `resource_patch`, `resource_update`, `resource_delete` | POST JSON (`PostAsJsonAsync`) | ✅ **โดน** |
| อัปโหลดไฟล์ CSV (`resource_create` / `resource_patch` แบบ multipart) | POST multipart (รู้ขนาดล่วงหน้า) | ❌ ไม่โดน — แต่ไม่เคยไปถึงขั้นนี้ เพราะสร้าง dataset ไม่ผ่านตั้งแต่แรก |

### ฝั่งผู้รับ: CKAN ที่อ่าน chunked body ไม่ได้

CKAN ใน Docker (image `ckan-base`) รันด้วย **uWSGI** — ดูได้จาก `/srv/app/start_ckan.sh`:

```
uwsgi ... --http [::]:5000 ...
```

HTTP router ของ uWSGI แบบนี้ส่ง body ที่เป็น chunked ต่อให้ CKAN (Flask) ไม่ได้ CKAN จึงได้ body ว่าง
CKAN ทดสอบทั้งสองชุดของเรา (`ckantestt1` → 8081, `ckantestt2` → 8082) เปิดพอร์ต 5000 ของ container ออกมาตรง ๆ
**ไม่มี nginx คั่นหน้า** จึงเจอปัญหานี้

## เคสไหนเจอ / เคสไหนไม่เจอ

| สถานการณ์ | เจอไหม | เหตุผล |
|---|---|---|
| แพ็กเกจ (ไม่มี handler) → CKAN Docker ที่เปิดพอร์ต uWSGI ตรง (8081 / 8082 ของเรา) | ✅ เจอ | ส่ง chunked + ตัวรับอ่าน chunked ไม่ได้ |
| แพ็กเกจ (ไม่มี handler) → CKAN ที่มี **nginx** เป็น reverse proxy ด้านหน้า (เช่นชุด `ckan-docker` มาตรฐาน หรือ production ส่วนใหญ่) | ❌ ไม่เจอ (โดยทั่วไป) | nginx รับ body จนครบก่อน (`proxy_request_buffering on` เป็นค่าเริ่มต้น) แล้วส่งต่อให้ CKAN พร้อม `Content-Length` |
| แพ็กเกจ (ไม่มี handler) → CKAN ที่รันด้วย server ที่รองรับ chunked (เช่น gunicorn, `ckan run` ตอน dev) | ❌ ไม่เจอ (โดยทั่วไป) | ตัวรับอ่าน chunked ได้เอง |
| ยิงด้วย curl / Postman / Python `requests` → CKAN ตัวไหนก็ได้ | ❌ ไม่เจอ | เครื่องมือพวกนี้ใส่ `Content-Length` ให้เอง |
| คำสั่งแบบ GET (ทดสอบการเชื่อมต่อ, `*_show`, `*_list`) | ❌ ไม่เจอ | ไม่มี body |
| **แพ็กเกจ + handler ของเว็บนี้** → CKAN ตัวไหนก็ได้ | ❌ ไม่เจอ | handler ใส่ `Content-Length` ให้ทุก request ที่ยังไม่มี |

> "โดยทั่วไป" = ขึ้นกับ config ของเครื่องปลายทาง ถ้าไม่แน่ใจให้ทดสอบตามหัวข้อถัดไป — ใช้เวลาไม่ถึงนาที

## วิธีเช็กว่า CKAN ปลายทางเจอปัญหานี้ไหม

ยิงคำสั่งเดียวกันสองแบบ (ไม่ต้องใช้ API key) — เปลี่ยน URL เป็นของ CKAN ปลายทาง

```powershell
# แบบปกติ (มี Content-Length) — ควรผ่านเสมอ
curl.exe -s -H "Content-Type: application/json" -d '{\"id\":\"mod-poc\"}' http://localhost:8081/api/3/action/organization_show

# แบบ chunked (เหมือนที่แพ็กเกจส่ง)
curl.exe -s -H "Content-Type: application/json" -H "Transfer-Encoding: chunked" -d '{\"id\":\"mod-poc\"}' http://localhost:8081/api/3/action/organization_show
```

| ผลแบบ chunked | แปลว่า |
|---|---|
| `"success": true` | CKAN ตัวนี้รับ chunked ได้ — ไม่มี handler ก็ใช้ได้ |
| `"id": ["Missing value"]` | CKAN ตัวนี้อ่าน chunked ไม่ได้ — **ต้องมี handler** (หรือแก้ฝั่ง CKAN) |

(เปลี่ยน `mod-poc` เป็นชื่อ organization ที่มีอยู่จริงบนเครื่องนั้น)

ผลทดสอบเมื่อ 6 ต.ค. 2026:

| CKAN | แบบปกติ | แบบ chunked | ผ่านแพ็กเกจ + handler |
|---|---|---|---|
| `localhost:8081` (ckantestt1) | ✅ | ❌ Missing value | ✅ สร้าง dataset `mod-news-test` + 3 แถวได้ |
| `localhost:8082` (ckantestt2) | ✅ | ❌ Missing value | ✅ (ทดสอบด้วยโปรแกรมทดสอบ ยังไม่ได้ publish จริง) |

## handler ทำงานยังไง

[Ckan/BufferedRequestContentHandler.cs](../Ckan/BufferedRequestContentHandler.cs) เป็น `DelegatingHandler` — ตัวกลางที่ HttpClient เรียกก่อนส่ง request ออกไปจริง

1. ถ้า request มี body **และยังไม่รู้ขนาด** (`ContentLength` เป็น null = จะถูกส่งแบบ chunked)
2. อ่าน body ทั้งหมดเข้า memory (`ReadAsByteArrayAsync`)
3. สร้าง body ใหม่เป็น `ByteArrayContent` คัด header เดิมมาครบ (เช่น `Content-Type: application/json; charset=utf-8`) และใส่ `Content-Length`
4. ส่งต่อตามปกติ

request ที่รู้ขนาดอยู่แล้ว (GET, multipart อัปโหลด CSV) ผ่านไปโดยไม่ถูกแตะ

ใน [Program.cs](../Program.cs):

```csharp
// CKAN (uWSGI) can't read chunked request bodies; see Ckan/BufferedRequestContentHandler.cs
builder.Services.AddTransient<MODumbraco.Ckan.BufferedRequestContentHandler>();
builder.Services.AddHttpClient("ckan").AddHttpMessageHandler<MODumbraco.Ckan.BufferedRequestContentHandler>();
```

- `"ckan"` คือชื่อ HttpClient ที่แพ็กเกจลงทะเบียนไว้ (`CkanClientFactory.HttpClientName`) — การเรียก `AddHttpClient("ckan")` ซ้ำ **เพิ่ม** handler เข้าไป ไม่ได้แทนที่ของแพ็กเกจ (การตั้งค่า certificate / timeout ของแพ็กเกจยังอยู่)
- มีผลเฉพาะ request ที่ไป CKAN — HttpClient อื่นของ Umbraco ไม่เกี่ยว

### ข้อจำกัด

- body ถูกเก็บใน memory ทั้งก้อนก่อนส่ง — payload ของ dataset/organization มีขนาดไม่กี่ KB ไม่มีปัญหา
- ถ้าวันหลังแพ็กเกจเปลี่ยนชื่อ HttpClient (ไม่ใช่ `"ckan"`) handler จะไม่ทำงานโดยไม่มี error → อาการ "Missing value" จะกลับมา ให้เช็กชื่อใน `CkanClientFactory` ของเวอร์ชันใหม่

## ทางแก้อื่น (ถ้าไม่อยากใช้ handler)

| ทาง | ทำที่ | หมายเหตุ |
|---|---|---|
| แก้ในแพ็กเกจ ให้ serialize JSON เป็น string/byte ก่อนแล้วส่ง (`StringContent` / `ByteArrayContent`) แทน `PostAsJsonAsync` | `CkanClient.PostAsync` / `PostWithoutResultAsync` | **ทางที่ถูกที่สุด** — แจ้งคนดูแล `umbraco_package/pkg.umbraco.ckan` |
| ใส่ nginx หน้า CKAN แล้วให้เว็บเรียกผ่าน nginx | config Docker ของ CKAN | ชุด `ckan-docker` มาตรฐานมี nginx อยู่แล้ว แต่ชุดทดสอบ `ckantestt1/2` เปิด uWSGI ตรง |

## เมื่อไหร่ควรลบ handler

เมื่อ `Pkg.Umbraco.CKAN` ออกเวอร์ชันที่ส่ง body พร้อม `Content-Length` แล้ว:

1. อัปเดตแพ็กเกจ
2. ลบ 3 บรรทัดใน [Program.cs](../Program.cs) และไฟล์ [Ckan/BufferedRequestContentHandler.cs](../Ckan/BufferedRequestContentHandler.cs)
3. Publish หน้าชุดข้อมูลสักหน้า แล้วดู Sync Logs ว่ายังเป็น `success`

(ถ้าไม่ลบก็ไม่เสียหาย — handler ไม่แตะ request ที่มี `Content-Length` อยู่แล้ว)

# 01 สิ่งที่ทำไปแล้ว

[← กลับหน้าหลัก](../README.md)

สรุปงานตามลำดับเวลา (6 ต.ค. 2026)

## 1. ผูกโปรเจคกับ GitHub

- สร้าง git repo ที่ `C:\web_source\MODumbraco` branch `main`
- ผูก remote `origin` กับ https://github.com/thanachot531070021/MODumbraco.git (ตอนนั้น repo ยังว่าง)

## 2. สร้างโปรเจค Umbraco

สร้างจาก template ทางการของ Umbraco (`dotnet new umbraco`) โดยเลือก

| ตัวเลือก | ค่า | เหตุผล |
|---|---|---|
| ฐานข้อมูล dev | SQLite | ไม่ต้องลง SQL Server บนเครื่อง dev |
| ติดตั้งแบบ unattended | เปิด | เปิดเว็บครั้งแรกแล้วสร้างฐานข้อมูลและแอดมินให้เอง ไม่ต้องผ่านหน้า wizard |
| จัดการเวอร์ชันแพ็กเกจ | Central (`Directory.Packages.props`) | เวอร์ชันทุกแพ็กเกจอยู่ไฟล์เดียว |

**รหัสแอดมินไม่ถูก commit** — template เดิมใส่อีเมล/รหัสผ่านแอดมินไว้ใน `appsettings.Development.json`
(ซึ่งขึ้น GitHub) จึงย้ายไปไว้ใน `appsettings.Local.json` ที่อยู่ใน `.gitignore` แทน

## 3. ยุบโฟลเดอร์ให้เหลือชั้นเดียว

template สร้างเป็น `MODumbraco/MODumbraco/...` (เผื่อมีหลายโปรเจค) แต่ตอนนี้มีโปรเจคเดียว
จึงย้ายทุกไฟล์ขึ้นมาที่ root ของ repo และใช้ `.gitignore` ตัวเดียว

> ถ้าวันหลังจะแยกเป็นหลายโปรเจค (เช่น `Core`, `Tests`) ค่อยย้ายเว็บกลับเข้าโฟลเดอร์ย่อย

## 4. ติดตั้งแพ็กเกจ `Pkg.Umbraco.CKAN`

แพ็กเกจอยู่ใน GitLab ภายใน (`http://10.16.1.64`, project `umbraco_package/pkg.umbraco.ckan`) จึงต้อง

1. เพิ่ม NuGet source ชื่อ `GitLab` พร้อม Personal Access Token (รายละเอียดใน [03](03-setup-new-machine.md))
2. ติดตั้งด้วย `dotnet add package Pkg.Umbraco.CKAN --version 0.1.0`

ปัญหาที่เจอระหว่างทาง (เผื่อคนอื่นเจอ):

| อาการ | สาเหตุ | วิธีแก้ |
|---|---|---|
| `nuget : The term 'nuget' is not recognized` | เครื่องไม่มี `nuget.exe` | ใช้ `dotnet nuget ...` / `dotnet add package` แทน |
| 401 Unauthorized | ใช้รหัสผ่านล็อกอิน GitLab แทน token | สร้าง Personal Access Token (scope `read_api`) |
| `There are no versions available` | `dotnet add package --source GitLab` มองคำว่า `GitLab` เป็น **path โฟลเดอร์** ไม่ใช่ชื่อ source | ไม่ต้องใส่ `--source` (source ที่ add ไว้ถูกใช้อยู่แล้ว) |

## 5. ลด Umbraco จาก 18.2 เป็น 17.7 (LTS)

ตอนแรกโปรเจคเป็น Umbraco 18.2.0 แต่ `Pkg.Umbraco.CKAN 0.1.0` รองรับเฉพาะ **Umbraco `>= 17.6.0` และ `< 18.0.0`**
บน 18 เว็บพังตอนบูต (`MissingMethodException: IDataTypeService.GetContainers`) — หน้าเว็บและหลังบ้าน error 500

จึงลดเป็น **17.7.0** (17.x ล่าสุด ณ วันที่ทำ, เป็นรุ่น LTS) และลบฐานข้อมูล SQLite เดิมแล้วติดตั้งใหม่
(ฐานข้อมูลที่ Umbraco 18 สร้างใช้กับ 17 ไม่ได้ — ตอนนั้นยังไม่มี content จึงไม่มีอะไรหาย)

ผลหลังลดเวอร์ชัน:

- build ผ่าน ไม่มี warning เวอร์ชันชน
- migration ของ CKAN รันครบ 11 ขั้น ไม่มี error
- หน้าเว็บ 200 ("No Published Content" — ปกติเพราะยังไม่มี content), หลังบ้าน 200, ล็อกอินผ่าน

## 6. สร้าง Document Type ทดสอบ และแก้ปัญหาส่งข้อมูลขึ้น CKAN

- สร้าง Document Type `home` / `datasetPage` / `dataItem` พร้อม template ตาม [04](04-ckan-document-types.md)
  (อยู่ในฐานข้อมูล SQLite ของเครื่อง dev — ส่วนที่อยู่ใน git มีแค่ไฟล์ template ใน `Views/`)
- Publish แล้ว sync ล้มด้วย `name: Missing value` — สาเหตุคือแพ็กเกจส่ง body แบบ chunked แต่ CKAN ทดสอบ
  (uWSGI ไม่มี nginx คั่น) อ่านไม่ได้ แก้ด้วย handler ฝั่งเว็บ รายละเอียดใน [07](07-ckan-chunked-fix.md)
- ทดสอบกับ CKAN `localhost:8081` สำเร็จ: organization `mod-poc` อัปเดต, dataset `mod-news-test` ถูกสร้างพร้อม JSON + CSV 3 แถว

## ประวัติ commit

| commit | เรื่อง |
|---|---|
| `5c45537` | Initial Umbraco 18.2 project |
| `2c5d7cc` | Flatten project into repository root |
| `dd39afe` | Downgrade to Umbraco 17.7 LTS and add Pkg.Umbraco.CKAN |

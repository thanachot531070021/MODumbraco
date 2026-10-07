# MODumbraco

เว็บ Umbraco 17 (LTS) ที่ใช้แพ็กเกจ `Pkg.Umbraco.CKAN` ส่งข้อมูลขึ้น CKAN (open data portal)

| | |
|---|---|
| Umbraco | 17.7.0 (LTS) |
| .NET | 10 |
| ฐานข้อมูลตอน dev | SQLite (`umbraco/Data/Umbraco.sqlite.db`) |
| แพ็กเกจเสริม | `Pkg.Umbraco.CKAN` 0.1.0 (จาก GitLab ภายใน) |
| Repo | https://github.com/thanachot531070021/MODumbraco |

## เริ่มเร็ว (เครื่องที่ตั้งค่าแล้ว)

```powershell
dotnet run
```

- เว็บ: http://localhost:9797 (หรือ https://localhost:44358)
- หลังบ้าน: http://localhost:9797/umbraco

เครื่องใหม่ต้องตั้งค่าก่อน ดู [03 ตั้งค่าเครื่องใหม่](docs/03-setup-new-machine.md)

## เอกสาร

| ไฟล์ | เนื้อหา |
|---|---|
| [01 สิ่งที่ทำไปแล้ว](docs/01-what-was-done.md) | ลำดับงานตั้งแต่สร้างโปรเจคจนถึงตอนนี้ และเหตุผลของแต่ละการตัดสินใจ |
| [02 สิ่งที่ติดตั้ง](docs/02-installed.md) | แพ็กเกจ, เวอร์ชัน, ไฟล์ config และของที่ CKAN สร้างในฐานข้อมูล |
| [03 ตั้งค่าเครื่องใหม่](docs/03-setup-new-machine.md) | clone โปรเจค, เพิ่ม NuGet source ของ GitLab, สร้างแอดมิน, รันเว็บ |
| [04 สร้าง Document Type สำหรับ CKAN](docs/04-ckan-document-types.md) | ต้องผูก composition อะไร และต้อง Add property อะไรเอง |
| [05 ตั้งค่าและทดสอบ CKAN](docs/05-ckan-settings-and-testing.md) | กรอกค่าการเชื่อมต่อ, ขั้นตอนทดสอบ, ปัญหาที่พบบ่อย |
| [06 งานที่ต้องทำต่อ](docs/06-todo.md) | checklist ก่อนใช้งานจริง / ขึ้น production |
| [07 แก้ CKAN ตอบ "Missing value"](docs/07-ckan-chunked-fix.md) | ทำไม sync ล้มทั้งที่ตั้งค่าถูก, เคสไหนเจอ/ไม่เจอ, handler ที่เว็บนี้ใช้แก้ |

เอกสารฉบับเต็มของแพ็กเกจ CKAN อยู่ในแพ็กเกจเอง:
`%UserProfile%\.nuget\packages\pkg.umbraco.ckan\0.1.0\README.md` และโฟลเดอร์ `content\docs\Pkg.Umbraco.CKAN\`

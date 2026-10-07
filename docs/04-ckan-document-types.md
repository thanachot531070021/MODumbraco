# 04 สร้าง Document Type สำหรับ CKAN

[← กลับหน้าหลัก](../README.md)

## สรุปสั้น

| ระดับ | ตัวอย่าง | ต้องทำ | ได้อะไรบน CKAN |
|---|---|---|---|
| หน้าแรก | Home | ผูก composition `pkgCkanRootSettings` | organization |
| หน้าชุดข้อมูล | หน้ารวมข่าว, หน้ารวมดาวน์โหลด | ผูก composition `pkgCkanDatasetSettings` | dataset (มีไฟล์ JSON + CSV) |
| หน้ารายการ | ข่าวแต่ละข่าว, ไฟล์แต่ละไฟล์ | **Add property `publishToCkan` เอง** | หนึ่งแถวใน dataset |

```
Home  (pkgCkanRootSettings)                → organization
 └─ News List  (pkgCkanDatasetSettings)    → dataset
     ├─ News Item  (publishToCkan)         → แถวที่ 1
     └─ News Item  (publishToCkan)         → แถวที่ 2
```

- หน้าแรกกับหน้าชุดข้อมูล **ไม่ต้อง** Add group / Add property เอง — composition มีให้ครบ
- แพ็กเกจตัดสินบทบาทจาก **property ที่ doctype มี** ไม่ใช่จากชื่อ doctype

---

## 1. หน้าแรก — ผูก `pkgCkanRootSettings`

1. **Settings → Document Types →** (doctype ของหน้าแรก)
2. กดปุ่ม **Compositions** (มุมขวาบนของ editor)
3. ติ๊ก **`pkgCkanRootSettings`** (อยู่ในโฟลเดอร์ `Packages` > `Pkg.CKAN`) → Submit → **Save**

จะได้แท็บ **CKAN** มาเอง มีช่อง เช่น CKAN Base Url, CKAN Organization Slug, CKAN Open Data Base Url, CKAN Api Key, อีเมลแจ้งเตือน

เงื่อนไข:

- ต้องเป็น node **ชั้นบนสุด** ของต้นไม้ content
- ช่องในแท็บนี้กรอกเฉพาะเมื่อเว็บนี้ต่างจากค่ากลางใน Settings > CKAN (ดู [05](05-ckan-settings-and-testing.md)) — ว่างไว้ = ใช้ค่ากลาง

## 2. หน้าชุดข้อมูล — ผูก `pkgCkanDatasetSettings`

1. **Settings → Document Types →** (doctype ของหน้าชุดข้อมูล เช่น หน้ารวมข่าว)
2. **Compositions** → ติ๊ก **`pkgCkanDatasetSettings`** → Submit → **Save**
3. แท็บ **Structure** → **Allowed child node types** → เพิ่ม doctype ของหน้ารายการ (ข้อ 3) → **Save**

ตอนสร้าง content ของหน้านี้:

- ติ๊ก **CKAN Enabled DataSet** — ไม่ติ๊ก = หน้านี้ไม่เป็น dataset
- กรอก **DatasetSlugs** เป็นภาษาอังกฤษ (เช่น `news-2026`) — ถ้าไม่กรอกและชื่อหน้าเป็นภาษาไทย ชื่อ dataset จะเป็น `umbraco-{guid}`
- เลือก **สัญญาอนุญาต** (license) — แพ็กเกจแปลงจากรหัสนำหน้าตัวเลือก (`01` = `cc-by`, `02` = `cc-by-sa`, … `98` = `notspecified`)

## 3. หน้ารายการ (แถวข้อมูล) — Add property `publishToCkan`

ไม่มี composition ให้ — ต้องเพิ่มเอง

1. **Settings → Document Types →** (doctype ของหน้ารายการ)
2. เลือก group ที่จะใส่ (group เดิม หรือ **Add group** ใหม่ชื่อ `CKAN` ก็ได้ — ชื่อ group ไม่มีผล)
3. **Add property**:

| ช่อง | ค่า |
|---|---|
| Name | `Publish to CKAN` |
| Alias | **`publishToCkan`** ← ต้องตรงทุกตัวอักษร |
| Editor (Data type) | **`CKAN - True/False (Default: True)`** |

4. **Save**

> ⚠️ Umbraco ตั้ง alias จากชื่อให้อัตโนมัติ — **ตรวจ alias ทุกครั้งก่อน Save** ถ้าผิด แพ็กเกจจะมองไม่เห็นช่องนี้และไม่ส่งอะไรเลยโดยไม่มี error

พฤติกรรม:

- ใช้ data type `CKAN - True/False (Default: True)` → content ที่ **สร้างใหม่** ติ๊กไว้ให้เลย (content ที่มีอยู่ก่อนต้องติ๊กเอง)
- ติ๊ก = อยู่ในรายการ, ไม่ติ๊ก = ถูกถอนออกตอน Publish ครั้งถัดไป
- แถวไปอยู่ใน dataset **ที่ใกล้ที่สุด** เหนือมัน (ลึกกี่ชั้นก็ได้) — ถ้าไม่มี dataset อยู่เหนือ = ไม่ถูกส่ง

### ช่องเนื้อหาที่แพ็กเกจอ่าน (ไม่บังคับ)

มีช่องไหนก็ส่งช่องนั้น ใช้ alias ตามนี้:

| คอลัมน์ใน CKAN | alias ที่อ่าน (ตามลำดับ) | Data type |
|---|---|---|
| title | `menuItemName` → `title` → ชื่อ node | Textstring |
| url | `link` → URL ของหน้า → `externalLink` | Multi URL Picker / Textstring |
| date | `date` | Date Picker (เรียงใหม่ → เก่า) |
| image | `image` | Media Picker |
| attachments | `attachments` | Media Picker (หลายไฟล์) |

ถ้า doctype ใช้ alias อื่นอยู่แล้ว (เช่นวันที่ชื่อ `publishDate`) ไม่ต้องเปลี่ยน — ไปตั้งที่
**Settings → CKAN → CKAN Settings → การ์ด "ฟิลด์ที่ส่งขึ้น CKAN"**

### ตัวอย่าง doctype "Download"

```
Download
  publishToCkan   CKAN - True/False (Default: True)   ← ต้องมี
  title           Textstring
  date            Date Picker
  attachments     Media Picker (multiple)
```

---

## ข้อห้าม / กับดัก

| อย่าทำ | เพราะ |
|---|---|
| เปลี่ยน alias ของ property หรือ composition ที่แพ็กเกจสร้าง | ถูกล็อกไว้ และถ้าเปลี่ยนได้ sync จะหยุดเงียบ ๆ |
| ลบ property / composition / data type ของแพ็กเกจ | ถูกล็อก และถ้าลบแล้วจะไม่ถูกสร้างคืน |
| สร้าง property alias ซ้ำกับใน composition ไว้ก่อน (เช่นทำแท็บ CKAN เอง) | Umbraco จะไม่ยอมให้ผูก composition — ต้องลบช่องเดิมก่อน และค่าที่กรอกไว้จะหาย (backup ก่อน) |

**ทำได้:** เปลี่ยนชื่อที่แสดง / description / ลำดับช่อง, เปิด-ปิด Mandatory, เพิ่ม property หรือ tab ของเว็บเอง

อยากได้ composition ที่ตัดบางช่องออก → ดูหัวข้อ "อยากได้ composition ที่ตัดช่องบางช่องออก" ใน README ของแพ็กเกจ

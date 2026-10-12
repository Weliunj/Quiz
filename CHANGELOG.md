# 📜 Changelog — Quiz Project

All notable changes to this project will be documented in this file. This repository adheres to **Keep a Changelog** standards and maps directly to **Conventional Commits**.

---

## 🔒 Quy Tắc Cập Nhật Dành Cho AI Agent

Khi hoàn thành bất kỳ tính năng (`feat`), sửa lỗi (`fix`), tái cấu trúc (`refactor`) nào, AI Agent **BẮT BUỘC** phải cập nhật file `CHANGELOG.md` này bằng cách thêm một mục mới vào phần log theo cấu trúc:

```markdown
### [YYYY-MM-DD HH:mm] — <type>(<scope>): <short description in English>
- **Tác vụ**: Mô tả chi tiết việc đã làm.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/.../Filename.cs` (Dòng modified/new)
- **Ảnh hưởng**: Những tính năng hay cấu trúc nào bị ảnh hưởng.
```

- **Lưu ý**: Log mới hãy ghi **lên trên đầu** (mục mới nhất luôn nằm trên cùng ngay dưới phần quy tắc này, theo thứ tự thời gian giảm dần).

---

## 📋 Nhật Ký Thay Đổi

### [2026-10-12 15:47] — feat(scroll): add CenterSnapScroll for PC mouse wheel horizontal scrolling and auto centering
- **Tác vụ**:
  - Tạo component [CenterSnapScroll.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/CenterSnapScroll.cs) hỗ trợ cuộn ngang (Scroll View) bằng chuột hoặc con lăn chuột trên PC.
  - Tự động tính toán padding để đưa thẻ Map đầu tiên hoặc nút `+` cuối cùng ra chính giữa màn hình mà không bị lệch.
  - Hỗ trợ cơ chế tự động hít (snap) mượt mà vào giữa màn hình khi dừng cuộn hoặc khi tạo thêm map mới.
  - Tích hợp gọi căn giữa tự động vào [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/CenterSnapScroll.cs` (new)
  - `Assets/Scripts/MapManager.cs` (lines 30-40, 56-70, 85-100 modified)
- **Ảnh hưởng**: Giải quyết vấn đề nút `+` và thẻ map bị kẹt lệch sang hai bên, cho phép cuộn ngang và căn giữa chính xác.

### [2026-10-12 15:35] — feat(ui): implement simplified MapManager for HomeMenu to MapManagerPanel transition
- **Tác vụ**:
  - Tạo script tinh gọn [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs) để quản lý việc chuyển đổi từ `HomeMenuPanel` sang `MapManagerPanel` khi bấm `StartBtn`.
  - Hỗ trợ sự kiện bấm nút dấu cộng `+` (`AddMapBtn`) để tạo thêm thẻ Map trong `LevelContent`.
  - Hướng dẫn chi tiết từng bước thiết lập trên Hierarchy và kéo thả Inspector trong Unity.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MapManager.cs` (new)
- **Ảnh hưởng**: Quản lý luồng UI cơ bản giữa màn hình Home Menu và Map Manager Panel.


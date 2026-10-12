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

### [2026-10-12 21:55] — feat(gameplay): implement mountain station layers spawning on play with custom station count and random trail offsets
- **Tác vụ**:
  - Cập nhật [QuizData.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuizData.cs):
    - Thêm trường `stationCount` vào cấu trúc `MapData` để lưu trữ số lượng trạm (tầng) cần đi qua để đến đỉnh núi cho từng bản đồ.
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Tích hợp cụm `Other` gồm 2 ô nhập: `NumberStations` (số lượng tầng trạm) và `NameLevel` (tên bản đồ).
    - Tự động nạp, cập nhật và lưu trữ tức thì cả 2 giá trị này vào file JSON khi người dùng nhập liệu hoặc lưu chỉnh sửa.
  - Tạo mới [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Khi bấm Play từ thẻ Map: Tự động tính toán và sinh ra các trạm theo từng tầng (layer) leo từ chân núi lên đỉnh núi.
    - Mỗi index là một tầng với cao độ Y tăng dần đều, tọa độ ngang X được random đảo hướng uốn lượn tự nhiên tạo thành con đường mòn ngoằn ngoèo leo dốc.
    - Trạm cuối cùng (`index = stationCount - 1`) là Trạm Đỉnh Núi (`🏔️ ĐỈNH NÚI`) với màu vàng kim nổi bật ở trung tâm trên cùng.
    - Tự động vẽ các vệt đường nối (trail line) giữa các trạm liền kề và hỗ trợ cuộn màn hình mượt mà từ chân núi lên đỉnh núi.
    - Tích hợp nút `Quay Lại` để thoát ra và trở về màn hình chọn map bất cứ lúc nào.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Khi bấm Play trên bất kỳ thẻ map nào, tự động gọi `StartGameOnMap` để chuyển sang màn hình leo núi của bản đồ đó.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (lines 48-66 modified)
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 100-115, 275-310, 385-415, 500-520, 740-755 modified)
  - `Assets/Scripts/MountainStationManager.cs` (file mới tạo)
  - `Assets/Scripts/MapManager.cs` (lines 16-22, 270-305 modified)
- **Ảnh hưởng**: Hoàn thiện tính năng cốt lõi cho gameplay: người dùng tự cấu hình số lượng trạm/tầng trong Edit map, khi bấm Play game sẽ sinh đường mòn leo núi theo đúng số tầng với các trạm uốn lượn ngẫu nhiên dẫn lên đỉnh núi.

### [2026-10-12 19:51] — fix(navigation): restore MapManagerPanel and snap view when clicking Back from EditMapPanel
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Thêm callback `onCloseCallback` vào phương thức `Open()`.
    - Trong phương thức `Close()`, khi `EditMapPanel` đóng lại (`gameObject.SetActive(false)`), tự động gọi callback đóng hoặc tự động tìm `MapManager` trong scene để kích hoạt lại bảng `MapManagerPanel`.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Truyền callback đóng khi gọi `editPanel.Open(...)`: Khi người dùng bấm `BackBtn`, `MapManagerPanel` tự động bật lại (`mapPanel.SetActive(true)`), làm mới danh sách map và khởi chạy snap căn giữa mượt mà.
    - Cập nhật `CloseEditMap()` để đảm bảo an toàn, không bị tắt toàn bộ màn hình khiến UI biến mất.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 110-115, 490-530 modified)
  - `Assets/Scripts/MapManager.cs` (lines 170-195 modified)
- **Ảnh hưởng**: Khắc phục triệt để lỗi bấm nút Back làm biến mất toàn bộ UI. Người dùng bấm Back từ `EditMapPanel` sẽ quay lại ngay màn hình danh sách map (`MapManagerPanel`).

### [2026-10-12 19:42] — feat(import): support asterisk mark (*) on any option to denote correct answer without changing question order
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Tối ưu hóa hàm `IsOptionLine` và bộ đọc CSV: Cho phép giáo viên chỉ cần thêm dấu `*` vào trước hoặc sau đáp án đúng (ví dụ `*B. Hà Nội`, `B*. Hà Nội`, hoặc `B. *Hà Nội`) mà **không cần phải đổi thứ tự câu hỏi hay chuyển đáp án đúng lên đầu**.
    - Dấu `*` tự động được nhận diện để gán `correctIndex` tương ứng (`*A.` -> 0, `*B.` -> 1, `*C.` -> 2, `*D.` -> 3) và tự động lọc bỏ dấu `*` khỏi nội dung text hiển thị để người chơi không thấy dấu sao.
    - Cập nhật nội dung hướng dẫn trong `DEFAULT_IMPORT_GUIDE` và `GuidePanel` để hướng dẫn chi tiết cách dùng dấu `*`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 115-140, 825-965 modified)
- **Ảnh hưởng**: Giúp giáo viên giữ nguyên 100% thứ tự câu hỏi và đáp án từ đề thi Word/Excel có sẵn, chỉ cần gõ thêm dấu `*` vào đáp án đúng là xong.

### [2026-10-12 19:39] — feat(import): implement question import from TXT/CSV files with default first answer correct and in-game guide panel
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Tích hợp cụm GameObject `Import` trong `CreateQuestion`: kết nối các nút `GuideImportBtn`, `ImportBtn`, `CloseBtn` và bảng `GuidePanel` (`guideText`).
    - Nút `GuideImportBtn`: Mở `GuidePanel` hiển thị chi tiết hướng dẫn cấu trúc soạn file câu hỏi bằng văn bản (.txt từ Word) và bảng tính (.csv từ Excel).
    - Thêm chú thích quan trọng theo yêu cầu: Mặc định đáp án A (đáp án đầu tiên) là đáp án đúng để giáo viên soạn đề nhanh nhất mà không cần đánh dấu phức tạp, hệ thống sẽ tự động xáo trộn ngẫu nhiên vị trí các đáp án A, B, C, D khi vào chơi game. Đồng thời vẫn hỗ trợ nhận diện các dòng chỉ định đáp án (ví dụ `Đáp án: B` hoặc `*B.`) nếu giáo viên đã có sẵn đề thi cũ.
    - Nút `ImportBtn`: Mở hộp thoại chọn file native trên Windows (`.txt` và `.csv`) trong cả Unity Editor lẫn bản build Standalone (Win32 API).
    - Xây dựng bộ phân tích cú pháp thông minh (`ParseQuestionsFromText`): Tự động tách câu hỏi, 4 đáp án A-D, gán đáp án đúng và nạp hàng loạt câu hỏi vào map hiện tại, lưu ngay vào file JSON và chuyển về màn hình danh sách để kiểm tra.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 65-80, 115-160, 215-245, 600-840 modified)
- **Ảnh hưởng**: Cho phép giáo viên import hàng chục/hàng trăm câu hỏi từ Word hoặc Excel vào map chỉ với 1 click, tiết kiệm tối đa thời gian nhập liệu thủ công.

### [2026-10-12 19:02] — feat(ui): add 4 mutually exclusive toggles for correct answer selection with default first toggle enabled
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Thêm 4 biến UI Toggle: `toggleA`, `toggleB`, `toggleC`, `toggleD` với Tooltip tiếng Việt để chọn đáp án đúng cho từng lựa chọn A, B, C, D.
    - Cài đặt cơ chế chọn thay phiên (radio button): Khi người dùng bấm chọn một Toggle thì Toggle đó bật (`isOn = true`) và 3 Toggle còn lại tự động tắt (`isOn = false`).
    - Đảm bảo luôn có 1 đáp án được chọn: Không cho phép tắt hết tất cả Toggle.
    - Mặc định khi mở bảng hoặc xóa trắng form (`ClearInputs`), Toggle đầu tiên (Đáp án A) luôn được chọn (`correctIndex = 0`, `isOn = true`).
    - Tự động nạp đúng Toggle tương ứng khi bấm sửa câu hỏi (`OnEditQuestionClicked`).
    - Lưu chính xác `correctIndex` dựa trên Toggle đang bật (`GetCorrectAnswerIndex()`).
    - Hỗ trợ hàm tìm kiếm tự động linh hoạt (`AutoFindReferences`) cho 4 Toggle theo tên hoặc quét trong container đáp án `D1`..`D4`. Đồng thời vẫn giữ tương thích đồng bộ với Dropdown nếu có.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 40-50, 135-165, 220-295, 410-445 modified)
- **Ảnh hưởng**: Giúp thao tác chọn đáp án đúng trực quan, nhanh chóng hơn trên giao diện thông qua 4 Toggle radio button thay vì phải mở dropdown.

### [2026-10-12 18:55] — feat(ui): highlight question on click without opening create panel, enable edit and delete buttons
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Khi bấm vào câu hỏi bất kỳ trong `ListQuestions`: Giữ người dùng ở lại giao diện danh sách (không tự động mở `CreateQuestion`), kích hoạt viền/nền `Selection` (Alpha = 1).
    - Thêm cơ chế làm mờ / làm sáng 2 nút thao tác (`EditQuestBtn` và `DeleteQuest`): Khi chưa chọn câu hỏi thì 2 nút bị mờ (`alpha = 0.4`, `interactable = false`). Khi chọn một câu hỏi thì 2 nút sáng lên rõ ràng (`alpha = 1`, `interactable = true`).
    - Nút `EditQuestBtn` (`OnEditQuestionClicked`): Nạp toàn bộ dữ liệu câu hỏi được chọn vào các ô input (`questionInput`, `answerA`..`answerD`) và Dropdown đáp án đúng, sau đó mới mở bảng `CreateQuestion` để chỉnh sửa.
    - Nút `DeleteQuest` (`OnDeleteClicked`): Xóa câu hỏi đang chọn khỏi map, lưu lại file JSON, làm mới danh sách và tự động làm mờ lại 2 nút.
    - Bổ sung `OnAddNewClicked` cho nút `AddNewBtn` trong danh sách để xóa trắng ô nhập và mở `CreateQuestion`.
    - Sau khi lưu (`OnCreateClicked`), tự động quay về danh sách câu hỏi để xem kết quả cập nhật.
  - Cập nhật [SampleScene.unity](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scenes/SampleScene.unity): Nối trường `editQuestBtn` vào component của `EditQuestBtn` trong scene.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 65-80, 140-160, 205-225, 280-385 modified)
  - `Assets/Scenes/SampleScene.unity` (line 2031 modified)
- **Ảnh hưởng**: Luồng tương tác danh sách câu hỏi mượt mà, đúng chuẩn UX: người dùng xem danh sách, click chọn câu hỏi thì 2 nút Sửa và Xóa sáng lên, sau đó chủ động bấm Sửa để vào form hoặc bấm Xóa để gỡ bỏ.

### [2026-10-12 18:38] — feat(ui): implement flexible sub-panel switching, correct answer selection, and fix map list initial state
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Khi mở `EditMapPanel`, tự động bật bảng nhập `CreateQuestion` trước và ẩn bảng `ListQuestions` để màn hình thoáng và tập trung soạn bài.
    - Hỗ trợ chuyển đổi chế độ linh hoạt giữa xem danh sách (`ShowListQuestionsMode`) và nhập câu hỏi (`ShowCreateQuestionMode`).
    - Bấm vào câu hỏi trong danh sách tự động nạp dữ liệu và chuyển thẳng sang chế độ nhập để xem/sửa.
    - Tích hợp chọn đáp án đúng (A, B, C hoặc D) qua Dropdown `correctDropdown` để lưu trữ chính xác đáp án chuẩn.
  - Sửa lỗi trong [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Loại bỏ việc tự động sinh `Bản đồ #1` khi dữ liệu rỗng.
    - Khi vừa clear save và chạy game, chỉ hiển thị duy nhất nút dấu `+` (`AddMapBtn`) ở giữa màn hình mà không bị đè nút hay sinh map thừa.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 15-45, 140-270 modified)
  - `Assets/Scripts/MapManager.cs` (lines 220-240, 275-295 modified)
- **Ảnh hưởng**: Trải nghiệm giao diện gọn gàng, hỗ trợ chọn đáp án đúng chuẩn xác và luồng khởi đầu sạch sẽ khi clear save.

### [2026-10-12 18:35] — feat(ui): toggle selection highlight with full opacity and auto-populate question inputs on item click
- **Tác vụ**:
  - Cập nhật [QuestionItemUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuestionItemUI.cs): Khi bấm vào câu hỏi bất kỳ, tự động kích hoạt GameObject `Selection`, đặt độ trong suốt Alpha = 1 (hoặc Image color alpha = 1) và tắt `raycastTarget` để không chặn sự kiện click.
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Khi bấm vào 1 câu hỏi, tự động đổ toàn bộ dữ liệu nội dung câu hỏi và 4 đáp án vào các ô input trong `CreateQuestion` (`questionInput`, `answerA`..`answerD`).
    - Nút `createBtn` tự động nhận biết câu hỏi đang chọn để cập nhật sửa đổi và lưu vào file JSON.
    - Nút `deleteBtn` thực hiện xóa câu hỏi đang chọn khỏi map, cập nhật file JSON và làm mới danh sách.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuestionItemUI.cs` (lines 75-105 modified)
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 190-260 modified)
- **Ảnh hưởng**: Trải nghiệm chọn, xem trước, sửa đổi và xóa câu hỏi trực quan và liền mạch hơn.

### [2026-10-12 17:50] — feat(editor): add 1-click tools and context menu to clear all save data for fresh testing
- **Tác vụ**:
  - Đã xóa sạch toàn bộ các file lưu cũ (`quiz_data.json`, `quiz_save.json`) trong thư mục máy tính để trả về trạng thái nguyên bản.
  - Thêm phương thức `QuizSaveSystem.ClearAllSaveData()` và công cụ trên thanh menu: `Tools > Quiz > Clear All Save Data (Xóa toàn bộ dữ liệu lưu để test lại)`.
  - Thêm `[ContextMenu]` trực tiếp vào component [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs) cho phép click chuột phải trong Inspector để xóa sạch dữ liệu save bất kỳ lúc nào.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (lines 110-135 modified)
  - `Assets/Scripts/MapManager.cs` (lines 58-68 modified)
- **Ảnh hưởng**: Giúp lập trình viên dễ dàng reset dữ liệu và kiểm thử luồng tạo map ban đầu chỉ với 1 click.

### [2026-10-12 17:47] — feat(editor): add shortcut menu to open persistentDataPath save directory
- **Tác vụ**:
  - Bổ sung menu tiện ích `Tools > Quiz > Open Save Data Folder (Mở thư mục lưu dữ liệu)` vào [QuizData.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuizData.cs) giúp mở nhanh thư mục lưu trữ file `quiz_save.json` trong Windows Explorer chỉ với 1 click.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (lines 115-130 new)
- **Ảnh hưởng**: Cho phép lập trình viên kiểm tra và quản lý trực tiếp file dữ liệu JSON của game.

### [2026-10-12 17:40] — refactor(ui): shorten serialized variable names and localize inspector tooltips to Vietnamese
- **Tác vụ**:
  - Rút gọn các tên biến tuần tự hóa dài thành các từ tiếng Anh ngắn gọn, súc tích (ví dụ: `questionInput`, `answerA`, `answerB`, `createBtn`, `resetBtn`, `listContent`, `questionPrefab`, `homePanel`, `mapPanel`, `editPanel`, `startBtn`, `addMapBtn`...).
  - Bản địa hóa toàn bộ `[Tooltip("...")]` và `[Header("...")]` sang tiếng Việt rõ ràng, giúp người dùng rê chuột vào bất kỳ ô nào trong Unity Inspector là hiểu ngay công dụng.
  - Vẫn duy trì cơ chế tìm kiếm tự động linh hoạt (`AutoFindReferences`) hỗ trợ mọi kiểu đặt tên GameObject trong Hierarchy.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (modified)
  - `Assets/Scripts/MapManager.cs` (modified)
  - `Assets/Scripts/QuestionItemUI.cs` (modified)
  - `Assets/Scripts/MapItemUI.cs` (modified)
  - `Assets/Scripts/CenterSnapScroll.cs` (modified)
- **Ảnh hưởng**: Giao diện Inspector ngắn gọn, hiển thị tiếng Việt trực quan khi rê chuột, dễ đọc và thao tác hơn.

### [2026-10-12 17:35] — refactor(naming): standardize variable names to English and add comprehensive inspector tooltips
- **Tác vụ**:
  - Chuẩn hóa toàn bộ tên biến tuần tự hóa (`[SerializeField]`) sang tiếng Anh chuẩn trong các script UI ([EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs), [QuestionItemUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuestionItemUI.cs), [MapItemUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapItemUI.cs), [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs), [CenterSnapScroll.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/CenterSnapScroll.cs)).
  - Bổ sung `[Tooltip("...")]` chi tiết và các `[Header("...")]` tiếng Anh cho từng trường trong Unity Inspector giúp người dùng rê chuột là hiểu ngay chức năng.
  - Tối ưu hóa hàm `AutoFindReferences` linh hoạt: hỗ trợ nhận diện cả tên phác thảo hiện tại (ví dụ `D1`, `CreateBtn`, `AddMapBtn`...) lẫn tên tiếng Anh chuẩn tương lai (`AnswerA`, `CreateQuestionButton`, `AddMapButton`...).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (modified)
  - `Assets/Scripts/QuestionItemUI.cs` (modified)
  - `Assets/Scripts/MapItemUI.cs` (modified)
  - `Assets/Scripts/MapManager.cs` (modified)
  - `Assets/Scripts/CenterSnapScroll.cs` (modified)
- **Ảnh hưởng**: Độ rõ ràng và chuyên nghiệp của Inspector, tương thích cả hiện tại lẫn sau khi đổi tên GameObject.

### [2026-10-12 17:02] — feat(quiz-creator): implement map and question creation with JSON save/load persistence
- **Tác vụ**:
  - Tạo cấu trúc dữ liệu `QuestionData`, `MapData`, `GameSaveData` và hệ thống lưu trữ [QuizData.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuizData.cs) lưu trữ qua file JSON (`persistentDataPath/quiz_save.json`).
  - Tạo [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs) điều khiển toàn bộ `EditMapPanel`:
    - Tạo câu hỏi mới (`CreateBtn`), reset ô nhập (`ResetBtn`).
    - Hiển thị danh sách câu hỏi trong `ListQuestions` bằng prefab `QuestionContent` thông qua [QuestionItemUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuestionItemUI.cs).
    - Hỗ trợ chọn câu hỏi, chỉnh sửa (`EditQuestBtn`) và xóa (`DeleteQuest`).
  - Tạo [MapItemUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapItemUI.cs) gắn vào `MapContent.prefab` để xử lý nút `EditBtn` (mở màn hình soạn câu hỏi) và `PlayBtn`.
  - Nâng cấp [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs) để tự động tải bản đồ đã lưu khi khởi động, kết nối chuyển đổi giữa `MapManagerPanel` và `EditMapPanel`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (new)
  - `Assets/Scripts/QuestionItemUI.cs` (new)
  - `Assets/Scripts/EditMapPanelUI.cs` (new)
  - `Assets/Scripts/MapItemUI.cs` (new)
  - `Assets/Scripts/MapManager.cs` (modified)
- **Ảnh hưởng**: Hoàn thiện toàn bộ chu trình tạo map, tạo/sửa/xóa câu hỏi và tự động lưu/tải dữ liệu bền vững.

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


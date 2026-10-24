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

### [2026-10-24 11:47] — fix(import-ui): support inline options on single line, auto-repair malformed questions, and format correctText to show only answer content
- **Tác vụ**:
  - **Hỗ trợ phân tích đáp án viết trên cùng 1 dòng (`EditMapPanelUI.cs`)**:
    - Nâng cấp bộ phân tích `ParseQuestionsFromText` bằng Regex `OptionPattern` để trích xuất chuẩn xác các đáp án A, B, C, D kể cả khi tất cả 4 đáp án nằm trên cùng 1 dòng (ví dụ `A. 6 + 3 * B. 7 + 3 C. 5 + 4 D. 8 + 1` hoặc `* A. 9 B. 8 C. 7 D. 10`).
    - Hỗ trợ nhận diện dấu `*` ở mọi vị trí (`* B.`, `*B.`, `B*.`, `B.*`, `B. *...`) để xác định đúng đáp án đúng.
  - **Tự động sửa lỗi câu hỏi cũ (`TryRepairMalformedQuestion`)**:
    - Tự động phát hiện và bóc tách các câu hỏi trước đây bị dồn toàn bộ đáp án vào ô `option1` khi nạp danh sách hoặc bấm chỉnh sửa câu hỏi.
    - Đã chạy sửa chữa toàn bộ 15 câu hỏi bị lỗi trong file lưu trữ `quiz_save.json`.
  - **Định dạng lại hiển thị của `correctText` (`QuestionItemUI.cs`)**:
    - Hiển thị theo định dạng `Đáp án: {correctAns}` (ví dụ: `Đáp án: 7 + 3`), lược bỏ tiền tố chữ cái `A.`/`B.` để người dùng thấy rõ nội dung đáp án đúng.
    - Thêm trường `[SerializeField] private bool showOptionLetter = false` để tùy chỉnh nếu muốn hiển thị lại chữ cái.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
  - `Assets/Scripts/QuestionItemUI.cs`
- **Ảnh hưởng**: Khi import file text có các đáp án trên cùng 1 dòng, 4 ô đáp án A, B, C, D trong form tạo/sửa câu hỏi được điền đầy đủ và đáp án đúng được đưa về ô A màu xanh; thẻ câu hỏi trong danh sách hiển thị tên đáp án đúng rõ ràng, sạch đẹp.


### [2026-10-17 22:45] — refactor(disaster): remove fallback loading & disasterList from Inspector, populate bad luck text in SOs
- **Tác vụ**:
  - **Xóa bỏ fallback và danh sách thiên tai dự phòng trên Inspector (`NaturalDisasterController.cs`)**:
    - Xóa trường `[SerializeField] private List<DisasterSO> disasterList`. Thay bằng danh sách nội bộ runtime `private List<DisasterSO> activeDisasters`.
    - Xóa bỏ hoàn toàn phương thức `EnsureDisastersLoaded()`. Thiên tai nay được nạp 100% từ `ThemeSO.themeDisasters` của chủ đề bản đồ đang chơi. Nếu theme không có danh sách thiên tai thì không tự ý nạp từ Assets/Resources.
    - Cập nhật các hàm `RollDisasterChance()` và `PickRandomDisaster()` sử dụng `activeDisasters`.
  - **Thêm nội dung Bad Luck Text và tỷ lệ thành công vào toàn bộ các asset ScriptableObject (`DisasterSO`)**:
    - Cập nhật trường `badLuckFailureMessage` và cấu hình `successChance: 0.85` cho tất cả 5 thiên tai:
      - `NarrowLedge.asset`: Thông báo khi dùng mỏm đá/neo leo núi nhưng bị nứt vách đá trượt ngã.
      - `Avalanche.asset`: Thông báo khi cố định vị trí nhưng sức ép tuyết lở quá lớn làm sập nơi ẩn nấp.
      - `Bear.asset`: Thông báo khi gấu tuyết quá hung hãn vẫn kịp cào trúng người chơi.
      - `Sleet.asset`: Thông báo khi bão tuyết gió giật làm tốc mái lều trú ẩn.
      - `Wolf.asset`: Thông báo khi sói đầu đàn tinh khôn vẫn lao tới tấn công.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs`
  - `Assets/ScriptableObjects/Disasters/NarrowLedge.asset`
  - `Assets/ScriptableObjects/Disasters/Avalanche.asset`
  - `Assets/ScriptableObjects/Disasters/Bear.asset`
  - `Assets/ScriptableObjects/Disasters/Sleet.asset`
  - `Assets/ScriptableObjects/Disasters/Wolf.asset`
- **Ảnh hưởng**: Inspector của NaturalDisasterController gọn gàng không còn ô Disaster List thừa; sự kiện thiên tai tuân thủ nghiêm ngặt danh sách theo Theme; kịch bản rủi ro không may mắn (bad luck) có lời thoại chuyên biệt cho từng thiên tai.

### [2026-10-17 22:30] — feat(disaster): support theme-specific events in ThemeSO and configurable success chance (0.0 to 1.0)
- **Tác vụ**:
  - **Quản lý danh sách sự kiện/thiên tai theo từng ThemeSO (`ThemeSO.cs`)**:
    - Thêm danh sách `public List<DisasterSO> themeDisasters` vào `ThemeSO`. Cho phép mỗi Theme (Núi tuyết, Sa mạc, Rừng rậm...) tự quản lý danh sách các thiên tai/sự kiện đặc thù của riêng mình.
    - Trong `NaturalDisasterController.cs`: thêm hàm `InitForMap(MapData, ThemeSO)` để nạp danh sách thiên tai tương ứng với Theme của map đang chơi (kèm cơ chế tự động tìm nạp fallback).
    - Trong `MapManager.cs`: gọi `naturalDisaster.InitForMap(mapData)` khi người chơi bắt đầu chặng leo núi.
  - **Thêm tỷ lệ thành công (Success Chance 0.0 -> 1.0) cho từng thiên tai (`DisasterSO.cs`)**:
    - Thêm trường `[Range(0f, 1f)] public float successChance = 1f;` và `badLuckFailureMessage` vào `DisasterSO`.
    - Trong `NaturalDisasterController.cs`: khi người chơi dùng đúng vật phẩm khắc chế, hệ thống sẽ kiểm tra xác suất thành công theo `successChance`. Nếu thành công sẽ không mất HP, nếu gặp rủi ro thất bại theo tỷ lệ sẽ trừ 1 HP và hiện thông báo phù hợp.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/ThemeSO.cs`
  - `Assets/Scripts/DisasterSO.cs`
  - `Assets/Scripts/NaturalDisasterController.cs`
  - `Assets/Scripts/MapManager.cs`
  - `Assets/ScriptableObjects/MapTheme/SnowMountain.asset`
- **Ảnh hưởng**: Cho phép cấu hình sự kiện thiên tai linh hoạt theo từng Theme và tùy biến xác suất thành công cho từng loại hiểm nguy.



### [2026-10-17 17:52] — feat(ui): add configurable character limit and automatic TextMeshPro Ellipsis (...) overflow
- **Tác vụ**:
  - **Hỗ trợ hiển thị dấu ba chấm `...` khi văn bản dài quá giới hạn trên thẻ câu hỏi (`QuestionItemUI.cs`)**:
    - Thêm 2 biến cấu hình Inspector: `maxQuestionLength` (số ký tự tối đa câu hỏi) và `maxAnswerLength` (số ký tự tối đa đáp án), tự động nối `...` khi vượt quá độ dài.
    - Thêm cờ `autoEnableTmpEllipsis` (mặc định bật): tự động kích hoạt `overflowMode = TextOverflowModes.Ellipsis` trên component TextMeshPro để văn bản dài quá khung RectTransform tự động kết thúc bằng `...`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuestionItemUI.cs`
- **Ảnh hưởng**: Thẻ câu hỏi trong danh sách hiển thị gọn gàng, không bị tràn viền hay đè lên các thành phần khác.

### [2026-10-17 17:36] — refactor(edit-map): remove redundant list buttons (viewListBtn and addNewBtn)
- **Tác vụ**:
  - Xóa bỏ 2 nút điều hướng phụ thừa trong `EditMapPanelUI.cs`: `viewListBtn` và `addNewBtn`.
  - Mọi thao tác chuyển tab Tạo câu hỏi và xem Danh sách nay đã được đảm nhiệm thống nhất bởi các nút tab dọc trong `MenuTabPanel`.
  - Giúp Inspector và mã nguồn sạch sẽ, không còn các biến tham chiếu thừa.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Tab danh sách câu hỏi gọn gàng, chỉ tập trung vào 2 thao tác chính: Sửa (`editQuestBtn`) và Xóa (`deleteBtn`).

### [2026-10-17 17:16] — refactor(edit-map): reorganize inspector fields by tabs and refine OnCreateClicked flow
- **Tác vụ**:
  - **Tái cơ cấu Inspector trong `EditMapPanelUI.cs`**:
    - Sắp xếp toàn bộ các trường theo đúng thứ tự 3 Tab: Tab 1 (MainPanel), Tab 2 (CreateQuestionPanel chứa `createBtn`, `resetBtn`, các ô nhập và Import), Tab 3 (ListQuestionsPanel chứa danh sách, `editQuestBtn`, `deleteBtn`).
  - **Tối ưu luồng bấm Tạo/Lưu (`OnCreateClicked`)**:
    - Luôn lưu dữ liệu (`SaveData()`) và tự động xóa sạch các ô nhập (`ClearInputs()`).
    - Nếu vừa chỉnh sửa một câu hỏi cũ (`wasEditing`): tự động quay về Tab 3 (Danh sách) để xem kết quả cập nhật.
    - Nếu tạo câu hỏi mới: ở lại Tab 2 với form đã được làm mới sạch sẽ để sẵn sàng gõ tiếp câu tiếp theo mà không bị gián đoạn.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Giao diện Inspector chuẩn chỉ, trải nghiệm nhập liệu câu hỏi liên tục mượt mà.

### [2026-10-17 17:02] — feat(edit-map): implement MenuTabPanel with 3 vertical tab buttons and Pos X shift
- **Tác vụ**:
  - **Sử dụng 3 nút tab dọc (`MainBtn`, `CreateBtn`, `ListBtn`) trong `MenuTabPanel` (`EditMapPanelUI.cs`)**:
    - Thêm các trường tham chiếu: `menuTabPanel`, `tabMainBtn`, `tabCreateBtn`, `tabListBtn`.
    - Bỏ 2 nút cũ trong `MainPanel` (`mainCreateBtn` và `mainListBtn`), loại bỏ khỏi Inspector.
    - Cập nhật hàm `AutoFindReferences()` tự động tìm chính xác `MenuTabPanel`, các nút con `MainBtn`, `CreateBtn`, `ListBtn`, cũng như các panel `MainPanel`, `CreateQuestionPanel`, `ListQuestionsPanel`, và ô nhập `NameMap`.
    - **Cơ chế đổi vị trí Pos X (RectTransform)**:
      - Nút của tab đang được chọn (active): `anchoredPosition.x = -18f`.
      - Nút của các tab không được chọn (inactive): `anchoredPosition.x = 13f`.
      - Tự động đồng bộ vị trí các nút khi chuyển tab từ `MenuTabPanel`, khi mở form sửa câu hỏi, hoặc khi lưu xong câu hỏi.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Giao diện EditMapPanel điều hướng bằng 3 nút tab dọc hiện đại và trực quan với hiệu ứng đẩy nút ra vào theo Pos X.

### [2026-10-17 16:27] — refactor(edit-map): completely remove toggle and dropdown variables and unused methods
- **Tác vụ**:
  - Dọn dẹp sạch sẽ toàn bộ các biến `toggleA`, `toggleB`, `toggleC`, `toggleD`, `correctDropdown` khỏi `EditMapPanelUI.cs`.
  - Xóa bỏ các phương thức không còn dùng: `SetupCorrectDropdown`, `FindToggleFlexible`, `BindToggleEvents`, `UnbindToggleEvents`, `SetCorrectAnswer`, `HideTogglesIfAny`.
  - Giúp mã nguồn tinh gọn và Inspector không còn hiển thị các trường Toggle dư thừa.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Giao diện Inspector của `EditMapPanelUI` gọn gàng, sạch sẽ, không còn phụ thuộc vào Toggle.

### [2026-10-17 16:22] — feat(edit-map): eliminate toggle requirement in UI, Option A always correct, preserve * in file import
- **Tác vụ**:
  - **Bỏ toggle đúng/sai ở giao diện nhập câu hỏi (`EditMapPanelUI.cs`)**:
    - Tự động ẩn các thành phần Toggle / Dropdown chọn đáp án đúng (`HideTogglesIfAny()`).
    - Quy ước cố định khi tạo hoặc sửa câu hỏi từ form UI: **ô Đáp án A luôn là đáp án đúng** (`correctIndex = 0`).
    - Khi bấm sửa một câu hỏi đã có: nếu câu hỏi ban đầu có đáp án đúng ở vị trí khác (B, C, D từ file import), hệ thống tự động hoán đổi nội dung để đưa đáp án đúng về ô A trên form, đảm bảo người dùng luôn thấy đáp án đúng nằm ở ô A.
    - **Nhập từ file (.txt, .csv) giữ nguyên**: các câu hỏi trong file văn bản hoặc Excel có dấu `*` (ví dụ `*B.`, `*C.`) vẫn được bộ phân tích nhận diện chính xác đáp án đúng theo dấu `*`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Giao diện tạo/sửa câu hỏi gọn gàng, giáo viên chỉ cần nhập câu đúng vào ô A mà không cần click chọn toggle, game vẫn tự động xáo trộn ngẫu nhiên thứ tự các đáp án khi chơi.

### [2026-10-17 16:01] — fix(edit-map): resolve CS0136 duplicate local variable scope error
- **Tác vụ**:
  - Tái sử dụng các biến cục bộ `imgT` và `titleT` trong `AutoFindReferences()` thay vì khai báo lại trong khối con, khắc phục triệt để lỗi biên dịch `CS0136`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Script biên dịch sạch sẽ không còn lỗi trong Unity.

### [2026-10-17 15:55] — feat(edit-map): update Theme button to set mapImg from ThemeSO mapBg and Title text to themeName
- **Tác vụ**:
  - **Sửa nút Theme trong EditMapPanelUI (`Assets/Scripts/EditMapPanelUI.cs`)**:
    - Thêm trường `mapImg` (`Image`) đại diện cho `Map/Mask/Image` của Theme.
    - Cập nhật hàm tìm kiếm sâu và tự động `FindTransformDeep` / `FindChildRecursive` để tự động dò ra các thành phần phân cấp con: `Theme/ThemeBtn`, `Theme/Map/Mask/Image`, `Theme/Map/Title/Text (TMP)` mà không bị lỗi `None` hoặc `Missing` trong Inspector.
    - Trong `UpdateThemeBtnUI()`: gán `mapImg.sprite = matchedTheme.mapBg` (ảnh từ `ThemeSO`) và `themeNameText.text = matchedTheme.themeName` (tên theme hiển thị vào Text trong Title).
    - Trong `OnThemeButtonClicked()`: xoay vòng chuyển đổi theme trong danh sách `availableThemes`, tự động cập nhật `currentMap.themeId`, `currentMap.itemIds` và làm mới giao diện ảnh/tên theme.
    - Hỗ trợ bắt sự kiện click cho cả `themeBtn` và `Map`/`mapImg` nếu được cấu hình Button.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs`
- **Ảnh hưởng**: Giao diện EditMap hiển thị đúng hình ảnh núi và tên theme tương ứng của Theme ScriptableObject khi bấm nút Theme.

### [2026-10-17 15:16] — feat(ui): bind dedicated TimeText component inside QuestionContent
- **Tác vụ**:
  - **Liên kết GameObject con `TimeText` trong thẻ câu hỏi (`QuestionItemUI.cs`)**:
    - Thêm trường `timeText` (`TMP_Text`) vào `QuestionItemUI`.
    - Tự động tìm GameObject con `TimeText` trong prefab `QuestionContent`.
    - Khi hiển thị câu hỏi: gán trực tiếp số giây làm bài vào `timeText` (ví dụ: `30s`, `15s`, `45s`).
    - Nếu thẻ không có `TimeText`, tự động fallback nối chuỗi thời gian vào cuối `correctText`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuestionItemUI.cs` (Lines 14-110)
  - `CHANGELOG.md`

### [2026-10-17 15:10] — feat(quiz): default 30s timer when time is omitted and update import guide text
- **Tác vụ**:
  - **Đặt thời gian mặc định là 30 giây khi câu hỏi/file không có thời gian (`QuizData.cs`, `EditMapPanelUI.cs`, `StationQuizController.cs`)**:
    - Khi người dùng tạo câu hỏi mà không nhập thời gian hoặc file import không có thông tin thời gian, hệ thống tự động gán mặc định là **30s**.
  - **Cập nhật nội dung bảng hướng dẫn Import (`guideText` / `DEFAULT_IMPORT_GUIDE`)**:
    - Thêm mục hướng dẫn chi tiết cách ghi thời gian trong file `.txt` (`[15s]` hoặc `Time: 15s`) và file `.csv` (cột thứ 6/7).
    - Nêu rõ quy tắc: nếu không ghi thời gian thì hệ thống sẽ tự động đặt 30 giây.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (Lines 20-45)
  - `Assets/Scripts/StationQuizController.cs` (Line 50)
  - `Assets/Scripts/EditMapPanelUI.cs` (Lines 146-170, 970-985, 1030-1045, 1175-1235)
  - `CHANGELOG.md`

### [2026-10-17 15:05] — feat(quiz): individual question time limit (per-question timer)
- **Tác vụ**:
  - **Hỗ trợ thời gian làm bài riêng cho từng câu hỏi (`QuizData.cs`, `EditMapPanelUI.cs`, `QuestionItemUI.cs`, `StationQuizController.cs`)**:
    - **Mô hình dữ liệu (`QuizData.cs`)**: Thêm trường `public float timeLimit = 20f;` vào `QuestionData` và cập nhật các constructor tương ứng.
    - **Trình soạn thảo Map (`EditMapPanelUI.cs`)**:
      - Thêm ô nhập `timeInput` để người dùng có thể nhập số giây mong muốn cho từng câu hỏi khi tạo mới hoặc sửa.
      - Tự động nạp thời gian của câu hỏi khi bấm sửa (`OnEditQuestionItem`).
      - Cập nhật bộ Import câu hỏi thông minh (`ParseQuestionsFromText`): hỗ trợ nhận diện thời gian từ file `.txt` (`Time: 15s`, `[15s]`, `Thời gian: 15`) và cột thứ 6/7 trong file `.csv`.
    - **Thẻ hiển thị danh sách (`QuestionItemUI.cs`)**: Hiển thị nhãn thời gian `⏱ [time]s` trên thẻ câu hỏi để người dùng dễ theo dõi.
    - **Hệ thống đếm giờ trả lời trạm (`StationQuizController.cs`)**:
      - Trong `DisplayQuestion(q)`, tự động đọc `q.timeLimit` của câu hỏi hiện tại để đếm ngược (nếu không có thì dùng mặc định 20s).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (Lines 19-45)
  - `Assets/Scripts/EditMapPanelUI.cs` (Lines 59-65, 260-270, 924-930, 960-985, 1020-1030, 1150-1200)
  - `Assets/Scripts/QuestionItemUI.cs` (Lines 90-100)
  - `Assets/Scripts/StationQuizController.cs` (Lines 1125-1135)
  - `CHANGELOG.md`
- **Ảnh hưởng**:
  - Người thiết kế map có thể thoải mái tạo câu hỏi khó với thời gian dài hơn (ví dụ 45s, 60s) hoặc câu hỏi phản xạ nhanh (10s, 15s) tùy ý.

### [2026-10-17 14:52] — fix(ui): assign item icon sprite directly to child ItemImg instead of root ItemBtn
- **Tác vụ**:
  - **Khắc phục lỗi không nhìn thấy ảnh icon vật phẩm ở nút trạm (`StationQuizController.cs`)**:
    - Nguyên nhân: Trước đó code gọi `btnObj.GetComponent<Image>()`, gán sprite vào Image của GameObject gốc (`ItemBtn`) - nơi có `alpha = 0` (làm vùng bấm vô hình cho button). Trong khi đó, GameObject con hiển thị ảnh thực tế là `ItemImg` lại không hề được gán sprite.
    - Khắc phục: Sử dụng `FindChildRecursive(btnObj.transform, "ItemImg", "Icon", "Image")` để tìm chính xác component `Image` của `ItemImg`, đồng thời thiết lập `sprite = itemData.icon`, `color = Color.white`, `preserveAspect = true` và `SetActive(true)`.
    - Thêm cơ chế tự động tra cứu fallback từ `itemDataLookup` nếu `itemData.icon` bị rỗng.
    - Áp dụng tương tự cho các item hiển thị trên `MainPanel` (`itemContentPrefab`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (Lines 415-430, 880-935)
  - `CHANGELOG.md`
- **Ảnh hưởng**:
  - Icon vật phẩm hiển thị sắc nét, đúng tỷ lệ tại `ItemImg` bên trong mỗi nút vật phẩm trạm và kho đồ.

### [2026-10-17 14:05] — feat(navigation): mutually exclusive Climb and Join actions to prevent UI conflicts
- **Tác vụ**:
  - **Khóa chéo nút Climb và nút Join (`MapManager.cs`, `StationQuizController.cs`)**:
    - **Khi bấm Climb (hoặc nhân vật đang leo núi / `IsPlayerMoving`)**:
      - Ẩn ngay lập tức nút Join (`stationQuiz.HideJoinPrompt()`) để người chơi không thể bấm Join khi đang trên đường leo giữa 2 trạm hoặc đang xử lý sự kiện thiên tai.
      - Tạm thời vô hiệu hóa nút Climb (`climbBtn.interactable = false`) trong lúc leo núi. Chỉ khi đã đặt chân tới trạm tiếp theo an toàn (`onComplete`) thì mới bật lại (`climbBtn.interactable = true`).
      - Chặn trong hàm `OnJoinButtonClicked()`: nếu nhân vật đang leo núi thì không xử lý và log cảnh báo.
    - **Khi bấm Join (đang mở StationPanel hoặc đang làm QuestPanel)**:
      - Ẩn nút Climb (`climbBtn.gameObject.SetActive(false)`) thông qua sự kiện `OnStationOpenStateChanged(true)` để người chơi không thể vừa trả lời câu hỏi vừa bấm leo núi.
      - Chặn trong hàm `OnClimbButtonClicked()`: nếu `stationQuiz.IsStationOpen` đang là `true` thì không cho phép di chuyển.
      - Khi bấm Exit trạm (`OnExitButtonClicked`) hoặc thoát map: mở lại nút Climb (`climbBtn.gameObject.SetActive(true)`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (Lines 80-92, 720-785)
  - `Assets/Scripts/MapManager.cs` (Lines 212-220, 395-405, 442-452, 475-520)
  - `CHANGELOG.md`
- **Ảnh hưởng**:
  - Loại bỏ hoàn toàn tình trạng xung đột trạng thái (bấm Join khi đang leo hoặc bấm Climb khi đang mở trạm làm bài). Luồng game liền mạch và an toàn.

### [2026-10-17 13:36] — fix(disaster-text): concise failure text with newline -1 HP instead of verbose deducted HP message
- **Tác vụ**:
  - **Chuẩn hóa thông điệp thất bại & fallback trong đối phó thiên tai (`NaturalDisasterController.cs`)**:
    - Thay thế các đoạn văn bản rườm rà "Bạn bị trừ 1 Tim (-1 HP)! Tim còn lại: x/y." ở các trường hợp Hết thời gian, Thiếu vật phẩm trong túi, hoặc fallback mặc định.
    - Chuyển thành cú pháp ngắn gọn, trực quan: xuống dòng `<color=#E57373><b>-1 HP</b></color>`.
    - Nâng cấp `FormatDialogTemplate`: tự động nhận diện `-1 HP` / `-1 Hp` / `-1hp` trong các mẫu tin ScriptableObject và tô màu đỏ đậm bắt mắt.
  - **Cập nhật `failureMessage` trong các asset DisasterSO (`Assets/ScriptableObjects/Disasters/`)**:
    - `Avalanche.asset`: Rút gọn thông điệp kết thúc bằng `\n-1 HP`.
    - `Sleet.asset`: Rút gọn thông điệp kết thúc bằng `\n-1 HP`.
    - `Bear.asset`: Rút gọn thông điệp kết thúc bằng `\n-1 HP`.
    - `Wolf.asset`: Rút gọn thông điệp kết thúc bằng `\n-1 HP`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (Lines 448-460, 583-603)
  - `Assets/ScriptableObjects/Disasters/Avalanche.asset` (Line 30)
  - `Assets/ScriptableObjects/Disasters/Sleet.asset` (Line 30)
  - `Assets/ScriptableObjects/Disasters/Bear.asset` (Line 32)
  - `Assets/ScriptableObjects/Disasters/Wolf.asset` (Line 32)
  - `CHANGELOG.md`
- **Ảnh hưởng**:
  - Giao diện thông báo dialog ngắn gọn, tinh tế, người chơi nắm bắt ngay lượng máu mất mà không bị rối mắt bởi câu chữ dài dòng.

### [2026-10-17 13:15] — fix(disaster-ui): keep natural disaster panel visible behind dialog until confirmation before closing blackscreen
- **Tác vụ**:
  - **Giữ Panel Thiên Tai (`naturalDisastersPanel`) hiển thị trong suốt quá trình hiển thị Dialog (`NaturalDisasterController.cs`)**:
    - Khắc phục lỗi panel thiên tai bị tắt quá sớm (`SetActive(false)`) làm mất hình ảnh và tên thiên tai khi dialog kết quả đang mở.
    - **Khi hết thời gian (không chọn item)**: Chỉ ẩn `selectItemContainer` (vùng nút), giữ nguyên ảnh và tên thiên tai trên màn hình, mở Dialog Thất Bại ("⌛ HẾT THỜI GIAN!").
    - **Khi không có bất kỳ vật phẩm nào trong túi đồ**: Thêm coroutine `CoHandleNoItems`, hiển thị panel thiên tai rồi mở Dialog Thất Bại ("⚠️ THIẾU VẬT PHẨM! - Túi đồ của bạn không có bất kỳ vật phẩm nào để đối phó...").
    - **Khi người chơi chọn vật phẩm (đúng hoặc sai)**: Giữ nguyên panel thiên tai phía sau dialog thành công/thất bại.
  - **Đồng bộ thời điểm đóng Panel & che Màn Đen**:
    - Chỉ sau khi người chơi bấm nút "Tiếp Tục" (hoặc dialog auto-close xong):
      1. Tắt `naturalDisastersPanel`.
      2. Màn đen che lại (`CutSceneTransition.Close`).
      3. Nhân vật tiếp tục di chuyển tới trạm tiếp theo (`onResume?.Invoke()`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (Modified)
- **Ảnh hưởng**:
  - Trải nghiệm thị giác liền mạch, người chơi luôn nhìn thấy rõ thiên tai/quái vật đang đối đầu trong khi đọc thông điệp dialog kết quả.

### [2026-10-17 12:45] — feat(station-layout): stations start from point 2 to point n-1 labeled 'Trạm [n]' with bottom & summit as camps
- **Tác vụ**:
  - **Tái cấu trúc bố cục trạm trên đường mòn núi (`MountainTrailGenerator.cs`, `StationWorldItem.cs`)**:
    - **Điểm 1 (Chân núi / BottomPos)**: Là điểm xuất phát ban đầu của người chơi, KHÔNG sinh GameObject trạm.
    - **Trạm 1**: Bắt đầu chính xác từ điểm thứ 2 trên đường mòn (`i = 1`), hiển thị text rõ ràng là **`Trạm 1`**.
    - **Trạm 2 ... Trạm N**: Các điểm kế tiếp được đánh số thứ tự liên tục **`Trạm 2`**, **`Trạm 3`**...
    - **Trạm cuối cùng**: Kết thúc ở điểm ngay trước đỉnh núi (`i = total - 2`).
    - **Điểm đỉnh núi (TopPos / Summit)**: Là đích đến cuối cùng, KHÔNG sinh GameObject trạm và tạm thời chưa kích hoạt màn hình win.
  - **Cập nhật hiển thị Text (`StationWorldItem.cs`)**:
    - Text hiển thị chuẩn xác dạng: `Trạm {stationNumber}` (ví dụ: `Trạm 1`, `Trạm 2`...).
    - Xóa bỏ text riêng `🏔️ Đỉnh` và xóa màu sắc tint riêng của bottom/summit để các trạm đồng bộ và hiển thị nguyên bản hình ảnh.
  - **Kiểm soát nút tham gia trạm (`MapManager.cs`)**:
    - Khi người chơi đứng ở chân núi (index 0) hoặc leo tới đỉnh núi (index cuối), không hiển thị popup nút Join câu hỏi. Chỉ hiển thị khi đến các trạm thực sự ở giữa (từ Trạm 1 đến trạm cuối).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainTrailGenerator.cs` (Modified)
  - `Assets/Scripts/StationWorldItem.cs` (Modified)
  - `Assets/Scripts/MountainStationManager.cs` (Modified)
  - `Assets/Scripts/MapManager.cs` (Modified)
- **Ảnh hưởng**:
  - Lộ trình leo núi logic và tự nhiên: xuất phát từ chân núi -> leo dần qua Trạm 1, Trạm 2... -> trạm áp chót -> chạm đỉnh núi.

### [2026-10-17 12:35] — feat(station-visuals): add random station sprite pool, random flip X, and label toggling
- **Tác vụ**:
  - **Hỗ trợ Random Sprite đa dạng cho các trạm trên sườn núi (`MountainTrailGenerator.cs`)**:
    - Thêm trường `stationSprites` (`List<Sprite>`): Cho phép kéo thả nhiều hình ảnh trạm khác nhau (lều, trạm trú ẩn, nhà gỗ, cột mốc...) trong Inspector.
    - Thêm trường `summitStationSprite` (`Sprite`): Dành riêng cho trạm đỉnh núi cao nhất (ví dụ: cờ chiến thắng, ngôi đền, chóp đỉnh).
    - Mỗi lần bấm Play hoặc tạo lại map, mỗi trạm sẽ chọn ngẫu nhiên 1 Sprite từ danh sách này.
  - **Tự động lật ngẫu nhiên chiều ngang (`SpriteRenderer.flipX`)**:
    - Thêm thuộc tính `randomFlipX` (mặc định bật): Mỗi trạm có 50% cơ hội lật gương hình ảnh theo trục X.
    - Sử dụng `SpriteRenderer.flipX` giúp lật hình ảnh trạm tự nhiên mà hoàn toàn **không làm ngược chữ số** của `NameText` (TextMeshPro) con.
  - **Quản lý linh hoạt nhãn Text số trạm (`StationWorldItem.cs`)**:
    - Thêm `showStationText` để người dùng có thể tùy ý bật hoặc tắt text số thứ tự trạm trên sườn núi.
    - Thêm `applyColorTint` (mặc định tắt) để giữ nguyên màu sắc gốc sắc nét của các Sprite vẽ sẵn.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainTrailGenerator.cs` (Modified)
  - `Assets/Scripts/StationWorldItem.cs` (Modified)
- **Ảnh hưởng**:
  - Các trạm trên đường mòn núi tuyết giờ đây sống động, đa dạng và không còn bị trùng lặp 1 hình ảnh cố định.

### [2026-10-17 10:50] — feat(disaster-wolf): add Wolf (Đàn Sói Tuyết) DisasterSO with multiple item counters
- **Tác vụ**:
  - **Khởi tạo ScriptableObject Chó Sói Tuyết (`Assets/ScriptableObjects/Disasters/Wolf.asset`)**:
    - Sprite: `Assets/Sprite/NaturalDisaster/Worf.png`
    - Tên hiển thị: "Đàn sói tuyết" (ID: `disaster_wolf`).
    - Kéo thả danh sách vật phẩm khắc chế:
      - `Meat` (`item_meat`): Ném thịt sống đánh lạc hướng bầy sói đói.
      - `Flare` (`item_flare`): Bắn pháo sáng xua đuổi bầy sói sợ lửa và ánh sáng chói.
      - `BearSpray` (`item_bearspray`): Xịt cay tự vệ xua đuổi dã thú hung dữ.
    - Soạn thảo thông điệp cảnh báo, thành công và thất bại chân thực, cuốn hút.
  - **Liên kết vào Scene (`SampleScene.unity`)**:
    - Thêm `Wolf.asset` vào `disasterList` của `NaturalDisasterController`.
- **Danh sách file thay đổi**:
  - `Assets/ScriptableObjects/Disasters/Wolf.asset` (New)
  - `Assets/ScriptableObjects/Disasters/Wolf.asset.meta` (New)
  - `Assets/Scenes/SampleScene.unity` (Modified)
- **Ảnh hưởng**:
  - Tăng độ phong phú cho các sự kiện thiên tai/thú dữ ngẫu nhiên khi leo núi tuyết.

### [2026-10-17 10:35] — feat(disaster-so): migrate natural disaster system to ScriptableObjects (DisasterSO) with ItemSO drag-and-drop counters
- **Tác vụ**:
  - **Chuyển đổi toàn bộ hệ thống dữ liệu thiên tai sang ScriptableObject (`DisasterSO.cs`)**:
    - Tạo `DisasterSO` kế thừa `ScriptableObject` với menu `Quiz/Disaster Data` (order = 3).
    - Hỗ trợ trường kéo thả danh sách vật phẩm khắc chế: `public List<ItemSO> counterItems`.
    - Hỗ trợ trường fallback chuỗi string: `counterItemId` và `counterItemIds`.
    - Hàm `GetAllValidCounterItems()` tự động hợp nhất cả các file `ItemSO` được kéo thả và mã ID chuỗi dự phòng, đồng thời chuẩn hóa tên/ID khi người chơi chọn.
    - Cung cấp 3 trường cấu hình nội dung thông điệp riêng cho thiên tai: `warningMessage`, `successMessage`, `failureMessage` (với các thẻ `{item}`, `{disaster}`, `{hp}`, `{maxhp}`).
  - **Tái cấu trúc bộ điều khiển thiên tai (`NaturalDisasterController.cs`)**:
    - Thay thế hoàn toàn class lồng `DisasterData` bằng `DisasterSO`.
    - Danh sách thiên tai trong Inspector giờ là `[SerializeField] private List<DisasterSO> disasterList`.
    - Cập nhật an toàn tất cả các phương thức hiển thị dialog, đếm ngược và kiểm tra vật phẩm khắc chế.
  - **Khởi tạo các file dữ liệu thiên tai ScriptableObject (`Assets/ScriptableObjects/Disasters/`)**:
    - `Avalanche.asset`: Sạt lở tuyết, khắc chế bằng `SnowAnchor` và `Tent`.
    - `Sleet.asset`: Bão tuyết, khắc chế bằng `Tent`.
    - `Bear.asset`: Gấu tuyết hung dữ, khắc chế bằng `BearSpray`, `Flare`, hoặc `Meat`.
  - **Cập nhật liên kết trong Scene (`SampleScene.unity`)**:
    - Trỏ `disasterList` của `NaturalDisasterController` trực tiếp vào 3 asset `DisasterSO` mới tạo.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/DisasterSO.cs` (New)
  - `Assets/Scripts/DisasterSO.cs.meta` (New)
  - `Assets/Scripts/NaturalDisasterController.cs` (Modified)
  - `Assets/ScriptableObjects/Disasters/` (New directory & meta)
  - `Assets/ScriptableObjects/Disasters/Avalanche.asset` + `.meta` (New)
  - `Assets/ScriptableObjects/Disasters/Sleet.asset` + `.meta` (New)
  - `Assets/ScriptableObjects/Disasters/Bear.asset` + `.meta` (New)
  - `Assets/Scenes/SampleScene.unity` (Modified)
- **Ảnh hưởng**:
  - Dễ dàng tạo mới bất kỳ sự kiện thiên tai nào trong Unity Editor qua menu `Create -> Quiz -> Disaster Data` và kéo thả trực tiếp các file `ItemSO` vào để thiết lập cách khắc chế mà không cần phải gõ code hay chuỗi ID bằng tay.

### [2026-10-17 09:20] — feat(disaster-dialog): separate dialogs for success & failure events with custom narratives and UI themes
- **Tác vụ**:
  - **Hệ thống Dialog riêng biệt cho các sự kiện Đối phó Thành công và Thất bại (`NaturalDisasterController.cs`)**:
    - **Cấu hình thông điệp riêng cho từng thiên tai (`DisasterData`)**:
      - Cấu hình thông điệp trong Inspector cho từng sự kiện: chỉ gồm 3 ô văn bản `warningMessage`, `successMessage`, `failureMessage` (toàn bộ tiêu đề cảnh báo/thành công/thất bại được cố định chuẩn mực tự động).
      - Hỗ trợ các thẻ thay thế tự động thông minh: `{item}` (tên item màu xanh lá nổi bật), `{item_raw}`, `{disaster}` (tên thiên tai màu vàng hổ phách), `{disaster_raw}`, `{hp}` (số tim hiện tại), `{maxhp}` (tổng số tim tối đa).
      - Tự động cung cấp văn bản mẫu sống động (Rich-text narrative fallback) cho Bão Tuyết Lở, Thú Dữ Tuyết,... nếu để trống.
    - **Sử dụng duy nhất 1 Dialog Panel động (Single Dynamic Dialog Panel)**:
      - Sử dụng trực tiếp `disasterDialogPanel` trong Scene và tự động chuyển đổi theme màu sắc (Xanh lá / Đỏ / Vàng) tương ứng với từng trạng thái:
        - **Thành công (Success)**: Màu xanh lá ngọc (`#81C784`), nền xanh rêu sang trọng, tôn vinh chiến thắng.
        - **Thất bại (Failure)**: Màu đỏ san hô / Crimson (`#E57373`), nền đỏ sẫm cảnh báo nguy hiểm khi mất máu.
        - **Cảnh báo (Warning)**: Màu vàng hổ phách (`#FFD54F`), nền xanh navy huyền ảo.
      - Gỡ bỏ hoàn toàn các trường khai báo panel riêng biệt rườm rà (`successDialogPanel`, `failureDialogPanel`...) giúp Inspector gọn gàng, tinh tươm.
    - **Kế thừa đầy đủ cơ chế tương tác mượt mà**:
      - Hiệu ứng gõ chữ typewriter.
      - Bấm nút hoặc click bất cứ đâu trên màn hình để **Skip nhảy full text ngay lập tức**.
      - Bộ đếm thời gian tự động đóng (`defaultDialogAutoCloseTime`) đóng panel và tiếp tục chặng leo núi mượt mà.
    - **Tối ưu & Tinh gọn Code**:
      - Loại bỏ hàm tạo UI tự động `CreateDynamicDialogPanel()` và hàm nạp dữ liệu mẫu `SetupDefaultDisastersIfEmpty()` (tiết kiệm hơn 250 dòng code thừa), giúp Controller tập trung 100% vào việc xử lý logic và đọc trực tiếp dữ liệu từ Inspector/Scene.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (Mở rộng `DisasterData`, thêm `DisasterDialogType`, `FormatDialogTemplate`, `ShowSuccessDialog`, `ShowFailureDialog`, `ShowWarningDialog`, `SetupDialogReferences`, cập nhật `OnItemSelected` và `CoDisasterCountdown`, loại bỏ các panel riêng thừa và tối ưu hóa mã nguồn).
- **Ảnh hưởng**: Hoàn toàn tương thích ngược với các scene cũ, mang lại trải nghiệm cốt truyện và thị giác khác biệt rõ rệt giữa thành công và thất bại.

### [2026-10-16 13:38] — feat(disaster-dialog): typewriter warning & result dialogs with skip to full text and auto-close timer
- **Tác vụ**:
  - **Tích hợp hệ thống Dialog thông báo thiên tai (`NaturalDisasterController.cs`)**:
    - **Dialog Cảnh Báo (trước khi tắt blackpanel)**:
      - Khi gặp thiên tai tại 50% chặng leo, màn đen vẫn che kín và hiển thị Dialog cảnh báo tên thiên tai đang xuất hiện, yêu cầu chuẩn bị chọn 1 item đối phó.
      - Chữ chạy ra từng ký tự mượt mà theo hiệu ứng Typewriter (`maxVisibleCharacters`).
      - Bấm nút hoặc click màn hình để **Skip nhảy full text ngay lập tức**! Bấm tiếp sẽ đóng dialog, tắt màn đen (`Open()`), hiện bảng chọn item và đếm ngược 10s.
      - Nếu không bấm gì: sau 3.5s sẽ tự động mở màn và vào sự kiện đối phó.
    - **Dialog Kết Quả (sau khi chọn xong item hoặc hết giờ)**:
      - Ẩn ngay panel chọn item và hiển thị Dialog thông báo kết quả:
        - Thành công: Ghi chú rõ item đã đối phó thành công thiên tai, không mất HP.
        - Thất bại: Ghi chú rõ item không thể đối phó, bị trừ 1 HP và mất 1 item.
        - Hết giờ: Ghi chú không kịp chọn item, bị trừ 1 HP.
      - Hỗ trợ gõ chữ typewriter, click skip full text, và tự động đóng sau 3.5s để bật lại màn đen (`Close()`) rồi tiếp tục hành trình leo núi.
    - **Hỗ trợ tạo UI động thông minh**:
      - Tự động dựng giao diện `DisasterDialogPanel` cực đẹp (backdrop mờ, khung card navy, tiêu đề, nội dung, nút bấm) nếu người dùng chưa tự thiết kế trong Scene, đồng thời hỗ trợ kéo thả component tùy chỉnh qua Inspector.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (lines 115-145, 230-380, 420-560, 750-840 modified)
- **Ảnh hưởng**:
  - Trải nghiệm cốt truyện và nhịp điệu game được nâng cấp rõ rệt, người chơi nắm rõ thông tin trước khi đối phó và hiểu rõ kết quả sau khi chọn item.

### [2026-10-16 12:45] — feat(disaster-multi-counter): support multiple counter items per disaster and consume item on wrong selection
- **Tác vụ**:
  - **Hỗ trợ thiên tai khắc chế bằng nhiều item (`NaturalDisasterController.cs`)**:
    - Bổ sung `counterItemIds` (dạng `List<string>`) vào class `DisasterData`.
    - Người dùng có thể cấu hình 1 hoặc nhiều item ID khắc chế cho 1 thiên tai (ví dụ truyền 2 item: `item_snowanchor1` hoặc `item_emergencytent1`).
    - Thêm hàm `GetAllValidCounterItems()` và `IsItemValidForDisaster()`: chỉ cần người chơi chọn đúng 1 trong các item hợp lệ thì sự kiện sẽ thành công, tiêu hao 1 item đó và không bị trừ HP.
    - Giữ nguyên tính tương thích ngược với trường `counterItemId` đơn lẻ cũ.
  - **Cơ chế tiêu hao item khi chọn sai**:
    - Khi người chơi chọn **SAI** item:
      - Vẫn **tiêu hao 1 item** đã chọn đó (`stationQuiz.ConsumeItem(chosenItemId, 1)`).
      - Đồng thời **bị trừ 1 HP** (`TakeDamage(1)`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (lines 35-60, 220-250, 500-580, 700-750 modified)
  - `Assets/Scripts/StationQuizController.cs` (lines 430-450 modified)
- **Ảnh hưởng**:
  - Cho phép gameplay giải quyết thiên tai linh hoạt hơn với nhiều giải pháp item khác nhau.
  - Tăng độ thử thách và rủi ro: việc chọn bừa hoặc sai item sẽ làm mất cả tim (HP) lẫn item đã dùng.

### [2026-10-15 17:50] — feat(cutscene-anim): add walking character sprite animation to cutscene black screen
- **Tác vụ**:
  - **Tạo component chuyên dụng `UISpriteAnimation.cs`**:
    - Hoạt ảnh sprite mượt mà trực tiếp trên `UnityEngine.UI.Image` trong Canvas.
    - Không cần Animator Controller hay AnimationClip dành cho 2D SpriteRenderer (vốn gây xung đột typeID và lỗi scale/pivot trên UI).
    - Tự động nạp sprite từ `Character.png`, hỗ trợ tùy chỉnh tốc độ FPS, vòng lặp (`loop`), và giữ tỷ lệ nhân vật (`preserveAspect`).
  - **Tích hợp vào `CutSceneTransition.cs`**:
    - Thêm tham chiếu `walkingCharacter` (tự động tìm `CutScreenPanel/WalkingCharacter` hoặc `CutScreenPanel/Character`).
    - Khi đóng màn đen (`Close()` / `SetClosedImmediate()`): tự động bật nhân vật bước đi ở giữa màn hình đen.
    - Khi mở màn lộ game (`Open()` / `SetOpenImmediate()`): tự động tắt nhân vật bước đi.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/UISpriteAnimation.cs` (New file)
  - `Assets/Scripts/CutSceneTransition.cs` (lines 35-45, 150-185, 275-315 modified)
- **Ảnh hưởng**:
  - Cho phép hiển thị hoạt ảnh nhân vật leo núi/đi bộ ngay trên màn đen chuyển cảnh một cách tự nhiên và mượt mà, giải quyết triệt để lỗi không chỉnh được Pivot khi dùng Sprite trong Canvas.

### [2026-10-15 16:10] — fix(disaster-matching & quiz-reward): award exact clicked item and support flexible item matching
- **Tác vụ**:
  - **Sửa lỗi trả thưởng nhầm item khi trả lời câu hỏi trạm dừng (`StationQuizController.cs`)**:
    - Trước đó `RewardThemeItem(1)` lấy ngẫu nhiên 1 item trong theme (`Random.Range`), khiến người chơi bấm chọn Campfire để làm câu hỏi nhưng lại bị thưởng nhầm Meat.
    - Đã sửa để `HandleCorrectAnswer()` truyền chính xác ID item người chơi đang làm (`currentActiveItemData.itemId`), đảm bảo làm Campfire thì nhận đúng 100% là Campfire.
    - Bổ sung hàm `NormalizeKey()` chuẩn hóa chuỗi và đối chiếu thông minh (chấp nhận cả `item_camfire1`, `item_campfire1`, `Campfire`, `campfire`) trong `RewardThemeItem`, `GetItemQuantity`, `ConsumeItem`, và `GetItemSO`.
  - **Sửa lỗi chọn đúng Campfire đối phó thiên tai mà vẫn bị trừ HP (`NaturalDisasterController.cs`)**:
    - **Nguyên nhân**: File `Campfire.asset` khai báo `itemId: item_camfire1` (thiếu chữ 'p'), trong khi trong Scene Inspector người dùng cấu hình thiên tai có `counterItemId: item_campfire1` (có chữ 'p'). So sánh trực tiếp `chosenItemId == disaster.counterItemId` dẫn tới `false` và trừ 1 HP vô lý.
    - **Giải pháp**:
      - Thêm hàm `NormalizeItemKey()` và `CheckItemMatches()`: tự động đồng bộ lỗi chính tả `camfire` <-> `campfire`, bỏ tiền tố `item_`, bỏ số thứ tự cuối, bỏ ký tự đặc biệt, và tra cứu qua `ItemSO`.
      - Khắc phục biến closure trong vòng lặp `PopulateSelectItems` (`string capturedId = itemId;`).
      - Khử trùng lặp (Deduplicate) danh sách item để không bao giờ sinh thừa nút trùng loại item.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (lines 430-610 modified)
  - `Assets/Scripts/NaturalDisasterController.cs` (lines 460-630 modified)
- **Ảnh hưởng**:
  - Người chơi luôn nhận đúng item mình đã chọn làm bài tại trạm dừng.
  - Chọn Campfire đối phó bão tuyết lở (Avalanche) được nhận diện chính xác 100%, trừ 1 Campfire và không bao giờ bị trừ HP oan.

### [2026-10-15 15:35] — feat(natural-disasters-hp): 5 HP hearts and midway natural disaster event with item counter selection
- **Tác vụ**:
  - **Tạo hệ thống `NaturalDisasterController.cs`**:
    - **Hệ thống 5 Tim (HP)**:
      - Quản lý 5 tim trong `GamePanel/MainPanel/HpContainer` bằng `Hp.prefab`.
      - Khởi tạo 5 tim khi bắt đầu map, tự động gắn `HorizontalLayoutGroup` để tim căn lề ngang gọn gàng.
      - Cung cấp hàm `TakeDamage(1)` để trừ tim (tắt dần tim từ phải sang trái khi chọn sai item) và kiểm tra trạng thái sống `IsAlive`.
    - **Hệ thống Thiên Tai (Natural Disasters)**:
      - Danh sách thiên tai gồm tên, hình ảnh (`disasterSprite` như `AvalancheDisasters.png`), và mã `counterItemId` cần thiết để chống lại (ví dụ `item_camfire1` cho Bão Tuyết, `item_meat1` cho Thú Dữ).
      - Xác suất xuất hiện thiên tai có thể cấu hình linh hoạt (`disasterEventChance`), có cờ `alwaysTriggerDisaster` để test.
    - **Sự kiện giữa chặng leo núi (Midway Event at 50%)**:
      - Khi màn đen đang bật và người chơi tới mốc giữa 2 trạm ($50\%$): nếu có thiên tai, người chơi dừng lại tại điểm giữa, màn đen tự động mở ra (`CutSceneTransition.Open()`).
      - Bật `NaturalDisastersPanel`, thay ảnh thiên tai vào `NaturalDisastersPanel/Image`.
      - Hiển thị tên thiên tai vào `NaturalDisastersPanel/Name`.
      - Khởi động bộ đếm ngược thời gian tại `NaturalDisastersPanel/Time` (mặc định 10s). Nếu hết giờ mà chưa chọn item thì bị trừ 1 HP!
      - Sinh các nút chọn item trong `NaturalDisastersPanel/SelectItemContainer` từ `SelectItemBtn.prefab`: **chỉ hiển thị những item có số lượng sở hữu $> 0$**, hiển thị tên (`NameText`) và số lượng (`NumberText`).
    - **Xử lý lựa chọn item**:
      - Dừng đếm ngược thời gian ngay khi người chơi bấm nút.
      - Nếu chọn đúng item khắc chế: Không bị trừ HP, trừ 1 số lượng item đã dùng.
      - Nếu chọn sai item (hoặc hết thời gian): Trừ 1 HP (mất 1 tim).
      - Sau đó: Ẩn `NaturalDisastersPanel`, bật lại màn đen (`CutSceneTransition.Close()`), người chơi tiếp tục leo và mở màn (`CutSceneTransition.Open()`) khi gần tới trạm tiếp theo.
  - **Cập nhật `MountainPlayerController.cs` & `MountainStationManager.cs`**:
    - Bổ sung callback `Action<Action> onMidwayEvent` tại $t = 0.5f$ để dừng người chơi ở giữa đường và chờ sự kiện thiên tai hoàn thành rồi mới đi tiếp.
  - **Cập nhật `StationQuizController.cs`**:
    - Thêm các hàm helper: `GetItemQuantity()`, `ConsumeItem()`, `GetInventoryItems()`, `GetCurrentThemeItems()`.
  - **Cập nhật `MapManager.cs`**:
    - Tự động liên kết `NaturalDisasterController`, khởi tạo 5 tim khi bắt đầu chơi (`OpenGamePlay`), và kích hoạt thiên tai giữa chặng khi leo (`OnClimbButtonClicked`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/NaturalDisasterController.cs` (New file)
  - `Assets/Scripts/StationQuizController.cs` (lines 450-520 modified)
  - `Assets/Scripts/MountainPlayerController.cs` (lines 80-175 modified)
  - `Assets/Scripts/MountainStationManager.cs` (lines 325-365 modified)
  - `Assets/Scripts/MapManager.cs` (lines 25-475 modified)
- **Ảnh hưởng**:
  - Tích hợp hoàn chỉnh vòng lặp gameplay: Leo núi $\to$ gặp thiên tai ngẫu nhiên giữa chặng $\to$ dùng item tích lũy được từ trạm để sinh tồn $\to$ bảo toàn 5 tim đến đỉnh núi.

### [2026-10-15 15:05] — feat(cutscene-transition): single-panel fade transition for climb and scene transitions
- **Tác vụ**:
  - **Tạo hệ thống chuyển cảnh `CutSceneTransition.cs`**:
    - Quản lý hiệu ứng chuyển cảnh bằng 1 tấm màn đen duy nhất (`fadeImage` kết hợp `CanvasGroup`).
    - Hỗ trợ 2 trạng thái mượt mà:
      - **Hiện rõ (`Close()` / Fade In)**: Alpha tăng từ $0 \to 1$ để che kín màn hình (mặc định `0.22s`, có thể tùy biến `duration`).
      - **Mờ dần (`Open()` / Fade Out)**: Alpha giảm từ $1 \to 0$ để lộ cảnh game ra (mặc định `0.35s`).
    - Cung cấp các hàm tĩnh tiện dụng: `CutSceneTransition.Close()`, `Open()`, `DelayedClose()`, `CrossFade()`, `DelayedCrossFade()`.
    - Tự động cấu hình RectTransform full screen, tự động thêm/tìm `CanvasGroup` và tự chặn tương tác nút bấm (`blocksRaycasts = true`) khi màn hình đang bị che.
  - **Tích hợp trình tự chuyển cảnh khi bấm Climb (`MapManager.cs`, `MountainPlayerController.cs`, `MountainStationManager.cs`)**:
    - Khi bấm **ClimbBtn**: Nhân vật lập tức bắt đầu bước đi (người chơi nhìn thấy nhân vật di chuyển trước).
    - **Vừa đi được một đoạn ngắn (`departThreshold = 0.10` ~ 10% chặng đường)**: Kích hoạt `onDepart` $\to$ Màn đen hiện rõ nhanh (`CutSceneTransition.Close()`).
    - **Trong lúc che đen**: Nhân vật tiếp tục leo và Camera bám theo.
    - **Khi gần tới trạm tiếp theo (`nearThreshold = 0.78` ~ 78% chặng đường)**: Kích hoạt `onNearComplete` $\to$ Màn đen mờ dần (`CutSceneTransition.Open()`) để lộ trạm mới và nhân vật bước vào đích.
    - Cả `departThreshold` và `nearThreshold` đều được đưa lên Inspector của `MountainPlayerController` để tùy chỉnh trực quan.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/CutSceneTransition.cs` (New file)
  - `Assets/Scripts/MountainPlayerController.cs` (lines 70-140 modified)
  - `Assets/Scripts/MountainStationManager.cs` (lines 325-365 modified)
  - `Assets/Scripts/MapManager.cs` (lines 455-470 modified)
- **Ảnh hưởng**:
  - Trải nghiệm leo núi trở nên liền mạch và đậm chất điện ảnh: thấy nhân vật bắt đầu leo $\to$ chuyển cảnh đen $\to$ mở màn đón nhân vật tới trạm mới.
  - Có thể tái sử dụng `CutSceneTransition` ở bất cứ đâu trong dự án khi cần chuyển màn hình hoặc nạp map mới.

### [2026-10-15 12:22] — feat(scale-adaptive-trail): dynamically adapt trail and station positions to MountainBg scale
- **Tác vụ**:
  - **Tự động nhận diện thay đổi Scale và vị trí mốc (`MountainStationManager.cs`)**:
    - Khi bạn thay đổi `localScale` của `MountainBg` hoặc dời mốc `TopPos` / `BottomPos`, hệ thống tự động phát hiện độ lệch khoảng cách và tính toán sinh lại con đường mới tương ứng, không còn bị giữ tọa độ cũ bị bẹp.
  - **Nhân hệ số Scale vào bề ngang (`MountainTrailGenerator.cs`)**:
    - Bề ngang chân núi `effectiveBaseSpreadX = baseSpreadX * scaleFactorX` và chóp đỉnh `effectiveSummitSpreadX = summitSpreadX * scaleFactorX` tự động tỉ lệ thuận theo độ phóng to của `MountainBg`.
  - **Gizmos trực quan theo thời gian thực**:
    - Cập nhật Gizmos trong Scene view tự co giãn chính xác theo scale thực tế của `MountainBg`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainTrailGenerator.cs` (lines 115-130, 350-365 modified)
  - `Assets/Scripts/MountainStationManager.cs` (lines 212-225 modified)
- **Ảnh hưởng**:
  - Khi scale ngọn núi cao lên (để chứa 30 trạm thoáng đãng), toàn bộ đường mòn và các trạm sẽ tự động giãn đều theo chiều cao mới.

### [2026-10-15 12:05] — feat(cone-tapering): cone-shaped X spread calculation to keep stations strictly inside mountain slope
- **Tác vụ**:
  - **Cơ chế thu hẹp sườn núi hình chóp nón (`Tapered Cone Spread`)**:
    - Khi leo từ chân núi (`BottomPos`) lên đỉnh núi (`TopPos`), bề ngang cho phép của các trạm tự động thu hẹp dần:
      $$\text{currentHalfSpreadX} = \text{Lerp}(\text{summitSpreadX}, \text{baseSpreadX}, (1 - \text{progress})^{\text{slopePower}})$$
    - Đảm bảo ở chân núi các trạm có thể dạt rộng tự nhiên, nhưng càng lên cao gần chóp đỉnh, trạm sẽ tự động co sát vào trục tâm, loại bỏ hoàn toàn hiện tượng trạm rớt ra ngoài khoảng không bầu trời.
  - **Trọng tâm trục X nội suy theo đường thẳng**:
    - `currentCenterX` trượt theo đường nối từ `startPos.x` đến `endPos.x` (ngay cả khi đỉnh núi nằm lệch tâm).
  - **Trực quan hóa Gizmos trong Scene view**:
    - Vẽ 2 đường biên sườn núi màu xanh cyan để bạn căn chỉnh trực tiếp kích thước trong Editor.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainTrailGenerator.cs` (lines 25-35, 120-145, 335-365 modified)
- **Ảnh hưởng**:
  - Mọi trạm được sinh ra đều nằm gọn gàng 100% bên trong bề mặt núi tuyết.

### [2026-10-15 11:58] — refactor(mountain-manager): modularize mountain system into separate focused components and support Start/End points in MountainBg
- **Tác vụ**:
  - **Tách `MountainStationManager` thành 4 component con chuyên biệt**:
    1. **`MountainBackground.cs`**:
       - Quản lý `mountainSprite` (SpriteRenderer) nạp ảnh nền núi theo `ThemeSO.mapBg`.
       - Quản lý 2 điểm mốc `BottomPos` (Start) và `TopPos` (End) nằm trong `MountainBg`.
       - Tự động nhận diện cấu trúc Hierarchy: `GameManager -> MountainBg -> TopPos / BottomPos` (hoặc `GameObject / GameObject (1)`).
       - Cung cấp hàm `GetStartAndEndPositions` và `GetMountainBounds` để tính toán giới hạn không gian núi.
    2. **`MountainTrailGenerator.cs`**:
       - Quản lý sinh các GameObject mốc trạm (`StationWorldItem`) và vẽ đường mòn uốn lượn Catmull-Rom (`LineRenderer`).
       - Sinh trạm trải đều chính xác từ điểm Start (`BottomPos`) lên đỉnh End (`TopPos`).
    3. **`MountainCameraController.cs`**:
       - Đảm nhận toàn bộ chuỗi điện ảnh Intro: Lướt chân núi lên đỉnh núi (Ease In-Out) -> Zoom toàn cảnh (Fast Snap 70% + Decelerate 30%) -> Bay về Player.
       - Điều khiển Camera bám sát Player và kéo Pan / lăn chuột Zoom tự do.
    4. **`MountainPlayerController.cs`**:
       - Đảm nhận visual nhân vật (badge cam viền vàng, bóng đổ), hiệu ứng nhún Idle và di chuyển nhảy bước nhịp nhàng giữa 2 trạm.
  - **Tái cấu trúc `MountainStationManager.cs` thành Facade Coordinator**:
    - Gắn trực tiếp lên `GameManager` (hoặc GameObject bất kỳ).
    - Tự động phát hiện và kết nối với `MountainBg`, `MountainCameraController`, `MountainTrailGenerator`, `MountainPlayerController`.
  - **Cập nhật `MapManager.cs`**:
    - Tự động tìm `MountainStationManager` trên `GameManager` hoặc scene nếu chưa gán trong Inspector.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainBackground.cs` (New file)
  - `Assets/Scripts/MountainCameraController.cs` (New file)
  - `Assets/Scripts/MountainPlayerController.cs` (New file)
  - `Assets/Scripts/MountainTrailGenerator.cs` (New file)
  - `Assets/Scripts/MountainStationManager.cs` (Refactored from 1327 lines to 380 clean lines)
  - `Assets/Scripts/MapManager.cs` (lines 160-170 modified)
- **Ảnh hưởng**:
  - Code modular, sạch sẽ, dễ bảo trì và mở rộng.
  - Vị trí trạm xuất phát và đỉnh núi giờ đây neo chính xác vào 2 điểm `BottomPos` và `TopPos` do bạn thiết lập trực quan trong `MountainBg`.

### [2026-10-15 11:08] — feat(theme-bg): add mapBg sprite to ThemeSO and bind to MapImg in map list & mountainSprite in gameplay
- **Tác vụ**:
  - **Thêm trường Sprite `mapBg` vào `ThemeSO.cs`**:
    - Bổ sung `public Sprite mapBg;` để cấu hình ảnh nền/background đại diện cho từng Theme (Rừng, Sa mạc, Băng tuyết...).
  - **Cập nhật hiển thị thẻ map (`MapItemUI.cs`)**:
    - Thêm tham chiếu `[SerializeField] private Image mapImg;` và cơ chế `AutoFindReferences()` tự động tìm Image `MapImg`, `MapImage`, `BgImg` trong hierarchy của `MapContent.prefab`.
    - Trong hàm `Setup()`: Tự động tra cứu `ThemeSO` tương ứng với `data.themeId` và gán `mapImg.sprite = theme.mapBg`.
  - **Cập nhật hiển thị nền ngọn núi (`MountainStationManager.cs`)**:
    - Khi mở màn chơi (`OpenMountain(mapData)`), tự động tra cứu `ThemeSO` theo `currentMap.themeId` và cập nhật `mountainSprite.sprite = theme.mapBg` (nếu có).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/ThemeSO.cs` (lines 18-20 added)
  - `Assets/Scripts/MapItemUI.cs` (lines 20-30, 41-47, 81-125, 148-158 modified)
  - `Assets/Scripts/MountainStationManager.cs` (lines 44-48, 303-305, 665-717 modified)
- **Ảnh hưởng**:
  - Tự động hiển thị đúng hình nền map tương ứng với Theme trên danh sách bản đồ và khi leo núi gameplay, không cần gán thủ công bằng tay.

### [2026-10-15 10:48] — feat(camera-zoom): fast-snap ease-out (70% fast snap + 30% smooth deceleration) for full overview and return to player
- **Tác vụ**:
  - **Tạo hàm nội suy `FastSnapEaseOut`**:
    - Dựa trên phương trình Ease-Out Quartic (`1 - (1 - x)^4`), camera di chuyển cực nhanh đạt ~70-80% khoảng cách ngay ở giai đoạn đầu giống như một cú zoom tức thì (snap zoom), sau đó hãm phanh chậm dần êm ái ở 20-30% cuối chặng để có độ trượt và cảm giác chuyển động mượt mà.
  - **Áp dụng đồng bộ cho cả 2 giai đoạn chuyển động của Camera**:
    - **Giai đoạn Zoom toàn cảnh (Zoom Full)**: Phóng to góc nhìn cực nhanh rồi chậm êm dịu khi tới khung hình toàn cảnh ngọn núi.
    - **Giai đoạn Quay về Player (Return to Player)**: Lướt nhanh về phía Player trong chớp mắt rồi giảm tốc mềm mại khi khóa mục tiêu vào nhân vật.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 850-955 modified)
- **Ảnh hưởng**:
  - Nhịp điệu máy quay dứt khoát, hiện đại, loại bỏ cảm giác chờ đợi rê máy lê thê nhưng vẫn giữ trọn vẹn sự êm ái và chuyển động điện ảnh ở điểm dừng.

### [2026-10-15 10:30] — feat(station-rules): conditional inventory display (>=1) and 3-tries station quota with 0/3 blur lock
- **Tác vụ**:
  - **Ẩn / Hiện Item trên MainPanel theo số lượng sở hữu**:
    - Khi bắt đầu màn chơi, tất cả item trên `MainPanel/ItemContainer` có số lượng `= 0` đều bị **ẩn** (`SetActive(false)`).
    - Chỉ khi trả lời đúng và nhận thưởng (`quantity >= 1`), icon item đó mới tự động **hiện ra** trên thanh HUD kèm số lượng (ví dụ: `x1`, `x2`...).
  - **Cơ chế giới hạn 3 lượt trả lời cho mỗi trạm dừng (`RemainText`)**:
    - Text hiển thị số lượt tại trạm theo định dạng chuẩn phân số `3/3` khi mới vào trạm.
    - Mỗi khi người chơi trả lời câu hỏi (bất kể **Đúng** hay **Sai / Hết giờ**): Số lượt giảm dần: `3/3` ➔ `2/3` ➔ `1/3` ➔ `0/3`.
  - **Khóa trạm khi hết lượt (0/3)**:
    - Khi đạt `0/3`: Toàn bộ các nút item còn lại trong trạm bị **làm mờ** (`alpha = 0.45`) và **khóa bấm** (`interactable = false`). Người chơi chỉ có thể bấm nút `ExitBtn` để ra ngoài tiếp tục leo núi.
    - Khi thoát ra ngoài và quay lại (`Exit` ➔ `Join`), trạm vẫn bảo lưu trạng thái `0/3`, không cấp thêm lượt và các item vẫn bị khóa mờ.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (lines 70-95, 285-300, 365-440, 550-630, 1050-1090 modified)
- **Ảnh hưởng**:
  - Giao diện MainPanel gọn gàng, sạch sẽ hơn (chỉ hiện item đã có); luật chơi tại mỗi trạm rõ ràng, công bằng với đúng 3 cơ hội thử thách.

### [2026-10-25 10:25] — feat(prefab-spawn): support standalone ItemBtn.prefab for dynamic station item instantiation
- **Tác vụ**:
  - Hỗ trợ việc xóa bỏ nút `ItemBtn` mẫu trên Scene và chuyển đổi hoàn toàn sang Prefab `Assets/Prefabs/ItemBtn.prefab`.
  - Tự động nạp `ItemBtn.prefab` qua `AssetDatabase` trong hàm `AutoFindReferences` của `StationQuizController.cs` khi chưa được gán trong Inspector.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (lines 205-218 modified)
- **Ảnh hưởng**:
  - Bạn có thể xóa sạch các nút `ItemBtn` mẫu có sẵn trong Scene (`StationPanel/ItemContainer`), hệ thống sẽ tự động dùng file Prefab để sinh ra 5 item ngẫu nhiên mà không bị lỗi.

### [2026-10-15 10:20] — fix(syntax): resolve CS0106 syntax error due to duplicated code block in StationQuizController.cs
- **Tác vụ**:
  - Loại bỏ đoạn code thừa bị lặp lại trong hàm `OnItemClicked` gây lệch cấu trúc dấu ngoặc nhọn `{}` và dẫn đến lỗi biên dịch `error CS0106: The modifier 'private' is not valid for this item`.
  - Kiểm tra lại toàn bộ cấu trúc block method và class đảm bảo đóng mở ngoặc chuẩn xác 100%.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (lines 680-705 modified)
- **Ảnh hưởng**:
  - Mã nguồn C# đã biên dịch sạch sẽ, không còn lỗi compiler trong Unity Editor.

### [2026-10-15 10:15] — feat(station-items): random spawn 5 theme items per station with persistent state on re-entry
- **Tác vụ**:
  - **Cơ chế sinh ngẫu nhiên 5 Item theo Theme cho từng trạm**:
    - Khi người chơi lần đầu bước chân vào trạm (`ShowJoinPrompt` ➔ `OnJoinButtonClicked`), hệ thống tự động bốc ngẫu nhiên **5 item** từ danh sách `ThemeSO` hiện tại của map (cho phép trùng lặp item như hình ảnh đính kèm: `Campfire`, `Meat`...).
    - Tự động gán Sprite Icon và tên hiển thị lên các nút `ItemBtn` trong `StationPanel/ItemContainer`.
  - **Lưu giữ trạng thái Item khi rời khỏi và vào lại trạm (Persistent State)**:
    - Quản lý qua `Dictionary<int, List<StationItemRuntimeData>> stationItemsState` theo chỉ số của từng trạm (`currentStationIndex`).
    - Nếu đã từng vào trạm này trước đó, hệ thống giữ nguyên chính xác danh sách các item đã sinh lần đầu:
      - Item đã trả lời **ĐÚNG**: Tiếp tục bị ẩn (`SetActive = false`).
      - Item đã trả lời **SAI / HẾT GIỜ**: Tiếp tục bị làm mờ (`alpha = 0.45`) và khóa bấm (`interactable = false`).
      - Item chưa trả lời: Giữ nguyên trạng thái sáng rõ và sẵn sàng mở câu hỏi khi bấm.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (lines 68-95, 260-280, 425-660, 940-1010 modified)
- **Ảnh hưởng**:
  - Đảm bảo trải nghiệm leo núi chân thực: Người chơi có thể tự do bấm `ExitBtn` ra ngoài ngắm cảnh núi non hoặc tiếp tục leo và khi bấm `JoinBtn` vào lại trạm cũ thì toàn bộ tiến trình câu đố và danh sách vật phẩm của trạm đó vẫn được bảo toàn nguyên vẹn.

### [2026-10-15 10:05] — feat(theme-so): implement ScriptableObject for ItemSO and ThemeSO with icon sprite integration
- **Tác vụ**:
  - **Tạo mới ScriptableObject `ItemSO.cs`**:
    - Chứa `itemId`, `itemName`, và `Sprite icon` để hiển thị ảnh trực quan lên `ItemImg`.
    - Menu tạo asset: `Create ➔ Quiz ➔ Item Data`.
  - **Tạo mới ScriptableObject `ThemeSO.cs`**:
    - Chứa `themeId`, `themeName`, `List<ItemSO> themeItems`, và `themeColor`.
    - Menu tạo asset: `Create ➔ Quiz ➔ Theme Data`.
  - **Nâng cấp `StationQuizController.cs`**:
    - Tự động nạp các `ThemeSO` có trong dự án.
    - Đọc Sprite Icon từ `ItemSO` để gán vào `ItemImg` trong prefab `ItemContent`.
  - **Nâng cấp `EditMapPanelUI.cs`**:
    - Tự động nhận diện danh sách `ThemeSO` và duyệt qua các Theme đã tạo khi bấm `ThemeBtn`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/ItemSO.cs` (Tạo mới)
  - `Assets/Scripts/ThemeSO.cs` (Tạo mới)
  - `Assets/Scripts/StationQuizController.cs` (lines 55-65, 260-325 modified)
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 35-40, 725-780 modified)
- **Ảnh hưởng**:
  - Giờ đây có thể tạo và quản lý Item, Theme trực tiếp qua giao diện Unity Inspector mà không cần sửa code C#. Các icon ảnh của Item sẽ tự động hiển thị lên UI `MainPanel/ItemContainer`.

### [2026-10-15 09:55] — feat(theme-items): integrate Theme-based item system and reward inventory counter on correct quiz answers
- **Tác vụ**:
  - **Mô hình hóa dữ liệu Theme & Item (`QuizData.cs`)**:
    - Bổ sung class `ItemData` (id, name, quantity) và `ThemeData` (id, themeName, itemIds).
    - Mở rộng `MapData` với `themeId` và danh sách các ID vật phẩm `itemIds` (mặc định Rừng xanh, Sa mạc, Băng tuyết, Núi lửa).
  - **Hiển thị & Cộng dồn Item trên giao diện MainPanel (`StationQuizController.cs`)**:
    - Tự động tìm và liên kết vùng chứa vật phẩm `GamePanel/MainPanel/ItemContainer` và prefab `ItemContent.prefab`.
    - Khởi tạo các item theo danh sách ID của theme hiện tại với số lượng ban đầu `x0`.
    - Khi người chơi trả lời **ĐÚNG** câu hỏi (`HandleCorrectAnswer`):
      - Kích hoạt nhận thưởng `RewardThemeItem(1)`.
      - Tăng số lượng item tương ứng và cập nhật trực quan lên text `QuanText` (ví dụ: `x1`, `x2`,...).
  - **Tích hợp nút chọn Theme (`EditMapPanelUI.cs`)**:
    - Liên kết nút `ThemeBtn` và text `themeNameText` trong `EditMapPanel/Other`.
    - Cho phép bấm `ThemeBtn` để chuyển đổi tuần tự qua các Theme (Rừng Xanh, Sa Mạc, Băng Tuyết, Núi Lửa), tự động cập nhật danh sách `itemIds` và lưu vào dữ liệu bản đồ.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (lines 63-115 modified)
  - `Assets/Scripts/StationQuizController.cs` (lines 48-65, 160-180, 220-330, 700-720 modified)
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 30-40, 225-240, 410-420, 710-770 modified)
- **Ảnh hưởng**:
  - Người chơi giờ đây có thể thấy các Item theo Theme của map trên thanh HUD `MainPanel/ItemContainer` và nhận thưởng số lượng item trực quan mỗi khi giải đúng câu đố trong trạm.

### [2026-10-15 09:41] — feat(quiz-gameplay): integrate station question system (StationPanel & QuestPanel) with timer, skip, and answer feedback
- **Tác vụ**:
  - **Tạo mới component `StationQuizController.cs`**:
    - **Tương tác vào trạm (Join Station)**:
      - Khi nhân vật leo tới trạm dừng (`OnStationReached`), tự động hiện nút `JoinBtn`.
      - Bấm `JoinBtn` mở `StationPanel`, hiển thị các item câu hỏi trong `ItemContainer`.
      - Hỗ trợ nút `ExitBtn` để rời khỏi trạm và quay lại góc nhìn ngọn núi leo tiếp.
    - **Tích hợp câu hỏi từ dữ liệu bản đồ (`MapData.questions`)**:
      - Bấm vào một item trong `ItemContainer` sẽ mở `QuestPanel` và gọi ngẫu nhiên câu hỏi từ danh sách map nhập vào.
      - **Cơ chế chống trùng lặp**: Lấy câu hỏi từ pool chưa dùng (`unusedQuestions`); chỉ khi trả lời hết toàn bộ danh sách mới tự động reset lại để tránh lặp.
      - **Xáo trộn đáp án ngẫu nhiên**: 4 phương án trên các nút `D1Btn`, `D1Btn (1)`, `D1Btn (2)`, `D1Btn (3)` được hoán vị ngẫu nhiên.
    - **Cơ chế chọn đáp án & đổi đáp án tự do**:
      - Người chơi được tự do bấm chọn hoặc đổi sang đáp án khác trước khi hết giờ (nút được chọn đổi màu highlight nhẹ, chưa hiển thị đúng/sai).
    - **Đếm ngược thời gian & Nút SkipTimeBtn**:
      - Hiển thị đếm lùi từng giây trên `TimeText`.
      - Bấm `SkipTimeBtn` lập tức rút ngắn thời gian còn **3 giây** (`3s ➔ 2s ➔ 1s ➔ 0s`).
    - **Phản hồi kết quả trực quan & Trì hoãn đóng 1s**:
      - Khi đồng hồ về 0s, hệ thống khóa đáp án và hiển thị màu kết quả:
        - **Chọn Đúng**: Nút đã chọn tô màu Xanh lá.
        - **Chọn Sai**: Nút đã chọn tô màu Đỏ, nút đáp án đúng tô màu Xanh lá.
        - **Không chọn gì**: Đáp án đúng tô màu Đỏ.
      - Dừng lại đúng 1 giây (`WaitForSeconds(1.0f)`) cho người chơi xem kết quả trước khi đóng `QuestPanel`.
      - Nếu **ĐÚNG**: Nút item đó biến mất (`SetActive(false)`), cập nhật giảm `RemainText`.
      - Nếu **SAI hoặc HẾT GIỜ**: Nút item bị làm mờ (`opacity = 0.45` qua `CanvasGroup`) và khóa tương tác (`interactable = false`).
  - **Cập nhật `MapManager.cs`**:
    - Tự động liên kết `StationQuizController` khi bắt đầu chơi map (`StartGameOnMap`).
    - Lắng nghe sự kiện `mountainManager.OnStationReached` để hiển thị prompt Join trạm.
    - Dọn dẹp sạch trạng thái trạm khi bấm Pause về menu (`OnPauseButtonClicked`).
  - **Hỗ trợ xóa bản đồ (`DeleteMap`)**:
    - Thêm `DeleteBtn` vào `MapItemUI.cs` và hàm `DeleteMap` trong `MapManager.cs` để xóa map và lưu lại save json.
  - **Tối ưu Camera Intro (`MountainStationManager.cs`)**:
    - Chuyển hướng camera lướt theo 1 đường thẳng trực tiếp từ chân núi lên đỉnh núi với nội suy mượt mà `Smootherstep` (chậm êm dịu lúc bắt đầu và hãm phanh chậm dần lúc chạm đỉnh).
    - Phóng to chiếu toàn cảnh chậm rãi để chiêm ngưỡng, sau đó lướt nhanh về lại Player và khóa bám theo nhân vật.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/StationQuizController.cs` (Tạo mới)
  - `Assets/Scripts/MapManager.cs` (lines 20-30, 150-165, 330-360, 375-430 modified)
  - `Assets/Scripts/MapItemUI.cs` (lines 14-40, 80-110 modified)
  - `Assets/Scripts/MountainStationManager.cs` (lines 155-165, 815-985, 1170-1185 modified)
- **Ảnh hưởng**: Hoàn tất chu trình Gameplay leo núi: từ di chuyển giữa các trạm, mở trạm làm bài thi, đếm giờ, skip thời gian, kiểm tra đúng/sai trực quan cho tới lưu trữ trạng thái bản đồ.

### [2026-10-13 11:28] — feat(station-generator): independent fixed layer spacing (layerSpacingY) and free horizontal random spread (horizontalSpreadX)
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Khoảng cách trục Y giữa các tầng cố định theo cấu hình (`layerSpacingY`)**:
      - Bỏ phụ thuộc vào kích thước sprite ảnh núi. Mỗi tầng trạm tiếp theo sẽ tự động cao hơn trạm trước đúng `layerSpacingY` (mặc định `2.2f`). Khi có 20 trạm, toàn bộ hành trình sẽ vươn cao tự nhiên theo số trạm: $Y_i = minY + (i \times layerSpacingY)$.
    - **Tọa độ X rải ngẫu nhiên tự do hoàn toàn (`horizontalSpreadX`)**:
      - Bỏ logic rải zic-zac luân phiên trái-phải-trái-phải. Trạm được sinh ngẫu nhiên tự do trên toàn bộ bề ngang `horizontalSpreadX` (mặc định `8.0f`), có thể nằm ở giữa, hơi lệch trái, hơi lệch phải, hoặc ngoài rìa mà không bị gò bó.
    - **Camera Panoramic Overview thích ứng theo độ cao thực tế**:
      - Camera tự động căn góc nhìn toàn cảnh theo độ cao trạm thực tế thay vì bị giới hạn bởi sprite núi, đảm bảo nhìn thấy trọn vẹn từ trạm 1 đến trạm 20.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 28-36, 430-485, 910-925 modified)
- **Ảnh hưởng**: Người dùng có thể tùy ý điều chỉnh khoảng cách giữa các tầng bằng 1 biến duy nhất `layerSpacingY` và độ rộng rải trạm bằng `horizontalSpreadX` mà không lo bị phụ thuộc vào ảnh background núi.

### [2026-10-13 11:22] — feat(station-generator): organic winding distribution and uniform vertical layer scaling for high station counts
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Khắc phục lỗi trạm chỉ bám ở 2 mép biên khi số lượng trạm lớn (ví dụ: 20 trạm)**:
      - Loại bỏ logic ép hướng nhị phân ping-pong (`previousX >= centerX ? -1 : 1` với bước nhảy cưỡng bức cách xa tâm).
      - Thay thế bằng thuật toán sóng uốn lượn tự nhiên (`organic winding path` dùng hàm sin theo pha kết hợp wander ngẫu nhiên và độ dốc thoải hình nón ngọn núi `mountainSlopeFactor`). Trạm giờ đây phân bổ mềm mại, có các trạm nằm ở giữa trung tâm, sườn trái, sườn phải, uốn lượn đều đặn.
    - **Phân bổ đều chiều cao trục Y giữa các tầng trạm**:
      - Tọa độ $Y$ được chia đều theo tỷ lệ tiến trình từ chân núi (`minY`) lên đỉnh núi (`maxY`) thay vì cộng dồn bước cố định có nguy cơ vượt đỉnh hoặc bị dồn cục ở đỉnh.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 425-475 modified)
- **Ảnh hưởng**: Khi tạo số lượng trạm lớn (như 20 trạm hoặc nhiều hơn), đường đi leo núi uốn lượn tự nhiên, xuất hiện hài hòa ở trung tâm lẫn 2 sườn núi và rải đều từ chân lên tới đỉnh mà không bị dồn ứ.

### [2026-10-13 11:09] — feat(camera): add 1.0s delay at base station before pan and 1.0s delay at summit before overview zoom
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Thêm thời gian dừng chờ tại chân núi (`baseStationDelay = 1.0f`)**:
      - Khi vừa vào map, camera focus vào Player ở chân núi và dừng chờ đúng 1 giây để người chơi định vị rõ vị trí xuất phát trước khi bắt đầu lướt theo đường mòn qua các trạm.
    - **Thêm thời gian dừng chờ tại đỉnh núi (`summitArrivalDelay = 1.0f`)**:
      - Khi camera hoàn tất hành trình lướt lên tới trạm đỉnh núi, camera sẽ dừng lại chiêm ngưỡng đỉnh núi 1 giây trước khi chuyển sang giai đoạn phóng to toàn cảnh ngọn núi (`zoom full`).
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 105-115, 829-885 modified)
- **Ảnh hưởng**: Nhịp điệu chuyển động của máy quay điện ảnh trở nên rõ ràng và chuyên nghiệp hơn, có các nhịp dừng ngắm có chủ đích ở cả điểm đầu (chân núi) và điểm mốc (đỉnh núi).

### [2026-10-13 10:17] — feat(camera & gameplay): slow down player climb, ease-in camera pan to summit, and ensure uninterrupted intro
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Giảm tốc độ di chuyển của Player cực chậm (`playerClimbDuration = 2.8f`)**:
      - Tăng thời gian di chuyển giữa 2 trạm từ 1.1s lên `2.8s`, giúp nhân vật leo từng bước điềm tĩnh, mượt mà và camera có đủ thời gian lia theo chậm rãi.
    - **Camera lướt lên đỉnh núi từ chậm chuyển sang nhanh dần (Ease-In Smooth Acceleration)**:
      - Tăng thời gian lướt lên đỉnh từ ~2.0s lên `4.2s` (`cameraPanToSummitDuration = 4.2f`) giúp không còn bị cảm giác quá vội.
      - Áp dụng hàm nội suy hàm mũ `t = (elapsed / panDuration)^2.2f` (Ease-In), xuất phát cực kỳ nhẹ nhàng, lướt êm ở các tầng thấp rồi tăng tốc dần khi phóng lên đỉnh núi.
    - **Luôn chiếu trọn vẹn màn Intro (Uninterrupted Showcase)**:
      - Loại bỏ sự kiện click chuột/chạm màn hình để skip intro trong `Update()`, đảm bảo máy quay luôn thực hiện trọn vẹn thước phim điện ảnh soi đường và zoom toàn cảnh mỗi lần vào map.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 100-115, 215-230, 827-880, 1115-1125 modified)
- **Ảnh hưởng**: Trải nghiệm điện ảnh thỏa mãn đúng mong đợi: mở đầu chậm rãi rồi vút lên đỉnh, không bị gián đoạn vô tình, và nhân vật leo núi cực kỳ từ tốn.

### [2026-10-13 10:12] — fix(camera & gameplay): fix intro re-entry after pause, disable free camera pan, speed up overview & return, shrink player scale, widen station spacing
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Khắc phục lỗi vào lại map không di chuyển camera**: Khi mở map (`OpenMountain`), thiết lập ngay vị trí và kích thước Camera tức thời về vị trí Player tại chân núi trước khi kích hoạt Coroutine intro, giúp chuỗi intro và follow luôn chạy chính xác 100% mỗi lần vào map sau khi Pause.
    - **Vô hiệu hóa kéo camera tự do (`enableFreeCameraPan = false`)**: Camera luôn khoá chặt theo Player, không bị trôi hay lệch góc nhìn khi người chơi lỡ tay chạm/kéo chuột trên màn hình.
    - **Tăng tốc độ Zoom toàn cảnh & Bay về Player**:
      - Giảm thời gian Zoom toàn cảnh ngọn núi xuống `0.55s` (`zoomOverviewSpeed`).
      - Giảm thời gian dừng ngắm toàn cảnh xuống `0.5s` (`overviewHoldDuration`).
      - Giảm thời gian bay từ đỉnh/toàn cảnh quay về lại Player xuống `0.65s` (`returnToPlayerSpeed`).
    - **Thu nhỏ kích thước Player Token**: Giảm `playerScale` từ `0.5f` xuống `0.28f`, căn chỉnh lại độ cao đứng `+0.22f` giúp nhân vật nhỏ gọn, thanh thoát, cân đối hoàn hảo với các trạm.
    - **Tăng khoảng cách giữa các trạm (`minStationSpacingY = 1.6f`, `widthMarginFactor = 0.88f`)**:
      - Trục Y (chiều cao đứng): Bổ sung cơ chế giãn tầng tối thiểu `minStationSpacingY` và giảm padding sườn núi (`0.5f`), giúp các trạm trải dài đều đặn từ chân núi lên đỉnh núi.
      - Trục X (chiều ngang sườn núi): Tăng biên độ lệch ngẫu nhiên sang 2 bên sườn núi (`maxOffsetSpread * 0.55f`), tạo con đường mòn uốn lượn zigzag rộng rãi và đẹp mắt hơn.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 25-35, 56-65, 88-115, 265-285, 385-435, 638-645, 663-700, 843-895, 950-955, 1085-1115 modified)
- **Ảnh hưởng**: Trải nghiệm vào map mượt mà, tốc độ intro nhanh dứt khoát không phải chờ lâu, camera khoá chặt người chơi và các trạm được phân bổ rộng rãi, thoáng đãng.

### [2026-10-13 09:50] — feat(camera): implement cinematic intro sequence, player character, and dynamic camera follow
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - **Hiệu ứng máy quay điện ảnh (Cinematic Intro Sequence)** khi vừa mở map (`PlayMountainIntroSequence`):
      1. Khởi đầu: Đặt Camera cận cảnh ngay tại vị trí Player ở chân núi (trạm xuất phát index 0) với kích thước thu nhỏ `camPlayerSize` (5.2f).
      2. Soi đường mòn (`cachedPathPoints`): Camera lướt mượt mà uốn lượn theo các khúc cua của con đường mòn leo núi từ chân lên đến trạm đỉnh.
      3. Zoom toàn cảnh ngọn núi (`camOverviewSize = 12f`): Đẩy camera về trung tâm ngọn núi, phóng tầm mắt bao quát toàn bộ ngọn núi và dừng lại 1 nhịp (`overviewHoldDuration = 1.0f`) để chiêm ngưỡng.
      4. Lao về lại Player + thu nhỏ cam: Camera bay nhanh và êm ái từ trung tâm ngọn núi quay trở về vị trí nhân vật, đồng thời thu nhỏ ống kính lại `camPlayerSize` (5.2f).
      5. Tự động chuyển sang chế độ Camera luôn theo sát Player (`isFollowingPlayer = true`).
      6. Hỗ trợ bỏ qua nhanh (Skip): Người chơi có thể click chuột hoặc bấm Space bất kỳ lúc nào để chuyển ngay vào góc nhìn bám theo Player.
    - **Nhân vật người chơi (Player Character)**:
      - Tự động sinh `Player_Climber` tại trạm xuất phát (hoặc dùng `playerPrefab` tùy chọn) với huy hiệu cam nổi bật viền vàng kim, bóng đổ, chữ `P` và hiệu ứng nhún nhảy nhịp nhàng (`UpdatePlayerIdleAnimation`).
      - Hỗ trợ di chuyển từng bước dọc theo đường mòn (`MovePlayerToNextStation`, `MovePlayerAlongTrailRoutine`) khi người chơi leo trạm.
    - **Camera luôn bám theo Player (`HandleCameraFollowPlayer`)**:
      - Camera liên tục Lerp mượt mà theo tọa độ người chơi với tốc độ `cameraFollowSpeed` và độ lệch nhìn trước `playerCameraOffsetY`.
      - Khi người chơi kéo rê chuột để khám phá ngọn núi, chế độ follow tạm dừng; sau `returnToPlayerDelay` (1.8s) không thao tác, camera sẽ tự động lướt êm ái quay về bám theo người chơi.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Kết nối sự kiện nút leo núi `OnClimbButtonClicked`: Gọi `mountainManager.MovePlayerToNextStation()` để di chuyển nhân vật lên trạm kế tiếp và camera tự động theo sát bước chân người chơi.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 95-135, 190-250, 455-470, 560-580, 595-750 modified/added)
  - `Assets/Scripts/MapManager.cs` (lines 393-404 modified)
- **Ảnh hưởng**: Hoàn thiện toàn bộ trải nghiệm điện ảnh và bám sát nhân vật khi vào màn chơi leo núi đúng như yêu cầu của người dùng.

### [2026-10-13 09:27] — fix(compiler): resolve CS1503 delegate conversion error for CloseMountain in UnityEvent
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Khắc phục lỗi biên dịch `CS1503: Argument 1: cannot convert from 'method group' to 'UnityAction'` khi đăng ký sự kiện cho nút `backBtn`.
    - Bổ sung phương thức nạp chồng không tham số `public void CloseMountain()` chuyển tiếp sang `CloseMountain(true)`.
    - Sử dụng cú pháp lambda `() => CloseMountain()` trong `backBtn.onClick.AddListener(...)` tương thích chuẩn với kiểu delegate `UnityAction`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 150-155, 212-220 modified)
- **Ảnh hưởng**: Khắc phục dứt điểm lỗi biên dịch trong Unity, scene biên dịch sạch sẽ không còn cảnh báo hay lỗi đỏ.

### [2026-10-13 09:14] — fix(navigation): resolve infinite recursion on PauseBtn, restore camera state, and prevent UI vanishing
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Khắc phục lỗi đệ quy lặp vô tận: Trong `CloseMountain(bool triggerCallback = true)`, reset `onBackCallback = null` trước khi gọi callback, tránh việc `CloseMountain` và `OnPauseButtonClicked` gọi chéo lẫn nhau gây văng ngoại lệ StackOverflow làm đứng luồng bật lại `MapManagerPanel`.
    - Bảo vệ an toàn GameObject và Canvas: Không bao giờ vô hiệu hóa (`SetActive(false)`) toàn bộ Canvas hoặc các Panel cốt lõi (`MapManagerPanel`, `HomeMenuPanel`, `GamePanel`) khi đóng ngọn núi.
    - Lưu và khôi phục Camera (`savedCameraPos`, `savedCameraSize`): Tự động ghi nhớ vị trí và cỡ Camera ban đầu trước khi vào màn chơi và phục hồi nguyên vẹn khi Pause/thoát về menu.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Trong `OnPauseButtonClicked`: Gọi `CloseMountain(triggerCallback: false)` để chặn đệ quy quay lại. Đảm bảo `MapManagerPanel` và `Canvas` luôn được kích hoạt lại (`SetActive(true)`) ngay lập tức kể cả khi tham chiếu bị mất.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 165-245 modified)
  - `Assets/Scripts/MapManager.cs` (lines 192-210, 360-395 modified)
- **Ảnh hưởng**: Khắc phục triệt để lỗi bấm Pause làm biến mất toàn bộ UI. Người dùng bấm Pause từ GamePanel sẽ đóng màn núi, phục hồi camera và quay lại ngay màn hình chọn map mượt mà 100%.

### [2026-10-13 09:05] — feat(ui): load MainPanel first when editing map, add 3-panel navigation for EditMapPanel
- **Tác vụ**:
  - Cập nhật [EditMapPanelUI.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/EditMapPanelUI.cs):
    - Thay đổi luồng mở bảng: Khi bấm `Edit` ở bất kỳ thẻ map nào từ `MapManager`, mặc định nạp và hiển thị `MainPanel` đầu tiên (`ShowMainPanelMode()`), ẩn `CreateQuestion` và `ListQuestions`.
    - Hỗ trợ đầy đủ các thành phần trong `MainPanel`:
      - Ô nhập số tầng trạm `NumberStations` (`numberStationsInput`).
      - Ô nhập tên bản đồ `NameLevel` (`nameLevelInput`).
      - Nút `CreateBtn` (`mainCreateBtn`): Chuyển sang bảng soạn câu hỏi `CreateQuestion`.
      - Nút `ListBtn` (`mainListBtn`): Chuyển sang bảng danh sách câu hỏi `ListQuestions`.
    - Tối ưu hóa nút `BackBtn` thành điều hướng phân tầng trực quan:
      - Khi đang ở `CreateQuestion` hoặc `ListQuestions`: Bấm `BackBtn` (hoặc nút Back con) sẽ quay trở lại `MainPanel`.
      - Khi đang ở `MainPanel`: Bấm `BackBtn` sẽ đóng `EditMapPanel` và quay trở về danh sách map (`MapManagerPanel`).
    - Sử dụng `[FormerlySerializedAs("otherArea")]` và cơ chế quét tự động thông minh (`AutoFindReferences`) giúp tương thích mượt mà với cây Hierarchy trong scene mà không làm mất liên kết.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/EditMapPanelUI.cs` (lines 17-45, 187-430, 609-710 modified)
- **Ảnh hưởng**: Hoàn thiện cấu trúc UI 3 phân vùng (MainPanel -> CreateQuestion / ListQuestions) cho EditMapPanel, mang lại trải nghiệm chỉnh sửa thông số bản đồ và quản lý câu hỏi rõ ràng, mạch lạc.

### [2026-10-13 08:44] — feat(gameplay): disable scroll zoom, scale down station sizes, and implement smooth curved LineRenderer trails
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Tắt zoom bằng con lăn chuột (`enableScrollZoom = false` mặc định), loại bỏ hoàn toàn tình trạng vô tình cuộn chuột làm đổi kích thước camera khi spawn và ngắm trạm; vẫn giữ nguyên thao tác kéo rê chuột (Pan Camera) dọc ngọn núi.
    - Thu nhỏ tỉ lệ trạm (`stationScale = 0.35f`, `summitScale = 0.5f`) áp dụng cho cả `stationPrefab` lẫn trạm mặc định, đồng thời giảm cỡ font chữ TextMeshPro để các trạm xuất hiện vừa vặn, thanh thoát trên sườn núi.
    - Cải tiến `LineRenderer` từ các đoạn thẳng gấp khúc sang đường cong uốn lượn mềm mại (Curved Mountain Trail) sử dụng thuật toán nội suy Catmull-Rom spline (`GenerateCurvedPath`), bo tròn góc cong và hai đầu mút (`numCornerVertices = 5`, `numCapVertices = 5`), nét vẽ mượt mà tự nhiên (`trailWidth = 0.12f`).
  - Cập nhật [StationWorldItem.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/StationWorldItem.cs):
    - Tối ưu nhãn hiển thị thành 1 dòng gọn gàng (`"🏔️ Đỉnh"`, `"1"`, `"2"`...) giúp nội dung nằm vừa vặn hoàn hảo bên trong biểu tượng trạm thu nhỏ.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 32-60, 107-120, 311-435, 529-550 modified)
  - `Assets/Scripts/StationWorldItem.cs` (lines 56-62 modified)
- **Ảnh hưởng**: Trải nghiệm hình ảnh ngọn núi leo dốc tự nhiên và thẩm mỹ cao hơn rõ rệt: đường mòn uốn lượn mềm mại qua từng trạm, các trạm nhỏ gọn tinh tế và thao tác camera ổn định không bị lăn chuột ngoài ý muốn.

### [2026-10-13 08:12] — feat(gameplay): verify exact station count spawning, disable station click interactions, and hook GamePanel PauseBtn
- **Tác vụ**:
  - Cập nhật [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Đảm bảo sinh chính xác 100% số lượng trạm theo đúng cấu hình `stationCount` của từng bản đồ (từ 1 trạm trở lên, an toàn không lỗi chia 0).
    - Xóa bỏ hoàn toàn cơ chế bấm vào trạm (`OnStationClicked`, không thêm collider nhận click): Các trạm hiện tại chỉ là các mốc trực quan (visual checkpoints/layers) trên sườn núi.
  - Cập nhật [StationWorldItem.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/StationWorldItem.cs):
    - Gỡ bỏ `[RequireComponent(typeof(Collider2D))]`, các hàm `OnMouseDown`, `OnMouseEnter`, `OnMouseExit` và callback bấm chuột để tránh nhận sự kiện click.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs):
    - Tích hợp `GamePanel` (gồm 2 nút `PauseBtn` và `ClimbBtn`):
      - Khi bấm `Play` trên thẻ map: Tự động bật `GamePanel` và mở màn chơi leo núi của map đó.
      - Khi bấm `PauseBtn`: Tắt `GamePanel`, đóng màn chơi leo núi và quay trở lại ngay màn hình chọn map (`MapManagerPanel`).
      - Khi bấm `ClimbBtn`: Giữ trạng thái chờ (chưa có tác dụng, sẵn sàng phát triển tiếp).
      - Đảm bảo `GamePanel` tự động ẩn khi ở màn hình chính `Home` hoặc `MapManagerPanel`.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/MountainStationManager.cs` (lines 200-340 modified)
  - `Assets/Scripts/StationWorldItem.cs` (viết lại tối giản)
  - `Assets/Scripts/MapManager.cs` (lines 15-30, 130-175, 290-335 modified)
- **Ảnh hưởng**: Hoàn thiện đúng luồng UI màn chơi: Trạm không còn bị click, nút Pause trở về màn hình chọn map chuẩn xác, Climb sẵn sàng mở rộng.

### [2026-10-13 07:50] — feat(gameplay): implement World Space mountain setup with station transform persistence and 2D camera controls
- **Tác vụ**:
  - Cập nhật [QuizData.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/QuizData.cs):
    - Bổ sung cấu trúc `StationPos` (chuyển đổi 2 chiều mượt mà với `Vector2`) để lưu trữ chính xác tọa độ World Transform `(x, y)` vào JSON.
    - Thêm danh sách `stationPositions` vào `MapData` để lưu vĩnh viễn tọa độ các trạm của từng map vào `quiz_save.json`.
  - Tạo mới [StationWorldItem.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/StationWorldItem.cs):
    - Quản lý Prefab trạm 2D trong không gian World: nhận diện click chuột (`OnMouseDown`), hiệu ứng hover co giãn, nhãn text tầng/đỉnh núi và đổi màu theo độ cao.
  - Tái cấu trúc [MountainStationManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MountainStationManager.cs):
    - Hỗ trợ ảnh ngọn núi to trong World Space (dùng `SpriteRenderer` tự động lấy kích thước `bounds` hoặc Transform World).
    - Tự động kiểm tra: Nếu bản đồ đã lưu tọa độ các trạm thì tải lại đúng vị trí đó; nếu chưa có thì random đường mòn ngoằn ngoèo lên đỉnh núi và lưu ngay vào `MapData` & `quiz_save.json`.
    - Vẽ đường mòn nối các trạm bằng `LineRenderer` trong World Space.
    - Tích hợp bộ điều khiển Camera 2D: Kéo rê chuột (Mouse Drag) để di chuyển dọc sườn núi từ chân lên đỉnh và lăn chuột (Scroll Wheel) để phóng to/thu nhỏ (Zoom).
    - Thêm nút tạo lại đường đi (`RegenerateMountainPath`) cho phép đổi dáng đường mòn mới bất cứ lúc nào.
  - Cập nhật [MapManager.cs](file:///c:/Users/Hi/Documents/Unity%20Project/Quiz/Assets/Scripts/MapManager.cs): Đồng bộ hóa callback lưu dữ liệu khi sinh trạm trong màn chơi.
- **Danh sách file thay đổi**:
  - `Assets/Scripts/QuizData.cs` (lines 45-75 modified)
  - `Assets/Scripts/StationWorldItem.cs` (file mới tạo)
  - `Assets/Scripts/MountainStationManager.cs` (viết lại hoàn toàn)
  - `Assets/Scripts/MapManager.cs` (lines 295-310 modified)
- **Ảnh hưởng**: Đáp ứng trọn vẹn yêu cầu lưu trữ tọa độ World Transform cho từng trạm, hỗ trợ ảnh núi to và prefab station trong không gian 2D với camera kéo cuộn mượt mà.

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


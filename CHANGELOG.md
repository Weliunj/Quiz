# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-10-12 - feat(quiz): implement ScriptableObject data architecture, QuizManager, and SaveSystem

> **Commit Message:**
> `feat(quiz): implement ScriptableObject data architecture, QuizManager, and SaveSystem`

### Added
- **ScriptableObject Architecture (Cơ chế dữ liệu phân cấp):**
  - `QuestionSO`: Định nghĩa cấu trúc câu hỏi trắc nghiệm 1/4, hỗ trợ đếm ngược thời gian và liên kết phần thưởng. Tích hợp `OnValidate` để tự động kiểm tra tính hợp lệ (chính xác 1 đáp án đúng).
  - `StationDataSO`: Quản lý kho câu hỏi riêng biệt cho từng trạm núi, hỗ trợ thuật toán `GetRandomQuestion()` bốc ngẫu nhiên câu hỏi không lặp lại. Cho phép tái sử dụng câu hỏi giữa các trạm khác nhau.
  - `MountainLevelSO`: Quản lý thông tin từng ngọn núi (Level), bao gồm tên núi, cốt truyện, ảnh bản đồ và danh sách các trạm theo thứ tự.
- **Hệ thống Quản lý và Lưu trữ (Management & Persistence):**
  - `QuizManager`: Điều phối hiển thị câu hỏi lên UI (TextMeshPro), quản lý đồng hồ đếm ngược 15s và hiệu ứng đổi màu đáp án (Kahoot style). Sử dụng Observer Pattern (`OnQuestionCompleted` event).
  - `SaveSystem`: Hỗ trợ lưu và đọc tiến trình người chơi (ngọn núi hiện tại, trạm hiện tại, 5 trái tim HP, điểm số, danh sách câu hỏi đã trả lời, túi đồ) vào JSON tại `Application.persistentDataPath`.
- **Cấu trúc thư mục & Công cụ Editor:**
  - Tổ chức cấu trúc thư mục chuẩn tại `Assets/ScriptableObjects/` (`Mountains/`, `Stations/`, `Questions/`).
  - Thêm `SampleDataGenerator.cs` trong `Assets/Editor/` cho phép tạo và liên kết tự động dữ liệu mẫu (Núi ➔ Trạm ➔ Câu hỏi) chỉ với 1 click từ menu `MountainQuest/Tools`.

## [0.1.0] - 2026-10-12 - chore: initialize Unity project structure and base configs

> **Commit Message:**
> `chore: initialize Unity project structure and base configs`

### Added
- Khởi tạo cấu trúc dự án Unity Quiz Game.
- Thiết lập `.gitignore`, `Packages`, và `ProjectSettings` ban đầu.
- Thêm tài liệu `README.md` và `CHANGELOG.md`.

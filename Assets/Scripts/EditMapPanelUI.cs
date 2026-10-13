using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuizGame
{
    public class EditMapPanelUI : MonoBehaviour
    {
        [Header("--- 1. BẢNG CHÍNH (MAIN PANEL) ---")]
        [Tooltip("Bảng chính của EditMapPanel (MainPanel: hiển thị đầu tiên khi bấm Edit trên thẻ map)")]
        [FormerlySerializedAs("otherArea")]
        [SerializeField] private GameObject mainPanelArea;

        [Tooltip("Nút chuyển từ MainPanel sang tạo câu hỏi (CreateBtn trong MainPanel)")]
        [SerializeField] private Button mainCreateBtn;

        [Tooltip("Nút chuyển từ MainPanel sang danh sách câu hỏi (ListBtn trong MainPanel)")]
        [SerializeField] private Button mainListBtn;

        [Tooltip("Ô nhập số lượng trạm đến đỉnh núi (NumberStations)")]
        [SerializeField] private TMP_InputField numberStationsInput;

        [Tooltip("Ô nhập tên màn chơi (NameLevel)")]
        [SerializeField] private TMP_InputField nameLevelInput;

        [Header("--- 2. KHU VỰC TẠO & SỬA CÂU HỎI (CREATE QUESTION) ---")]
        [Tooltip("Khu vực nhập câu hỏi & đáp án (CreateQuestion)")]
        [SerializeField] private GameObject createQuestionArea;

        [Header("--- 3. KHU VỰC DANH SÁCH CÂU HỎI (LIST QUESTIONS) ---")]
        [Tooltip("Khu vực danh sách các câu hỏi (ListQuestions)")]
        [SerializeField] private GameObject listQuestionsArea;

        [Header("--- NÚT CHUYỂN ĐỔI CHẾ ĐỘ PHỤ ---")]
        [Tooltip("Nút chuyển sang xem danh sách câu hỏi")]
        [SerializeField] private Button viewListBtn;

        [Tooltip("Nút chuyển sang chế độ tạo câu hỏi mới")]
        [SerializeField] private Button addNewBtn;

        [Header("--- Ô NHẬP CÂU HỎI & ĐÁP ÁN ---")]
        [Tooltip("Ô nhập nội dung câu hỏi")]
        [SerializeField] private TMP_InputField questionInput;

        [Tooltip("Ô nhập đáp án A (1)")]
        [SerializeField] private TMP_InputField answerA;

        [Tooltip("Ô nhập đáp án B (2)")]
        [SerializeField] private TMP_InputField answerB;

        [Tooltip("Ô nhập đáp án C (3)")]
        [SerializeField] private TMP_InputField answerC;

        [Tooltip("Ô nhập đáp án D (4)")]
        [SerializeField] private TMP_InputField answerD;

        [Header("--- CHỌN ĐÁP ÁN ĐÚNG (4 TOGGLES) ---")]
        [Tooltip("Toggle chọn đáp án A là đáp án đúng")]
        [SerializeField] private Toggle toggleA;

        [Tooltip("Toggle chọn đáp án B là đáp án đúng")]
        [SerializeField] private Toggle toggleB;

        [Tooltip("Toggle chọn đáp án C là đáp án đúng")]
        [SerializeField] private Toggle toggleC;

        [Tooltip("Toggle chọn đáp án D là đáp án đúng")]
        [SerializeField] private Toggle toggleD;

        [Tooltip("Dropdown chọn đáp án đúng (tùy chọn hoặc dự phòng)")]
        [SerializeField] private TMP_Dropdown correctDropdown;

        [Header("--- NÚT HÀNH ĐỘNG TẠO & LÀM MỚI ---")]
        [Tooltip("Nút tạo mới hoặc lưu chỉnh sửa câu hỏi")]
        [SerializeField] private Button createBtn;

        [Tooltip("Nút xóa trắng các ô nhập để viết câu mới")]
        [SerializeField] private Button resetBtn;

        [Header("--- NHẬP CÂU HỎI TỪ FILE (IMPORT) ---")]
        [Tooltip("Khu vực chứa tính năng Import")]
        [SerializeField] private GameObject importArea;

        [Tooltip("Nút mở bảng hướng dẫn soạn file import (GuideImportBtn)")]
        [SerializeField] private Button guideImportBtn;

        [Tooltip("Nút mở cửa sổ chọn file để import câu hỏi (ImportBtn)")]
        [SerializeField] private Button importBtn;

        [Tooltip("Bảng hiển thị nội dung hướng dẫn soạn file (GuidePanel)")]
        [SerializeField] private GameObject guidePanel;

        [Tooltip("Nội dung text hướng dẫn bên trong GuidePanel")]
        [SerializeField] private TMP_Text guideText;

        [Tooltip("Nút đóng bảng hướng dẫn (CloseBtn)")]
        [SerializeField] private Button closeGuideBtn;

        [Header("--- DANH SÁCH CÂU HỎI ---")]
        [Tooltip("Vùng chứa danh sách câu hỏi trong Scroll View Viewport")]
        [SerializeField] private Transform listContent;

        [Tooltip("Prefab hiển thị một dòng câu hỏi (QuestionContent.prefab)")]
        [SerializeField] private GameObject questionPrefab;

        [Header("--- NÚT THAO TÁC CÂU HỎI TRONG DANH SÁCH ---")]
        [Tooltip("Nút mở form chỉnh sửa câu hỏi đã chọn trong danh sách (EditQuestBtn)")]
        [SerializeField] private Button editQuestBtn;

        [Tooltip("Nút xóa câu hỏi đang chọn khỏi map (DeleteQuest)")]
        [SerializeField] private Button deleteBtn;


        [Header("--- ĐIỀU HƯỚNG ---")]
        [Tooltip("Nút đóng bảng soạn câu hỏi và quay về Map Manager")]
        [SerializeField] private Button backBtn;

        private MapData currentMap;
        private QuestionData selectedQuestion;
        private QuestionItemUI selectedItemUI;
        private QuestionData editingQuestion;

        private Action onDataChangedCallback;
        private Action onCloseCallback;

        private const string DEFAULT_IMPORT_GUIDE =
            "<b>HƯỚNG DẪN SOẠN FILE CÂU HỎI ĐỂ IMPORT</b>\n\n" +
            "<b>1. File văn bản (.TXT) — Đánh dấu sao (*) đáp án đúng:</b>\n" +
            "Soạn trên Word hoặc Notepad, mỗi câu cách nhau 1 dòng trống. <b>Chỉ cần thêm dấu * trước đáp án đúng</b> (không cần đổi thứ tự câu hỏi hay ghi thêm dòng đáp án!):\n\n" +
            "<i>Câu 1: Thủ đô của Việt Nam là gì?\n" +
            "A. TP. Hồ Chí Minh\n" +
            "<b>*B. Hà Nội</b>\n" +
            "C. Đà Nẵng\n" +
            "D. Huế</i>\n\n" +
            "<color=#FFE600><b>💡 ĐẶC BIỆT TIỆN LỢI:</b></color>\n" +
            "• Thêm dấu <b>*</b> vào đáp án đúng ở bất kỳ đâu: <i>*A.</i>, <i>*B.</i>, <i>*C.</i>, hoặc <i>*D.</i> (dấu * sẽ tự biến mất khi vào game).\n" +
            "• Nếu câu nào <b>không có dấu *</b>, hệ thống sẽ tự động lấy đáp án đầu tiên (A) là đúng.\n" +
            "• Khi vào màn chơi, game sẽ <b>tự động xáo trộn ngẫu nhiên</b> các đáp án nên không bao giờ lo bị lộ vị trí!\n\n" +
            "<b>2. File Excel (.CSV):</b>\n" +
            "Soạn theo cột: <i>Câu hỏi, Đáp án A, Đáp án B, Đáp án C, Đáp án D</i> (thêm dấu * vào ô đáp án đúng, ví dụ: <i>*Hà Nội</i>).";

        private void Awake()
        {
            AutoFindReferences();
            SetupCorrectDropdown();
            SetupGuideContent();
            BindEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoFindReferences();
        }
#endif

        private void SetupGuideContent()
        {
            if (guideText != null)
            {
                guideText.text = DEFAULT_IMPORT_GUIDE;
            }
        }

        private void SetupCorrectDropdown()
        {
            if (correctDropdown != null)
            {
                correctDropdown.ClearOptions();
                correctDropdown.AddOptions(new List<string>
                {
                    "Đáp án A (D1) là Đúng",
                    "Đáp án B (D2) là Đúng",
                    "Đáp án C (D3) là Đúng",
                    "Đáp án D (D4) là Đúng"
                });
            }
        }

        private void AutoFindReferences()
        {
            // 1. Tìm MainPanel
            if (mainPanelArea == null)
            {
                Transform t = FindChildTransform(transform, "MainPanel", "Main", "Other", "OtherSettings", "MapSettings");
                if (t != null) mainPanelArea = t.gameObject;
            }

            // Tìm các thành phần trong MainPanel
            Transform mainRoot = mainPanelArea != null ? mainPanelArea.transform : transform;
            if (mainCreateBtn == null) mainCreateBtn = FindButtonFlexible(mainRoot, "CreateBtn", "CreateQuestionBtn", "NewQuestionBtn", "Create");
            if (mainListBtn == null) mainListBtn = FindButtonFlexible(mainRoot, "ListBtn", "ListQuestionsBtn", "ViewListBtn", "List");

            if (numberStationsInput == null)
            {
                numberStationsInput = FindInputFieldFlexible(mainRoot, "NumberStations/InputField (TMP)", "NumberStations", "StationCount", "Stations");
                if (numberStationsInput == null)
                {
                    Transform t = FindChildTransform(mainRoot, "NumberStations");
                    if (t != null) numberStationsInput = t.GetComponentInChildren<TMP_InputField>(true);
                }
            }

            if (nameLevelInput == null)
            {
                nameLevelInput = FindInputFieldFlexible(mainRoot, "NameLevel/InputField (TMP)", "NameLevel", "LevelName", "MapName");
                if (nameLevelInput == null)
                {
                    Transform t = FindChildTransform(mainRoot, "NameLevel");
                    if (t != null) nameLevelInput = t.GetComponentInChildren<TMP_InputField>(true);
                }
            }

            // 2. Tìm CreateQuestion
            if (createQuestionArea == null)
            {
                Transform t = FindChildTransform(transform, "CreateQuestion", "QuestionEditor", "CreateArea");
                if (t != null) createQuestionArea = t.gameObject;
            }

            // 3. Tìm ListQuestions
            if (listQuestionsArea == null)
            {
                Transform t = FindChildTransform(transform, "ListQuestions", "QuestionList");
                if (t != null) listQuestionsArea = t.gameObject;
            }

            // Tìm các ô nhập trong CreateQuestion
            if (createQuestionArea != null)
            {
                Transform createQ = createQuestionArea.transform;
                if (questionInput == null)
                {
                    Transform q = FindChildTransform(createQ, "Question/InputField (TMP)", "QuestionInput", "Question", "InputField (TMP)");
                    if (q != null) questionInput = q.GetComponent<TMP_InputField>();
                }

                if (answerA == null) answerA = FindInputFieldFlexible(createQ, "AnswerA", "Answer1", "OptionA", "Option1", "D1");
                if (answerB == null) answerB = FindInputFieldFlexible(createQ, "AnswerB", "Answer2", "OptionB", "Option2", "D1 (1)", "D2");
                if (answerC == null) answerC = FindInputFieldFlexible(createQ, "AnswerC", "Answer3", "OptionC", "Option3", "D1 (2)", "D3");
                if (answerD == null) answerD = FindInputFieldFlexible(createQ, "AnswerD", "Answer4", "OptionD", "Option4", "D1 (3)", "D4");

                // Tự động tìm 4 Toggle chọn đáp án đúng
                if (toggleA == null) toggleA = FindToggleFlexible(createQ, "ToggleA", "Toggle1", "CorrectA", "Correct1", "Toggle_A");
                if (toggleB == null) toggleB = FindToggleFlexible(createQ, "ToggleB", "Toggle2", "CorrectB", "Correct2", "Toggle_B");
                if (toggleC == null) toggleC = FindToggleFlexible(createQ, "ToggleC", "Toggle3", "CorrectC", "Correct3", "Toggle_C");
                if (toggleD == null) toggleD = FindToggleFlexible(createQ, "ToggleD", "Toggle4", "CorrectD", "Correct4", "Toggle_D");

                // Nếu chưa tìm thấy theo tên, thử tìm bên trong các container đáp án D1, D2, D3, D4
                if (toggleA == null && answerA != null && answerA.transform.parent != null) toggleA = answerA.transform.parent.GetComponentInChildren<Toggle>(true);
                if (toggleB == null && answerB != null && answerB.transform.parent != null) toggleB = answerB.transform.parent.GetComponentInChildren<Toggle>(true);
                if (toggleC == null && answerC != null && answerC.transform.parent != null) toggleC = answerC.transform.parent.GetComponentInChildren<Toggle>(true);
                if (toggleD == null && answerD != null && answerD.transform.parent != null) toggleD = answerD.transform.parent.GetComponentInChildren<Toggle>(true);

                // Nếu vẫn chưa có, lấy lần lượt 4 Toggle con bất kỳ trong createQ
                if (toggleA == null || toggleB == null || toggleC == null || toggleD == null)
                {
                    Toggle[] allToggles = createQ.GetComponentsInChildren<Toggle>(true);
                    if (allToggles.Length >= 4)
                    {
                        if (toggleA == null) toggleA = allToggles[0];
                        if (toggleB == null) toggleB = allToggles[1];
                        if (toggleC == null) toggleC = allToggles[2];
                        if (toggleD == null) toggleD = allToggles[3];
                    }
                }

                if (correctDropdown == null)
                {
                    Transform d = FindChildTransform(createQ, "CorrectDropdown", "CorrectSelect", "Dropdown", "CorrectAnswerDropdown");
                    if (d != null) correctDropdown = d.GetComponent<TMP_Dropdown>();
                }

                if (createBtn == null) createBtn = FindButtonFlexible(createQ, "CreateBtn", "CreateQuestionButton", "CreateButton", "SaveBtn");
                if (resetBtn == null) resetBtn = FindButtonFlexible(createQ, "ResetBtn", "ResetInputsButton", "ResetButton", "ClearBtn");
                if (viewListBtn == null) viewListBtn = FindButtonFlexible(createQ, "ViewListBtn", "ListBtn", "ViewQuestionsBtn", "ShowListBtn");

                // Tìm cụm Import
                if (importArea == null)
                {
                    Transform t = FindChildTransform(createQ, "Import", "ImportArea", "ImportPanel");
                    if (t != null) importArea = t.gameObject;
                }
            }

            // Tìm các thành phần Import
            Transform importRoot = importArea != null ? importArea.transform : (createQuestionArea != null ? createQuestionArea.transform : transform);
            if (guideImportBtn == null) guideImportBtn = FindButtonFlexible(importRoot, "GuideImportBtn", "GuideBtn", "HelpBtn");
            if (importBtn == null) importBtn = FindButtonFlexible(importRoot, "ImportBtn", "OpenFileBtn");
            if (guidePanel == null)
            {
                Transform t = FindChildTransform(importRoot, "GuidePanel", "HelpPanel", "InstructionPanel");
                if (t != null) guidePanel = t.gameObject;
            }

            if (guidePanel != null)
            {
                Transform gRoot = guidePanel.transform;
                if (closeGuideBtn == null) closeGuideBtn = FindButtonFlexible(gRoot, "CloseBtn", "CloseGuideBtn", "ExitBtn");
                if (guideText == null)
                {
                    Transform t = FindChildTransform(gRoot, "Text (TMP)", "GuideText", "ContentText");
                    if (t != null) guideText = t.GetComponent<TMP_Text>();
                }
            }

            // Tìm danh sách câu hỏi và các nút trong ListQuestions
            if (listQuestionsArea != null)
            {
                Transform listQ = listQuestionsArea.transform;
                if (listContent == null)
                {
                    Transform content = FindChildTransform(listQ, "Scroll View/Viewport/ListQuestions", "ScrollView/Viewport/Content", "Viewport/ListQuestions", "Viewport/Content");
                    if (content != null) listContent = content;
                }

                if (editQuestBtn == null) editQuestBtn = FindButtonFlexible(listQ, "EditQuestBtn", "EditQuest", "EditBtn", "EditButton");
                if (deleteBtn == null) deleteBtn = FindButtonFlexible(listQ, "DeleteQuest", "DeleteQuestBtn", "DeleteQuestionButton", "DeleteBtn");
                if (addNewBtn == null) addNewBtn = FindButtonFlexible(listQ, "AddNewBtn", "CreateNewBtn", "AddBtn", "NewQuestionBtn");
            }

            // Dự phòng tìm kiếm trên toàn bảng nếu chưa thấy
            if (editQuestBtn == null) editQuestBtn = FindButtonFlexible(transform, "EditQuestBtn", "EditQuest", "EditBtn", "EditButton");
            if (deleteBtn == null) deleteBtn = FindButtonFlexible(transform, "DeleteQuest", "DeleteQuestBtn", "DeleteQuestionButton", "DeleteBtn");

            // Tìm nút Back chung
            if (backBtn == null) backBtn = FindButtonFlexible(transform, "BackBtn", "BackButton", "CloseBtn", "CloseButton");
        }

        private Transform FindChildTransform(Transform parent, params string[] possibleNames)
        {
            if (parent == null) return null;
            foreach (string name in possibleNames)
            {
                Transform found = parent.Find(name);
                if (found != null) return found;
            }
            return null;
        }

        private TMP_InputField FindInputFieldFlexible(Transform parent, params string[] possibleNames)
        {
            Transform t = FindChildTransform(parent, possibleNames);
            if (t == null) return null;

            TMP_InputField input = t.GetComponent<TMP_InputField>();
            if (input == null) input = t.GetComponentInChildren<TMP_InputField>(true);
            return input;
        }

        private Button FindButtonFlexible(Transform parent, params string[] possibleNames)
        {
            Transform t = FindChildTransform(parent, possibleNames);
            if (t == null) return null;

            Button btn = t.GetComponent<Button>();
            if (btn == null) btn = t.GetComponentInChildren<Button>(true);
            return btn;
        }

        private Toggle FindToggleFlexible(Transform parent, params string[] possibleNames)
        {
            Transform t = FindChildTransform(parent, possibleNames);
            if (t == null) return null;

            Toggle toggle = t.GetComponent<Toggle>();
            if (toggle == null) toggle = t.GetComponentInChildren<Toggle>(true);
            return toggle;
        }

        private void BindEvents()
        {
            // Gán sự kiện cho các nút trên MainPanel
            if (mainCreateBtn != null)
            {
                mainCreateBtn.onClick.RemoveAllListeners();
                mainCreateBtn.onClick.AddListener(() =>
                {
                    ClearInputs();
                    ShowCreateQuestionMode();
                });
            }

            if (mainListBtn != null)
            {
                mainListBtn.onClick.RemoveAllListeners();
                mainListBtn.onClick.AddListener(ShowListQuestionsMode);
            }

            if (createBtn != null)
            {
                createBtn.onClick.RemoveAllListeners();
                createBtn.onClick.AddListener(OnCreateClicked);
            }
            if (resetBtn != null)
            {
                resetBtn.onClick.RemoveAllListeners();
                resetBtn.onClick.AddListener(OnResetClicked);
            }
            if (editQuestBtn != null)
            {
                editQuestBtn.onClick.RemoveAllListeners();
                editQuestBtn.onClick.AddListener(OnEditQuestionClicked);
            }
            if (deleteBtn != null)
            {
                deleteBtn.onClick.RemoveAllListeners();
                deleteBtn.onClick.AddListener(OnDeleteClicked);
            }
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OnBackClicked);
            }

            // Gán sự kiện nút Back con bên trong CreateQuestion hoặc ListQuestions (nếu có)
            if (createQuestionArea != null)
            {
                Button backInCreate = FindButtonFlexible(createQuestionArea.transform, "BackBtn", "BackToMainBtn", "CloseBtn", "CancelBtn");
                if (backInCreate != null && backInCreate != backBtn)
                {
                    backInCreate.onClick.RemoveAllListeners();
                    backInCreate.onClick.AddListener(ShowMainPanelMode);
                }
            }

            if (listQuestionsArea != null)
            {
                Button backInList = FindButtonFlexible(listQuestionsArea.transform, "BackBtn", "BackToMainBtn", "CloseBtn", "CancelBtn");
                if (backInList != null && backInList != backBtn)
                {
                    backInList.onClick.RemoveAllListeners();
                    backInList.onClick.AddListener(ShowMainPanelMode);
                }
            }

            if (viewListBtn != null)
            {
                viewListBtn.onClick.RemoveAllListeners();
                viewListBtn.onClick.AddListener(ShowListQuestionsMode);
            }
            if (addNewBtn != null)
            {
                addNewBtn.onClick.RemoveAllListeners();
                addNewBtn.onClick.AddListener(OnAddNewClicked);
            }

            // Gán sự kiện Import & Hướng dẫn
            if (guideImportBtn != null)
            {
                guideImportBtn.onClick.RemoveAllListeners();
                guideImportBtn.onClick.AddListener(ShowGuidePanel);
            }
            if (closeGuideBtn != null)
            {
                closeGuideBtn.onClick.RemoveAllListeners();
                closeGuideBtn.onClick.AddListener(HideGuidePanel);
            }
            if (importBtn != null)
            {
                importBtn.onClick.RemoveAllListeners();
                importBtn.onClick.AddListener(OnImportClicked);
            }

            // Gán sự kiện thay phiên chọn cho 4 Toggle
            BindToggleEvents();

            if (correctDropdown != null)
            {
                correctDropdown.onValueChanged.RemoveAllListeners();
                correctDropdown.onValueChanged.AddListener(val => SetCorrectAnswer(val));
            }

            // Gán sự kiện thay đổi tên map và số lượng trạm
            if (nameLevelInput != null)
            {
                nameLevelInput.onEndEdit.RemoveAllListeners();
                nameLevelInput.onEndEdit.AddListener(val =>
                {
                    if (currentMap != null && !string.IsNullOrWhiteSpace(val))
                    {
                        currentMap.mapName = val.Trim();
                        SaveData();
                    }
                });
            }

            if (numberStationsInput != null)
            {
                numberStationsInput.onEndEdit.RemoveAllListeners();
                numberStationsInput.onEndEdit.AddListener(val =>
                {
                    if (currentMap != null && int.TryParse(val, out int count))
                    {
                        currentMap.stationCount = Mathf.Max(1, count);
                        SaveData();
                    }
                });
            }
        }

        public void ShowGuidePanel()
        {
            if (guidePanel != null)
            {
                guidePanel.SetActive(true);
                SetupGuideContent();
            }
        }

        public void HideGuidePanel()
        {
            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
            }
        }

        private void BindToggleEvents()
        {
            UnbindToggleEvents();
            if (toggleA != null) toggleA.onValueChanged.AddListener(isOn => { if (isOn) SetCorrectAnswer(0); else EnsureAtLeastOneToggleOn(0); });
            if (toggleB != null) toggleB.onValueChanged.AddListener(isOn => { if (isOn) SetCorrectAnswer(1); else EnsureAtLeastOneToggleOn(1); });
            if (toggleC != null) toggleC.onValueChanged.AddListener(isOn => { if (isOn) SetCorrectAnswer(2); else EnsureAtLeastOneToggleOn(2); });
            if (toggleD != null) toggleD.onValueChanged.AddListener(isOn => { if (isOn) SetCorrectAnswer(3); else EnsureAtLeastOneToggleOn(3); });
        }

        private void UnbindToggleEvents()
        {
            if (toggleA != null) toggleA.onValueChanged.RemoveAllListeners();
            if (toggleB != null) toggleB.onValueChanged.RemoveAllListeners();
            if (toggleC != null) toggleC.onValueChanged.RemoveAllListeners();
            if (toggleD != null) toggleD.onValueChanged.RemoveAllListeners();
        }

        private void EnsureAtLeastOneToggleOn(int turnedOffIndex)
        {
            if (GetCorrectAnswerIndex() == -1)
            {
                SetCorrectAnswer(turnedOffIndex);
            }
        }

        /// <summary>
        /// Đặt toggle cho đáp án đúng (0: A, 1: B, 2: C, 3: D). Mặc định index = 0 (A) là correct và isOn = true
        /// </summary>
        public void SetCorrectAnswer(int index)
        {
            index = Mathf.Clamp(index, 0, 3);

            UnbindToggleEvents();

            if (toggleA != null) toggleA.isOn = (index == 0);
            if (toggleB != null) toggleB.isOn = (index == 1);
            if (toggleC != null) toggleC.isOn = (index == 2);
            if (toggleD != null) toggleD.isOn = (index == 3);

            BindToggleEvents();

            if (correctDropdown != null && correctDropdown.value != index)
            {
                correctDropdown.SetValueWithoutNotify(index);
            }
        }

        /// <summary>
        /// Lấy index của đáp án đúng hiện tại (0: A, 1: B, 2: C, 3: D). Mặc định là 0 nếu không có toggle nào bật
        /// </summary>
        public int GetCorrectAnswerIndex()
        {
            if (toggleA != null && toggleA.isOn) return 0;
            if (toggleB != null && toggleB.isOn) return 1;
            if (toggleC != null && toggleC.isOn) return 2;
            if (toggleD != null && toggleD.isOn) return 3;

            if (toggleA == null && toggleB == null && toggleC == null && toggleD == null)
            {
                if (correctDropdown != null) return correctDropdown.value;
            }

            return 0; // Mặc định đáp án A (index 0) là đúng
        }

        /// <summary>
        /// Bật/tắt trạng thái của nút: chỉnh interactable và độ sáng rõ rệt (Alpha = 1 khi bật, Alpha = 0.4 khi tắt)
        /// </summary>
        private void SetButtonState(Button btn, bool enabled)
        {
            if (btn == null) return;

            btn.interactable = enabled;

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = btn.gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = enabled ? 1f : 0.4f;
        }

        /// <summary>
        /// Mở bảng EditMapPanel: Mặc định hiển thị MainPanel đầu tiên khi bấm Edit ở thẻ map
        /// </summary>
        public void Open(MapData map, Action onDataChanged = null, Action onClose = null)
        {
            currentMap = map;
            onDataChangedCallback = onDataChanged;
            onCloseCallback = onClose;

            if (currentMap.questions == null)
            {
                currentMap.questions = new List<QuestionData>();
            }

            gameObject.SetActive(true);
            ClearInputs();
            HideGuidePanel();

            // MẶC ĐỊNH LOAD MAINPANEL ĐẦU TIÊN KHI BẤM EDIT Ở MAP
            ShowMainPanelMode();
        }

        /// <summary>
        /// Xử lý bấm nút Back: Nếu đang ở CreateQuestion hoặc ListQuestions thì quay về MainPanel,
        /// nếu đang ở MainPanel thì đóng bảng Edit và quay về MapManagerPanel.
        /// </summary>
        public void OnBackClicked()
        {
            if ((createQuestionArea != null && createQuestionArea.activeSelf) ||
                (listQuestionsArea != null && listQuestionsArea.activeSelf))
            {
                ShowMainPanelMode();
            }
            else
            {
                Close();
            }
        }

        public void Close()
        {
            HideGuidePanel();
            gameObject.SetActive(false);

            if (onCloseCallback != null)
            {
                onCloseCallback.Invoke();
            }
            else
            {
                // Dự phòng: Nếu không có callback, tự động tìm MapManager trong scene để mở lại MapManagerPanel
                MapManager mapMgr = FindFirstObjectByType<MapManager>();
                if (mapMgr != null)
                {
                    mapMgr.CloseEditMap();
                }
            }
        }

        /// <summary>
        /// Chế độ 0: Bật bảng chính (MainPanel), ẩn CreateQuestion và ListQuestions
        /// </summary>
        public void ShowMainPanelMode()
        {
            if (mainPanelArea != null) mainPanelArea.SetActive(true);
            if (createQuestionArea != null) createQuestionArea.SetActive(false);
            if (listQuestionsArea != null) listQuestionsArea.SetActive(false);
            HideGuidePanel();

            // Cập nhật dữ liệu hiển thị trên MainPanel
            if (currentMap != null)
            {
                if (nameLevelInput != null)
                {
                    nameLevelInput.text = currentMap.mapName;
                }
                if (numberStationsInput != null)
                {
                    numberStationsInput.text = (currentMap.stationCount <= 0 ? 5 : currentMap.stationCount).ToString();
                }
            }
        }

        /// <summary>
        /// Chế độ 1: Bật bảng nhập câu hỏi (CreateQuestion), ẩn MainPanel và ListQuestions
        /// </summary>
        public void ShowCreateQuestionMode()
        {
            if (mainPanelArea != null) mainPanelArea.SetActive(false);
            if (createQuestionArea != null) createQuestionArea.SetActive(true);
            if (listQuestionsArea != null) listQuestionsArea.SetActive(false);
        }

        /// <summary>
        /// Chế độ 2: Bật bảng danh sách (ListQuestions), ẩn MainPanel và CreateQuestion
        /// </summary>
        public void ShowListQuestionsMode()
        {
            if (mainPanelArea != null) mainPanelArea.SetActive(false);
            if (createQuestionArea != null) createQuestionArea.SetActive(false);
            if (listQuestionsArea != null) listQuestionsArea.SetActive(true);
            RefreshListUI();
        }

        public void RefreshListUI()
        {
            if (listContent == null) return;

            foreach (Transform child in listContent)
            {
                Destroy(child.gameObject);
            }

            selectedQuestion = null;
            selectedItemUI = null;

            // Ban đầu chưa chọn câu hỏi nào -> 2 nút Chỉnh sửa & Xóa bị làm mờ / tắt
            SetButtonState(editQuestBtn, false);
            SetButtonState(deleteBtn, false);

            if (currentMap == null || currentMap.questions == null) return;

            for (int i = 0; i < currentMap.questions.Count; i++)
            {
                QuestionData q = currentMap.questions[i];
                GameObject itemObj = null;

                if (questionPrefab != null)
                {
                    itemObj = Instantiate(questionPrefab, listContent);
                }
                else
                {
                    itemObj = new GameObject($"QuestionItem_{i + 1}", typeof(RectTransform));
                    itemObj.transform.SetParent(listContent, false);
                }

                QuestionItemUI itemUI = itemObj.GetComponent<QuestionItemUI>();
                if (itemUI == null) itemUI = itemObj.AddComponent<QuestionItemUI>();

                itemUI.Setup(q, OnSelectQuestionItem);
            }
        }

        /// <summary>
        /// Khi bấm vào 1 câu hỏi trong danh sách:
        /// 1. Bật Selection và đặt Alpha = 1 cho câu hỏi đó
        /// 2. BẬT SÁNG 2 NÚT (EditQuestBtn và DeleteQuest) để người dùng chọn hành động Sửa hoặc Xóa!
        /// 3. VẪN Ở LẠI BẢNG DANH SÁCH (Không tự động nhảy sang CreateQuestion)
        /// </summary>
        private void OnSelectQuestionItem(QuestionData data, QuestionItemUI ui)
        {
            if (selectedItemUI != null)
            {
                selectedItemUI.SetSelected(false);
            }

            selectedQuestion = data;
            selectedItemUI = ui;

            if (selectedItemUI != null)
            {
                selectedItemUI.SetSelected(true);
            }

            // 2 nút sáng lên và cho phép bấm
            SetButtonState(editQuestBtn, true);
            SetButtonState(deleteBtn, true);

            Debug.Log($"[EditMapPanelUI] Đã chọn câu hỏi: '{data.questionText}'. 2 nút Chỉnh sửa và Xóa đã sáng lên!");
        }

        /// <summary>
        /// Bấm nút Sửa câu hỏi (EditQuestBtn): Nạp dữ liệu câu hỏi đã chọn và mở bảng CreateQuestion
        /// </summary>
        private void OnEditQuestionClicked()
        {
            if (selectedQuestion == null)
            {
                Debug.LogWarning("[EditMapPanelUI] Hãy chọn một câu hỏi để chỉnh sửa!");
                return;
            }

            // Nạp dữ liệu vào ô input
            editingQuestion = selectedQuestion;
            if (questionInput != null) questionInput.text = editingQuestion.questionText;
            if (answerA != null) answerA.text = editingQuestion.option1;
            if (answerB != null) answerB.text = editingQuestion.option2;
            if (answerC != null) answerC.text = editingQuestion.option3;
            if (answerD != null) answerD.text = editingQuestion.option4;

            // Nạp đáp án đúng vào Toggle (hoặc Dropdown)
            SetCorrectAnswer(editingQuestion.correctIndex);

            // Mở bảng CreateQuestion để chỉnh sửa
            ShowCreateQuestionMode();
            Debug.Log($"[EditMapPanelUI] Bấm nút Sửa: mở form chỉnh sửa câu hỏi '{editingQuestion.questionText}' (Đáp án đúng: {(char)('A' + editingQuestion.correctIndex)})!");
        }

        /// <summary>
        /// Bấm nút AddNewBtn: Xóa trắng form và mở CreateQuestion để tạo câu mới
        /// </summary>
        private void OnAddNewClicked()
        {
            ClearInputs();
            ShowCreateQuestionMode();
        }

        /// <summary>
        /// Bấm CreateBtn: Lưu câu hỏi mới hoặc cập nhật câu hỏi đang sửa
        /// </summary>
        private void OnCreateClicked()
        {
            string qText = questionInput != null ? questionInput.text.Trim() : "";
            string opt1 = answerA != null ? answerA.text.Trim() : "";
            string opt2 = answerB != null ? answerB.text.Trim() : "";
            string opt3 = answerC != null ? answerC.text.Trim() : "";
            string opt4 = answerD != null ? answerD.text.Trim() : "";

            int correctIdx = GetCorrectAnswerIndex();

            if (string.IsNullOrEmpty(qText))
            {
                qText = "Câu hỏi mới";
            }

            if (editingQuestion != null)
            {
                editingQuestion.questionText = qText;
                editingQuestion.option1 = opt1;
                editingQuestion.option2 = opt2;
                editingQuestion.option3 = opt3;
                editingQuestion.option4 = opt4;
                editingQuestion.correctIndex = correctIdx;
                Debug.Log($"[EditMapPanelUI] Đã cập nhật và lưu câu hỏi! (Đáp án đúng: {(char)('A' + correctIdx)})");
            }
            else
            {
                QuestionData newQ = new QuestionData(qText, opt1, opt2, opt3, opt4, correctIdx);
                currentMap.questions.Add(newQ);
                Debug.Log($"[EditMapPanelUI] Đã tạo câu hỏi mới (Đáp án đúng: {(char)('A' + correctIdx)}): '{qText}'");
            }

            SaveData();
            ClearInputs();
            // Trở về danh sách câu hỏi để xem câu vừa tạo/sửa
            ShowListQuestionsMode();
        }

        private void OnResetClicked()
        {
            ClearInputs();
        }

        /// <summary>
        /// Xóa câu hỏi đang chọn khỏi map và lưu lại
        /// </summary>
        private void OnDeleteClicked()
        {
            if (selectedQuestion == null)
            {
                Debug.LogWarning("[EditMapPanelUI] Hãy chọn một câu hỏi để xóa!");
                return;
            }

            currentMap.questions.Remove(selectedQuestion);
            Debug.Log($"[EditMapPanelUI] Đã xóa câu hỏi: '{selectedQuestion.questionText}' khỏi map!");

            SaveData();
            ClearInputs();
            RefreshListUI();
        }

        private void ClearInputs()
        {
            editingQuestion = null;
            selectedQuestion = null;

            if (selectedItemUI != null)
            {
                selectedItemUI.SetSelected(false);
                selectedItemUI = null;
            }

            if (questionInput != null) questionInput.text = "";
            if (answerA != null) answerA.text = "";
            if (answerB != null) answerB.text = "";
            if (answerC != null) answerC.text = "";
            if (answerD != null) answerD.text = "";

            // MẶC ĐỊNH ĐÁP ÁN ĐẦU TIÊN (A) LÀ CORRECT VÀ ISON = TRUE
            SetCorrectAnswer(0);

            SetButtonState(editQuestBtn, false);
            SetButtonState(deleteBtn, false);
        }

        private void SaveData()
        {
            if (currentMap != null)
            {
                if (nameLevelInput != null && !string.IsNullOrWhiteSpace(nameLevelInput.text))
                {
                    currentMap.mapName = nameLevelInput.text.Trim();
                }
                if (numberStationsInput != null && int.TryParse(numberStationsInput.text, out int count))
                {
                    currentMap.stationCount = Mathf.Max(1, count);
                }
            }
            onDataChangedCallback?.Invoke();
        }

        #region --- TÍNH NĂNG IMPORT CÂU HỎI TỪ FILE (.TXT / .CSV) ---

        /// <summary>
        /// Bấm nút ImportBtn: Mở hộp thoại chọn file trên Windows (.txt hoặc .csv) và nạp câu hỏi
        /// </summary>
        public void OnImportClicked()
        {
            string filePath = OpenQuestionFileDialog();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.Log("[EditMapPanelUI] Người dùng đã hủy chọn file hoặc file không tồn tại.");
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                List<QuestionData> importedList = ParseQuestionsFromText(content, filePath);

                if (importedList != null && importedList.Count > 0)
                {
                    if (currentMap.questions == null)
                    {
                        currentMap.questions = new List<QuestionData>();
                    }

                    currentMap.questions.AddRange(importedList);
                    SaveData();
                    RefreshListUI();

                    Debug.Log($"[EditMapPanelUI] ĐÃ IMPORT THÀNH CÔNG {importedList.Count} CÂU HỎI từ file: {Path.GetFileName(filePath)}!");

                    // Chuyển sang màn hình danh sách để giáo viên xem kết quả ngay
                    ShowListQuestionsMode();
                }
                else
                {
                    Debug.LogWarning("[EditMapPanelUI] Không tìm thấy câu hỏi hợp lệ trong file! Hãy bấm 'Hướng dẫn' để xem cấu trúc file.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EditMapPanelUI] Lỗi khi đọc file import: {ex.Message}");
            }
        }

        private string OpenQuestionFileDialog()
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel("Chọn file câu hỏi để Import (.txt, .csv)", "", "txt,csv");
#elif UNITY_STANDALONE_WIN
            string filter = "Question Files (*.txt;*.csv)\0*.txt;*.csv\0Text Files (*.txt)\0*.txt\0CSV Files (*.csv)\0*.csv\0All Files (*.*)\0*.*\0\0";
            return Win32FileBrowser.OpenFile("Chọn file câu hỏi để Import", filter);
#else
            return null;
#endif
        }

        /// <summary>
        /// Bộ phân tích thông minh đọc các câu hỏi từ văn bản .txt (chuẩn giáo viên) hoặc .csv (Excel)
        /// Mặc định: Đáp án A (đầu tiên) là đáp án đúng (để sau này game tự xáo trộn ngẫu nhiên)
        /// </summary>
        public static List<QuestionData> ParseQuestionsFromText(string content, string filePath = "")
        {
            List<QuestionData> list = new List<QuestionData>();
            if (string.IsNullOrEmpty(content)) return list;

            bool isCsv = !string.IsNullOrEmpty(filePath) && filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            string[] rawLines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (isCsv)
            {
                for (int i = 0; i < rawLines.Length; i++)
                {
                    string line = rawLines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Bỏ qua dòng tiêu đề nếu có
                    if (i == 0 && (line.ToLower().Contains("câu hỏi") || line.ToLower().Contains("question"))) continue;

                    string[] parts = SplitCsvLine(line);
                    if (parts.Length >= 5)
                    {
                        string q = parts[0].Trim();
                        string a = parts[1].Trim();
                        string b = parts[2].Trim();
                        string c = parts[3].Trim();
                        string d = parts[4].Trim();
                        int correct = 0; // Mặc định đáp án đầu tiên (A) là đúng

                        // Kiểm tra nếu có dấu * ở bất kỳ đáp án nào
                        if (a.StartsWith("*") || a.EndsWith("*")) { correct = 0; }
                        else if (b.StartsWith("*") || b.EndsWith("*")) { correct = 1; }
                        else if (c.StartsWith("*") || c.EndsWith("*")) { correct = 2; }
                        else if (d.StartsWith("*") || d.EndsWith("*")) { correct = 3; }
                        else if (parts.Length >= 6 && !string.IsNullOrEmpty(parts[5].Trim()))
                        {
                            correct = ParseAnswerLetterToIndex(parts[5].Trim());
                        }

                        // Xóa dấu * khỏi nội dung text hiển thị
                        a = a.Trim('*').Trim();
                        b = b.Trim('*').Trim();
                        c = c.Trim('*').Trim();
                        d = d.Trim('*').Trim();

                        if (!string.IsNullOrEmpty(q))
                        {
                            list.Add(new QuestionData(q, a, b, c, d, correct));
                        }
                    }
                }
            }
            else
            {
                // Xử lý TXT (Chuẩn đề thi giáo viên)
                string currentQuestion = "";
                string optA = "";
                string optB = "";
                string optC = "";
                string optD = "";
                int correctIdx = 0; // Mặc định đáp án A (đầu tiên) là đúng nếu không có dấu *

                Action commitQuestion = () =>
                {
                    if (!string.IsNullOrEmpty(currentQuestion) && (!string.IsNullOrEmpty(optA) || !string.IsNullOrEmpty(optB)))
                    {
                        list.Add(new QuestionData(currentQuestion.Trim(), optA.Trim(), optB.Trim(), optC.Trim(), optD.Trim(), correctIdx));
                    }
                    currentQuestion = "";
                    optA = "";
                    optB = "";
                    optC = "";
                    optD = "";
                    correctIdx = 0; // Reset về mặc định
                };

                foreach (string rawLine in rawLines)
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        if (!string.IsNullOrEmpty(currentQuestion) && !string.IsNullOrEmpty(optA))
                        {
                            commitQuestion();
                        }
                        continue;
                    }

                    // Kiểm tra dòng chỉ định đáp án đúng (Đáp án: A, ĐA: B, Answer: C...)
                    if (line.StartsWith("Đáp án:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Đáp án", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("ĐA:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Answer:", StringComparison.OrdinalIgnoreCase))
                    {
                        string ansStr = line.Substring(line.IndexOfAny(new[] { ':', ' ' }) + 1).Trim();
                        correctIdx = ParseAnswerLetterToIndex(ansStr);
                        continue;
                    }

                    // Kiểm tra các dòng lựa chọn A, B, C, D (chỉ cần thêm dấu * trước hoặc sau đáp án đúng)
                    if (IsOptionLine(line, 'A', out string textA, out bool isStarA))
                    {
                        optA = textA;
                        if (isStarA) correctIdx = 0;
                        continue;
                    }
                    if (IsOptionLine(line, 'B', out string textB, out bool isStarB))
                    {
                        optB = textB;
                        if (isStarB) correctIdx = 1;
                        continue;
                    }
                    if (IsOptionLine(line, 'C', out string textC, out bool isStarC))
                    {
                        optC = textC;
                        if (isStarC) correctIdx = 2;
                        continue;
                    }
                    if (IsOptionLine(line, 'D', out string textD, out bool isStarD))
                    {
                        optD = textD;
                        if (isStarD) correctIdx = 3;
                        continue;
                    }

                    // Nếu là dòng bắt đầu câu hỏi mới (ví dụ: Câu 1., Câu 2:, Question 1...)
                    if (IsQuestionStart(line))
                    {
                        commitQuestion();
                        currentQuestion = CleanQuestionPrefix(line);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(currentQuestion))
                        {
                            currentQuestion = line;
                        }
                        else if (string.IsNullOrEmpty(optA))
                        {
                            currentQuestion += " " + line;
                        }
                    }
                }

                commitQuestion();
            }

            return list;
        }

        private static bool IsOptionLine(string line, char letter, out string content, out bool isStar)
        {
            content = "";
            isStar = false;
            string s = line.Trim();

            // 1. Dấu * ở đầu dòng (ví dụ: *A. hoặc *B. hoặc * B.)
            if (s.StartsWith("*"))
            {
                isStar = true;
                s = s.Substring(1).Trim();
            }

            // 2. Chữ cái đại diện đáp án (A, B, C, D)
            if (s.Length >= 2 && char.ToUpper(s[0]) == letter)
            {
                // Kiểm tra dấu * ngay cạnh chữ cái (ví dụ A*. hoặc A* hoặc A.*)
                if (s.Length >= 3 && (s[1] == '*' || s[2] == '*'))
                {
                    isStar = true;
                    s = s[0] + s.Substring(1).Replace("*", "");
                }

                if (s.Length >= 2 && (s[1] == '.' || s[1] == ')' || s[1] == ':' || s[1] == '-'))
                {
                    content = s.Substring(2).Trim();

                    // Kiểm tra dấu * trong nội dung đáp án (ví dụ: B. *Hà Nội hoặc B. Hà Nội*)
                    if (content.StartsWith("*"))
                    {
                        isStar = true;
                        content = content.TrimStart('*').Trim();
                    }
                    if (content.EndsWith("*"))
                    {
                        isStar = true;
                        content = content.TrimEnd('*').Trim();
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool IsQuestionStart(string line)
        {
            string lower = line.ToLower();
            return lower.StartsWith("câu ") || lower.StartsWith("cau ") || lower.StartsWith("question ");
        }

        private static string CleanQuestionPrefix(string line)
        {
            string s = line;
            int colonIndex = s.IndexOf(':');
            int dotIndex = s.IndexOf('.');
            int splitIdx = -1;

            if (colonIndex > 0 && colonIndex < 12) splitIdx = colonIndex;
            else if (dotIndex > 0 && dotIndex < 12) splitIdx = dotIndex;

            if (splitIdx > 0 && splitIdx + 1 < s.Length)
            {
                return s.Substring(splitIdx + 1).Trim();
            }
            return s;
        }

        private static int ParseAnswerLetterToIndex(string str)
        {
            str = str.Trim().ToUpper();
            if (str.Contains("A") || str.Contains("1")) return 0;
            if (str.Contains("B") || str.Contains("2")) return 1;
            if (str.Contains("C") || str.Contains("3")) return 2;
            if (str.Contains("D") || str.Contains("4")) return 3;
            return 0; // Mặc định là A (đáp án đầu tiên)
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);
            return result.ToArray();
        }

        #endregion
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string filter = null;
        public string customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public string file = null;
        public int maxFile = 0;
        public string fileTitle = null;
        public int maxFileTitle = 0;
        public string initialDir = null;
        public string title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public string defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    public static class Win32FileBrowser
    {
        [System.Runtime.InteropServices.DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool GetOpenFileName([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] OpenFileName ofn);

        public static string OpenFile(string title, string filter)
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = System.Runtime.InteropServices.Marshal.SizeOf(ofn);
            ofn.filter = filter;
            ofn.file = new string(new char[260]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = UnityEngine.Application.dataPath;
            ofn.title = title;
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR

            if (GetOpenFileName(ofn))
            {
                return ofn.file;
            }
            return null;
        }
    }
#endif
}

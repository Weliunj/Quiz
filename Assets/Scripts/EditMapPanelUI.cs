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
        [Header("--- 1. BẢNG CHÍNH (TAB 1: MAIN PANEL) ---")]
        [Tooltip("Bảng chính của EditMapPanel (MainPanel: hiển thị đầu tiên khi bấm Edit trên thẻ map)")]
        [FormerlySerializedAs("otherArea")]
        [SerializeField] private GameObject mainPanelArea;

        [Tooltip("Ô nhập tên màn chơi (NameMap)")]
        [SerializeField] private TMP_InputField nameLevelInput;

        [Tooltip("Ô nhập số lượng trạm đến đỉnh núi (NumberStations)")]
        [SerializeField] private TMP_InputField numberStationsInput;

        [Tooltip("Nút chọn Theme cho bản đồ (ThemeBtn trong MainPanel/Theme)")]
        [SerializeField] private Button themeBtn;

        [Tooltip("Text hiển thị tên Theme (Map/Title/Text (TMP))")]
        [SerializeField] private TMP_Text themeNameText;

        [Tooltip("Ảnh hiển thị theme bản đồ (Map/Mask/Image hoặc MapImg)")]
        [FormerlySerializedAs("mapThemeImage")]
        [SerializeField] private Image mapImg;

        [Tooltip("Danh sách ThemeSO tùy chọn cấu hình trong game")]
        [SerializeField] private List<ThemeSO> availableThemes = new List<ThemeSO>();

        [Header("--- 2. THANH TAB DỌC (MENU TAB PANEL) ---")]
        [Tooltip("Bảng chứa 3 nút chuyển Tab dọc (MenuTabPanel)")]
        [SerializeField] private GameObject menuTabPanel;

        [Tooltip("Nút chuyển sang tab MainPanel (MainBtn trong MenuTabPanel)")]
        [SerializeField] private Button tabMainBtn;

        [Tooltip("Nút chuyển sang tab Tạo câu hỏi (CreateBtn trong MenuTabPanel)")]
        [SerializeField] private Button tabCreateBtn;

        [Tooltip("Nút chuyển sang tab Danh sách câu hỏi (ListBtn trong MenuTabPanel)")]
        [SerializeField] private Button tabListBtn;

        [Header("--- 3. KHU VỰC TẠO & SỬA CÂU HỎI (TAB 2: CREATE QUESTION PANEL) ---")]
        [Tooltip("Khu vực nhập câu hỏi & đáp án (CreateQuestionPanel)")]
        [SerializeField] private GameObject createQuestionArea;

        [Tooltip("Ô nhập nội dung câu hỏi")]
        [SerializeField] private TMP_InputField questionInput;

        [Tooltip("Ô nhập thời gian làm câu hỏi (giây), mặc định 30s")]
        [SerializeField] private TMP_InputField timeInput;

        [Tooltip("Ô nhập đáp án A (1) - LUÔN LÀ ĐÁP ÁN ĐÚNG")]
        [SerializeField] private TMP_InputField answerA;

        [Tooltip("Ô nhập đáp án B (2)")]
        [SerializeField] private TMP_InputField answerB;

        [Tooltip("Ô nhập đáp án C (3)")]
        [SerializeField] private TMP_InputField answerC;

        [Tooltip("Ô nhập đáp án D (4)")]
        [SerializeField] private TMP_InputField answerD;

        [Tooltip("Nút tạo mới hoặc lưu chỉnh sửa câu hỏi (CreateBtn trong Tab 2)")]
        [SerializeField] private Button createBtn;

        [Tooltip("Nút xóa trắng các ô nhập để viết câu mới (ResetBtn trong Tab 2)")]
        [SerializeField] private Button resetBtn;

        [Header("--- TÍNH NĂNG IMPORT TRONG TAB 2 ---")]
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

        [Tooltip("Nút copy nội dung hướng dẫn (DEFAULT_IMPORT_GUIDE) vào Clipboard")]
        [SerializeField] private Button copyGuideBtn;

        [Header("--- 4. KHU VỰC DANH SÁCH CÂU HỎI (TAB 3: LIST QUESTIONS PANEL) ---")]
        [Tooltip("Khu vực danh sách các câu hỏi (ListQuestionsPanel)")]
        [SerializeField] private GameObject listQuestionsArea;

        [Tooltip("Vùng chứa danh sách câu hỏi trong Scroll View Viewport")]
        [SerializeField] private Transform listContent;

        [Tooltip("Prefab hiển thị một dòng câu hỏi (QuestionContent.prefab)")]
        [SerializeField] private GameObject questionPrefab;

        [Tooltip("Nút mở form chỉnh sửa câu hỏi đã chọn trong danh sách (EditQuestBtn)")]
        [SerializeField] private Button editQuestBtn;

        [Tooltip("Nút xóa câu hỏi đang chọn khỏi map (DeleteQuest)")]
        [SerializeField] private Button deleteBtn;



        [Header("--- 5. ĐIỀU HƯỚNG CHUNG ---")]
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
            "<b>1. File văn bản (.TXT) — Đánh dấu sao (*) đáp án đúng & cài thời gian:</b>\n" +
            "Soạn trên Word hoặc Notepad, mỗi câu cách nhau 1 dòng trống. <b>Chỉ cần thêm dấu * trước đáp án đúng</b> (không cần đổi thứ tự câu hỏi hay ghi thêm dòng đáp án!):\n\n" +
            "<i>  Câu 1: Đỉnh núi Fansipan cao bao nhiêu mét?\n" +
            "<b>  Time: 45s</b>\n" +
            "<b>*  A. 3.143m</b>\n" +
            "  B. 2.800m\n" +
            "  C. 3.000m\n" +
            "  D. 3.500m</i>\n\n" +
            "<b>2. File Excel (.CSV):</b>\n" +
            "Soạn theo cột: <i>Câu hỏi, Đáp án A, Đáp án B, Đáp án C, Đáp án D, Đáp án đúng, Thời gian(s)</i>.\n" +
            "Ví dụ: <i>\"Thủ đô của Việt Nam là gì?\", \"Hà Nội*\", \"Huế\", \"Đà Nẵng\", \"TP.HCM\", \"A\", \"15\"</i> (cột thời gian nếu để trống sẽ tự động lấy mặc định 30s).";

        private void Awake()
        {
            AutoFindReferences();
            SetupGuideContent();
            BindEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoFindReferences();
            EnsureThemesLoaded();
            UpdateThemeBtnUI();
            UpdateTabVisuals(0);
        }
#endif

        private void SetupGuideContent()
        {
            if (guideText != null)
            {
                guideText.text = DEFAULT_IMPORT_GUIDE;
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

            // Tìm MenuTabPanel và 3 nút tab dọc (MainBtn, CreateBtn, ListBtn)
            if (menuTabPanel == null)
            {
                Transform t = FindChildTransform(transform, "MenuTabPanel", "TabPanel", "SideTabPanel", "MenuPanel");
                if (t != null) menuTabPanel = t.gameObject;
            }

            Transform tabRoot = menuTabPanel != null ? menuTabPanel.transform : transform;
            if (tabMainBtn == null)
            {
                tabMainBtn = FindButtonFlexible(tabRoot, "MainBtn", "TabMainBtn", "MainTabBtn", "MainButton");
            }
            if (tabCreateBtn == null)
            {
                tabCreateBtn = FindButtonFlexible(tabRoot, "CreateBtn", "TabCreateBtn", "CreateTabBtn", "CreateButton");
            }
            if (tabListBtn == null)
            {
                tabListBtn = FindButtonFlexible(tabRoot, "ListBtn", "TabListBtn", "ListTabBtn", "ListButton");
            }

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
                nameLevelInput = FindInputFieldFlexible(mainRoot, "NameMap/InputField (TMP)", "NameMap", "NameLevel/InputField (TMP)", "NameLevel", "LevelName", "MapName");
                if (nameLevelInput == null)
                {
                    Transform t = FindChildTransform(mainRoot, "NameMap", "NameLevel");
                    if (t != null) nameLevelInput = t.GetComponentInChildren<TMP_InputField>(true);
                }
            }

            // Tìm ThemeBtn, mapImg và themeNameText (Title/Text (TMP))
            if (themeBtn == null)
            {
                themeBtn = FindButtonFlexible(mainRoot, "Theme/ThemeBtn", "ThemeBtn", "SelectThemeBtn", "ThemeButton");
                if (themeBtn == null)
                {
                    Transform t = FindTransformDeep(mainRoot, "ThemeBtn", "SelectThemeBtn");
                    if (t != null) themeBtn = t.GetComponent<Button>() ?? t.GetComponentInChildren<Button>(true);
                }
                if (themeBtn == null)
                {
                    Transform themeGroup = FindTransformDeep(mainRoot, "Theme");
                    if (themeGroup != null) themeBtn = themeGroup.GetComponentInChildren<Button>(true);
                }
                if (themeBtn == null)
                {
                    themeBtn = FindButtonFlexible(transform, "Theme/ThemeBtn", "ThemeBtn", "SelectThemeBtn");
                }
            }

            if (mapImg == null)
            {
                Transform imgT = FindTransformDeep(mainRoot,
                    "Theme/Map/Mask/Image",
                    "Map/Mask/Image",
                    "Mask/Image",
                    "MapImg",
                    "MapImage",
                    "ThemeImage");
                if (imgT != null)
                {
                    mapImg = imgT.GetComponent<Image>();
                }
                if (mapImg == null)
                {
                    Transform mapTransform = FindTransformDeep(mainRoot, "Map", "Theme");
                    if (mapTransform != null)
                    {
                        Transform maskT = FindTransformDeep(mapTransform, "Mask");
                        if (maskT != null) mapImg = maskT.GetComponentInChildren<Image>(true);
                        if (mapImg == null) mapImg = mapTransform.GetComponentInChildren<Image>(true);
                    }
                }
                if (mapImg == null)
                {
                    imgT = FindTransformDeep(transform, "Theme/Map/Mask/Image", "Map/Mask/Image");
                    if (imgT != null) mapImg = imgT.GetComponent<Image>();
                }
            }

            if (themeNameText == null)
            {
                Transform titleT = FindTransformDeep(mainRoot,
                    "Theme/Map/Title/Text (TMP)",
                    "Map/Title/Text (TMP)",
                    "Title/Text (TMP)",
                    "Theme/Title/Text (TMP)",
                    "Theme/Map/Title",
                    "Map/Title",
                    "Title");
                if (titleT != null)
                {
                    themeNameText = titleT.GetComponent<TMP_Text>() ?? titleT.GetComponentInChildren<TMP_Text>(true);
                }
                if (themeNameText == null && themeBtn != null)
                {
                    themeNameText = themeBtn.GetComponentInChildren<TMP_Text>(true);
                }
                if (themeNameText == null)
                {
                    titleT = FindTransformDeep(transform, "Theme/Map/Title/Text (TMP)", "Map/Title/Text (TMP)");
                    if (titleT != null)
                    {
                        themeNameText = titleT.GetComponent<TMP_Text>() ?? titleT.GetComponentInChildren<TMP_Text>(true);
                    }
                }
            }

            // 3. Tìm CreateQuestionPanel
            if (createQuestionArea == null)
            {
                Transform t = FindChildTransform(transform, "CreateQuestionPanel", "CreateQuestion", "QuestionEditor", "CreateArea");
                if (t != null) createQuestionArea = t.gameObject;
            }

            // 4. Tìm ListQuestionsPanel
            if (listQuestionsArea == null)
            {
                Transform t = FindChildTransform(transform, "ListQuestionsPanel", "ListQuestions", "QuestionList");
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

                if (timeInput == null)
                {
                    Transform t = FindChildTransform(createQ, "TimeInput", "TimerInput", "Time", "Duration", "QuestionTime", "TimeInputField");
                    if (t != null) timeInput = t.GetComponent<TMP_InputField>();
                }

                if (answerA == null) answerA = FindInputFieldFlexible(createQ, "AnswerA", "Answer1", "OptionA", "Option1", "D1");
                if (answerB == null) answerB = FindInputFieldFlexible(createQ, "AnswerB", "Answer2", "OptionB", "Option2", "D1 (1)", "D2");
                if (answerC == null) answerC = FindInputFieldFlexible(createQ, "AnswerC", "Answer3", "OptionC", "Option3", "D1 (2)", "D3");
                if (answerD == null) answerD = FindInputFieldFlexible(createQ, "AnswerD", "Answer4", "OptionD", "Option4", "D1 (3)", "D4");



                if (createBtn == null) createBtn = FindButtonFlexible(createQ, "CreateBtn", "CreateQuestionButton", "CreateButton", "SaveBtn");
                if (resetBtn == null) resetBtn = FindButtonFlexible(createQ, "ResetBtn", "ResetInputsButton", "ResetButton", "ClearBtn");

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
                if (copyGuideBtn == null) copyGuideBtn = FindButtonFlexible(gRoot, "CopyGuideBtn", "CopyBtn", "CopyPromptBtn", "Copy");
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
            }

            // Dự phòng tìm kiếm trên toàn bảng nếu chưa thấy
            if (editQuestBtn == null) editQuestBtn = FindButtonFlexible(transform, "EditQuestBtn", "EditQuest", "EditBtn", "EditButton");
            if (deleteBtn == null) deleteBtn = FindButtonFlexible(transform, "DeleteQuest", "DeleteQuestBtn", "DeleteQuestionButton", "DeleteBtn");

            // Tìm nút Back chung
            if (backBtn == null) backBtn = FindButtonFlexible(transform, "BackBtn", "BackButton", "CloseBtn", "CloseButton");
        }

        private Transform FindChildRecursive(Transform parent, string targetName)
        {
            if (parent == null || string.IsNullOrEmpty(targetName)) return null;
            if (parent.name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) return child;
                Transform found = FindChildRecursive(child, targetName);
                if (found != null) return found;
            }
            return null;
        }

        private Transform FindTransformDeep(Transform parent, params string[] namesOrPaths)
        {
            if (parent == null || namesOrPaths == null) return null;
            foreach (string np in namesOrPaths)
            {
                if (string.IsNullOrEmpty(np)) continue;

                // 1. Tìm trực tiếp theo path (Find hỗ trợ A/B/C)
                Transform direct = parent.Find(np);
                if (direct != null) return direct;

                // 2. Nếu không có dấu gạch chéo '/', tìm đệ quy theo tên GameObject
                if (!np.Contains("/"))
                {
                    Transform rec = FindChildRecursive(parent, np);
                    if (rec != null) return rec;
                }
                else
                {
                    // 3. Nếu là đường dẫn tương đối nhiều cấp lồng nhau nhưng bắt đầu từ một nút con nào đó
                    string[] parts = np.Split('/');
                    Transform firstPart = FindChildRecursive(parent, parts[0]);
                    if (firstPart != null)
                    {
                        string remainingPath = string.Join("/", parts, 1, parts.Length - 1);
                        Transform sub = firstPart.Find(remainingPath);
                        if (sub != null) return sub;
                    }
                }
            }
            return null;
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



        private void BindEvents()
        {
            // Gán sự kiện cho 3 nút Tab dọc trên MenuTabPanel
            if (tabMainBtn != null)
            {
                tabMainBtn.onClick.RemoveAllListeners();
                tabMainBtn.onClick.AddListener(ShowMainPanelMode);
            }

            if (tabCreateBtn != null)
            {
                tabCreateBtn.onClick.RemoveAllListeners();
                tabCreateBtn.onClick.AddListener(() =>
                {
                    ClearInputs();
                    ShowCreateQuestionMode();
                });
            }

            if (tabListBtn != null)
            {
                tabListBtn.onClick.RemoveAllListeners();
                tabListBtn.onClick.AddListener(ShowListQuestionsMode);
            }

            if (themeBtn != null)
            {
                themeBtn.onClick.RemoveAllListeners();
                themeBtn.onClick.AddListener(OnThemeButtonClicked);
            }

            // Gán sự kiện click nếu Map hoặc mapImg có Button
            Button mapBtn = mapImg != null ? mapImg.GetComponent<Button>() : null;
            if (mapBtn == null && mapImg != null && mapImg.transform.parent != null)
            {
                mapBtn = mapImg.transform.parent.GetComponentInParent<Button>();
            }
            if (mapBtn != null && mapBtn != themeBtn)
            {
                mapBtn.onClick.RemoveAllListeners();
                mapBtn.onClick.AddListener(OnThemeButtonClicked);
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
            if (copyGuideBtn != null)
            {
                copyGuideBtn.onClick.RemoveAllListeners();
                copyGuideBtn.onClick.AddListener(OnCopyGuideClicked);
            }
            if (importBtn != null)
            {
                importBtn.onClick.RemoveAllListeners();
                importBtn.onClick.AddListener(OnImportClicked);
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

        /// <summary>
        /// Bấm nút Copy: copy toàn bộ nội dung hướng dẫn mẫu DEFAULT_IMPORT_GUIDE vào Clipboard (đã lọc sạch thẻ HTML)
        /// </summary>
        public void OnCopyGuideClicked()
        {
            // Lọc bỏ các thẻ HTML RichText (<b>, <i>...) để khi dán vào AI/Notepad được văn bản sạch đẹp
            string cleanText = System.Text.RegularExpressions.Regex.Replace(DEFAULT_IMPORT_GUIDE, "<.*?>", string.Empty);
            GUIUtility.systemCopyBuffer = cleanText;
            Debug.Log("[EditMapPanelUI] Đã copy nội dung DEFAULT_IMPORT_GUIDE vào Clipboard!");

            StartCoroutine(CopyFeedbackRoutine());
        }

        private System.Collections.IEnumerator CopyFeedbackRoutine()
        {
            if (copyGuideBtn == null) yield break;
            TMP_Text btnText = copyGuideBtn.GetComponentInChildren<TMP_Text>(true);
            if (btnText != null)
            {
                string originalText = btnText.text;
                btnText.text = "Copied!";
                yield return new WaitForSeconds(1.5f);
                btnText.text = originalText;
            }
        }



        /// <summary>
        /// Lấy index của đáp án đúng hiện tại.
        /// Quy ước: Đối với nhập liệu từ giao diện, ô A (index 0) luôn là đáp án đúng.
        /// (Khi vào chơi game, hệ thống sẽ tự động xáo trộn ngẫu nhiên vị trí các đáp án)
        /// </summary>
        public int GetCorrectAnswerIndex()
        {
            return 0; // Câu A luôn là đáp án đúng khi tạo từ form
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

        private const float TAB_UNSELECTED_POS_X = 5f;
        private const float TAB_SELECTED_POS_X = -23f;

        private void SetTabButtonPosX(Button btn, float targetX)
        {
            if (btn == null) return;
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 pos = rt.anchoredPosition;
                pos.x = targetX;
                rt.anchoredPosition = pos;
            }
        }

        public void UpdateTabVisuals(int activeTabIndex) // 0: Main, 1: Create, 2: List
        {
            SetTabButtonPosX(tabMainBtn, activeTabIndex == 0 ? TAB_SELECTED_POS_X : TAB_UNSELECTED_POS_X);
            SetTabButtonPosX(tabCreateBtn, activeTabIndex == 1 ? TAB_SELECTED_POS_X : TAB_UNSELECTED_POS_X);
            SetTabButtonPosX(tabListBtn, activeTabIndex == 2 ? TAB_SELECTED_POS_X : TAB_UNSELECTED_POS_X);
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
            UpdateTabVisuals(0);

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

                UpdateThemeBtnUI();
            }
        }

        private void UpdateThemeBtnUI()
        {
            EnsureThemesLoaded();

            ThemeSO matchedTheme = null;
            if (availableThemes != null && availableThemes.Count > 0)
            {
                if (currentMap != null && !string.IsNullOrEmpty(currentMap.themeId))
                {
                    matchedTheme = availableThemes.Find(t => t != null && t.themeId == currentMap.themeId);
                }

                // Nếu themeId chưa có hoặc không khớp với ThemeSO nào, chọn ThemeSO đầu tiên
                if (matchedTheme == null)
                {
                    matchedTheme = availableThemes[0];
                    if (currentMap != null && (string.IsNullOrEmpty(currentMap.themeId) || currentMap.themeId == "default" || currentMap.themeId == "theme_forest"))
                    {
                        currentMap.themeId = matchedTheme.themeId;
                    }
                }
            }

            // 1. Text trong title là tên theme đó
            if (themeNameText != null)
            {
                if (matchedTheme != null && !string.IsNullOrEmpty(matchedTheme.themeName))
                {
                    themeNameText.text = matchedTheme.themeName;
                }
                else if (currentMap != null)
                {
                    themeNameText.text = GetThemeDisplayName(currentMap.themeId);
                }
            }

            // 2. mapimg thay đổi image trong map là ảnh ScriptableObject (mapBg)
            if (mapImg != null)
            {
                if (matchedTheme != null && matchedTheme.mapBg != null)
                {
                    mapImg.sprite = matchedTheme.mapBg;
                    mapImg.color = Color.white;
                }
            }
        }

        private void EnsureThemesLoaded()
        {
            if (availableThemes == null)
            {
                availableThemes = new List<ThemeSO>();
            }

            if (availableThemes.Count == 0)
            {
                ThemeSO[] loaded = Resources.FindObjectsOfTypeAll<ThemeSO>();
                if (loaded != null && loaded.Length > 0)
                {
                    foreach (var t in loaded)
                    {
                        if (t != null && !availableThemes.Contains(t))
                        {
                            availableThemes.Add(t);
                        }
                    }
                }

#if UNITY_EDITOR
                if (availableThemes.Count == 0)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ThemeSO");
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        ThemeSO tSO = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeSO>(path);
                        if (tSO != null && !availableThemes.Contains(tSO))
                        {
                            availableThemes.Add(tSO);
                        }
                    }
                }
#endif
            }
        }

        private string GetThemeDisplayName(string themeId)
        {
            EnsureThemesLoaded();
            if (availableThemes != null)
            {
                ThemeSO found = availableThemes.Find(t => t != null && t.themeId == themeId);
                if (found != null) return found.themeName;
            }

            switch (themeId)
            {
                case "theme_desert": return "Sa Mạc";
                case "theme_ice": return "Băng Tuyết";
                case "theme_volcano": return "Núi Lửa";
                case "theme_mountain": return "Núi Tuyết";
                case "theme_forest":
                default: return "Rừng Xanh";
            }
        }

        private void OnThemeButtonClicked()
        {
            EnsureThemesLoaded();

            if (availableThemes != null && availableThemes.Count > 0)
            {
                int curIdx = -1;
                if (currentMap != null && !string.IsNullOrEmpty(currentMap.themeId))
                {
                    curIdx = availableThemes.FindIndex(t => t != null && t.themeId == currentMap.themeId);
                }
                int nextIdx = (curIdx + 1) % availableThemes.Count;
                ThemeSO nextTheme = availableThemes[nextIdx];

                if (nextTheme != null)
                {
                    if (currentMap != null)
                    {
                        currentMap.themeId = nextTheme.themeId;
                        currentMap.itemIds = new List<string>();
                        if (nextTheme.themeItems != null)
                        {
                            foreach (var itm in nextTheme.themeItems)
                            {
                                if (itm != null)
                                {
                                    currentMap.itemIds.Add(string.IsNullOrEmpty(itm.itemId) ? itm.name : itm.itemId);
                                }
                            }
                        }
                    }
                }
            }
            else if (currentMap != null)
            {
                // Fallback nếu chưa tạo file ThemeSO nào
                if (currentMap.themeId == "theme_forest")
                {
                    currentMap.themeId = "theme_desert";
                    currentMap.itemIds = new List<string>() { "item_cactus", "item_fossil", "item_water" };
                }
                else if (currentMap.themeId == "theme_desert")
                {
                    currentMap.themeId = "theme_ice";
                    currentMap.itemIds = new List<string>() { "item_snowflake", "item_crystal", "item_gem" };
                }
                else if (currentMap.themeId == "theme_ice")
                {
                    currentMap.themeId = "theme_volcano";
                    currentMap.itemIds = new List<string>() { "item_fire", "item_ruby", "item_ember" };
                }
                else
                {
                    currentMap.themeId = "theme_forest";
                    currentMap.itemIds = new List<string>() { "item_leaf", "item_gem", "item_potion" };
                }
            }

            UpdateThemeBtnUI();
            onDataChangedCallback?.Invoke();
            if (currentMap != null)
            {
                Debug.Log($"[EditMapPanelUI] Đã chọn Theme: {GetThemeDisplayName(currentMap.themeId)} ({currentMap.themeId})");
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
            UpdateTabVisuals(1);
        }

        /// <summary>
        /// Chế độ 2: Bật bảng danh sách (ListQuestions), ẩn MainPanel và CreateQuestion
        /// </summary>
        public void ShowListQuestionsMode()
        {
            if (mainPanelArea != null) mainPanelArea.SetActive(false);
            if (createQuestionArea != null) createQuestionArea.SetActive(false);
            if (listQuestionsArea != null) listQuestionsArea.SetActive(true);
            UpdateTabVisuals(2);
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

            string optA = editingQuestion.option1;
            string optB = editingQuestion.option2;
            string optC = editingQuestion.option3;
            string optD = editingQuestion.option4;

            // Nếu câu hỏi ban đầu có đáp án đúng không phải A (ví dụ import từ file có * ở B/C/D),
            // hoán đổi để đáp án đúng luôn được đưa về ô A, đảm bảo đúng quy ước "ô A luôn là đáp án đúng"
            if (editingQuestion.correctIndex == 1)
            {
                string temp = optA;
                optA = optB;
                optB = temp;
            }
            else if (editingQuestion.correctIndex == 2)
            {
                string temp = optA;
                optA = optC;
                optC = temp;
            }
            else if (editingQuestion.correctIndex == 3)
            {
                string temp = optA;
                optA = optD;
                optD = temp;
            }

            if (answerA != null) answerA.text = optA;
            if (answerB != null) answerB.text = optB;
            if (answerC != null) answerC.text = optC;
            if (answerD != null) answerD.text = optD;
            if (timeInput != null) timeInput.text = (editingQuestion.timeLimit > 0f ? editingQuestion.timeLimit : 30f).ToString("0");

            // Mở bảng CreateQuestion để chỉnh sửa
            ShowCreateQuestionMode();
            Debug.Log($"[EditMapPanelUI] Bấm nút Sửa: mở form chỉnh sửa câu hỏi '{editingQuestion.questionText}' (Đáp án đúng đặt tại ô A)!");
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

            int correctIdx = 0; // Quy ước: Câu A luôn là đáp án đúng khi tạo từ form

            if (string.IsNullOrEmpty(qText))
            {
                qText = "Câu hỏi mới";
            }

            float qTime = 30f;
            if (timeInput != null && !string.IsNullOrWhiteSpace(timeInput.text))
            {
                if (float.TryParse(timeInput.text.Trim(), out float parsed) && parsed > 0f)
                {
                    qTime = parsed;
                }
            }

            bool wasEditing = (editingQuestion != null);

            if (wasEditing)
            {
                editingQuestion.questionText = qText;
                editingQuestion.option1 = opt1;
                editingQuestion.option2 = opt2;
                editingQuestion.option3 = opt3;
                editingQuestion.option4 = opt4;
                editingQuestion.correctIndex = 0; // Luôn là câu A đúng
                editingQuestion.timeLimit = qTime;
                Debug.Log($"[EditMapPanelUI] Đã cập nhật và lưu câu hỏi! (Thời gian: {qTime}s, Đáp án đúng: A)");
            }
            else
            {
                QuestionData newQ = new QuestionData(qText, opt1, opt2, opt3, opt4, 0, qTime);
                currentMap.questions.Add(newQ);
                Debug.Log($"[EditMapPanelUI] Đã tạo câu hỏi mới (Thời gian: {qTime}s, Đáp án đúng: A): '{qText}'");
            }

            SaveData();
            ClearInputs();

            // Nếu là vừa sửa câu hỏi xong -> quay về Danh sách (Tab 3) để xem kết quả
            // Nếu là tạo câu mới -> ở lại Tab 2 (Tạo câu hỏi) với các ô nhập đã được tự động xóa trắng để tiếp tục viết câu tiếp theo
            if (wasEditing)
            {
                ShowListQuestionsMode();
            }
            else
            {
                ShowCreateQuestionMode();
            }
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
            if (timeInput != null) timeInput.text = "30";

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

                        float timeLimit = 30f;
                        if (parts.Length >= 7 && float.TryParse(parts[6].Trim().TrimEnd('s', 'S'), out float parsedT))
                        {
                            timeLimit = parsedT;
                        }
                        else if (parts.Length >= 6 && float.TryParse(parts[5].Trim().TrimEnd('s', 'S'), out float parsedT2))
                        {
                            timeLimit = parsedT2;
                        }

                        if (!string.IsNullOrEmpty(q))
                        {
                            list.Add(new QuestionData(q, a, b, c, d, correct, timeLimit));
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
                float currentTimeLimit = 30f;

                Action commitQuestion = () =>
                {
                    if (!string.IsNullOrEmpty(currentQuestion) && (!string.IsNullOrEmpty(optA) || !string.IsNullOrEmpty(optB)))
                    {
                        // Kiểm tra nếu nội dung câu hỏi có kèm [15s] hoặc (15s)
                        var match = System.Text.RegularExpressions.Regex.Match(currentQuestion, @"\[(\d+)s?\]|\((\d+)s\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            string val = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
                            if (float.TryParse(val, out float parsed)) currentTimeLimit = parsed;
                            currentQuestion = currentQuestion.Replace(match.Value, "").Trim();
                        }

                        list.Add(new QuestionData(currentQuestion.Trim(), optA.Trim(), optB.Trim(), optC.Trim(), optD.Trim(), correctIdx, currentTimeLimit));
                    }
                    currentQuestion = "";
                    optA = "";
                    optB = "";
                    optC = "";
                    optD = "";
                    correctIdx = 0; // Reset về mặc định
                    currentTimeLimit = 30f;
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

                    // Kiểm tra dòng thời gian làm câu hỏi (Time: 15s, Thời gian: 15, Timer: 10...)
                    if (line.StartsWith("Time:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Thời gian:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Timer:", StringComparison.OrdinalIgnoreCase))
                    {
                        string tStr = line.Substring(line.IndexOf(':') + 1).Trim().TrimEnd('s', 'S');
                        if (float.TryParse(tStr, out float parsed) && parsed > 0f) currentTimeLimit = parsed;
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

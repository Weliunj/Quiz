using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Quản lý giao diện trạm dừng (StationPanel) và câu hỏi (QuestPanel).
    /// </summary>
    public class StationQuizController : MonoBehaviour
    {
        [Header("--- 1. CÁC PANEL CHÍNH ---")]
        [Tooltip("Panel trạm (StationPanel) chứa danh sách item")]
        [SerializeField] private GameObject stationPanel;

        [Tooltip("Panel hiển thị câu hỏi (QuestPanel)")]
        [SerializeField] private GameObject questPanel;

        [Tooltip("Vùng chứa các Item trong StationPanel")]
        [SerializeField] private Transform itemContainer;

        [Header("--- 2. NÚT ĐIỀU HƯỚNG TRẠM ---")]
        [Tooltip("Nút Join để vào trạm (hiển thị khi vừa tới trạm)")]
        [SerializeField] private Button joinBtn;

        [Tooltip("Nút rời khỏi trạm (ExitBtn)")]
        [SerializeField] private Button exitBtn;

        [Tooltip("Nút bỏ qua thời gian làm câu hỏi (SkipTimeBtn)")]
        [SerializeField] private Button skipTimeBtn;

        [Tooltip("Text hiển thị số câu hỏi / item còn lại (RemainText)")]
        [SerializeField] private TMP_Text remainText;

        [Header("--- 3. THÀNH PHẦN CÂU HỎI (QUEST PANEL) ---")]
        [Tooltip("Text nội dung câu hỏi")]
        [SerializeField] private TMP_Text questionText;

        [Tooltip("Text đếm ngược thời gian")]
        [SerializeField] private TMP_Text timeText;

        [Tooltip("Danh sách 4 nút đáp án")]
        [SerializeField] private Button[] answerButtons = new Button[4];

        [Header("--- 4. THÔNG SỐ ĐẾM GIỜ & THỜI GIAN ---")]
        [Tooltip("Thời gian làm bài cho mỗi câu hỏi (giây)")]
        [SerializeField] private float questionTimeLimit = 20f;

        [Header("--- 5. UI ITEM INVENTORY (MAINPANEL) ---")]
        [Tooltip("Vùng chứa Item trong MainPanel (GamePanel/MainPanel/ItemContainer)")]
        [SerializeField] private Transform mainItemContainer;

        [Tooltip("Prefab của ItemContent (chứa ItemImg và QuanText)")]
        [SerializeField] private GameObject itemContentPrefab;

        [Tooltip("Danh sách cấu hình các ThemeSO có trong game (kéo các ThemeSO vào đây hoặc tự động nạp từ Resources)")]
        [SerializeField] private List<ThemeSO> availableThemes = new List<ThemeSO>();

        // Lưu trữ số lượng item nhận được trong map hiện tại: itemId -> quantity
        private Dictionary<string, int> inventoryItems = new Dictionary<string, int>();
        // Tham chiếu các UI element tương ứng với từng itemId
        private Dictionary<string, TMP_Text> itemQuantityTexts = new Dictionary<string, TMP_Text>();
        // Tham chiếu icon của từng item
        private Dictionary<string, ItemSO> itemDataLookup = new Dictionary<string, ItemSO>();

        [Header("--- 6. QUẢN LÝ ITEM TRẠM DỪNG (STATION ITEMS) ---")]
        [Tooltip("Số lượng item ngẫu nhiên sinh ra mỗi trạm (mặc định 5)")]
        [SerializeField] private int itemsPerStation = 5;

        [Tooltip("Số lượt trả lời tối đa tại mỗi trạm (mặc định 3 lượt)")]
        [SerializeField] private int maxTriesPerStation = 3;

        [Tooltip("Prefab hoặc mẫu nút Item trong StationPanel/ItemContainer (nếu để trống sẽ tự lấy con đầu tiên làm mẫu)")]
        [SerializeField] private GameObject stationItemPrefab;

        private int currentStationIndex = 0;

        // Lưu số lượt chơi còn lại của từng trạm: stationIndex -> remaining tries (ví dụ: 3, 2, 1, 0)
        private Dictionary<int, int> stationRemainingTries = new Dictionary<int, int>();

        // Lưu trạng thái item đã sinh của từng trạm: stationIndex -> danh sách dữ liệu các item của trạm đó
        public class StationItemRuntimeData
        {
            public string itemId;
            public string displayName;
            public Sprite icon;
            public bool isAnsweredCorrect = false; // Đã đúng -> ẩn
            public bool isAnsweredWrong = false;   // Đã sai -> làm mờ
            public Button runtimeBtn;
        }

        private Dictionary<int, List<StationItemRuntimeData>> stationItemsState = new Dictionary<int, List<StationItemRuntimeData>>();
        private GameObject cachedStationItemTemplate;

        // Lưu các GameObject của item trên MainPanel để ẩn/hiện theo số lượng
        private Dictionary<string, GameObject> mainItemObjects = new Dictionary<string, GameObject>();

        private MapData currentMap;
        private List<QuestionData> poolQuestions = new List<QuestionData>();
        private List<QuestionData> unusedQuestions = new List<QuestionData>();

        private List<Button> spawnedItems = new List<Button>();
        private Button currentActiveItemBtn;
        private StationItemRuntimeData currentActiveItemData;

        private Coroutine timerCoroutine;
        private float currentTimeLeft = 0f;
        private bool isAnswering = false;

        // Lưu danh sách đáp án random của câu hỏi hiện tại
        private int currentCorrectShuffledIndex = 0;

        private void Awake()
        {
            AutoFindReferences();
            BindEvents();
        }

        public void AutoFindReferences()
        {
            Transform root = transform;

            if (stationPanel == null)
            {
                Transform t = FindChildRecursive(root, "StationPanel");
                if (t != null) stationPanel = t.gameObject;
            }

            if (stationPanel != null)
            {
                if (itemContainer == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "ItemContainer");
                    if (t != null) itemContainer = t;
                }

                if (questPanel == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "QuestPanel");
                    if (t != null) questPanel = t.gameObject;
                }

                if (exitBtn == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "ExitBtn", "ExitButton", "CloseBtn");
                    if (t != null) exitBtn = t.GetComponent<Button>();
                }

                if (remainText == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "RemainText", "Remain", "StatusText");
                    if (t != null) remainText = t.GetComponent<TMP_Text>();
                }
            }

            if (questPanel != null)
            {
                if (questionText == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "QuestionText", "Question");
                    if (t != null) questionText = t.GetComponent<TMP_Text>();
                }

                if (timeText == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "TimeText", "TimerText", "Time");
                    if (t != null) timeText = t.GetComponent<TMP_Text>();
                }

                if (skipTimeBtn == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "SkipTimeBtn", "SkipBtn", "Skip");
                    if (t != null) skipTimeBtn = t.GetComponent<Button>();
                }

                // Tìm 4 nút D1Btn, D1Btn (1), D1Btn (2), D1Btn (3)
                if (answerButtons == null || answerButtons.Length != 4 || answerButtons[0] == null)
                {
                    answerButtons = new Button[4];
                    string[] btnNames = new string[] { "D1Btn", "D1Btn (1)", "D1Btn (2)", "D1Btn (3)" };
                    for (int i = 0; i < 4; i++)
                    {
                        Transform t = FindChildRecursive(questPanel.transform, btnNames[i], $"AnswerBtn_{i + 1}", $"Option_{i + 1}");
                        if (t != null) answerButtons[i] = t.GetComponent<Button>();
                    }
                }
            }

            // Tìm nút JoinBtn (có thể nằm ở GamePanel hoặc MainPanel)
            if (joinBtn == null)
            {
                Transform t = FindChildRecursive(root, "JoinBtn", "JoinButton", "Join");
                if (t != null) joinBtn = t.GetComponent<Button>();
            }

            // Tìm ItemContainer trong MainPanel (GamePanel/MainPanel/ItemContainer)
            if (mainItemContainer == null)
            {
                Transform mainPanelTransform = FindChildRecursive(root, "MainPanel");
                if (mainPanelTransform != null)
                {
                    Transform t = FindChildRecursive(mainPanelTransform, "ItemContainer");
                    if (t != null) mainItemContainer = t;
                }
            }

            // Load prefab ItemContent nếu chưa có
            if (itemContentPrefab == null)
            {
#if UNITY_EDITOR
                itemContentPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ItemContent.prefab");
#endif
            }

            // Load prefab ItemBtn (cho StationPanel/ItemContainer) nếu chưa gán
            if (stationItemPrefab == null)
            {
#if UNITY_EDITOR
                stationItemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ItemBtn.prefab");
#endif
            }
        }

        private Transform FindChildRecursive(Transform parent, params string[] names)
        {
            if (parent == null) return null;
            foreach (var n in names)
            {
                Transform t = parent.Find(n);
                if (t != null) return t;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), names);
                if (found != null) return found;
            }
            return null;
        }

        private void BindEvents()
        {
            if (joinBtn != null)
            {
                joinBtn.onClick.RemoveAllListeners();
                joinBtn.onClick.AddListener(OnJoinButtonClicked);
            }

            if (exitBtn != null)
            {
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(OnExitButtonClicked);
            }

            if (skipTimeBtn != null)
            {
                skipTimeBtn.onClick.RemoveAllListeners();
                skipTimeBtn.onClick.AddListener(OnSkipTimeClicked);
            }

            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    int index = i;
                    if (answerButtons[i] != null)
                    {
                        answerButtons[i].onClick.RemoveAllListeners();
                        answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                    }
                }
            }
        }

        /// <summary>
        /// Khởi tạo khi vào một Map mới
        /// </summary>
        public void InitForMap(MapData map)
        {
            AutoFindReferences();
            currentMap = map;
            poolQuestions.Clear();
            unusedQuestions.Clear();

            // Reset trạng thái các trạm dừng khi load map mới
            stationItemsState.Clear();
            stationRemainingTries.Clear();
            currentStationIndex = 0;

            if (map != null && map.questions != null)
            {
                poolQuestions.AddRange(map.questions);
                unusedQuestions.AddRange(map.questions);
            }

            InitThemeInventory(map);

            HideAllStationPanels();
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);
        }

        /// <summary>
        /// Khởi tạo danh sách Item theo Theme lên MainPanel UI
        /// </summary>
        private void InitThemeInventory(MapData map)
        {
            inventoryItems.Clear();
            itemQuantityTexts.Clear();
            itemDataLookup.Clear();

            // Tự động load ThemeSO nếu chưa gán trong Inspector
            if (availableThemes == null || availableThemes.Count == 0)
            {
#if UNITY_EDITOR
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
#endif
            }

            // Tìm ThemeSO khớp với themeId của map
            ThemeSO matchedTheme = null;
            if (availableThemes != null && map != null)
            {
                matchedTheme = availableThemes.Find(t => t != null && t.themeId == map.themeId);
            }

            if (matchedTheme != null && matchedTheme.themeItems != null && matchedTheme.themeItems.Count > 0)
            {
                foreach (var itemSO in matchedTheme.themeItems)
                {
                    if (itemSO != null)
                    {
                        string id = string.IsNullOrEmpty(itemSO.itemId) ? itemSO.name : itemSO.itemId;
                        inventoryItems[id] = 0;
                        itemDataLookup[id] = itemSO;
                    }
                }
            }
            else
            {
                // Fallback nếu chưa tạo ThemeSO
                List<string> items = (map != null && map.itemIds != null && map.itemIds.Count > 0)
                    ? map.itemIds
                    : new List<string>() { "item_1", "item_2", "item_3" };

                foreach (var id in items)
                {
                    inventoryItems[id] = 0;
                }
            }

            SetupMainPanelItemUI();
        }

        /// <summary>
        /// Đồng bộ hiển thị kho đồ trên ItemContainer (MainPanel).
        /// QUY TẮC: Item chỉ HIỂN THỊ khi số lượng >= 1, nếu = 0 sẽ ẩn đi.
        /// </summary>
        private void SetupMainPanelItemUI()
        {
            if (mainItemContainer == null) return;

            // Xóa sạch các child cũ nếu đã sinh trước đó
            for (int i = mainItemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(mainItemContainer.GetChild(i).gameObject);
            }

            itemQuantityTexts.Clear();
            mainItemObjects.Clear();

            foreach (var kvp in inventoryItems)
            {
                string itemId = kvp.Key;
                int count = kvp.Value;

                GameObject itemObj = null;
                if (itemContentPrefab != null)
                {
                    itemObj = Instantiate(itemContentPrefab, mainItemContainer);
                }
                else
                {
                    // Fallback tạo nhanh GameObject nếu không có prefab
                    itemObj = new GameObject($"Item_{itemId}", typeof(RectTransform));
                    itemObj.transform.SetParent(mainItemContainer, false);
                }

                itemObj.name = $"ItemContent_{itemId}";
                mainItemObjects[itemId] = itemObj;

                // Chỉ hiển thị nếu số lượng >= 1
                itemObj.SetActive(count >= 1);

                // Cập nhật Sprite Icon nếu có ItemSO
                if (itemDataLookup.TryGetValue(itemId, out ItemSO itemSO) && itemSO != null && itemSO.icon != null)
                {
                    Image img = itemObj.GetComponentInChildren<Image>(true);
                    if (img != null)
                    {
                        img.sprite = itemSO.icon;
                        img.color = Color.white;
                    }
                }

                // Tìm QuanText trong ItemContent
                TMP_Text quanText = itemObj.GetComponentInChildren<TMP_Text>(true);
                if (quanText != null)
                {
                    quanText.text = $"x{count}";
                    itemQuantityTexts[itemId] = quanText;
                }
            }
        }

        /// <summary>
        /// Thêm số lượng cho item ngẫu nhiên (hoặc item chỉ định) thuộc Theme khi trả lời đúng
        /// </summary>
        public void RewardThemeItem(int amount = 1)
        {
            if (inventoryItems == null || inventoryItems.Count == 0) return;

            // Chọn 1 item id từ theme của map hiện tại
            List<string> keys = new List<string>(inventoryItems.Keys);
            string chosenId = keys[UnityEngine.Random.Range(0, keys.Count)];

            inventoryItems[chosenId] += amount;
            int newCount = inventoryItems[chosenId];

            // Nếu số lượng >= 1 thì hiển thị đối tượng này trên MainPanel
            if (mainItemObjects.TryGetValue(chosenId, out GameObject obj) && obj != null)
            {
                obj.SetActive(newCount >= 1);
            }

            if (itemQuantityTexts.TryGetValue(chosenId, out TMP_Text textComp) && textComp != null)
            {
                textComp.text = $"x{newCount}";
            }

            Debug.Log($"[StationQuizController] Nhận được +{amount} {chosenId}! Tổng hiện có: {newCount}");
        }

        /// <summary>
        /// Khi người chơi di chuyển đến trạm bất kỳ -> Hiển thị nút Join
        /// </summary>
        public void ShowJoinPrompt(int stationIndex)
        {
            AutoFindReferences();
            currentStationIndex = stationIndex;

            if (joinBtn != null)
            {
                joinBtn.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Ẩn toàn bộ khi rời khỏi map
        /// </summary>
        public void HideAllStationPanels()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            if (stationPanel != null) stationPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);
            isAnswering = false;
        }

        /// <summary>
        /// Bấm nút Join để mở StationPanel
        /// </summary>
        public void OnJoinButtonClicked()
        {
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);

            if (stationPanel != null)
            {
                stationPanel.SetActive(true);
            }

            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }

            SetupStationItems();
            UpdateRemainText();
        }

        /// <summary>
        /// Bấm nút ExitBtn để rời khỏi StationPanel
        /// </summary>
        public void OnExitButtonClicked()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            if (questPanel != null) questPanel.SetActive(false);
            if (stationPanel != null) stationPanel.SetActive(false);

            // Cho phép hiển thị lại nút Join nếu người chơi vẫn ở trạm đó
            if (joinBtn != null) joinBtn.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sinh ngẫu nhiên 5 item thuộc Theme cho trạm nếu là lần đầu bước vào,
        /// hoặc giữ nguyên đúng trạng thái của các item nếu out trạm rồi vào lại.
        /// </summary>
        private void SetupStationItems()
        {
            spawnedItems.Clear();
            if (itemContainer == null) return;

            // Lưu trữ mẫu nút Item đầu tiên làm template nếu chưa có prefab
            if (cachedStationItemTemplate == null)
            {
                if (stationItemPrefab != null)
                {
                    cachedStationItemTemplate = stationItemPrefab;
                }
                else if (itemContainer.childCount > 0)
                {
                    cachedStationItemTemplate = itemContainer.GetChild(0).gameObject;
                }
            }

            // Kiểm tra xem trạm hiện tại đã sinh danh sách item lần đầu chưa
            if (!stationItemsState.ContainsKey(currentStationIndex))
            {
                // Lần đầu vào trạm -> sinh ngẫu nhiên 5 item từ Theme hiện tại
                List<StationItemRuntimeData> newStationItems = GenerateRandomItemsForStation(itemsPerStation);
                stationItemsState[currentStationIndex] = newStationItems;
            }

            // Kiểm tra số lượt còn lại của trạm này (mặc định maxTriesPerStation = 3)
            if (!stationRemainingTries.ContainsKey(currentStationIndex))
            {
                stationRemainingTries[currentStationIndex] = maxTriesPerStation;
            }

            int currentTries = stationRemainingTries[currentStationIndex];
            bool isStationOutOfTries = (currentTries <= 0);

            List<StationItemRuntimeData> currentStationList = stationItemsState[currentStationIndex];

            // Dọn dẹp các nút hiện tại trong itemContainer để vẽ lại theo trạng thái lưu trữ
            // Nếu dùng template là con thì ẩn tạm thay vì xóa ngay lập tức
            for (int i = itemContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = itemContainer.GetChild(i).gameObject;
                if (child == cachedStationItemTemplate)
                {
                    child.SetActive(false); // Ẩn template gốc
                }
                else
                {
                    Destroy(child);
                }
            }

            // Tạo các nút ItemBtn hiển thị trên giao diện theo trạng thái đã lưu
            foreach (var itemData in currentStationList)
            {
                GameObject btnObj = null;
                if (cachedStationItemTemplate != null)
                {
                    btnObj = Instantiate(cachedStationItemTemplate, itemContainer);
                }
                else
                {
                    btnObj = new GameObject($"ItemBtn_{itemData.itemId}", typeof(RectTransform), typeof(Button), typeof(Image));
                    btnObj.transform.SetParent(itemContainer, false);
                }

                btnObj.name = $"ItemBtn_{itemData.itemId}";
                Button btn = btnObj.GetComponent<Button>();
                if (btn == null) btn = btnObj.AddComponent<Button>();

                itemData.runtimeBtn = btn;

                // Cập nhật Sprite Icon cho nút nếu ItemSO có icon
                if (itemData.icon != null)
                {
                    Image btnImg = btnObj.GetComponent<Image>();
                    if (btnImg != null)
                    {
                        btnImg.sprite = itemData.icon;
                        btnImg.color = Color.white;
                    }
                }

                // Cập nhật text hiển thị (nếu có Text TMP con)
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>(true);
                if (btnText != null)
                {
                    btnText.text = string.IsNullOrEmpty(itemData.displayName) ? "Quest" : itemData.displayName;
                }

                // Áp dụng trạng thái đã lưu:
                // 1. Đã trả lời ĐÚNG -> biến mất
                // 2. Đã trả lời SAI hoặc HẾT LƯỢT TRẠM (0/3) -> làm mờ, khóa tương tác
                // 3. Chưa trả lời và trạm còn lượt -> bình thường
                if (itemData.isAnsweredCorrect)
                {
                    btnObj.SetActive(false);
                }
                else if (itemData.isAnsweredWrong || isStationOutOfTries)
                {
                    btnObj.SetActive(true);
                    SetButtonOpacity(btn, 0.45f);
                    btn.interactable = false;
                    spawnedItems.Add(btn);
                }
                else
                {
                    btnObj.SetActive(true);
                    SetButtonOpacity(btn, 1f);
                    btn.interactable = true;
                    spawnedItems.Add(btn);

                    StationItemRuntimeData capturedData = itemData;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnItemClicked(btn, capturedData));
                }
            }
        }

        /// <summary>
        /// Tạo danh sách ngẫu nhiên số lượng item từ Theme hiện tại của map (có thể trùng item)
        /// </summary>
        private List<StationItemRuntimeData> GenerateRandomItemsForStation(int count)
        {
            List<StationItemRuntimeData> list = new List<StationItemRuntimeData>();

            // Lấy danh sách item từ Theme hiện tại
            List<ItemSO> candidateItems = new List<ItemSO>();

            ThemeSO matchedTheme = null;
            if (availableThemes != null && currentMap != null)
            {
                matchedTheme = availableThemes.Find(t => t != null && t.themeId == currentMap.themeId);
            }

            if (matchedTheme != null && matchedTheme.themeItems != null && matchedTheme.themeItems.Count > 0)
            {
                candidateItems.AddRange(matchedTheme.themeItems);
            }
            else
            {
                // Nếu chưa có ThemeSO, lấy từ dictionary lookup hoặc tạo placeholder
                foreach (var kvp in itemDataLookup)
                {
                    if (kvp.Value != null) candidateItems.Add(kvp.Value);
                }
            }

            for (int i = 0; i < count; i++)
            {
                StationItemRuntimeData itm = new StationItemRuntimeData();
                if (candidateItems.Count > 0)
                {
                    // Random ngẫu nhiên (cho phép trùng lặp)
                    ItemSO picked = candidateItems[UnityEngine.Random.Range(0, candidateItems.Count)];
                    itm.itemId = string.IsNullOrEmpty(picked.itemId) ? picked.name : picked.itemId;
                    itm.displayName = picked.itemName;
                    itm.icon = picked.icon;
                }
                else
                {
                    // Fallback nếu không có item nào được định nghĩa
                    string fbId = (currentMap != null && currentMap.itemIds != null && currentMap.itemIds.Count > 0)
                        ? currentMap.itemIds[UnityEngine.Random.Range(0, currentMap.itemIds.Count)]
                        : $"item_{i + 1}";
                    itm.itemId = fbId;
                    itm.displayName = fbId;
                    itm.icon = null;
                }

                itm.isAnsweredCorrect = false;
                itm.isAnsweredWrong = false;
                list.Add(itm);
            }

            return list;
        }

        /// <summary>
        /// Bấm vào 1 Item trong StationPanel để mở QuestPanel và hiển thị câu hỏi ngẫu nhiên
        /// </summary>
        public void OnItemClicked(Button itemBtn, StationItemRuntimeData itemData = null)
        {
            currentActiveItemBtn = itemBtn;
            currentActiveItemData = itemData;

            QuestionData q = PickNextQuestion();
            if (q == null)
            {
                Debug.LogWarning("[StationQuizController] Không có câu hỏi nào trong danh sách!");
                return;
            }

            if (questPanel != null)
            {
                questPanel.SetActive(true);
            }

            DisplayQuestion(q);
        }

        /// <summary>
        /// Lấy ngẫu nhiên câu hỏi từ danh sách, tránh trùng lặp cho đến khi hết danh sách thì xáo lại
        /// </summary>
        private QuestionData PickNextQuestion()
        {
            if (poolQuestions.Count == 0) return null;

            if (unusedQuestions.Count == 0)
            {
                // Đã trả lời hết danh sách -> Reset lại danh sách để tiếp tục
                unusedQuestions.AddRange(poolQuestions);
            }

            int randomIndex = UnityEngine.Random.Range(0, unusedQuestions.Count);
            QuestionData picked = unusedQuestions[randomIndex];
            unusedQuestions.RemoveAt(randomIndex);
            return picked;
        }

        /// <summary>
        /// Hiển thị câu hỏi và xáo ngẫu nhiên 4 đáp án
        /// </summary>
        private void DisplayQuestion(QuestionData q)
        {
            isAnswering = true;

            if (questionText != null)
            {
                questionText.text = q.questionText;
            }

            // Chuẩn bị 4 đáp án và vị trí đúng
            string[] rawOptions = new string[] { q.option1, q.option2, q.option3, q.option4 };
            int originalCorrect = Mathf.Clamp(q.correctIndex, 0, 3);

            // Xáo ngẫu nhiên thứ tự 4 đáp án
            List<int> shuffledIndices = new List<int> { 0, 1, 2, 3 };
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                int rnd = UnityEngine.Random.Range(i, shuffledIndices.Count);
                int tmp = shuffledIndices[i];
                shuffledIndices[i] = shuffledIndices[rnd];
                shuffledIndices[rnd] = tmp;
            }

            currentSelectedIndex = -1;

            for (int i = 0; i < 4; i++)
            {
                int optIdx = shuffledIndices[i];
                if (optIdx == originalCorrect)
                {
                    currentCorrectShuffledIndex = i;
                }

                if (answerButtons != null && i < answerButtons.Length && answerButtons[i] != null)
                {
                    answerButtons[i].interactable = true;

                    Image btnImg = answerButtons[i].GetComponent<Image>();
                    if (btnImg != null) btnImg.color = defaultButtonColor;

                    TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>(true);
                    if (btnText != null)
                    {
                        btnText.text = rawOptions[optIdx];
                    }
                }
            }

            // Khởi động đếm ngược thời gian
            currentTimeLeft = questionTimeLimit;
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(CountdownRoutine());
        }

        private Color defaultButtonColor = Color.white;
        private Color selectedHighlightColor = new Color(0.65f, 0.85f, 1f, 1f); // Màu xanh dương nhạt khi đang chọn
        private Color correctColor = new Color(0.2f, 0.85f, 0.3f, 1f);          // Màu xanh lá khi đúng
        private Color wrongColor = new Color(0.95f, 0.28f, 0.28f, 1f);          // Màu đỏ khi sai
        private int currentSelectedIndex = -1;

        private IEnumerator CountdownRoutine()
        {
            while (currentTimeLeft > 0f)
            {
                if (timeText != null)
                {
                    timeText.text = Mathf.CeilToInt(currentTimeLeft).ToString();
                }

                yield return new WaitForSeconds(1f);
                currentTimeLeft -= 1f;
            }

            // Hết giờ
            if (timeText != null) timeText.text = "0";
            StartCoroutine(OnQuestionEndRoutine());
        }

        /// <summary>
        /// Bấm nút Skip: giảm thời gian xuống còn đúng 3 giây
        /// </summary>
        public void OnSkipTimeClicked()
        {
            if (!isAnswering) return;

            if (currentTimeLeft > 3f)
            {
                currentTimeLeft = 3f;
                if (timeText != null) timeText.text = "3";
                Debug.Log("[StationQuizController] Đã bấm SkipTimeBtn: Thời gian giảm còn 3s!");
            }
        }

        /// <summary>
        /// Khi người chơi bấm chọn 1 đáp án:
        /// - Chưa biết đúng sai!
        /// - Có thể tự do đổi sang đáp án khác bất kỳ lúc nào khi còn thời gian.
        /// - Đổi màu highlight cho đáp án đang chọn.
        /// </summary>
        public void OnAnswerSelected(int selectedIndex)
        {
            if (!isAnswering) return;

            currentSelectedIndex = selectedIndex;

            // Highlight nút đang chọn và reset lại các nút khác
            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    if (answerButtons[i] != null)
                    {
                        Image img = answerButtons[i].GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = (i == currentSelectedIndex) ? selectedHighlightColor : defaultButtonColor;
                        }
                    }
                }
            }

            Debug.Log($"[StationQuizController] Người chơi đã chọn đáp án #{selectedIndex + 1} (chưa kiểm tra đúng sai, có thể đổi tiếp)!");
        }

        /// <summary>
        /// Khi hết thời gian (hoặc sau khi bấm Skip đếm hết 3s về 0):
        /// - Khóa đáp án.
        /// - Đổi màu đáp án (Xanh nếu đúng, Đỏ nếu sai).
        /// - Nếu không chọn gì: hiện đáp án đúng nhưng hiển thị màu Đỏ.
        /// - Chờ 1 giây để người chơi nhìn rõ kết quả rồi mới đóng QuestPanel!
        /// </summary>
        private IEnumerator OnQuestionEndRoutine()
        {
            if (!isAnswering) yield break;
            isAnswering = false;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            // Khóa tương tác toàn bộ nút đáp án
            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    if (answerButtons[i] != null) answerButtons[i].interactable = false;
                }
            }

            bool hasAnswered = (currentSelectedIndex >= 0);
            bool isCorrect = (hasAnswered && currentSelectedIndex == currentCorrectShuffledIndex);

            if (!hasAnswered)
            {
                // Người chơi KHÔNG CHỌN GÌ mà hết giờ -> show đáp án đúng nhưng tô màu ĐỎ
                Debug.Log("[StationQuizController] Không chọn đáp án: Show đáp án đúng nhưng tô màu ĐỎ!");
                if (answerButtons != null && currentCorrectShuffledIndex >= 0 && currentCorrectShuffledIndex < answerButtons.Length)
                {
                    Image correctImg = answerButtons[currentCorrectShuffledIndex].GetComponent<Image>();
                    if (correctImg != null) correctImg.color = wrongColor;
                }
            }
            else
            {
                if (isCorrect)
                {
                    // Trả lời ĐÚNG -> tô màu Xanh
                    Debug.Log("[StationQuizController] Trả lời ĐÚNG: Tô màu XANH!");
                    if (answerButtons != null && currentSelectedIndex < answerButtons.Length)
                    {
                        Image img = answerButtons[currentSelectedIndex].GetComponent<Image>();
                        if (img != null) img.color = correctColor;
                    }
                }
                else
                {
                    // Trả lời SAI -> đáp án người chọn tô Đỏ, đáp án đúng tô Xanh
                    Debug.Log("[StationQuizController] Trả lời SAI: Đáp án chọn tô ĐỎ, đáp án đúng tô XANH!");
                    if (answerButtons != null && currentSelectedIndex < answerButtons.Length)
                    {
                        Image img = answerButtons[currentSelectedIndex].GetComponent<Image>();
                        if (img != null) img.color = wrongColor;
                    }
                    if (answerButtons != null && currentCorrectShuffledIndex >= 0 && currentCorrectShuffledIndex < answerButtons.Length)
                    {
                        Image correctImg = answerButtons[currentCorrectShuffledIndex].GetComponent<Image>();
                        if (correctImg != null) correctImg.color = correctColor;
                    }
                }
            }

            // Đợi 1 giây hiển thị kết quả cho người chơi xem
            yield return new WaitForSeconds(1.0f);

            // Thực hiện kết quả và đóng QuestPanel
            if (isCorrect)
            {
                HandleCorrectAnswer();
            }
            else if (!hasAnswered)
            {
                HandleTimeout();
            }
            else
            {
                HandleWrongAnswer();
            }
        }

        /// <summary>
        /// Trả lời ĐÚNG:
        /// - Giảm số remain còn 2/3 (hoặc giảm số item yêu cầu)
        /// - Item (nút đó) biến mất (SetActive = false)
        /// - Đóng QuestPanel
        /// - Tăng số lượng item thuộc Theme và cập nhật hiển thị lên MainPanel
        /// - Ghi nhận trạng thái vào bộ nhớ trạm để khi vào lại vẫn ẩn item này
        /// </summary>
        private void HandleCorrectAnswer()
        {
            if (currentActiveItemData != null)
            {
                currentActiveItemData.isAnsweredCorrect = true;
            }

            if (currentActiveItemBtn != null)
            {
                currentActiveItemBtn.gameObject.SetActive(false);
                spawnedItems.Remove(currentActiveItemBtn);
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            ReduceRemainingCount();
            UpdateRemainText();

            // Nhận thưởng item theo Theme hiển thị lên MainPanel/ItemContainer
            RewardThemeItem(1);
        }

        /// <summary>
        /// Trả lời SAI:
        /// - Làm mờ nút item đó
        /// - Đóng QuestPanel
        /// - Ghi nhận trạng thái vào bộ nhớ trạm để khi vào lại vẫn làm mờ item này
        /// </summary>
        private void HandleWrongAnswer()
        {
            if (currentActiveItemData != null)
            {
                currentActiveItemData.isAnsweredWrong = true;
            }

            if (currentActiveItemBtn != null)
            {
                SetButtonOpacity(currentActiveItemBtn, 0.45f);
                currentActiveItemBtn.interactable = false;
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            UpdateRemainText();
        }

        /// <summary>
        /// Hết thời gian:
        /// - Giảm remain còn 2/3
        /// - Làm mờ nút item
        /// - Ghi nhận trạng thái vào bộ nhớ trạm
        /// </summary>
        private void HandleTimeout()
        {
            if (currentActiveItemData != null)
            {
                currentActiveItemData.isAnsweredWrong = true;
            }

            if (currentActiveItemBtn != null)
            {
                SetButtonOpacity(currentActiveItemBtn, 0.45f);
                currentActiveItemBtn.interactable = false;
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            ReduceRemainingCount();
            UpdateRemainText();
        }

        private void CloseQuestPanel()
        {
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Giảm số lượt trả lời của trạm (ví dụ từ 3/3 xuống 2/3, 1/3, 0/3).
        /// Khi về 0/3: Làm mờ tất cả các item còn lại và khóa tương tác (chỉ có thể bấm Exit).
        /// </summary>
        private void ReduceRemainingCount()
        {
            if (!stationRemainingTries.ContainsKey(currentStationIndex))
            {
                stationRemainingTries[currentStationIndex] = maxTriesPerStation;
            }

            if (stationRemainingTries[currentStationIndex] > 0)
            {
                stationRemainingTries[currentStationIndex]--;
            }

            int currentTries = stationRemainingTries[currentStationIndex];
            Debug.Log($"[StationQuizController] Trạm {currentStationIndex} còn lại: {currentTries}/{maxTriesPerStation} lượt trả lời!");

            // Nếu hết lượt (0/3): Làm mờ toàn bộ các item còn lại trong trạm và khóa bấm
            if (currentTries <= 0)
            {
                if (stationItemsState.ContainsKey(currentStationIndex))
                {
                    foreach (var itm in stationItemsState[currentStationIndex])
                    {
                        if (itm.runtimeBtn != null && !itm.isAnsweredCorrect)
                        {
                            SetButtonOpacity(itm.runtimeBtn, 0.45f);
                            itm.runtimeBtn.interactable = false;
                        }
                    }
                }
            }
        }

        private void UpdateRemainText()
        {
            if (remainText == null) return;

            if (!stationRemainingTries.ContainsKey(currentStationIndex))
            {
                stationRemainingTries[currentStationIndex] = maxTriesPerStation;
            }

            int currentTries = stationRemainingTries[currentStationIndex];
            remainText.text = $"{currentTries}/{maxTriesPerStation}";
        }

        private void SetButtonOpacity(Button btn, float alpha)
        {
            if (btn == null) return;

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }
    }
}

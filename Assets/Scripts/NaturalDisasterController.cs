using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Quản lý hệ thống 5 Tim (HP) và Sự kiện Thiên Tai (Natural Disasters) xuất hiện giữa đường leo.
    /// - Quản lý 5 tim trong MainPanel/HpContainer bằng prefab Hp.prefab.
    /// - Khi đang che đen ở mốc giữa chặng (50%), mở màn hình ra, dừng player ở giữa 2 trạm.
    /// - Hiển thị NaturalDisastersPanel với ảnh thiên tai ngẫu nhiên.
    /// - Sinh các nút chọn item trong SelectItemContainer từ SelectItemBtn.prefab.
    /// - Chọn đúng: thành công (không trừ HP, tiêu hao 1 item nếu có).
    /// - Chọn sai: trừ 1 HP (mất 1 tim).
    /// - Đóng panel, che đen lại, player tiếp tục leo và mở màn khi gần đến trạm tiếp theo.
    /// </summary>
    public class NaturalDisasterController : MonoBehaviour
    {
        public static NaturalDisasterController Instance { get; private set; }

        [System.Serializable]
        public class DisasterData
        {
            [Tooltip("Mã định danh thiên tai (ví dụ: avalanche, blizzard, wild_beast...)")]
            public string disasterId = "avalanche";

            [Tooltip("Tên hiển thị thiên tai")]
            public string disasterName = "Bão Tuyết Lở";

            [Tooltip("Hình ảnh thiên tai hiển thị trên NaturalDisastersPanel/Image")]
            public Sprite disasterSprite;

            [Tooltip("Mã itemId của item khắc chế được thiên tai này (dùng nếu chỉ có 1 item, ví dụ: item_camfire1)")]
            public string counterItemId = "item_camfire1";

            [Tooltip("Danh sách các mã item khắc chế được thiên tai này (nếu có từ 2 item trở lên, người chơi chọn 1 trong các item này đều sẽ thành công và không bị trừ HP)")]
            public List<string> counterItemIds = new List<string>();

            /// <summary>
            /// Lấy toàn bộ danh sách item khắc chế hợp lệ (kết hợp cả counterItemId và counterItemIds)
            /// </summary>
            public List<string> GetAllValidCounterItems()
            {
                List<string> list = new List<string>();
                if (counterItemIds != null)
                {
                    foreach (var id in counterItemIds)
                    {
                        if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                            list.Add(id);
                    }
                }
                if (!string.IsNullOrEmpty(counterItemId) && !list.Contains(counterItemId))
                {
                    list.Add(counterItemId);
                }
                return list;
            }
        }

        // ─── Inspector ────────────────────────────────────────────────────────────
        [Header("--- 1. HỆ THỐNG TIM (HP) ---")]
        [Tooltip("Vùng chứa các tim (GamePanel/MainPanel/HpContainer)")]
        [SerializeField] private Transform hpContainer;

        [Tooltip("Prefab của 1 tim (Assets/Prefabs/Hp.prefab)")]
        [SerializeField] private GameObject hpPrefab;

        [Tooltip("Số lượng tim tối đa khi bắt đầu")]
        [SerializeField] private int maxHp = 5;

        [Header("--- 2. DANH SÁCH THIÊN TAI (DISASTERS) ---")]
        [Tooltip("Danh sách các thiên tai có thể xảy ra và item tương ứng để chống lại")]
        [SerializeField] private List<DisasterData> disasterList = new List<DisasterData>();

        [Tooltip("Tỷ lệ xuất hiện sự kiện thiên tai khi leo giữa 2 trạm (0 = tắt, 1 = 100%)")]
        [Range(0f, 1f)]
        [SerializeField] private float disasterEventChance = 0.65f;

        [Tooltip("Bật để luôn luôn kích hoạt thiên tai mỗi khi leo núi (dành cho test)")]
        [SerializeField] private bool alwaysTriggerDisaster = false;

        [Header("--- 3. UI GIAO DIỆN THIÊN TAI ---")]
        [Tooltip("Panel thiên tai (GamePanel/NaturalDisastersPanel)")]
        [SerializeField] private GameObject naturalDisastersPanel;

        [Tooltip("Image hiển thị ảnh thiên tai (NaturalDisastersPanel/Image)")]
        [SerializeField] private Image disasterImage;

        [Tooltip("Text hiển thị tên thiên tai (NaturalDisastersPanel/Name)")]
        [SerializeField] private TMP_Text disasterNameText;

        [Tooltip("Text hiển thị thời gian đếm ngược (NaturalDisastersPanel/Time)")]
        [SerializeField] private TMP_Text disasterTimeText;

        [Tooltip("Thời gian đếm ngược đối phó thiên tai (giây)")]
        [SerializeField] private float disasterCountdown = 10f;

        [Tooltip("Vùng chứa các nút chọn item (NaturalDisastersPanel/SelectItemContainer)")]
        [SerializeField] private Transform selectItemContainer;

        [Tooltip("Prefab nút chọn item (Assets/Prefabs/SelectItemBtn.prefab)")]
        [SerializeField] private GameObject selectItemBtnPrefab;

        [Header("--- 4. THAM CHIẾU HỆ THỐNG ---")]
        [SerializeField] private StationQuizController stationQuiz;

        [Header("--- 5. DIALOG HỘP THOẠI THIÊN TAI ---")]
        [Tooltip("Panel hộp thoại thông báo thiên tai (trước & sau khi chọn item)")]
        [SerializeField] private GameObject disasterDialogPanel;

        [Tooltip("Text tiêu đề hộp thoại (Warning / Result)")]
        [SerializeField] private TMP_Text dialogTitleText;

        [Tooltip("Text nội dung chi tiết hộp thoại")]
        [SerializeField] private TMP_Text dialogMessageText;

        [Tooltip("Nút xác nhận hộp thoại (Đối phó ngay / Tiếp tục)")]
        [SerializeField] private Button dialogConfirmBtn;

        [Tooltip("Text của nút xác nhận")]
        [SerializeField] private TMP_Text dialogBtnText;

        [Tooltip("Tốc độ gõ chữ Typewriter (giây/ký tự, ví dụ 0.025s)")]
        [SerializeField] private float typewriterSpeed = 0.025f;

        [Tooltip("Thời gian tự động đóng Dialog nếu không bấm (giây, mặc định 3.5s)")]
        [SerializeField] private float defaultDialogAutoCloseTime = 3.5f;

        // ─── Runtime State ────────────────────────────────────────────────────────
        private int _currentHp = 5;
        private List<GameObject> _activeHearts = new List<GameObject>();
        private Coroutine _countdownCoroutine;
        private bool _hasChosenItem = false;
        private Coroutine _typewriterCoroutine;
        private Coroutine _autoCloseCoroutine;
        private bool _isTyping = false;
        private Action _pendingConfirmAction;

        public int CurrentHp => _currentHp;
        public int MaxHp => maxHp;
        public bool IsAlive => _currentHp > 0;

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoFindReferences();
        }

        private void Start()
        {
            InitHp(maxHp);
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(false);
            }
            HideDialog();
        }

        // ─── Auto Reference Finding ───────────────────────────────────────────────
        public void AutoFindReferences()
        {
            Transform root = transform;

            // 1. Tìm StationQuizController
            if (stationQuiz == null)
            {
                stationQuiz = GetComponentInChildren<StationQuizController>(true);
                if (stationQuiz == null) stationQuiz = FindFirstObjectByType<StationQuizController>();
            }

            // 2. Tìm HpContainer trong MainPanel
            if (hpContainer == null)
            {
                Transform mainPanel = FindChildRecursive(root, "MainPanel");
                if (mainPanel != null)
                {
                    hpContainer = FindChildRecursive(mainPanel, "HpContainer");
                }
                if (hpContainer == null)
                {
                    hpContainer = FindChildRecursive(root, "HpContainer");
                }
            }

            // 3. Tìm NaturalDisastersPanel và các con (Image, Name, Time, SelectItemContainer)
            if (naturalDisastersPanel == null)
            {
                Transform panelT = FindChildRecursive(root, "NaturalDisastersPanel");
                if (panelT == null)
                {
                    Canvas c = FindFirstObjectByType<Canvas>();
                    if (c != null) panelT = FindChildRecursive(c.transform, "NaturalDisastersPanel");
                }
                if (panelT != null) naturalDisastersPanel = panelT.gameObject;
            }

            if (naturalDisastersPanel != null)
            {
                if (disasterImage == null)
                {
                    Transform imgT = naturalDisastersPanel.transform.Find("Image");
                    if (imgT != null) disasterImage = imgT.GetComponent<Image>();
                }

                if (disasterNameText == null)
                {
                    Transform nameT = naturalDisastersPanel.transform.Find("Name");
                    if (nameT != null) disasterNameText = nameT.GetComponent<TMP_Text>();
                }

                if (disasterTimeText == null)
                {
                    Transform timeT = naturalDisastersPanel.transform.Find("Time");
                    if (timeT != null) disasterTimeText = timeT.GetComponent<TMP_Text>();
                }

                if (selectItemContainer == null)
                {
                    selectItemContainer = naturalDisastersPanel.transform.Find("SelectItemContainer");
                }
            }

            // 4. Load prefabs nếu chưa gán
#if UNITY_EDITOR
            if (hpPrefab == null)
            {
                hpPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hp.prefab");
            }
            if (selectItemBtnPrefab == null)
            {
                selectItemBtnPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SelectItemBtn.prefab");
            }
#endif

            // 5. Tìm hoặc tự động tạo Dialog Panel
            SetupDialogReferences();

            // 6. Cấu hình danh sách thiên tai mặc định nếu danh sách trống
            SetupDefaultDisastersIfEmpty();
        }

        private void SetupDialogReferences()
        {
            Transform root = transform;
            if (disasterDialogPanel == null)
            {
                Transform dPanel = FindChildRecursive(root, "DisasterDialogPanel");
                if (dPanel == null)
                {
                    Canvas c = FindFirstObjectByType<Canvas>();
                    if (c != null) dPanel = FindChildRecursive(c.transform, "DisasterDialogPanel");
                }
                if (dPanel != null) disasterDialogPanel = dPanel.gameObject;
            }

            if (disasterDialogPanel != null)
            {
                if (dialogTitleText == null)
                {
                    Transform t = FindChildRecursive(disasterDialogPanel.transform, "TitleText", "Title");
                    if (t != null) dialogTitleText = t.GetComponent<TMP_Text>();
                }
                if (dialogMessageText == null)
                {
                    Transform t = FindChildRecursive(disasterDialogPanel.transform, "MessageText", "Message", "ContentText", "Content");
                    if (t != null) dialogMessageText = t.GetComponent<TMP_Text>();
                }
                if (dialogConfirmBtn == null)
                {
                    Transform t = FindChildRecursive(disasterDialogPanel.transform, "ConfirmBtn", "Button", "OkBtn", "SkipBtn");
                    if (t != null) dialogConfirmBtn = t.GetComponent<Button>();
                }
                if (dialogConfirmBtn != null && dialogBtnText == null)
                {
                    dialogBtnText = dialogConfirmBtn.GetComponentInChildren<TMP_Text>(true);
                }
            }
            else
            {
                CreateDynamicDialogPanel();
            }
        }

        private void CreateDynamicDialogPanel()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tạo Backdrop phủ toàn màn hình (có Button để click bất cứ đâu cũng skip/tiếp tục)
            GameObject panelObj = new GameObject("DisasterDialogPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImg = panelObj.GetComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.75f); // Nền tối mờ ảo

            // 2. Tạo Card Dialog ở giữa
            GameObject cardObj = new GameObject("DialogCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            cardObj.transform.SetParent(panelObj.transform, false);

            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(540f, 290f);
            cardRT.anchoredPosition = Vector2.zero;

            Image cardImg = cardObj.GetComponent<Image>();
            cardImg.color = new Color(0.11f, 0.15f, 0.22f, 0.98f); // Màu xanh đêm navy

            // 3. Tiêu đề (TitleText)
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -18f);
            titleRT.sizeDelta = new Vector2(-40f, 38f);

            dialogTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            dialogTitleText.fontSize = 24;
            dialogTitleText.fontStyle = FontStyles.Bold;
            dialogTitleText.alignment = TextAlignmentOptions.Center;
            dialogTitleText.color = new Color(1f, 0.85f, 0.3f, 1f);
            dialogTitleText.text = "THÔNG BÁO THIÊN TAI";

            // 4. Nội dung (MessageText)
            GameObject msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgObj.transform.SetParent(cardObj.transform, false);
            RectTransform msgRT = msgObj.GetComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0f, 0f);
            msgRT.anchorMax = new Vector2(1f, 1f);
            msgRT.pivot = new Vector2(0.5f, 0.5f);
            msgRT.anchoredPosition = new Vector2(0f, 6f);
            msgRT.sizeDelta = new Vector2(-50f, -130f);

            dialogMessageText = msgObj.GetComponent<TextMeshProUGUI>();
            dialogMessageText.fontSize = 17;
            dialogMessageText.alignment = TextAlignmentOptions.Center;
            dialogMessageText.color = new Color(0.92f, 0.94f, 0.98f, 1f);
            dialogMessageText.textWrappingMode = TextWrappingModes.Normal;
            dialogMessageText.text = "...";

            // 5. Nút bấm xác nhận (ConfirmBtn)
            GameObject btnObj = new GameObject("ConfirmBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(cardObj.transform, false);
            RectTransform btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0f);
            btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 22f);
            btnRT.sizeDelta = new Vector2(200f, 44f);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.18f, 0.62f, 0.95f, 1f);

            dialogConfirmBtn = btnObj.GetComponent<Button>();

            // Text của nút bấm
            GameObject btnTextObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRT = btnTextObj.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;

            dialogBtnText = btnTextObj.GetComponent<TextMeshProUGUI>();
            dialogBtnText.fontSize = 17;
            dialogBtnText.fontStyle = FontStyles.Bold;
            dialogBtnText.alignment = TextAlignmentOptions.Center;
            dialogBtnText.color = Color.white;
            dialogBtnText.text = "Tiếp Tục";

            disasterDialogPanel = panelObj;
            disasterDialogPanel.SetActive(false);
        }

        private void SetupDefaultDisastersIfEmpty()
        {
            if (disasterList == null) disasterList = new List<DisasterData>();
            if (disasterList.Count > 0) return;

            Sprite avalancheSprite = null;
#if UNITY_EDITOR
            avalancheSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Avalanche.png");
            if (avalancheSprite == null)
            {
                avalancheSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Sleet.png");
            }
#endif

            // Thiên tai 1: Sạt lở tuyết -> có thể giải quyết bằng 1 trong 2 item: Snow Anchor hoặc Tent
            disasterList.Add(new DisasterData
            {
                disasterId = "avalanche",
                disasterName = "Sạt Lở Tuyết",
                disasterSprite = avalancheSprite,
                counterItemId = "item_snowanchor1",
                counterItemIds = new List<string> { "item_snowanchor1", "item_emergencytent1" }
            });

            // Thiên tai 2: Thú dữ tuyết / Đói rét -> dùng Meat
            disasterList.Add(new DisasterData
            {
                disasterId = "wild_beast",
                disasterName = "Thú Dữ Tuyết",
                disasterSprite = avalancheSprite,
                counterItemId = "item_meat1",
                counterItemIds = new List<string> { "item_meat1" }
            });
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

        // ─── 1. HP SYSTEM (TIM) ───────────────────────────────────────────────────

        /// <summary>
        /// Khởi tạo lại 5 tim khi bắt đầu map chơi
        /// </summary>
        public void InitHp(int max = 5)
        {
            maxHp = Mathf.Max(1, max);
            _currentHp = maxHp;

            AutoFindReferences();

            if (hpContainer == null) return;

            // Xóa toàn bộ tim cũ nếu có
            for (int i = hpContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(hpContainer.GetChild(i).gameObject);
            }
            _activeHearts.Clear();

            // Đảm bảo có HorizontalLayoutGroup để 5 tim xếp ngang đẹp mắt
            HorizontalLayoutGroup layout = hpContainer.GetComponent<HorizontalLayoutGroup>();
            if (layout == null) layout = hpContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 10f;

            // Sinh 5 tim
            for (int i = 0; i < maxHp; i++)
            {
                GameObject heartObj = null;
                if (hpPrefab != null)
                {
                    heartObj = Instantiate(hpPrefab, hpContainer);
                }
                else
                {
                    heartObj = new GameObject($"Hp_{i + 1}", typeof(RectTransform), typeof(Image));
                    heartObj.transform.SetParent(hpContainer, false);
                    Image img = heartObj.GetComponent<Image>();
                    img.color = Color.red;
                }

                heartObj.name = $"Heart_{i + 1}";
                RectTransform rt = heartObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(48f, 48f); // Kích thước chuẩn vừa vặn
                    rt.localScale = Vector3.one;
                }

                _activeHearts.Add(heartObj);
            }

            Debug.Log($"[NaturalDisasterController] Đã khởi tạo {maxHp} tim (HP) thành công!");
        }

        /// <summary>
        /// Trừ máu (mất tim) khi chọn sai item đối phó thiên tai
        /// </summary>
        public void TakeDamage(int amount = 1)
        {
            if (_currentHp <= 0) return;

            int oldHp = _currentHp;
            _currentHp = Mathf.Max(0, _currentHp - amount);

            // Ẩn các tim tương ứng từ phải sang trái
            for (int i = oldHp - 1; i >= _currentHp; i--)
            {
                if (i >= 0 && i < _activeHearts.Count && _activeHearts[i] != null)
                {
                    _activeHearts[i].SetActive(false);
                }
            }

            Debug.Log($"[NaturalDisasterController] Người chơi bị trừ {amount} HP! Máu hiện tại: {_currentHp}/{maxHp}");

            if (_currentHp <= 0)
            {
                Debug.LogWarning("[NaturalDisasterController] Người chơi đã hết tim (0 HP)!");
            }
        }

        /// <summary>
        /// Hồi máu (thêm tim) nếu cần
        /// </summary>
        public void Heal(int amount = 1)
        {
            int oldHp = _currentHp;
            _currentHp = Mathf.Min(maxHp, _currentHp + amount);

            for (int i = oldHp; i < _currentHp; i++)
            {
                if (i >= 0 && i < _activeHearts.Count && _activeHearts[i] != null)
                {
                    _activeHearts[i].SetActive(true);
                }
            }
        }

        // ─── 2. THIÊN TAI (NATURAL DISASTERS) ─────────────────────────────────────

        /// <summary>
        /// Kiểm tra xác suất xem có phát sinh sự kiện thiên tai giữa chặng hay không
        /// </summary>
        public bool RollDisasterChance()
        {
            if (alwaysTriggerDisaster) return true;
            if (disasterList == null || disasterList.Count == 0) return false;
            return UnityEngine.Random.value <= disasterEventChance;
        }

        /// <summary>
        /// Được kích hoạt tại mốc giữa 2 trạm (t = 50%):
        /// 1. Tắt màn đen (Open).
        /// 2. Bật NaturalDisastersPanel, thay ảnh thiên tai ngẫu nhiên.
        /// 3. Sinh các nút chọn item trong SelectItemContainer.
        /// 4. Đợi người chơi chọn xong -> đóng panel, bật lại màn đen, gọi onResume để player đi tiếp.
        /// </summary>
        /// <summary>
        /// Được kích hoạt tại mốc giữa 2 trạm (t = 50%):
        /// 1. Tắt màn đen (Open).
        /// 2. Bật NaturalDisastersPanel, thay ảnh thiên tai ngẫu nhiên, gán Tên thiên tai.
        /// 3. Khởi động đếm ngược thời gian (Time). Nếu hết giờ mà chưa chọn -> trừ 1 HP!
        /// 4. Sinh các nút chọn item trong SelectItemContainer (CHỈ HIỂN THỊ ITEM CÓ SỐ LƯỢNG > 0).
        /// 5. Đợi người chơi chọn xong -> đóng panel, bật lại màn đen, gọi onResume để player đi tiếp.
        /// </summary>
        /// <summary>
        /// Hiển thị hộp thoại Dialog với hiệu ứng Typewriter (gõ chữ từng ký tự).
        /// - Đang gõ chữ: bấm nút hoặc click màn hình để SKIP (nhảy full text ngay lập tức).
        /// - Đã hiện full text: bấm để xác nhận đóng và tiếp tục.
        /// - Nếu không bấm: tự động đóng sau autoCloseDelay giây.
        /// </summary>
        public void ShowDialog(string title, string message, string buttonText, Action onConfirm, float autoCloseDelay = 3.5f)
        {
            AutoFindReferences();

            if (disasterDialogPanel != null)
            {
                disasterDialogPanel.SetActive(true);
                disasterDialogPanel.transform.SetAsLastSibling(); // Luôn hiển thị trên cùng
            }

            if (dialogTitleText != null)
            {
                dialogTitleText.text = title;
            }

            if (dialogBtnText != null)
            {
                dialogBtnText.text = buttonText;
            }

            _pendingConfirmAction = onConfirm;

            // Gán sự kiện click: Skip khi đang gõ, hoặc Confirm khi đã gõ xong
            if (dialogConfirmBtn != null)
            {
                dialogConfirmBtn.onClick.RemoveAllListeners();
                dialogConfirmBtn.onClick.AddListener(OnDialogClicked);
            }

            // Gán click trên panel để người chơi bấm bất cứ đâu cũng nhận diện Skip / Tiếp tục
            Button panelBtn = disasterDialogPanel != null ? disasterDialogPanel.GetComponent<Button>() : null;
            if (panelBtn != null)
            {
                panelBtn.onClick.RemoveAllListeners();
                panelBtn.onClick.AddListener(OnDialogClicked);
            }

            Transform cardT = disasterDialogPanel != null ? disasterDialogPanel.transform.Find("DialogCard") : null;
            if (cardT != null)
            {
                Button cardBtn = cardT.GetComponent<Button>();
                if (cardBtn != null)
                {
                    cardBtn.onClick.RemoveAllListeners();
                    cardBtn.onClick.AddListener(OnDialogClicked);
                }
            }

            // Dừng các coroutine typewriter cũ nếu có
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);

            _typewriterCoroutine = StartCoroutine(CoTypewriterRoutine(message, autoCloseDelay));
        }

        private IEnumerator CoTypewriterRoutine(string message, float autoCloseDelay)
        {
            _isTyping = true;

            if (dialogMessageText != null)
            {
                dialogMessageText.text = message;
                dialogMessageText.ForceMeshUpdate();

                int totalChars = dialogMessageText.textInfo.characterCount;
                dialogMessageText.maxVisibleCharacters = 0;

                for (int i = 0; i <= totalChars; i++)
                {
                    dialogMessageText.maxVisibleCharacters = i;
                    yield return new WaitForSecondsRealtime(typewriterSpeed);
                }

                dialogMessageText.maxVisibleCharacters = totalChars;
            }

            _isTyping = false;

            // Sau khi chạy xong chữ, tự động đóng sau autoCloseDelay giây nếu không bấm
            if (autoCloseDelay > 0f)
            {
                _autoCloseCoroutine = StartCoroutine(CoAutoCloseRoutine(autoCloseDelay));
            }
        }

        private IEnumerator CoAutoCloseRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ConfirmAndCloseDialog();
        }

        /// <summary>
        /// Bấm nút hoặc click màn hình:
        /// - Đang gõ chữ -> NHẢY FULL TEXT NGAY LẬP TỨC (Skip typewriter).
        /// - Đã hiện full text -> ĐÓNG DIALOG VÀ TIẾP TỤC HÀNH TRÌNH.
        /// </summary>
        public void OnDialogClicked()
        {
            if (_isTyping)
            {
                // BẤM SKIP: Nhảy full text ngay lập tức!
                if (_typewriterCoroutine != null)
                {
                    StopCoroutine(_typewriterCoroutine);
                    _typewriterCoroutine = null;
                }

                if (dialogMessageText != null)
                {
                    dialogMessageText.maxVisibleCharacters = dialogMessageText.textInfo.characterCount;
                }

                _isTyping = false;

                // Khởi động lại đếm ngược tự đóng sau khi đã hiện full text
                if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
                _autoCloseCoroutine = StartCoroutine(CoAutoCloseRoutine(defaultDialogAutoCloseTime));
            }
            else
            {
                // Đã hiện full text -> Đóng dialog và tiếp tục!
                ConfirmAndCloseDialog();
            }
        }

        private void ConfirmAndCloseDialog()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
            if (_autoCloseCoroutine != null)
            {
                StopCoroutine(_autoCloseCoroutine);
                _autoCloseCoroutine = null;
            }

            _isTyping = false;
            HideDialog();

            Action callback = _pendingConfirmAction;
            _pendingConfirmAction = null;
            callback?.Invoke();
        }

        public void HideDialog()
        {
            if (disasterDialogPanel != null)
            {
                disasterDialogPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Được kích hoạt tại mốc giữa 2 trạm (t = 50%):
        /// 1. Màn đen vẫn che kín. Hiển thị Dialog cảnh báo thiên tai trước (có Typewriter, Skip, Auto-close).
        /// 2. Khi bấm "Đối phó ngay" (hoặc hết giờ dialog):
        ///    - Tắt màn đen (Open), lộ cảnh nhân vật giữa núi tuyết.
        ///    - Bật NaturalDisastersPanel, thay ảnh/tên thiên tai, sinh các nút item sở hữu > 0.
        ///    - Đếm ngược 10s.
        /// </summary>
        public void TriggerDisasterEvent(Action onResume)
        {
            AutoFindReferences();

            // 1. Chọn ngẫu nhiên 1 thiên tai
            DisasterData chosenDisaster = PickRandomDisaster();

            if (chosenDisaster != null)
            {
                if (disasterImage != null && chosenDisaster.disasterSprite != null)
                {
                    disasterImage.sprite = chosenDisaster.disasterSprite;
                    disasterImage.color = Color.white;
                }

                if (disasterNameText != null)
                {
                    disasterNameText.text = chosenDisaster.disasterName;
                }
            }

            // 2. HIỂN THỊ DIALOG CẢNH BÁO TRƯỚC KHI TẮT MÀN ĐEN
            string dName = (chosenDisaster != null) ? chosenDisaster.disasterName : "Thiên Tai";
            string warnTitle = "⚠️ CẢNH BÁO THIÊN TAI!";
            string warnMsg = $"Phía trước đang xuất hiện <color=#FFD54F><b>{dName}</b></color> cực kỳ nguy hiểm!\n\nHãy chuẩn bị chọn 1 vật phẩm thích hợp trong túi đồ để đối phó và bảo toàn tính mạng!";

            ShowDialog(warnTitle, warnMsg, "Đối Phó Ngay", () =>
            {
                // Khi bấm Đối Phó (hoặc hết giờ dialog):
                // Mở màn đen để lộ cảnh nhân vật đang đứng giữa 2 trạm
                CutSceneTransition.Open(duration: 0.25f);

                // Hiển thị Panel Thiên Tai
                if (naturalDisastersPanel != null)
                {
                    naturalDisastersPanel.SetActive(true);
                }

                // Sinh các nút chọn item (CHỈ HIỆN ITEM CÓ SỐ LƯỢNG > 0)
                PopulateSelectItems(chosenDisaster, onResume);

                // Khởi động đếm ngược thời gian (10s)
                if (_countdownCoroutine != null)
                {
                    StopCoroutine(_countdownCoroutine);
                }
                _countdownCoroutine = StartCoroutine(CoDisasterCountdown(chosenDisaster, onResume));
            }, defaultDialogAutoCloseTime);
        }

        private IEnumerator CoDisasterCountdown(DisasterData disaster, Action onResume)
        {
            _hasChosenItem = false;
            float timeLeft = disasterCountdown;

            while (timeLeft > 0f)
            {
                if (disasterTimeText != null)
                {
                    disasterTimeText.text = Mathf.CeilToInt(timeLeft).ToString();
                }

                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;

                if (_hasChosenItem) yield break;
            }

            if (_hasChosenItem) yield break;

            // HẾT THỜI GIAN -> BỊ TRỪ 1 HP!
            if (disasterTimeText != null) disasterTimeText.text = "0";
            string dName = (disaster != null) ? disaster.disasterName : "Thiên tai";
            Debug.LogWarning($"[NaturalDisaster] ⌛ HẾT THỜI GIAN đối phó thiên tai '{dName}'! Bị trừ 1 HP!");
            TakeDamage(1);

            // Ẩn panel thiên tai
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(false);
            }

            // HIỂN THỊ DIALOG HẾT THỜI GIAN
            string timeoutTitle = "⌛ HẾT THỜI GIAN!";
            string timeoutMsg = $"Bạn đã không kịp lựa chọn vật phẩm để đối phó <color=#FFD54F><b>{dName}</b></color>!\n\nBạn bị <color=#E57373><b>trừ 1 Tim (-1 HP)</b></color>!\nTim hiện tại: <b>{_currentHp}/{maxHp}</b>.";

            ShowDialog(timeoutTitle, timeoutMsg, "Tiếp Tục", () =>
            {
                // Bật lại màn đen che kín, khi che xong thì player tiếp tục leo
                CutSceneTransition.Close(onComplete: () =>
                {
                    onResume?.Invoke();
                }, duration: 0.22f);
            }, defaultDialogAutoCloseTime);
        }

        private DisasterData PickRandomDisaster()
        {
            if (disasterList == null || disasterList.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, disasterList.Count);
            return disasterList[idx];
        }

        /// <summary>
        /// Chuẩn hóa chuỗi ID/tên item để so sánh linh hoạt (bỏ qua 'item_', '_', '-', số thứ tự cuối, v.v., và sửa lỗi chính tả camfire vs campfire)
        /// </summary>
        public static string NormalizeItemKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            string s = key.Trim().ToLowerInvariant();

            // Bỏ tiền tố thông dụng nếu có
            if (s.StartsWith("item_")) s = s.Substring(5);
            else if (s.StartsWith("item")) s = s.Substring(4);

            // Xóa dấu gạch dưới, khoảng trắng, gạch ngang
            s = s.Replace("_", "").Replace("-", "").Replace(" ", "");

            // Chuẩn hóa lỗi chính tả phổ biến "camfire" -> "campfire"
            s = s.Replace("camfire", "campfire");

            // Chuẩn hóa emergencytent -> tent
            if (s.Contains("emergencytent")) s = s.Replace("emergencytent", "tent");

            // Bỏ số ở cuối nếu có (ví dụ: campfire1 -> campfire, meat1 -> meat)
            while (s.Length > 0 && char.IsDigit(s[s.Length - 1]))
            {
                s = s.Substring(0, s.Length - 1);
            }

            return s;
        }

        /// <summary>
        /// Kiểm tra xem item người chơi chọn có khắc chế được thiên tai hay không.
        /// Hỗ trợ cả khớp chính xác, khớp chuẩn hóa (item_camfire1 == item_campfire1 == Campfire), và tra cứu qua ItemSO.
        /// </summary>
        public bool CheckItemMatches(string chosenItemId, string counterItemId)
        {
            if (string.IsNullOrEmpty(chosenItemId) || string.IsNullOrEmpty(counterItemId))
                return false;

            // 1. So sánh trực tiếp không phân biệt hoa thường
            if (chosenItemId.Equals(counterItemId, StringComparison.OrdinalIgnoreCase))
                return true;

            // 2. Chuẩn hóa chuỗi (ví dụ item_camfire1 vs Campfire vs item_campfire1 đều ra 'campfire')
            string normChosen = NormalizeItemKey(chosenItemId);
            string normCounter = NormalizeItemKey(counterItemId);
            if (!string.IsNullOrEmpty(normChosen) && normChosen.Equals(normCounter, StringComparison.OrdinalIgnoreCase))
                return true;

            // 3. Tra cứu qua ItemSO nếu có stationQuiz
            if (stationQuiz != null)
            {
                ItemSO chosenSO = stationQuiz.GetItemSO(chosenItemId);
                if (chosenSO != null)
                {
                    if (chosenSO.name.Equals(counterItemId, StringComparison.OrdinalIgnoreCase) ||
                        chosenSO.itemName.Equals(counterItemId, StringComparison.OrdinalIgnoreCase) ||
                        chosenSO.itemId.Equals(counterItemId, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (NormalizeItemKey(chosenSO.name).Equals(normCounter, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeItemKey(chosenSO.itemName).Equals(normCounter, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeItemKey(chosenSO.itemId).Equals(normCounter, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                ItemSO counterSO = stationQuiz.GetItemSO(counterItemId);
                if (counterSO != null)
                {
                    if (counterSO.name.Equals(chosenItemId, StringComparison.OrdinalIgnoreCase) ||
                        counterSO.itemName.Equals(chosenItemId, StringComparison.OrdinalIgnoreCase) ||
                        counterSO.itemId.Equals(chosenItemId, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (NormalizeItemKey(counterSO.name).Equals(normChosen, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeItemKey(counterSO.itemName).Equals(normChosen, StringComparison.OrdinalIgnoreCase) ||
                        NormalizeItemKey(counterSO.itemId).Equals(normChosen, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xem item người chơi chọn có nằm trong danh sách item khắc chế được thiên tai này không.
        /// Chỉ cần 1 trong các item hợp lệ là vượt qua thành công (không mất HP)!
        /// </summary>
        public bool IsItemValidForDisaster(string chosenItemId, DisasterData disaster)
        {
            if (disaster == null || string.IsNullOrEmpty(chosenItemId)) return false;

            List<string> validCounters = disaster.GetAllValidCounterItems();
            if (validCounters == null || validCounters.Count == 0) return false;

            foreach (var counter in validCounters)
            {
                if (CheckItemMatches(chosenItemId, counter))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sinh danh sách các nút item mà người chơi có thể chọn để đối phó thiên tai.
        /// QUY TẮC: CHỈ HIỂN THỊ CÁC ITEM CÓ SỐ LƯỢNG > 0 TRONG TÚI!
        /// </summary>
        private void PopulateSelectItems(DisasterData disaster, Action onResume)
        {
            if (selectItemContainer == null) return;

            // Xóa sạch các nút cũ trong SelectItemContainer
            for (int i = selectItemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(selectItemContainer.GetChild(i).gameObject);
            }

            // Lấy danh sách item từ theme hiện tại hoặc từ inventory
            List<ItemSO> candidateItems = new List<ItemSO>();
            if (stationQuiz != null)
            {
                candidateItems = stationQuiz.GetCurrentThemeItems();

                // Bổ sung các item trong kho có số lượng > 0 nếu chưa có trong list
                var inv = stationQuiz.GetInventoryItems();
                foreach (var kvp in inv)
                {
                    if (kvp.Value > 0)
                    {
                        if (!candidateItems.Exists(x => x != null && (CheckItemMatches(x.itemId, kvp.Key) || CheckItemMatches(x.name, kvp.Key))))
                        {
                            ItemSO so = stationQuiz.GetItemSO(kvp.Key);
                            if (so != null)
                            {
                                candidateItems.Add(so);
                            }
                            else
                            {
                                ItemSO fallbackSO = ScriptableObject.CreateInstance<ItemSO>();
                                fallbackSO.itemId = kvp.Key;
                                fallbackSO.itemName = NormalizeItemKey(kvp.Key).Equals("campfire") ? "Campfire" : (NormalizeItemKey(kvp.Key).Equals("meat") ? "Meat" : (NormalizeItemKey(kvp.Key).Equals("tent") ? "Tent" : kvp.Key));
                                candidateItems.Add(fallbackSO);
                            }
                        }
                    }
                }
            }

            // Deduplicate candidateItems: đảm bảo mỗi item duy nhất chỉ hiển thị 1 nút
            List<ItemSO> uniqueItems = new List<ItemSO>();
            HashSet<string> seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var itm in candidateItems)
            {
                if (itm == null) continue;
                string k = NormalizeItemKey(!string.IsNullOrEmpty(itm.itemId) ? itm.itemId : itm.name);
                if (seenKeys.Add(k))
                {
                    uniqueItems.Add(itm);
                }
            }
            candidateItems = uniqueItems;

            int spawnedCount = 0;

            foreach (var itemSO in candidateItems)
            {
                if (itemSO == null) continue;

                string itemId = string.IsNullOrEmpty(itemSO.itemId) ? itemSO.name : itemSO.itemId;
                int count = (stationQuiz != null) ? stationQuiz.GetItemQuantity(itemId) : 0;

                // ─── ĐIỀU KIỆN QUAN TRỌNG: CHỈ HIỆN ITEM SỐ LƯỢNG > 0 ───
                if (count <= 0) continue;

                spawnedCount++;
                string itemName = string.IsNullOrEmpty(itemSO.itemName) ? itemId : itemSO.itemName;

                GameObject btnObj = null;
                if (selectItemBtnPrefab != null)
                {
                    btnObj = Instantiate(selectItemBtnPrefab, selectItemContainer);
                }
                else
                {
                    btnObj = new GameObject($"SelectItem_{itemId}", typeof(RectTransform), typeof(Button), typeof(Image));
                    btnObj.transform.SetParent(selectItemContainer, false);
                }

                btnObj.name = $"SelectItemBtn_{itemId}";
                btnObj.SetActive(true);

                // Gán icon nếu prefab có Image con
                Transform iconT = FindChildRecursive(btnObj.transform, "ItemImg", "Icon", "Image");
                if (iconT != null && itemSO.icon != null)
                {
                    Image iconImg = iconT.GetComponent<Image>();
                    if (iconImg != null)
                    {
                        iconImg.sprite = itemSO.icon;
                        iconImg.color = Color.white;
                    }
                }

                // Gán tên Item vào NameText
                TMP_Text nameText = FindChildRecursive(btnObj.transform, "NameText")?.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    nameText.text = itemName;
                }

                // Gán số lượng sở hữu vào NumberText
                TMP_Text numText = FindChildRecursive(btnObj.transform, "NumberText")?.GetComponent<TMP_Text>();
                if (numText != null)
                {
                    numText.text = $"x{count}";
                }

                // Gán sự kiện khi bấm nút chọn item này (dùng capturedId để tránh bug closure)
                Button btnComp = btnObj.GetComponent<Button>();
                if (btnComp != null)
                {
                    string capturedId = itemId;
                    btnComp.onClick.RemoveAllListeners();
                    btnComp.onClick.AddListener(() =>
                    {
                        OnItemSelected(capturedId, disaster, onResume);
                    });
                }
            }

            if (spawnedCount == 0)
            {
                Debug.LogWarning("[NaturalDisaster] Người chơi không có bất kỳ item nào trong túi (tất cả số lượng đều = 0)!");
            }
        }

        /// <summary>
        /// Xử lý khi người chơi bấm chọn 1 item đối phó:
        /// - Dừng đếm ngược thời gian ngay lập tức.
        /// - Ẩn panel chọn item.
        /// - Kiểm tra xem item đó có đối phó thành công hay thất bại.
        /// - Hiển thị Dialog thông báo kết quả (Typewriter, bấm Skip để hiện full chữ, có auto-close timer).
        /// - Khi xác nhận đóng dialog -> Bật màn đen (Close) và tiếp tục hành trình leo núi.
        /// </summary>
        private void OnItemSelected(string chosenItemId, DisasterData disaster, Action onResume)
        {
            _hasChosenItem = true;
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            // 1. Tắt panel thiên tai
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(false);
            }

            bool isCorrect = IsItemValidForDisaster(chosenItemId, disaster);
            int availableCount = (stationQuiz != null) ? stationQuiz.GetItemQuantity(chosenItemId) : 0;
            string dName = (disaster != null) ? disaster.disasterName : "Thiên tai";

            // Lấy tên hiển thị của item
            string itemName = chosenItemId;
            if (stationQuiz != null)
            {
                ItemSO so = stationQuiz.GetItemSO(chosenItemId);
                if (so != null && !string.IsNullOrEmpty(so.itemName)) itemName = so.itemName;
            }

            string resTitle;
            string resMsg;

            if (isCorrect && availableCount > 0)
            {
                // Chọn ĐÚNG và CÓ ITEM -> Tiêu hao 1 item, Thành công (Không trừ HP)
                if (stationQuiz != null)
                {
                    stationQuiz.ConsumeItem(chosenItemId, 1);
                }
                Debug.Log($"[NaturalDisaster] 🎉 ĐỐI PHÓ THÀNH CÔNG thiên tai '{dName}' bằng '{chosenItemId}'! Đã tiêu hao 1 item, không bị trừ HP.");
                resTitle = "🎉 ĐỐI PHÓ THÀNH CÔNG!";
                resMsg = $"Vật phẩm <color=#81C784><b>{itemName}</b></color> đã <color=#81C784><b>ĐỐI PHÓ THÀNH CÔNG</b></color> thiên tai <color=#FFD54F>{dName}</color>!\n\nBạn bảo toàn nguyên vẹn <b>{_currentHp}/{maxHp} Tim (HP)</b> và an toàn vượt qua vùng hiểm trở.";
            }
            else
            {
                // Chọn SAI hoặc KHÔNG CÓ ITEM
                if (availableCount > 0)
                {
                    // Chọn SAI nhưng CÓ ITEM trong túi -> VẪN TIÊU HAO 1 ITEM ĐÃ CHỌN ĐÓ, VÀ TRỪ 1 HP!
                    if (stationQuiz != null)
                    {
                        stationQuiz.ConsumeItem(chosenItemId, 1);
                    }
                    string validListStr = (disaster != null) ? string.Join(", ", disaster.GetAllValidCounterItems()) : "none";
                    Debug.LogWarning($"[NaturalDisaster] ❌ Chọn SAI item đối phó ('{chosenItemId}' không khắc chế được [{validListStr}])! Đã tiêu hao 1 '{chosenItemId}' và bị trừ 1 HP!");
                    resTitle = "❌ ĐỐI PHÓ THẤT BẠI!";
                    resMsg = $"Vật phẩm <color=#E57373><b>{itemName}</b></color> <color=#E57373><b>KHÔNG THỂ ĐỐI PHÓ</b></color> được <color=#FFD54F>{dName}</color>!\n\nBạn bị <color=#E57373><b>trừ 1 Tim (-1 HP)</b></color> và tiêu hao 1 {itemName}.\nTim còn lại: <b>{Mathf.Max(0, _currentHp - 1)}/{maxHp}</b>.";
                }
                else
                {
                    Debug.LogWarning($"[NaturalDisaster] ⚠️ Item '{chosenItemId}' số lượng bằng 0 (x0)! Bị trừ 1 HP!");
                    resTitle = "⚠️ THIẾU VẬT PHẨM!";
                    resMsg = $"Bạn không có đủ số lượng vật phẩm <color=#E57373><b>{itemName}</b></color>!\n\nBạn bị <color=#E57373><b>trừ 1 Tim (-1 HP)</b></color>.\nTim còn lại: <b>{Mathf.Max(0, _currentHp - 1)}/{maxHp}</b>.";
                }

                TakeDamage(1);
            }

            // HIỂN THỊ DIALOG KẾT QUẢ VỚI TYPEWRITER & AUTO-CLOSE
            ShowDialog(resTitle, resMsg, "Tiếp Tục", () =>
            {
                // Bật lại màn đen che kín (Close)
                // Khi màn đen đã che kín, kích hoạt onResume để nhân vật tiếp tục bước đi từ giữa đường tới trạm đích
                CutSceneTransition.Close(onComplete: () =>
                {
                    onResume?.Invoke();
                }, duration: 0.22f);
            }, defaultDialogAutoCloseTime);
        }
    }
}

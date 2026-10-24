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

        // ─── Inspector ────────────────────────────────────────────────────────────
        [Header("--- 1. HỆ THỐNG TIM (HP) ---")]
        [Tooltip("Vùng chứa các tim (GamePanel/MainPanel/HpContainer)")]
        [SerializeField] private Transform hpContainer;

        [Tooltip("Prefab của 1 tim (Assets/Prefabs/Hp.prefab)")]
        [SerializeField] private GameObject hpPrefab;

        [Tooltip("Số lượng tim tối đa khi bắt đầu")]
        [SerializeField] private int maxHp = 5;

        [Header("--- 2. CẤU HÌNH TỶ LỆ THIÊN TAI ---")]
        [Tooltip("Tỷ lệ xuất hiện sự kiện thiên tai khi leo giữa 2 trạm (0 = tắt, 1 = 100%)")]
        [Range(0f, 1f)]
        [SerializeField] private float disasterEventChance = 0.65f;

        [Tooltip("Bật để luôn luôn kích hoạt thiên tai mỗi khi leo núi (dành cho test)")]
        [SerializeField] private bool alwaysTriggerDisaster = false;

        // Danh sách thiên tai đang áp dụng cho map hiện tại (lấy động từ ThemeSO khi vào map)
        private List<DisasterSO> activeDisasters = new List<DisasterSO>();

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

        [Header("--- 5. DIALOG HỘP THOẠI THIÊN TAI (CHUNG / CẢNH BÁO) ---")]
        [Tooltip("Panel hộp thoại thông báo thiên tai (trước & sau khi chọn item, dùng làm mặc định nếu không gán panel riêng)")]
        [SerializeField] private GameObject disasterDialogPanel;

        [Tooltip("Text tiêu đề hộp thoại (Warning / Result)")]
        [SerializeField] private TMP_Text dialogTitleText;

        [Tooltip("Text nội dung chi tiết hộp thoại")]
        [SerializeField] private TMP_Text dialogMessageText;

        [Tooltip("Nút xác nhận hộp thoại (Đối phó ngay / Tiếp tục)")]
        [SerializeField] private Button dialogConfirmBtn;

        [Tooltip("Text của nút xác nhận")]
        [SerializeField] private TMP_Text dialogBtnText;

        [Header("--- CẤU HÌNH HIỆU ỨNG GÕ CHỮ & THỜI GIAN ---")]
        [Tooltip("Tốc độ gõ chữ Typewriter (giây/ký tự, ví dụ 0.025s)")]
        [SerializeField] private float typewriterSpeed = 0.025f;

        [Tooltip("Thời gian tự động đóng Dialog nếu không bấm (giây, mặc định 3.5s)")]
        [SerializeField] private float defaultDialogAutoCloseTime = 3.5f;

        // ─── Runtime State ────────────────────────────────────────────────────────
        private int _currentHp = 5;
        private List<GameObject> _activeHearts = new List<GameObject>();
        private Coroutine _countdownCoroutine;
        private bool _hasChosenItem = false;
        private Color _defaultDisasterTimeColor = Color.white;
        private readonly Color _warningRedColor = new Color(0.95f, 0.25f, 0.25f, 1f);
        private Coroutine _typewriterCoroutine;
        private Coroutine _autoCloseCoroutine;
        private bool _isTyping = false;
        private Action _pendingConfirmAction;
        private GameObject _activeDialogPanel;
        private TMP_Text _activeMessageText;
        private DisasterDialogType _currentDialogType = DisasterDialogType.Warning;

        // Lưu trữ danh sách nút chọn item để điều khiển highlight và đổi lựa chọn
        private class SelectItemHolder
        {
            public string itemId;
            public string itemName;
            public int count;
            public GameObject btnObj;
            public Image borderImg;
            public Color defaultColor;
            public Vector3 defaultScale;
        }

        private List<SelectItemHolder> _spawnedItemHolders = new List<SelectItemHolder>();
        private SelectItemHolder _currentSelectedHolder = null;

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
                if (disasterTimeText != null && (disasterTimeText.color.r < 0.85f || disasterTimeText.color.g > 0.35f || disasterTimeText.color.b > 0.35f))
                {
                    _defaultDisasterTimeColor = disasterTimeText.color;
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

            // 5. Tìm các tham chiếu Dialog Panel
            SetupDialogReferences();
        }

        private void SetupDialogReferences()
        {
            Transform root = transform;
            Canvas c = FindFirstObjectByType<Canvas>();

            // 1. Setup disasterDialogPanel (Chung / Mặc định / Cảnh báo)
            if (disasterDialogPanel == null)
            {
                Transform dPanel = FindChildRecursive(root, "DisasterDialogPanel");
                if (dPanel == null && c != null)
                {
                    dPanel = FindChildRecursive(c.transform, "DisasterDialogPanel");
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

        /// <summary>
        /// Khởi tạo sự kiện thiên tai theo Theme của Map (lấy các thiên tai được gán trong ThemeSO)
        /// </summary>
        public void InitForMap(MapData mapData, ThemeSO theme = null)
        {
            activeDisasters.Clear();

            if (theme == null && mapData != null && !string.IsNullOrEmpty(mapData.themeId))
            {
                theme = FindThemeById(mapData.themeId);
            }
            if (theme == null)
            {
                theme = FindAnyTheme();
            }

            if (theme != null && theme.themeDisasters != null && theme.themeDisasters.Count > 0)
            {
                activeDisasters.AddRange(theme.themeDisasters);
                Debug.Log($"[NaturalDisasterController] Đã nạp {activeDisasters.Count} sự kiện thiên tai từ ThemeSO '{theme.themeName}'!");
            }
            else
            {
                Debug.LogWarning("[NaturalDisasterController] Theme không có danh sách thiên tai (themeDisasters). Không kích hoạt thiên tai cho map này.");
            }
        }

        private ThemeSO FindThemeById(string themeId)
        {
            ThemeSO[] loaded = Resources.FindObjectsOfTypeAll<ThemeSO>();
            if (loaded != null)
            {
                foreach (var t in loaded)
                {
                    if (t != null && t.themeId == themeId) return t;
                }
            }
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ThemeSO");
            foreach (var g in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                ThemeSO tSO = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeSO>(path);
                if (tSO != null && tSO.themeId == themeId) return tSO;
            }
#endif
            return null;
        }

        private ThemeSO FindAnyTheme()
        {
            ThemeSO[] loaded = Resources.FindObjectsOfTypeAll<ThemeSO>();
            if (loaded != null && loaded.Length > 0)
            {
                foreach (var t in loaded)
                {
                    if (t != null) return t;
                }
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

            _activeHearts.Clear();

            // Nếu trong hpContainer đã có sẵn các icon tim do bạn cài đặt chuẩn trong Editor -> sử dụng trực tiếp
            if (hpContainer.childCount >= maxHp)
            {
                for (int i = 0; i < hpContainer.childCount; i++)
                {
                    GameObject heartObj = hpContainer.GetChild(i).gameObject;
                    bool shouldShow = (i < maxHp);
                    heartObj.SetActive(shouldShow);
                    if (shouldShow)
                    {
                        _activeHearts.Add(heartObj);
                    }
                }
            }
            else
            {
                // Nếu chưa có tim trong container thì mới dọn dẹp và sinh mới từ hpPrefab
                for (int i = hpContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(hpContainer.GetChild(i).gameObject);
                }

                // Sinh các tim
                for (int i = 0; i < maxHp; i++)
                {
                    GameObject heartObj = null;
                    if (hpPrefab != null)
                    {
                        heartObj = Instantiate(hpPrefab, hpContainer);
                    }
                    else
                    {
                        heartObj = new GameObject($"Heart_{i + 1}", typeof(RectTransform), typeof(Image));
                        heartObj.transform.SetParent(hpContainer, false);
                        Image img = heartObj.GetComponent<Image>();
                        img.color = Color.red;
                    }

                    heartObj.name = $"Heart_{i + 1}";
                    heartObj.SetActive(true);
                    _activeHearts.Add(heartObj);
                }
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
            if (activeDisasters == null || activeDisasters.Count == 0) return false;
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
        // ─── DIALOG SYSTEM CHO CÁC SỰ KIỆN THIÊN TAI ─────────────────────────────
        public enum DisasterDialogType
        {
            Warning,
            Success,
            Failure
        }

        /// <summary>
        /// Thay thế các thẻ mẫu trong văn bản:
        /// {item} -> Tên item màu xanh lá nổi bật
        /// {item_raw} -> Tên item thuần
        /// {disaster} -> Tên thiên tai màu vàng hổ phách
        /// {disaster_raw} -> Tên thiên tai thuần
        /// {hp} -> Số tim hiện tại
        /// {maxhp} -> Số tim tối đa
        /// </summary>
        public static string FormatDialogTemplate(string template, string itemName, string disasterName, int currentHp, int maxHp)
        {
            if (string.IsNullOrEmpty(template)) return "";
            string formatted = template;

            if (!string.IsNullOrEmpty(itemName))
            {
                formatted = formatted.Replace("{item}", $"<color=#81C784><b>{itemName}</b></color>");
                formatted = formatted.Replace("{item_raw}", itemName);
            }
            else
            {
                formatted = formatted.Replace("{item}", "");
                formatted = formatted.Replace("{item_raw}", "");
            }

            if (!string.IsNullOrEmpty(disasterName))
            {
                formatted = formatted.Replace("{disaster}", $"<color=#FFD54F><b>{disasterName}</b></color>");
                formatted = formatted.Replace("{disaster_raw}", disasterName);
            }
            else
            {
                formatted = formatted.Replace("{disaster}", "");
                formatted = formatted.Replace("{disaster_raw}", "");
            }

            formatted = formatted.Replace("{hp}", currentHp.ToString());
            formatted = formatted.Replace("{maxhp}", maxHp.ToString());

            // Tự động định dạng màu đỏ nổi bật cho -1 HP nếu có trong nội dung
            if (formatted.Contains("-1 HP") && !formatted.Contains("<color=#E57373><b>-1 HP</b></color>"))
            {
                formatted = formatted.Replace("-1 HP", "<color=#E57373><b>-1 HP</b></color>");
            }
            else if (formatted.Contains("-1 Hp") && !formatted.Contains("<color=#E57373><b>-1 HP</b></color>"))
            {
                formatted = formatted.Replace("-1 Hp", "<color=#E57373><b>-1 HP</b></color>");
            }
            else if (formatted.Contains("-1hp") && !formatted.Contains("<color=#E57373><b>-1 HP</b></color>"))
            {
                formatted = formatted.Replace("-1hp", "<color=#E57373><b>-1 HP</b></color>");
            }

            return formatted;
        }

        /// <summary>
        /// Hiển thị Dialog riêng cho từng loại sự kiện (Cảnh báo, Thành công, Thất bại):
        /// - Nếu có Panel riêng trong Inspector (successDialogPanel, failureDialogPanel) -> Sử dụng panel đó.
        /// - Nếu không -> Sử dụng chung disasterDialogPanel và tự động chuyển đổi theme màu sắc (Xanh lá / Đỏ / Vàng).
        /// - Có hiệu ứng Typewriter, bấm nút hoặc click màn hình để SKIP ngay lập tức, tự động đóng sau autoCloseDelay giây.
        /// </summary>
        public void ShowDialogInternal(DisasterDialogType type, string title, string message, string buttonText, Action onConfirm, float autoCloseDelay = 3.5f)
        {
            AutoFindReferences();
            _currentDialogType = type;
            HideDialog();

            if (disasterDialogPanel != null)
            {
                Transform cardT = disasterDialogPanel.transform.Find("DialogCard");
                Image cardImg = (cardT != null) ? cardT.GetComponent<Image>() : null;
                Image btnImg = (dialogConfirmBtn != null) ? dialogConfirmBtn.GetComponent<Image>() : null;

                switch (type)
                {
                    case DisasterDialogType.Success:
                        if (dialogTitleText != null) dialogTitleText.color = new Color(0.35f, 0.9f, 0.45f, 1f); // Xanh lá ngọc
                        if (btnImg != null) btnImg.color = new Color(0.18f, 0.72f, 0.45f, 1f);
                        if (cardImg != null) cardImg.color = new Color(0.08f, 0.18f, 0.14f, 0.98f); // Nền xanh sẫm
                        break;
                    case DisasterDialogType.Failure:
                        if (dialogTitleText != null) dialogTitleText.color = new Color(1f, 0.4f, 0.4f, 1f); // Đỏ san hô
                        if (btnImg != null) btnImg.color = new Color(0.88f, 0.28f, 0.28f, 1f);
                        if (cardImg != null) cardImg.color = new Color(0.2f, 0.08f, 0.1f, 0.98f); // Nền đỏ sẫm
                        break;
                    case DisasterDialogType.Warning:
                    default:
                        if (dialogTitleText != null) dialogTitleText.color = new Color(1f, 0.85f, 0.3f, 1f); // Vàng hổ phách
                        if (btnImg != null) btnImg.color = new Color(0.18f, 0.62f, 0.95f, 1f);
                        if (cardImg != null) cardImg.color = new Color(0.11f, 0.15f, 0.22f, 0.98f); // Nền navy
                        break;
                }

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

            // Gán click trên panel backdrop để bấm bất cứ đâu cũng nhận diện Skip / Tiếp tục
            if (disasterDialogPanel != null)
            {
                Button panelBtn = disasterDialogPanel.GetComponent<Button>();
                if (panelBtn != null)
                {
                    panelBtn.onClick.RemoveAllListeners();
                    panelBtn.onClick.AddListener(OnDialogClicked);
                }

                Transform cardT = disasterDialogPanel.transform.Find("DialogCard");
                if (cardT != null)
                {
                    Button cardBtn = cardT.GetComponent<Button>();
                    if (cardBtn != null)
                    {
                        cardBtn.onClick.RemoveAllListeners();
                        cardBtn.onClick.AddListener(OnDialogClicked);
                    }
                }
            }

            // Dừng các coroutine typewriter cũ nếu có
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);

            _typewriterCoroutine = StartCoroutine(CoTypewriterRoutine(dialogMessageText, message, autoCloseDelay));
        }

        /// <summary>
        /// Hiển thị Dialog riêng khi ĐỐI PHÓ THÀNH CÔNG
        /// </summary>
        public void ShowSuccessDialog(DisasterSO disaster, string chosenItemId, string itemName, Action onConfirm, float autoCloseDelay = -1f)
        {
            float delay = (autoCloseDelay > 0f) ? autoCloseDelay : defaultDialogAutoCloseTime;
            string dName = (disaster != null) ? disaster.disasterName : "Thiên tai";

            string title = "🎉 ĐỐI PHÓ THÀNH CÔNG!";

            string message = (!string.IsNullOrEmpty(disaster?.successMessage))
                ? FormatDialogTemplate(disaster.successMessage, itemName, dName, _currentHp, maxHp)
                : $"Vật phẩm <color=#81C784><b>{itemName}</b></color> đã <color=#81C784><b>ĐỐI PHÓ THÀNH CÔNG</b></color> thiên tai <color=#FFD54F>{dName}</color>!\n\nBạn bảo toàn nguyên vẹn <b>{_currentHp}/{maxHp} Tim (HP)</b> và an toàn vượt qua vùng hiểm trở.";

            ShowDialogInternal(DisasterDialogType.Success, title, message, "Tiếp Tục", onConfirm, delay);
        }

        /// <summary>
        /// Hiển thị Dialog riêng khi ĐỐI PHÓ THẤT BẠI
        /// </summary>
        public void ShowFailureDialog(DisasterSO disaster, string chosenItemId, string itemName, bool hasItemInBag, bool isTimeout, Action onConfirm, float autoCloseDelay = -1f)
        {
            float delay = (autoCloseDelay > 0f) ? autoCloseDelay : defaultDialogAutoCloseTime;
            string dName = (disaster != null) ? disaster.disasterName : "Thiên tai";

            string title;
            string message;

            if (isTimeout)
            {
                title = "⌛ HẾT THỜI GIAN!";
                message = $"Bạn đã không kịp lựa chọn vật phẩm để đối phó <color=#FFD54F><b>{dName}</b></color>!\n<color=#E57373><b>-1 HP</b></color>";
            }
            else if (!hasItemInBag)
            {
                title = "⚠️ THIẾU VẬT PHẨM!";
                message = string.IsNullOrEmpty(itemName)
                    ? $"Túi đồ của bạn không có bất kỳ vật phẩm nào để đối phó <color=#FFD54F><b>{dName}</b></color>!\n<color=#E57373><b>-1 HP</b></color>"
                    : $"Bạn không có đủ số lượng vật phẩm <color=#E57373><b>{itemName}</b></color>!\n<color=#E57373><b>-1 HP</b></color>";
            }
            else
            {
                title = "❌ ĐỐI PHÓ THẤT BẠI!";

                message = (!string.IsNullOrEmpty(disaster?.failureMessage))
                    ? FormatDialogTemplate(disaster.failureMessage, itemName, dName, _currentHp, maxHp)
                    : $"Vật phẩm <color=#E57373><b>{itemName}</b></color> <color=#E57373><b>KHÔNG THỂ ĐỐI PHÓ</b></color> được <color=#FFD54F>{dName}</color>!\n<color=#E57373><b>-1 HP</b></color>";
            }

            ShowDialogInternal(DisasterDialogType.Failure, title, message, "Tiếp Tục", onConfirm, delay);
        }

        /// <summary>
        /// Hiển thị Dialog khi chọn ĐÚNG vật phẩm nhưng KHÔNG MAY MẮN thất bại do xác suất rủi ro (successChance < 1)
        /// </summary>
        public void ShowBadLuckFailureDialog(DisasterSO disaster, string chosenItemId, string itemName, Action onConfirm, float autoCloseDelay = -1f)
        {
            float delay = (autoCloseDelay > 0f) ? autoCloseDelay : defaultDialogAutoCloseTime;
            string dName = (disaster != null) ? disaster.disasterName : "Thiên tai";

            string title = "⚠️ KHÔNG MAY MẮN!";

            string message = (!string.IsNullOrEmpty(disaster?.badLuckFailureMessage))
                ? FormatDialogTemplate(disaster.badLuckFailureMessage, itemName, dName, _currentHp, maxHp)
                : $"Dù bạn đã dùng vật phẩm đúng là <color=#FFD54F><b>{itemName}</b></color>, nhưng do điều kiện <color=#FFD54F>{dName}</color> quá bất lợi và khắc nghiệt, vật phẩm không thể phát huy hết hiệu quả!\n<color=#E57373><b>-1 HP</b></color>";

            ShowDialogInternal(DisasterDialogType.Failure, title, message, "Tiếp Tục", onConfirm, delay);
        }

        /// <summary>
        /// Hiển thị Dialog riêng khi CẢNH BÁO THIÊN TAI
        /// </summary>
        public void ShowWarningDialog(DisasterSO disaster, Action onConfirm, float autoCloseDelay = -1f)
        {
            float delay = (autoCloseDelay > 0f) ? autoCloseDelay : defaultDialogAutoCloseTime;
            string dName = (disaster != null) ? disaster.disasterName : "Thiên Tai";

            string title = "⚠️ CẢNH BÁO THIÊN TAI!";

            string message = (!string.IsNullOrEmpty(disaster?.warningMessage))
                ? FormatDialogTemplate(disaster.warningMessage, "", dName, _currentHp, maxHp)
                : $"Phía trước đang xuất hiện <color=#FFD54F><b>{dName}</b></color> cực kỳ nguy hiểm!\n\nHãy chuẩn bị chọn 1 vật phẩm thích hợp trong túi đồ để đối phó và bảo toàn tính mạng!";

            ShowDialogInternal(DisasterDialogType.Warning, title, message, "Đối Phó Ngay", onConfirm, delay);
        }

        /// <summary>
        /// Hàm hiển thị dialog tương thích ngược với code cũ
        /// </summary>
        public void ShowDialog(string title, string message, string buttonText, Action onConfirm, float autoCloseDelay = 3.5f)
        {
            ShowDialogInternal(DisasterDialogType.Warning, title, message, buttonText, onConfirm, autoCloseDelay);
        }

        private IEnumerator CoTypewriterRoutine(TMP_Text targetText, string message, float autoCloseDelay)
        {
            _isTyping = true;
            _activeMessageText = targetText;

            if (targetText != null)
            {
                targetText.text = message;
                targetText.ForceMeshUpdate();

                int totalChars = targetText.textInfo.characterCount;
                targetText.maxVisibleCharacters = 0;

                for (int i = 0; i <= totalChars; i++)
                {
                    targetText.maxVisibleCharacters = i;
                    yield return new WaitForSecondsRealtime(typewriterSpeed);
                }

                targetText.maxVisibleCharacters = totalChars;
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

                if (_activeMessageText != null)
                {
                    _activeMessageText.maxVisibleCharacters = _activeMessageText.textInfo.characterCount;
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

            // Nếu là Warning Dialog -> ẩn ngay để lộ panel đối phó thiên tai
            // Nếu là Result Dialog (Success / Failure / BadLuck) -> giữ lại để màn đen che kín trước, rồi mới tắt ngầm bên dưới
            if (_currentDialogType == DisasterDialogType.Warning)
            {
                HideAllDialogs();
            }

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
            _activeDialogPanel = null;
            _activeMessageText = null;
        }

        public void HideAllDialogs()
        {
            HideDialog();
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
            DisasterSO chosenDisaster = PickRandomDisaster();

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
            ShowWarningDialog(chosenDisaster, () =>
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

                // Khởi động đếm ngược thời gian theo cấu hình thiên tai (hoặc fallback mặc định)
                float initialTime = (chosenDisaster != null && chosenDisaster.countdownTime > 0f) ? chosenDisaster.countdownTime : disasterCountdown;
                if (disasterTimeText != null)
                {
                    disasterTimeText.text = $"{Mathf.CeilToInt(initialTime)}s";
                    disasterTimeText.color = (initialTime <= 3.05f) ? _warningRedColor : _defaultDisasterTimeColor;
                }

                if (_countdownCoroutine != null)
                {
                    StopCoroutine(_countdownCoroutine);
                }
                _countdownCoroutine = StartCoroutine(CoDisasterCountdown(chosenDisaster, onResume));
            }, defaultDialogAutoCloseTime);
        }

        private IEnumerator CoDisasterCountdown(DisasterSO disaster, Action onResume)
        {
            _hasChosenItem = false;
            float timeLeft = (disaster != null && disaster.countdownTime > 0f) ? disaster.countdownTime : disasterCountdown;

            while (timeLeft > 0f)
            {
                if (disasterTimeText != null)
                {
                    int sec = Mathf.CeilToInt(timeLeft);
                    disasterTimeText.text = $"{sec}s";

                    // Đổi sang màu đỏ cảnh báo khi thời gian còn <= 3 giây
                    if (timeLeft <= 3.05f)
                    {
                        disasterTimeText.color = _warningRedColor;
                    }
                    else
                    {
                        disasterTimeText.color = _defaultDisasterTimeColor;
                    }
                }

                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;

                if (_hasChosenItem) yield break;
            }

            if (_hasChosenItem) yield break;

            // HẾT THỜI GIAN:
            if (disasterTimeText != null)
            {
                disasterTimeText.text = "0s";
                disasterTimeText.color = _warningRedColor;
            }

            // Tự động chốt theo item đang được chọn (nếu có chọn), hoặc xử lý timeout nếu chưa chọn item nào
            if (_currentSelectedHolder != null)
            {
                Debug.Log($"[NaturalDisaster] ⌛ HẾT THỜI GIAN! Tự động chốt theo item đang chọn: '{_currentSelectedHolder.itemId}'");
                ExecuteDisasterDecision(_currentSelectedHolder.itemId, disaster, onResume);
            }
            else
            {
                Debug.LogWarning($"[NaturalDisaster] ⌛ HẾT THỜI GIAN mà chưa chọn bất kỳ item nào! Bị trừ 1 HP!");
                ExecuteDisasterDecision(null, disaster, onResume);
            }
        }

        private DisasterSO PickRandomDisaster()
        {
            if (activeDisasters == null || activeDisasters.Count == 0) return null;
            var validDisasters = activeDisasters.FindAll(d => d != null);
            if (validDisasters == null || validDisasters.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, validDisasters.Count);
            return validDisasters[idx];
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
        public bool IsItemValidForDisaster(string chosenItemId, DisasterSO disaster)
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
        private void PopulateSelectItems(DisasterSO disaster, Action onResume)
        {
            if (selectItemContainer == null) return;

            selectItemContainer.gameObject.SetActive(true);

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

            _spawnedItemHolders.Clear();
            _currentSelectedHolder = null;
            _hasChosenItem = false;

            int spawnedCount = 0;

            foreach (var itemSO in candidateItems)
            {
                if (itemSO == null) continue;

                string itemId = string.IsNullOrEmpty(itemSO.itemId) ? itemSO.name : itemSO.itemId;
                int count = (stationQuiz != null) ? stationQuiz.GetItemQuantity(itemId) : 0;

                // CHỈ HIỂN THỊ CÁC ITEM ĐANG SỞ HỮU TRONG TÚI (SỐ LƯỢNG > 0)
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
                Transform iconT = FindChildRecursive(btnObj.transform, "ItemImg", "Icon");
                Image iconImg = null;
                if (iconT != null)
                {
                    iconImg = iconT.GetComponent<Image>();
                }
                else
                {
                    Image[] allImgs = btnObj.GetComponentsInChildren<Image>(true);
                    foreach (var img in allImgs)
                    {
                        if (img.gameObject != btnObj && img.gameObject.name != "Image")
                        {
                            iconImg = img;
                            break;
                        }
                    }
                }

                if (iconImg != null && itemSO.icon != null)
                {
                    iconImg.sprite = itemSO.icon;
                    iconImg.color = Color.white;
                    iconImg.preserveAspect = true;
                }

                // Gán tên Item vào NameText
                TMP_Text nameText = FindChildRecursive(btnObj.transform, "NameText", "Name")?.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    nameText.text = itemName;
                }

                // Gán số lượng sở hữu vào QuanText / NumberText
                TMP_Text numText = FindChildRecursive(btnObj.transform, "QuanText", "NumberText", "Quantity")?.GetComponent<TMP_Text>();
                if (numText != null)
                {
                    numText.text = $"x{count}";
                    numText.color = Color.white;
                }

                // Tìm Image làm khung viền để highlight
                Image borderImg = null;
                Transform frameT = FindChildRecursive(btnObj.transform, "Image", "Frame", "Border", "Bg");
                if (frameT != null) borderImg = frameT.GetComponent<Image>();
                if (borderImg == null) borderImg = btnObj.GetComponent<Image>();

                SelectItemHolder holder = new SelectItemHolder
                {
                    itemId = itemId,
                    itemName = itemName,
                    count = count,
                    btnObj = btnObj,
                    borderImg = borderImg,
                    defaultColor = (borderImg != null) ? borderImg.color : Color.white,
                    defaultScale = btnObj.transform.localScale
                };
                _spawnedItemHolders.Add(holder);

                // Gán sự kiện click: chọn item hoặc đổi sang item khác, click lại chính item đó để chốt ngay
                Button btnComp = btnObj.GetComponent<Button>();
                if (btnComp != null)
                {
                    btnComp.onClick.RemoveAllListeners();
                    btnComp.onClick.AddListener(() =>
                    {
                        OnSelectItemClicked(holder, disaster, onResume);
                    });
                }
            }

            if (spawnedCount == 0)
            {
                Debug.LogWarning("[NaturalDisaster] Không tìm thấy bất kỳ item nào để sinh nút đối phó!");
            }
        }

        /// <summary>
        /// Xử lý bấm chọn item:
        /// - Lần 1: Chọn item đó (highlight viền xanh sáng và phóng to nhẹ). Người chơi có thể tự do bấm chọn item khác để đổi lựa chọn.
        /// - Lần 2 (bấm lại chính item đang chọn): Xác nhận chốt ngay lập tức mà không cần chờ hết thời gian đếm ngược.
        /// </summary>
        private void OnSelectItemClicked(SelectItemHolder holder, DisasterSO disaster, Action onResume)
        {
            if (_hasChosenItem || holder == null) return;

            // Nếu người chơi bấm lại chính nút đang chọn -> Chốt lựa chọn ngay lập tức!
            if (_currentSelectedHolder == holder)
            {
                Debug.Log($"[NaturalDisaster] ⚡ Người chơi bấm xác nhận chốt ngay item: '{holder.itemId}'");
                ExecuteDisasterDecision(holder.itemId, disaster, onResume);
                return;
            }

            // Đổi lựa chọn sang nút mới
            _currentSelectedHolder = holder;
            UpdateSelectItemVisuals();
            Debug.Log($"[NaturalDisaster] 🔄 Đã đổi chọn item: '{holder.itemId}' (Số lượng: {holder.count}). Bấm lại lần nữa để chốt ngay hoặc đợi hết giờ!");
        }

        private void UpdateSelectItemVisuals()
        {
            Color selectedHighlightColor = new Color(0.2f, 0.9f, 1f, 1f); // Xanh cyan sáng nổi bật

            foreach (var h in _spawnedItemHolders)
            {
                if (h == null || h.btnObj == null) continue;

                if (h == _currentSelectedHolder)
                {
                    if (h.borderImg != null)
                    {
                        h.borderImg.color = selectedHighlightColor;
                    }
                    h.btnObj.transform.localScale = h.defaultScale * 1.1f;
                }
                else
                {
                    if (h.borderImg != null)
                    {
                        h.borderImg.color = h.defaultColor;
                    }
                    h.btnObj.transform.localScale = h.defaultScale;
                }
            }
        }

        /// <summary>
        /// Chốt kết quả đối phó thiên tai (gọi khi bấm chốt hoặc khi hết thời gian đếm ngược)
        /// </summary>
        private void ExecuteDisasterDecision(string chosenItemId, DisasterSO disaster, Action onResume)
        {
            if (_hasChosenItem) return;
            _hasChosenItem = true;

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            // Ẩn vùng nút chọn item
            if (selectItemContainer != null)
            {
                selectItemContainer.gameObject.SetActive(false);
            }

            // Đảm bảo panel thiên tai vẫn đang bật
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(true);
            }

            // TRƯỜNG HỢP 0: Người chơi KHÔNG CHỌN BẤT KỲ ITEM NÀO mà hết giờ -> Bị trừ 1 HP!
            if (string.IsNullOrEmpty(chosenItemId))
            {
                if (disasterTimeText != null)
                {
                    disasterTimeText.text = "0s";
                    disasterTimeText.color = _warningRedColor;
                }
                string dNameTimeout = (disaster != null) ? disaster.disasterName : "Thiên tai";
                TakeDamage(1);

                bool hasAnyItems = false;
                if (stationQuiz != null)
                {
                    var inv = stationQuiz.GetInventoryItems();
                    if (inv != null)
                    {
                        foreach (var kvp in inv)
                        {
                            if (kvp.Value > 0) { hasAnyItems = true; break; }
                        }
                    }
                }

                if (!hasAnyItems)
                {
                    Debug.LogWarning($"[NaturalDisaster] ⚠️ Người chơi không có bất kỳ item nào trong túi đối phó '{dNameTimeout}'! Bị trừ 1 HP!");
                    ShowFailureDialog(disaster, "", "", false, false, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
                else
                {
                    Debug.LogWarning($"[NaturalDisaster] ⌛ HẾT THỜI GIAN không kịp chọn item đối phó '{dNameTimeout}'! Bị trừ 1 HP!");
                    ShowFailureDialog(disaster, "", "", true, true, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
                return;
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

            if (isCorrect && availableCount > 0)
            {
                // Chọn ĐÚNG và CÓ ITEM TRONG TÚI -> Tiêu hao 1 item
                if (stationQuiz != null)
                {
                    stationQuiz.ConsumeItem(chosenItemId, 1);
                }

                // Kiểm tra tỷ lệ thành công (successChance: 0.0 -> 1.0)
                float chance = (disaster != null) ? Mathf.Clamp01(disaster.successChance) : 1f;
                bool isSuccess = (UnityEngine.Random.value <= chance);

                if (isSuccess)
                {
                    Debug.Log($"[NaturalDisaster] 🎉 ĐỐI PHÓ THÀNH CÔNG thiên tai '{dName}' bằng '{chosenItemId}' (Tỷ lệ: {chance * 100:0.#}%)! Đã tiêu hao 1 item, không bị trừ HP.");

                    ShowSuccessDialog(disaster, chosenItemId, itemName, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
                else
                {
                    // Dù chọn đúng vật phẩm nhưng xui xẻo trúng tỷ lệ thất bại -> Trừ 1 HP!
                    TakeDamage(1);
                    Debug.LogWarning($"[NaturalDisaster] ⚠️ Dù chọn đúng '{chosenItemId}' nhưng không may mắn thất bại theo tỷ lệ rủi ro! Đã bị trừ 1 HP.");

                    ShowBadLuckFailureDialog(disaster, chosenItemId, itemName, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
            }
            else
            {
                // Chọn SAI hoặc CÓ 0 ITEM TRONG TÚI (x0) -> TRỪ 1 HP!
                TakeDamage(1);

                if (availableCount > 0)
                {
                    // Chọn SAI nhưng CÓ ITEM trong túi -> Tiêu hao 1 item đã chọn đó, trừ 1 HP
                    if (stationQuiz != null)
                    {
                        stationQuiz.ConsumeItem(chosenItemId, 1);
                    }
                    string validListStr = (disaster != null) ? string.Join(", ", disaster.GetAllValidCounterItems()) : "none";
                    Debug.LogWarning($"[NaturalDisaster] ❌ Chọn sai item '{chosenItemId}' (không thể khắc chế [{validListStr}])! Đã tiêu hao 1 item và trừ 1 HP.");

                    ShowFailureDialog(disaster, chosenItemId, itemName, hasItemInBag: true, isTimeout: false, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
                else
                {
                    // Số lượng = 0 trong túi (chọn item x0)
                    Debug.LogWarning($"[NaturalDisaster] ⚠️ Người chơi chọn item '{chosenItemId}' nhưng số lượng trong túi bằng 0 (x0)! Bị trừ 1 HP do thiếu vật phẩm.");

                    ShowFailureDialog(disaster, chosenItemId, itemName, hasItemInBag: false, isTimeout: false, () =>
                    {
                        FinishDisasterEventWithBlackScreen(onResume);
                    }, defaultDialogAutoCloseTime);
                }
            }
        }

        /// <summary>
        /// Kết thúc sự kiện thiên tai:
        /// 1. Bật màn đen che kín màn hình trước (CutSceneTransition.Close).
        /// 2. Khi màn hình ĐÃ ĐEN HOÀN TOÀN (onComplete) -> Mới tắt ngầm naturalDisastersPanel và Dialog.
        /// 3. Tiếp tục hành trình leo núi (onResume) ngầm dưới màn đen.
        /// </summary>
        private void FinishDisasterEventWithBlackScreen(Action onResume)
        {
            CutSceneTransition.Close(onComplete: () =>
            {
                if (naturalDisastersPanel != null)
                {
                    naturalDisastersPanel.SetActive(false);
                }
                HideAllDialogs();

                onResume?.Invoke();
            }, duration: 0.25f);
        }
    }
}

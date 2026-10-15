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

            [Tooltip("Mã itemId của item khắc chế được thiên tai này (ví dụ: item_camfire1, item_meat1)")]
            public string counterItemId = "item_camfire1";
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

        // ─── Runtime State ────────────────────────────────────────────────────────
        private int _currentHp = 5;
        private List<GameObject> _activeHearts = new List<GameObject>();
        private Coroutine _countdownCoroutine;
        private bool _hasChosenItem = false;

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

            // 5. Cấu hình danh sách thiên tai mặc định nếu danh sách trống
            SetupDefaultDisastersIfEmpty();
        }

        private void SetupDefaultDisastersIfEmpty()
        {
            if (disasterList == null) disasterList = new List<DisasterData>();
            if (disasterList.Count > 0) return;

            Sprite avalancheSprite = null;
#if UNITY_EDITOR
            avalancheSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/AvalancheDisasters.png");
#endif

            // Thiên tai 1: Tuyết lở -> dùng Campfire (item_camfire1)
            disasterList.Add(new DisasterData
            {
                disasterId = "avalanche",
                disasterName = "Bão Tuyết Lở",
                disasterSprite = avalancheSprite,
                counterItemId = "item_camfire1"
            });

            // Thiên tai 2: Thú dữ tuyết / Đói rét -> dùng Meat (item_meat1)
            disasterList.Add(new DisasterData
            {
                disasterId = "wild_beast",
                disasterName = "Thú Dữ Tuyết",
                disasterSprite = avalancheSprite,
                counterItemId = "item_meat1"
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
        public void TriggerDisasterEvent(Action onResume)
        {
            AutoFindReferences();

            // 1. Tắt bỏ màn đen để lộ cảnh nhân vật đang đứng giữa 2 trạm
            CutSceneTransition.Open(duration: 0.25f);

            // 2. Chọn ngẫu nhiên 1 thiên tai
            DisasterData chosenDisaster = PickRandomDisaster();

            if (chosenDisaster != null)
            {
                if (disasterImage != null && chosenDisaster.disasterSprite != null)
                {
                    disasterImage.sprite = chosenDisaster.disasterSprite;
                    disasterImage.color = Color.white;
                }

                // Gán tên thiên tai vào NameText
                if (disasterNameText != null)
                {
                    disasterNameText.text = chosenDisaster.disasterName;
                }
            }

            // 3. Hiển thị Panel Thiên Tai
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(true);
            }

            // 4. Sinh các nút chọn item (CHỈ HIỆN ITEM CÓ SỐ LƯỢNG > 0)
            PopulateSelectItems(chosenDisaster, onResume);

            // 5. Khởi động đếm ngược thời gian
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }
            _countdownCoroutine = StartCoroutine(CoDisasterCountdown(chosenDisaster, onResume));
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

            // Bật lại màn đen che kín, khi che xong thì player tiếp tục leo
            CutSceneTransition.Close(onComplete: () =>
            {
                onResume?.Invoke();
            }, duration: 0.22f);
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
                                fallbackSO.itemName = NormalizeItemKey(kvp.Key).Equals("campfire") ? "Campfire" : (NormalizeItemKey(kvp.Key).Equals("meat") ? "Meat" : kvp.Key);
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
        /// - Đúng item: Không trừ HP (tiêu hao 1 item nếu có).
        /// - Sai item: Trừ 1 HP.
        /// - Đóng panel thiên tai, bật lại màn đen (Fade In), rồi gọi onResume để nhân vật tiếp tục chặng leo.
        /// </summary>
        private void OnItemSelected(string chosenItemId, DisasterData disaster, Action onResume)
        {
            _hasChosenItem = true;
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            bool isCorrect = (disaster != null && CheckItemMatches(chosenItemId, disaster.counterItemId));
            int availableCount = (stationQuiz != null) ? stationQuiz.GetItemQuantity(chosenItemId) : 0;

            if (isCorrect && availableCount > 0)
            {
                // Chọn ĐÚNG và CÓ ITEM trong túi -> Thành công
                Debug.Log($"[NaturalDisaster] 🎉 ĐỐI PHÓ THÀNH CÔNG thiên tai '{disaster.disasterName}' bằng '{chosenItemId}' (khắc chế '{disaster.counterItemId}')! Không bị trừ HP.");
                if (stationQuiz != null)
                {
                    stationQuiz.ConsumeItem(chosenItemId, 1);
                }
            }
            else
            {
                // Chọn SAI hoặc KHÔNG CÓ ITEM -> Bị trừ 1 HP
                if (isCorrect && availableCount <= 0)
                {
                    Debug.LogWarning($"[NaturalDisaster] ⚠️ Chọn đúng item '{chosenItemId}' nhưng số lượng bằng 0 (x0)! Bị trừ 1 HP!");
                }
                else
                {
                    string counter = (disaster != null) ? disaster.counterItemId : "null";
                    Debug.LogWarning($"[NaturalDisaster] ❌ Chọn SAI item đối phó ('{chosenItemId}' không khắc chế được '{counter}')! Bị trừ 1 HP!");
                }
                TakeDamage(1);
            }

            // 1. Tắt panel thiên tai
            if (naturalDisastersPanel != null)
            {
                naturalDisastersPanel.SetActive(false);
            }

            // 2. Bật lại màn đen che kín (Close)
            // Khi màn đen đã che kín, kích hoạt onResume để nhân vật tiếp tục bước đi từ giữa đường tới trạm đích
            CutSceneTransition.Close(onComplete: () =>
            {
                onResume?.Invoke();
            }, duration: 0.22f);
        }
    }
}

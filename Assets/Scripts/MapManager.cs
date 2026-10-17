using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    public class MapManager : MonoBehaviour
    {
        [Header("--- CÁC BẢNG GIAO DIỆN (PANELS) ---")]
        [Tooltip("Panel màn hình chính")]
        [SerializeField] private GameObject homePanel;

        [Tooltip("Panel quản lý và chọn các map")]
        [SerializeField] private GameObject mapPanel;

        [Tooltip("Panel tạo và chỉnh sửa câu hỏi cho map")]
        [SerializeField] private EditMapPanelUI editPanel;

        [Tooltip("Bảng giao diện trong màn chơi (GamePanel)")]
        [SerializeField] private GameObject gamePanel;

        [Tooltip("Hệ thống sinh trạm leo núi khi bấm Play")]
        [SerializeField] private MountainStationManager mountainManager;

        [Tooltip("Hệ thống quản lý câu hỏi và các trạm dừng (StationPanel & QuestPanel)")]
        [SerializeField] private StationQuizController stationQuiz;

        [Tooltip("Hệ thống quản lý 5 Tim (HP) và Sự Kiện Thiên Tai")]
        [SerializeField] private NaturalDisasterController naturalDisaster;

        [Header("--- NÚT BẤM (BUTTONS) ---")]
        [Tooltip("Nút Start ở màn hình chính để vào chọn map")]
        [SerializeField] private Button startBtn;

        [Tooltip("Nút Pause trên GamePanel để quay về chọn map")]
        [SerializeField] private Button pauseBtn;

        [Tooltip("Nút Climb trên GamePanel")]
        [SerializeField] private Button climbBtn;

        [Tooltip("Nút dấu '+' để tạo thêm map mới")]
        [SerializeField] private Button addMapBtn;

        [Tooltip("Nút quay lại màn hình chính Home")]
        [SerializeField] private Button backBtn;

        [Header("--- DANH SÁCH BẢN ĐỒ (MAPS) ---")]
        [Tooltip("Vùng chứa danh sách các thẻ map (LevelContent) trong Scroll View")]
        [SerializeField] private Transform levelContent;

        [Tooltip("Prefab thẻ hiển thị bản đồ (MapContent.prefab)")]
        [SerializeField] private GameObject mapPrefab;

        [Tooltip("Component hỗ trợ cuộn chuột ngang và tự động căn giữa")]
        [SerializeField] private CenterSnapScroll snapScroll;

        private GameSaveData saveData;
        private List<GameObject> spawnedMapObjects = new List<GameObject>();

        private void Awake()
        {
            AutoFindReferences();
            BindEvents();
        }

        private void Start()
        {
            saveData = QuizSaveSystem.Load();
            if (saveData == null || saveData.maps == null)
            {
                saveData = new GameSaveData();
            }

            ShowHomeMenu();
        }

        [ContextMenu("Clear All Save Data (Xóa toàn bộ dữ liệu lưu để test)")]
        public void ClearSaveDataFromInspector()
        {
            QuizSaveSystem.ClearAllSaveData();
            saveData = new GameSaveData();
            RefreshMapListUI();
            Debug.Log("[MapManager] Đã làm mới và xóa toàn bộ dữ liệu save!");
        }

        private void AutoFindReferences()
        {
            if (homePanel == null)
            {
                Transform t = FindChild(transform, "HomeMenuPanel", "HomePanel", "MenuPanel");
                if (t != null) homePanel = t.gameObject;
            }

            if (mapPanel == null)
            {
                Transform t = FindChild(transform, "MapManagerPanel", "MapPanel", "LevelPanel");
                if (t != null) mapPanel = t.gameObject;
            }

            if (editPanel == null)
            {
                Transform t = FindChild(transform, "EditMapPanel", "QuestionEditorPanel");
                if (t == null && transform.parent != null) t = FindChild(transform.parent, "EditMapPanel", "QuestionEditorPanel");
                if (t != null) editPanel = t.GetComponent<EditMapPanelUI>();
            }

            if (startBtn == null && homePanel != null)
            {
                Transform t = FindChild(homePanel.transform, "StartButton", "StartBtn", "PlayButton", "PlayBtn");
                if (t != null) startBtn = t.GetComponent<Button>();
                else startBtn = homePanel.GetComponentInChildren<Button>(true);
            }

            if (mapPanel != null)
            {
                if (levelContent == null)
                {
                    Transform t = FindChild(mapPanel.transform, "MapScrollView/Viewport/LevelContent", "ScrollView/Viewport/Content", "Viewport/LevelContent", "LevelContent");
                    if (t != null) levelContent = t;
                }

                if (addMapBtn == null)
                {
                    Transform t = FindChild(mapPanel.transform, "AddMapButton", "AddMapBtn", "AddButton", "AddBtn");
                    if (t != null) addMapBtn = t.GetComponent<Button>();
                }

                if (backBtn == null)
                {
                    Transform t = FindChild(mapPanel.transform, "BackToHomeButton", "BackButton", "BackBtn");
                    if (t != null) backBtn = t.GetComponent<Button>();
                }

                if (snapScroll == null)
                {
                    snapScroll = mapPanel.GetComponentInChildren<CenterSnapScroll>(true);
                }
            }

            // Tìm GamePanel và các nút PauseBtn, ClimbBtn
            if (gamePanel == null)
            {
                Transform t = FindChild(transform, "GamePanel");
                if (t == null && transform.parent != null) t = FindChild(transform.parent, "GamePanel");
                if (t != null) gamePanel = t.gameObject;
            }

            if (gamePanel != null)
            {
                if (pauseBtn == null)
                {
                    Transform t = FindChild(gamePanel.transform, "PauseBtn", "PauseButton", "Pause");
                    if (t != null) pauseBtn = t.GetComponent<Button>();
                }
                if (climbBtn == null)
                {
                    Transform t = FindChild(gamePanel.transform, "ClimbBtn", "ClimbButton", "Climb");
                    if (t != null) climbBtn = t.GetComponent<Button>();
                }

                if (stationQuiz == null)
                {
                    stationQuiz = gamePanel.GetComponentInChildren<StationQuizController>(true);
                    if (stationQuiz == null) stationQuiz = gamePanel.AddComponent<StationQuizController>();
                }

                if (naturalDisaster == null)
                {
                    naturalDisaster = gamePanel.GetComponentInChildren<NaturalDisasterController>(true);
                    if (naturalDisaster == null) naturalDisaster = gamePanel.AddComponent<NaturalDisasterController>();
                }
            }

            if (mountainManager == null)
            {
                mountainManager = GetComponent<MountainStationManager>();
                if (mountainManager == null) mountainManager = GetComponentInChildren<MountainStationManager>(true);
                if (mountainManager == null) mountainManager = FindFirstObjectByType<MountainStationManager>();
            }
        }

        private Transform FindChild(Transform parent, params string[] names)
        {
            if (parent == null) return null;
            foreach (var n in names)
            {
                Transform t = parent.Find(n);
                if (t != null) return t;
            }
            return null;
        }

        private void BindEvents()
        {
            if (startBtn != null)
                startBtn.onClick.AddListener(OnStartButtonClicked);

            if (addMapBtn != null)
                addMapBtn.onClick.AddListener(OnAddMapButtonClicked);

            if (backBtn != null)
                backBtn.onClick.AddListener(OnBackButtonClicked);

            if (pauseBtn != null)
            {
                pauseBtn.onClick.RemoveAllListeners();
                pauseBtn.onClick.AddListener(OnPauseButtonClicked);
            }

            if (climbBtn != null)
            {
                climbBtn.onClick.RemoveAllListeners();
                climbBtn.onClick.AddListener(OnClimbButtonClicked);
            }

            if (stationQuiz != null)
            {
                stationQuiz.OnStationOpenStateChanged -= HandleStationOpenStateChanged;
                stationQuiz.OnStationOpenStateChanged += HandleStationOpenStateChanged;
            }
        }

        public void ShowHomeMenu()
        {
            if (homePanel != null) homePanel.SetActive(true);
            if (mapPanel != null) mapPanel.SetActive(false);
            if (editPanel != null) editPanel.gameObject.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false);
            if (mountainManager != null) mountainManager.CloseMountain(triggerCallback: false);
        }

        public void OnStartButtonClicked()
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(true);
            if (editPanel != null) editPanel.gameObject.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false);
            if (mountainManager != null) mountainManager.CloseMountain(triggerCallback: false);

            RefreshMapListUI();
            StartCoroutine(InitSnapRoutine());
            Debug.Log("[MapManager] Đã mở MapManagerPanel!");
        }

        public void OnBackButtonClicked()
        {
            ShowHomeMenu();
        }

        public void OpenEditMap(MapData map)
        {
            if (editPanel != null)
            {
                if (mapPanel != null) mapPanel.SetActive(false);
                editPanel.Open(map, () =>
                {
                    QuizSaveSystem.Save(saveData);
                }, () =>
                {
                    // Khi đóng EditMapPanel, tự động bật lại MapManagerPanel
                    if (mapPanel != null) mapPanel.SetActive(true);
                    RefreshMapListUI();
                    StartCoroutine(InitSnapRoutine());
                    Debug.Log("[MapManager] Đã quay lại MapManagerPanel thành công!");
                });
            }
            else
            {
                Debug.LogWarning("[MapManager] Chưa gán EditMapPanelUI!");
            }
        }

        public void CloseEditMap()
        {
            if (editPanel != null && editPanel.gameObject.activeSelf) editPanel.Close();
            if (mapPanel != null) mapPanel.SetActive(true);
            RefreshMapListUI();
            StartCoroutine(InitSnapRoutine());
            Debug.Log("[MapManager] Đã mở lại MapManagerPanel!");
        }

        public void OnAddMapButtonClicked()
        {
            int nextIndex = saveData.maps.Count + 1;
            MapData newMap = new MapData($"Bản đồ #{nextIndex}");
            saveData.maps.Add(newMap);

            QuizSaveSystem.Save(saveData);
            Debug.Log($"[MapManager] Đã tạo và lưu Map mới: '{newMap.mapName}'");

            GameObject newMapObj = SpawnMapCard(newMap, nextIndex);

            if (addMapBtn != null)
            {
                addMapBtn.transform.SetAsLastSibling();
            }

            if (newMapObj != null)
            {
                StartCoroutine(SnapToTargetRoutine(newMapObj.GetComponent<RectTransform>()));
            }
        }

        private void RefreshMapListUI()
        {
            if (levelContent == null) return;

            foreach (var obj in spawnedMapObjects)
            {
                if (obj != null) Destroy(obj);
            }
            spawnedMapObjects.Clear();

            // Nếu không có map nào, chỉ hiển thị nút dấu + để người dùng tự tạo
            for (int i = 0; i < saveData.maps.Count; i++)
            {
                SpawnMapCard(saveData.maps[i], i + 1);
            }

            if (addMapBtn != null)
            {
                addMapBtn.transform.SetAsLastSibling();
            }
        }

        private GameObject SpawnMapCard(MapData mapData, int index)
        {
            if (levelContent == null) return null;

            GameObject mapObj = null;
            if (mapPrefab != null)
            {
                mapObj = Instantiate(mapPrefab, levelContent);
            }
            else
            {
                mapObj = new GameObject($"Map_{index}", typeof(RectTransform));
                mapObj.transform.SetParent(levelContent, false);
            }

            mapObj.name = $"Map_{index}";
            mapObj.SetActive(true);
            spawnedMapObjects.Add(mapObj);

            MapItemUI itemUI = mapObj.GetComponent<MapItemUI>();
            if (itemUI == null) itemUI = mapObj.AddComponent<MapItemUI>();

            itemUI.Setup(
                mapData,
                onEdit: (data) => OpenEditMap(data),
                onPlay: (data) =>
                {
                    StartGameOnMap(data);
                },
                onDelete: (data) =>
                {
                    DeleteMap(data);
                }
            );

            return mapObj;
        }

        public void DeleteMap(MapData mapData)
        {
            if (mapData == null || saveData == null || saveData.maps == null) return;

            if (saveData.maps.Contains(mapData))
            {
                saveData.maps.Remove(mapData);
                QuizSaveSystem.Save(saveData);
                Debug.Log($"[MapManager] Đã xóa bản đồ: '{mapData.mapName}' và lưu dữ liệu thành công!");

                RefreshMapListUI();
                StartCoroutine(InitSnapRoutine());
            }
        }

        public void StartGameOnMap(MapData mapData)
        {
            if (mapData == null) return;

            if (homePanel != null) homePanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);
            if (editPanel != null) editPanel.gameObject.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(true);

            if (mountainManager == null)
            {
                mountainManager = FindFirstObjectByType<MountainStationManager>();
                if (mountainManager == null)
                {
                    Canvas canvas = FindFirstObjectByType<Canvas>();
                    GameObject obj = new GameObject("MountainStationManager", typeof(RectTransform));
                    if (canvas != null) obj.transform.SetParent(canvas.transform, false);
                    mountainManager = obj.AddComponent<MountainStationManager>();
                }
            }

            if (climbBtn != null)
            {
                climbBtn.gameObject.SetActive(true);
                climbBtn.interactable = true;
            }

            if (stationQuiz != null)
            {
                stationQuiz.InitForMap(mapData);
                stationQuiz.OnStationOpenStateChanged -= HandleStationOpenStateChanged;
                stationQuiz.OnStationOpenStateChanged += HandleStationOpenStateChanged;
            }

            // Khởi tạo 5 tim (HP) khi bắt đầu màn chơi
            if (naturalDisaster != null)
            {
                naturalDisaster.InitHp(5);
            }

            mountainManager.OnStationReached -= HandleStationReached;
            mountainManager.OnStationReached += HandleStationReached;

            mountainManager.OpenMountain(mapData, onBack: () =>
            {
                OnPauseButtonClicked();
            }, onSave: () =>
            {
                QuizSaveSystem.Save(saveData);
            });

            Debug.Log($"[MapManager] Bắt đầu chơi: {mapData.mapName} (Số trạm: {mapData.stationCount})");
        }

        private void HandleStationReached(int stationIndex)
        {
            // Điểm 0 là Chân núi (xuất phát), điểm cuối cùng là Đỉnh núi
            // Chỉ hiển thị Join trạm câu hỏi khi ở các trạm giữa (từ Trạm 1 đến trước Đỉnh núi)
            if (mountainManager != null && mountainManager.CurrentMap != null && mountainManager.CurrentMap.stationPositions != null)
            {
                int totalPoints = mountainManager.CurrentMap.stationPositions.Count;
                if (stationIndex <= 0 || stationIndex >= totalPoints - 1)
                {
                    return;
                }
            }

            if (stationQuiz != null)
            {
                stationQuiz.ShowJoinPrompt(stationIndex);
            }
        }

        public void OnPauseButtonClicked()
        {
            if (stationQuiz != null)
            {
                stationQuiz.HideAllStationPanels();
            }

            if (climbBtn != null)
            {
                climbBtn.gameObject.SetActive(true);
                climbBtn.interactable = true;
            }

            if (mountainManager != null)
            {
                mountainManager.OnStationReached -= HandleStationReached;
            }

            if (gamePanel != null) gamePanel.SetActive(false);
            if (mountainManager != null) mountainManager.CloseMountain(triggerCallback: false);

            if (mapPanel == null)
            {
                Transform t = FindChild(transform, "MapManagerPanel", "MapPanel", "LevelPanel");
                if (t != null) mapPanel = t.gameObject;
            }

            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
            }
            else
            {
                if (homePanel != null) homePanel.SetActive(true);
            }

            // Đảm bảo Canvas không bị tắt nhầm
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null && !canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(true);
            }

            RefreshMapListUI();
            StartCoroutine(InitSnapRoutine());
            Debug.Log("[MapManager] Đã bấm PauseBtn: Trở về MapManagerPanel thành công!");
        }

        private void HandleStationOpenStateChanged(bool isOpen)
        {
            if (climbBtn != null)
            {
                // Khi trạm đang mở (StationPanel hoặc QuestPanel) -> Ẩn nút Climb để người chơi không thể vừa làm Quiz vừa leo núi
                climbBtn.gameObject.SetActive(!isOpen);
            }
        }

        public void OnClimbButtonClicked()
        {
            if (mountainManager == null)
            {
                Debug.Log("[MapManager] Bấm ClimbBtn: Không tìm thấy MountainStationManager.");
                return;
            }

            // Nếu người chơi đang leo núi -> Không cho bấm tiếp
            if (mountainManager.IsPlayerMoving)
            {
                return;
            }

            // Nếu trạm đang mở hoặc đang làm câu hỏi -> Không cho bấm Climb
            if (stationQuiz != null && stationQuiz.IsStationOpen)
            {
                Debug.LogWarning("[MapManager] Đang ở trong trạm/làm câu hỏi, không thể bấm Climb!");
                return;
            }

            if (naturalDisaster != null && !naturalDisaster.IsAlive)
            {
                Debug.LogWarning("[MapManager] Người chơi đã hết HP (0 tim)! Không thể leo núi tiếp.");
                return;
            }

            // 1. Ẩn ngay nút Join khi bấm Climb để không thể bấm Join khi đang leo
            if (stationQuiz != null)
            {
                stationQuiz.HideJoinPrompt();
            }

            // 2. Khóa tạm thời nút Climb trong lúc đang leo
            if (climbBtn != null)
            {
                climbBtn.interactable = false;
            }

            // Kiểm tra xem lần leo này có xuất hiện thiên tai ở mốc giữa chặng (50%) hay không
            bool willTriggerDisaster = (naturalDisaster != null && naturalDisaster.RollDisasterChance());

            // Trình tự: Nhân vật bước đi trước -> hiện rõ màn đen -> nếu có thiên tai thì dừng ở giữa & chọn item -> gần tới trạm tiếp theo thì mở màn (mờ dần)
            mountainManager.MovePlayerToNextStation(
                onComplete:     () =>
                {
                    // Khi đã tới trạm tiếp theo an toàn -> Mở lại nút Climb
                    if (climbBtn != null)
                    {
                        climbBtn.interactable = true;
                    }
                },
                onNearComplete: () => CutSceneTransition.Open(),
                onDepart:       () => CutSceneTransition.Close(),
                onMidwayEvent:  willTriggerDisaster ? (onResume) => naturalDisaster.TriggerDisasterEvent(onResume) : null
            );
        }

        private System.Collections.IEnumerator InitSnapRoutine()
        {
            yield return null;
            if (snapScroll != null)
            {
                snapScroll.UpdatePadding();
                if (saveData.maps.Count == 0 && addMapBtn != null)
                {
                    snapScroll.SnapTo(addMapBtn.GetComponent<RectTransform>(), true);
                }
                else
                {
                    snapScroll.SnapToNearestChild(true);
                }
            }
        }

        private System.Collections.IEnumerator SnapToTargetRoutine(RectTransform target)
        {
            yield return null;
            if (snapScroll != null && target != null)
            {
                snapScroll.UpdatePadding();
                snapScroll.SnapTo(target, false);
            }
        }
    }
}

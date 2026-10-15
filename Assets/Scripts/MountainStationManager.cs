using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Điều phối chính hệ thống leo núi (Mountain Station Manager):
    /// - Quản lý liên kết các hệ thống con: MountainBackground, MountainTrailGenerator, MountainCameraController, MountainPlayerController.
    /// - Gắn vào GameManager hoặc GameObject trong scene.
    /// - Tự động phát hiện cấu trúc đối tượng: GameManager -> MountainBg -> TopPos / BottomPos.
    /// </summary>
    public class MountainStationManager : MonoBehaviour
    {
        [Header("--- 1. CÁC HỆ THỐNG CON (MODULAR SUB-SYSTEMS) ---")]
        [Tooltip("Hệ thống quản lý ảnh nền và điểm Start / End (BottomPos, TopPos)")]
        [SerializeField] private MountainBackground mountainBg;

        [Tooltip("Hệ thống sinh trạm và vẽ đường mòn LineRenderer")]
        [SerializeField] private MountainTrailGenerator trailGenerator;

        [Tooltip("Hệ thống điều khiển máy quay (Intro, Follow, Pan, Zoom)")]
        [SerializeField] private MountainCameraController cameraController;

        [Tooltip("Hệ thống nhân vật người chơi leo núi")]
        [SerializeField] private MountainPlayerController playerController;

        [Header("--- 2. CÀI ĐẶT CHUNG (GENERAL SETTINGS) ---")]
        [Tooltip("Luôn tạo mới ngẫu nhiên vị trí trạm mỗi lần bấm Play")]
        [SerializeField] private bool alwaysRandomOnPlay = true;

        [Header("--- 3. GIAO DIỆN PHỦ (UI OVERLAY) ---")]
        [Tooltip("Canvas hoặc GameObject chứa các nút điều hướng")]
        [SerializeField] private GameObject uiOverlay;

        [Tooltip("Text hiển thị tên bản đồ và số lượng trạm")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Nút quay lại danh sách bản đồ")]
        [SerializeField] private Button backBtn;

        [Tooltip("Nút tạo lại ngẫu nhiên con đường mới")]
        [SerializeField] private Button rerandomBtn;

        private MapData currentMap;
        private Action onBackCallback;
        private Action onSaveCallback;
        private int currentStationIndex = 0;

        public int CurrentStationIndex => currentStationIndex;
        public bool IsPlayerMoving => playerController != null && playerController.IsPlayerMoving;
        public event Action<int> OnStationReached;

        private void Awake()
        {
            AutoFindReferences();
            BindUIEvents();
        }

        private void OnValidate()
        {
            AutoFindReferences();
        }

        /// <summary>
        /// Tự động dò tìm các module con gắn trên cùng GameObject hoặc trong Hierarchy:
        /// Tìm MountainBg (chứa TopPos, BottomPos), TrailGenerator, CameraController, PlayerController
        /// </summary>
        public void AutoFindReferences()
        {
            // 1. Tìm MountainBackground (hỗ trợ GameManager -> MountainBg)
            if (mountainBg == null)
            {
                mountainBg = GetComponentInChildren<MountainBackground>(true);
                if (mountainBg == null)
                {
                    Transform bgT = FindChild(transform, "MountainBg", "Mountain", "Bg", "Background");
                    if (bgT != null)
                    {
                        mountainBg = bgT.GetComponent<MountainBackground>();
                        if (mountainBg == null) mountainBg = bgT.gameObject.AddComponent<MountainBackground>();
                    }
                }
                if (mountainBg == null) mountainBg = FindFirstObjectByType<MountainBackground>();
            }

            // 2. Tìm hoặc thêm TrailGenerator
            if (trailGenerator == null)
            {
                trailGenerator = GetComponent<MountainTrailGenerator>();
                if (trailGenerator == null) trailGenerator = GetComponentInChildren<MountainTrailGenerator>(true);
                if (trailGenerator == null) trailGenerator = gameObject.AddComponent<MountainTrailGenerator>();
            }
            if (trailGenerator != null)
            {
                Transform parentRoot = (mountainBg != null) ? mountainBg.transform : transform;
                trailGenerator.AutoFindReferences(parentRoot);
            }

            // 3. Tìm hoặc thêm CameraController
            if (cameraController == null)
            {
                cameraController = GetComponent<MountainCameraController>();
                if (cameraController == null) cameraController = GetComponentInChildren<MountainCameraController>(true);
                if (cameraController == null) cameraController = gameObject.AddComponent<MountainCameraController>();
            }
            if (cameraController != null)
            {
                cameraController.Init(mountainBg);
            }

            // 4. Tìm hoặc thêm PlayerController
            if (playerController == null)
            {
                playerController = GetComponent<MountainPlayerController>();
                if (playerController == null) playerController = GetComponentInChildren<MountainPlayerController>(true);
                if (playerController == null) playerController = gameObject.AddComponent<MountainPlayerController>();
            }

            EnsureUIHierarchy();
        }

        private Transform FindChild(Transform parent, params string[] names)
        {
            foreach (var n in names)
            {
                Transform t = parent.Find(n);
                if (t != null) return t;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                foreach (var n in names)
                {
                    if (child.name.Equals(n, StringComparison.OrdinalIgnoreCase)) return child;
                }
            }
            return null;
        }

        private void BindUIEvents()
        {
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(() => CloseMountain());
            }

            if (rerandomBtn != null)
            {
                rerandomBtn.onClick.RemoveAllListeners();
                rerandomBtn.onClick.AddListener(RegenerateMountainPath);
            }
        }

        /// <summary>
        /// Mở màn chơi leo núi với MapData được chọn
        /// </summary>
        public void OpenMountain(MapData map, Action onBack = null, Action onSave = null)
        {
            currentMap = map;
            onBackCallback = onBack;
            onSaveCallback = onSave;
            currentStationIndex = 0;

            AutoFindReferences();

            if (cameraController != null)
            {
                cameraController.SaveCameraState();
            }

            if (GetComponent<Canvas>() == null)
            {
                gameObject.SetActive(true);
            }

            if (uiOverlay != null && uiOverlay.GetComponent<Canvas>() == null)
            {
                uiOverlay.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = $"🏔️ {currentMap.mapName} ({currentMap.stationCount} Trạm Đến Đỉnh)";
            }

            // 1. Áp dụng ảnh nền Theme mapBg vào MountainBg
            if (mountainBg != null)
            {
                mountainBg.ApplyTheme(currentMap.themeId);
            }

            // 2. Tải hoặc sinh mới các trạm
            LoadOrGenerateStations();

            // 3. Tạo nhân vật người chơi xuất phát tại trạm đầu tiên (chân núi)
            SpawnOrUpdatePlayer();

            // 4. Kích hoạt hiệu ứng Camera Intro (Soi đường -> Zoom toàn cảnh -> Bay về Player)
            StartIntroCamera();
        }

        private void LoadOrGenerateStations()
        {
            if (currentMap == null) return;
            int count = Mathf.Max(2, currentMap.stationCount);

            bool hasValidPositions = (!alwaysRandomOnPlay && currentMap.stationPositions != null && currentMap.stationPositions.Count == count);

            // Kiểm tra xem vị trí mốc đỉnh núi (TopPos / BottomPos) có bị thay đổi do đổi scale hay không
            if (hasValidPositions && mountainBg != null && currentMap.stationPositions.Count >= 2)
            {
                mountainBg.GetStartAndEndPositions(out Vector3 curStart, out Vector3 curEnd);
                Vector2 savedStart = currentMap.stationPositions[0].ToVector2();
                Vector2 savedEnd = currentMap.stationPositions[currentMap.stationPositions.Count - 1].ToVector2();

                // Nếu bạn vừa scale mapBg khiến khoảng cách Start/End lệch > 0.5 đơn vị -> tự động tạo lại
                if (Vector2.Distance(savedStart, curStart) > 0.5f || Vector2.Distance(savedEnd, curEnd) > 0.5f)
                {
                    hasValidPositions = false;
                }
            }

            if (!hasValidPositions)
            {
                if (trailGenerator != null)
                {
                    trailGenerator.GenerateRandomStations(currentMap, mountainBg, onSaveCallback);
                }
            }

            Transform parentRoot = (mountainBg != null) ? mountainBg.transform : transform;
            if (trailGenerator != null)
            {
                trailGenerator.SpawnStations(currentMap, parentRoot);
            }
        }

        private void SpawnOrUpdatePlayer()
        {
            if (currentMap == null || currentMap.stationPositions == null || currentMap.stationPositions.Count == 0) return;

            currentStationIndex = Mathf.Clamp(currentStationIndex, 0, currentMap.stationPositions.Count - 1);
            Vector2 targetPos = currentMap.stationPositions[currentStationIndex].ToVector2();

            Transform parentRoot = (mountainBg != null) ? mountainBg.transform : transform;
            if (playerController != null)
            {
                playerController.SpawnOrUpdatePlayer(parentRoot, targetPos);
                if (cameraController != null)
                {
                    cameraController.SetPlayerTarget(playerController.PlayerTransform);
                }
            }
        }

        private void StartIntroCamera()
        {
            if (cameraController == null || currentMap == null || currentMap.stationPositions == null || currentMap.stationPositions.Count == 0)
            {
                OnStationReached?.Invoke(currentStationIndex);
                return;
            }

            Vector3[] waypoints = new Vector3[currentMap.stationPositions.Count];
            for (int i = 0; i < currentMap.stationPositions.Count; i++)
            {
                waypoints[i] = currentMap.stationPositions[i].ToVector2();
            }

            Vector3 playerPos = (playerController != null && playerController.CurrentPlayerObj != null)
                ? playerController.CurrentPlayerObj.transform.position
                : waypoints[0];

            float spreadX = (trailGenerator != null) ? trailGenerator.HorizontalSpreadX : 8f;

            cameraController.StartIntro(waypoints, playerPos, spreadX, () =>
            {
                OnStationReached?.Invoke(currentStationIndex);
            });
        }

        public void RegenerateMountainPath()
        {
            if (currentMap == null) return;
            currentStationIndex = 0;

            if (trailGenerator != null)
            {
                trailGenerator.GenerateRandomStations(currentMap, mountainBg, onSaveCallback);
                Transform parentRoot = (mountainBg != null) ? mountainBg.transform : transform;
                trailGenerator.SpawnStations(currentMap, parentRoot);
            }

            SpawnOrUpdatePlayer();
            StartIntroCamera();
        }

        /// <summary>
        /// Di chuyển nhân vật người chơi lên trạm tiếp theo dọc theo đường mòn
        /// </summary>
        public void MovePlayerToNextStation(Action onComplete = null)
        {
            if (IsPlayerMoving) return;
            if (currentMap == null || currentMap.stationPositions == null) return;

            if (currentStationIndex >= currentMap.stationPositions.Count - 1)
            {
                Debug.Log("[MountainStationManager] Người chơi đã đứng trên đỉnh núi cao nhất!");
                onComplete?.Invoke();
                return;
            }

            int fromIdx = currentStationIndex;
            int toIdx = currentStationIndex + 1;
            Vector3 targetStationPos = currentMap.stationPositions[toIdx].ToVector2();

            Vector3[] pathPoints = (trailGenerator != null) ? trailGenerator.CachedPathPoints : null;
            int curveRes = (trailGenerator != null) ? trailGenerator.CurveResolutionPerSegment : 12;

            if (playerController != null)
            {
                playerController.MovePlayerAlongPath(fromIdx, toIdx, pathPoints, targetStationPos, curveRes, () =>
                {
                    currentStationIndex = toIdx;
                    Debug.Log($"[MountainStationManager] Người chơi đã leo tới trạm {toIdx + 1} / {currentMap.stationPositions.Count}!");
                    OnStationReached?.Invoke(toIdx);
                    onComplete?.Invoke();
                });
            }
        }

        public void SkipIntroSequence()
        {
            if (cameraController == null) return;
            Vector3 pPos = (playerController != null && playerController.CurrentPlayerObj != null)
                ? playerController.CurrentPlayerObj.transform.position
                : Vector3.zero;

            cameraController.SkipIntro(pPos, () =>
            {
                OnStationReached?.Invoke(currentStationIndex);
            });
        }

        public void CloseMountain(bool triggerCallback = true)
        {
            if (cameraController != null)
            {
                cameraController.RestoreCameraState();
            }

            if (trailGenerator != null)
            {
                trailGenerator.ClearStations();
            }

            if (playerController != null)
            {
                playerController.ClearPlayer();
            }

            if (GetComponent<Canvas>() == null)
            {
                gameObject.SetActive(false);
            }

            if (uiOverlay != null && uiOverlay.GetComponent<Canvas>() == null)
            {
                uiOverlay.SetActive(false);
            }

            if (triggerCallback)
            {
                onBackCallback?.Invoke();
            }
            Debug.Log("[MountainStationManager] Đã đóng giao diện ngọn núi!");
        }

        private void EnsureUIHierarchy()
        {
            if (uiOverlay == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    uiOverlay = new GameObject("MountainUI_Overlay", typeof(RectTransform));
                    uiOverlay.transform.SetParent(canvas.transform, false);

                    RectTransform rt = uiOverlay.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                }
            }

            if (uiOverlay != null)
            {
                Transform uRoot = uiOverlay.transform;
                if (backBtn == null)
                {
                    Transform t = uRoot.Find("BackBtn");
                    if (t != null) backBtn = t.GetComponent<Button>();
                }
                if (titleText == null)
                {
                    Transform t = uRoot.Find("TitleText");
                    if (t != null) titleText = t.GetComponent<TMP_Text>();
                }
            }
        }
    }
}

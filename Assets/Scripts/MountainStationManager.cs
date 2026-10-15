using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Quản lý toàn bộ ngọn núi, đường mòn và các trạm (Station) trong không gian World Transform 2D
    /// </summary>
    public class MountainStationManager : MonoBehaviour
    {
        [Header("--- 1. CÀI ĐẶT NGỌN NÚI (MOUNTAIN WORLD TRANSFORM) ---")]
        [Tooltip("Ảnh ngọn núi to trong World Space (gắn SpriteRenderer)")]
        [SerializeField] private SpriteRenderer mountainSprite;

        [Tooltip("Transform gốc của ngọn núi (nếu không dùng SpriteRenderer)")]
        [SerializeField] private Transform mountainRoot;

        [Tooltip("Chiều rộng ngọn núi trong World units (dùng khi không có SpriteRenderer)")]
        [SerializeField] private float mountainWidth = 12f;

        [Tooltip("Chiều cao ngọn núi trong World units (dùng khi không có SpriteRenderer)")]
        [SerializeField] private float mountainHeight = 25f;

        [Tooltip("Khoảng cách cố định giữa 2 tầng trạm trên trục Y (mỗi trạm cao hơn trạm trước đúng khoảng này)")]
        [SerializeField] private float layerSpacingY = 2.2f;

        [Tooltip("Khoảng cách tối thiểu giữa 2 trạm kề nhau trên trục Y (độ giãn đứng)")]
        [SerializeField] private float minStationSpacingY = 1.6f;

        [Tooltip("Bề ngang tối đa để rải ngẫu nhiên tọa độ X cho các trạm (độc lập với background núi)")]
        [SerializeField] private float horizontalSpreadX = 8.0f;

        [Tooltip("Luôn tạo mới ngẫu nhiên vị trí trạm mỗi lần bấm Play thay vì load lại vị trí cũ")]
        [SerializeField] private bool alwaysRandomOnPlay = true;

        [Tooltip("Tỷ lệ lề an toàn 2 bên sườn núi (0.7 = 70% bề ngang núi để trạm không rớt ra ngoài)")]
        [Range(0.2f, 0.98f)]
        [SerializeField] private float widthMarginFactor = 0.88f;

        [Header("--- CẤU HÌNH THEME (THEME SETTINGS) ---")]
        [Tooltip("Danh sách ThemeSO dùng để tra cứu ảnh nền ngọn núi mapBg")]
        [SerializeField] private List<ThemeSO> availableThemes = new List<ThemeSO>();

        [Header("--- 2. PREFAB & KÍCH THƯỚC TRẠM (STATION SETTINGS) ---")]
        [Tooltip("Prefab của trạm trong World Space (nếu để trống sẽ tự tạo vòng tròn mặc định)")]
        [SerializeField] private GameObject stationPrefab;

        [Tooltip("Tỷ lệ kích thước trạm thường (Scale) - Mặc định 0.35 để gọn gàng trên núi")]
        [SerializeField] private float stationScale = 0.35f;

        [Tooltip("Tỷ lệ kích thước trạm đỉnh núi (Scale)")]
        [SerializeField] private float summitScale = 0.5f;

        [Header("--- 3. ĐƯỜNG MÒN NỐI TRẠM (TRAIL LINE RENDERER) ---")]
        [Tooltip("LineRenderer dùng để vẽ con đường mòn nối các trạm trong World Space")]
        [SerializeField] private LineRenderer pathLineRenderer;

        [Tooltip("Bật chế độ đường mòn uốn cong mềm mại thay vì đường thẳng")]
        [SerializeField] private bool useCurvedPath = true;

        [Tooltip("Độ phân giải làm mịn đường cong cho mỗi đoạn (số điểm nội suy càng cao càng cong mượt)")]
        [Range(4, 25)]
        [SerializeField] private int curveResolutionPerSegment = 12;

        [Tooltip("Độ rộng (độ dày) của nét vẽ đường mòn")]
        [SerializeField] private float trailWidth = 0.12f;

        [Header("--- 4. CAMERA ĐIỀU KHIỂN (2D PAN & ZOOM) ---")]
        [Tooltip("Camera chính dùng để ngắm và theo dõi ngọn núi")]
        [SerializeField] private Camera worldCamera;

        [Tooltip("Cho phép kéo chuột để di chuyển camera tự do (Mặc định: TẮT để camera luôn khoá theo Player)")]
        [SerializeField] private bool enableFreeCameraPan = false;

        [Tooltip("Tốc độ kéo chuột camera (Drag Speed)")]
        [SerializeField] private float dragSpeed = 1.2f;

        [Tooltip("Cho phép lăn chuột để phóng to/thu nhỏ (Mặc định: TẮT để chỉ tập trung xem spawn trạm)")]
        [SerializeField] private bool enableScrollZoom = false;

        [Tooltip("Tốc độ lăn chuột zoom gần/xa (khi enableScrollZoom = true)")]
        [SerializeField] private float zoomSpeed = 3f;

        [Tooltip("Kích thước Zoom tối thiểu và tối đa của Orthographic Camera")]
        [SerializeField] private Vector2 zoomLimits = new Vector2(4f, 16f);

        [Tooltip("Độ phóng camera mặc định khi mở màn chơi")]
        [SerializeField] private float defaultCameraSize = 7f;

        [Header("--- 5. GIAO DIỆN PHỦ (UI OVERLAY) ---")]
        [Tooltip("Canvas chứa các nút điều hướng")]
        [SerializeField] private GameObject uiOverlay;

        [Tooltip("Text hiển thị tên bản đồ và số lượng trạm")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Nút quay lại danh sách bản đồ")]
        [SerializeField] private Button backBtn;

        [Tooltip("Nút tạo lại ngẫu nhiên con đường mới")]
        [SerializeField] private Button rerandomBtn;

        [Header("--- 6. NHÂN VẬT NGƯỜI CHƠI (PLAYER CHARACTER) ---")]
        [Tooltip("Prefab nhân vật người chơi trong World Space (nếu trống sẽ tự sinh nhân vật leo núi mặc định)")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("Kích thước nhân vật người chơi (Scale) - Mặc định 0.28 để vừa vặn, không bị quá to")]
        [SerializeField] private float playerScale = 0.28f;

        [Tooltip("Thời gian nhân vật người chơi leo giữa 2 trạm (giây) - Mặc định 2.8s cho di chuyển cực chậm, điềm tĩnh)")]
        [SerializeField] private float playerClimbDuration = 2.8f;

        [Header("--- 7. HIỆU ỨNG INTRO & CAMERA BÁM THEO (CINEMATIC & FOLLOW) ---")]
        [Tooltip("Bật hiệu ứng máy quay soi đường và zoom toàn cảnh khi vừa mở màn chơi")]
        [SerializeField] private bool playIntroSequence = true;

        [Tooltip("Thời gian dừng chờ tại chân núi (Player) trước khi bắt đầu soi đường lên các trạm (giây)")]
        [SerializeField] private float baseStationDelay = 1.0f;

        [Tooltip("Thời gian máy quay lướt từ chân núi lên đỉnh núi (giây)")]
        [SerializeField] private float cameraPanToSummitDuration = 4.2f;

        [Tooltip("Thời gian dừng chờ tại đỉnh núi trước khi bắt đầu Zoom toàn cảnh (giây)")]
        [SerializeField] private float summitArrivalDelay = 1.0f;

        [Tooltip("Kích thước Camera (Orthographic Size) khi tập trung vào Player (Mặc định: 5.2 - góc nhìn gần)")]
        [SerializeField] private float camPlayerSize = 5.2f;

        [Tooltip("Kích thước Camera khi Zoom toàn cảnh ngọn núi (Mặc định: 12 - góc nhìn rộng)")]
        [SerializeField] private float camOverviewSize = 12f;

        [Tooltip("Thời gian chuyển đổi zoom toàn cảnh ngọn núi (giây) - Càng nhỏ càng nhanh")]
        [SerializeField] private float zoomOverviewSpeed = 0.55f;

        [Tooltip("Thời gian bay từ toàn cảnh trở về Player (giây) - Càng nhỏ càng nhanh")]
        [SerializeField] private float returnToPlayerSpeed = 0.65f;

        [Tooltip("Thời gian dừng ngắm toàn cảnh ngọn núi trong Intro (giây)")]
        [SerializeField] private float overviewHoldDuration = 0.5f;

        [Tooltip("Độ lệch trục Y của Camera so với Player (để nhìn thoáng đường đi phía trước)")]
        [SerializeField] private float playerCameraOffsetY = 0.4f;

        [Tooltip("Tốc độ Camera bám theo Player")]
        [SerializeField] private float cameraFollowSpeed = 4f;

        [Tooltip("Tự động quay về bám Player sau khi người chơi thả chuột kéo camera")]
        [SerializeField] private bool autoReturnToPlayerAfterPan = true;

        [Tooltip("Thời gian chờ (giây) sau khi ngừng kéo chuột để Camera quay về lại Player")]
        [SerializeField] private float returnToPlayerDelay = 1.8f;

        private MapData currentMap;
        private Action onBackCallback;
        private Action onSaveCallback;

        private List<GameObject> spawnedStations = new List<GameObject>();
        private GameObject currentPlayerObj;
        private int currentStationIndex = 0;
        private bool isPlayerMoving = false;

        public int CurrentStationIndex => currentStationIndex;
        public bool IsPlayerMoving => isPlayerMoving;
        public event Action<int> OnStationReached;

        private Vector3[] cachedPathPoints;
        private Coroutine introCoroutine;
        private bool isIntroPlaying = false;
        private bool isFollowingPlayer = true;
        private float lastPanTime = 0f;

        private bool isDraggingCamera = false;
        private Vector3 dragOriginMouse;
        private Vector3 dragOriginCam;

        private void Awake()
        {
            AutoFindReferences();
            BindUIEvents();
        }

        private void AutoFindReferences()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
                if (worldCamera == null) worldCamera = FindFirstObjectByType<Camera>();
            }

            if (mountainSprite == null)
            {
                mountainSprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (mountainRoot == null)
            {
                mountainRoot = (mountainSprite != null) ? mountainSprite.transform : transform;
            }

            if (pathLineRenderer == null)
            {
                pathLineRenderer = GetComponentInChildren<LineRenderer>();
                if (pathLineRenderer == null)
                {
                    GameObject lineObj = new GameObject("MountainTrail_LineRenderer", typeof(LineRenderer));
                    lineObj.transform.SetParent(mountainRoot, false);
                    pathLineRenderer = lineObj.GetComponent<LineRenderer>();
                    SetupDefaultLineRenderer(pathLineRenderer);
                }
            }
        }

        private void SetupDefaultLineRenderer(LineRenderer lr)
        {
            lr.startWidth = trailWidth;
            lr.endWidth = trailWidth;
            lr.numCornerVertices = 5;
            lr.numCapVertices = 5;
            lr.useWorldSpace = true;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0.9f, 0.6f, 0.85f);
            lr.endColor = new Color(1f, 0.95f, 0.8f, 0.95f);
            lr.sortingOrder = 1; // Nằm trên ảnh núi, dưới trạm
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

        private void Update()
        {
            if (isIntroPlaying)
            {
                // Luôn để camera chiếu trọn vẹn intro, không skip khi bấm vào màn hình
                UpdatePlayerIdleAnimation();
                return;
            }

            HandleCameraPanAndZoom();
            HandleCameraFollowPlayer();
            UpdatePlayerIdleAnimation();
        }

        private Vector3 savedCameraPos = new Vector3(0f, 0f, -10f);
        private float savedCameraSize = 6f;
        private bool hasSavedCameraState = false;

        /// <summary>
        /// Mở màn chơi leo núi với MapData được chọn
        /// </summary>
        public void OpenMountain(MapData map, Action onBack = null, Action onSave = null)
        {
            currentMap = map;
            onBackCallback = onBack;
            onSaveCallback = onSave;
            currentStationIndex = 0;

            if (worldCamera == null)
            {
                AutoFindReferences();
            }

            // Lưu lại vị trí và kích thước Camera trước khi vào núi để khi Pause quay về menu khôi phục lại
            if (worldCamera != null && !hasSavedCameraState)
            {
                savedCameraPos = worldCamera.transform.position;
                savedCameraSize = worldCamera.orthographic ? worldCamera.orthographicSize : 6f;
                hasSavedCameraState = true;
            }

            if (GetComponent<Canvas>() == null)
            {
                gameObject.SetActive(true);
            }

            if (uiOverlay != null && uiOverlay.GetComponent<Canvas>() == null)
            {
                uiOverlay.SetActive(true);
            }

            EnsureUIHierarchy();

            if (titleText != null)
            {
                titleText.text = $"🏔️ {currentMap.mapName} ({currentMap.stationCount} Trạm Đến Đỉnh)";
            }

            // Cập nhật Sprite ngọn núi theo ThemeSO (mapBg)
            ApplyThemeBackground(currentMap.themeId);

            // 1. Tải hoặc sinh mới các trạm ở dạng World Transform
            LoadOrGenerateStations();

            // 2. Tạo nhân vật người chơi xuất phát tại chân núi (trạm đầu tiên)
            SpawnOrUpdatePlayer();

            // Đặt Camera tại vị trí xuất phát ngay lập tức để chuẩn bị cho intro/follow
            if (worldCamera != null && currentPlayerObj != null)
            {
                float camZ = worldCamera.transform.position.z;
                worldCamera.transform.position = new Vector3(
                    currentPlayerObj.transform.position.x,
                    currentPlayerObj.transform.position.y + playerCameraOffsetY,
                    camZ
                );
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = camPlayerSize;
                }
            }

            // 3. Khởi chạy hiệu ứng máy quay điện ảnh (soi đường lên đỉnh -> zoom toàn cảnh -> về player + thu nhỏ cam)
            if (playIntroSequence)
            {
                if (introCoroutine != null) StopCoroutine(introCoroutine);
                introCoroutine = StartCoroutine(PlayMountainIntroSequence());
            }
            else
            {
                FinishIntroSequence();
            }
        }

        /// <summary>
        /// Đóng màn chơi núi, dọn dẹp và khôi phục camera về menu
        /// </summary>
        public void CloseMountain()
        {
            CloseMountain(true);
        }

        public void CloseMountain(bool triggerCallback)
        {
            // 0. Dừng mọi coroutine đang chạy (intro camera, leo núi)
            StopAllCoroutines();
            introCoroutine = null;
            isIntroPlaying = false;
            isPlayerMoving = false;

            // 1. Dọn dẹp trạm, nhân vật và đường mòn
            ClearOldStations();

            // 2. Khôi phục lại Camera về trạng thái ban đầu của menu
            if (worldCamera != null && hasSavedCameraState)
            {
                worldCamera.transform.position = savedCameraPos;
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = savedCameraSize;
                }
                hasSavedCameraState = false;
            }

            // 3. Chỉ tắt gameObject nếu script không nằm trên Canvas hoặc MapManager
            if (GetComponent<Canvas>() == null && GetComponent<MapManager>() == null)
            {
                gameObject.SetActive(false);
            }

            // 4. Chỉ tắt uiOverlay nếu uiOverlay KHÔNG PHẢI là Canvas hay panel chính
            if (uiOverlay != null)
            {
                if (uiOverlay.GetComponent<Canvas>() == null &&
                    uiOverlay.name != "Canvas" &&
                    uiOverlay.name != "MapManagerPanel" &&
                    uiOverlay.name != "GamePanel" &&
                    uiOverlay.name != "HomeMenuPanel")
                {
                    uiOverlay.SetActive(false);
                }
            }

            // 5. Kích hoạt callback đóng an toàn (triệt tiêu đệ quy lặp vô tận)
            if (triggerCallback && onBackCallback != null)
            {
                Action cb = onBackCallback;
                onBackCallback = null; // Reset trước khi invoke để chống vòng lặp đệ quy!
                cb.Invoke();
            }
            else
            {
                onBackCallback = null;
            }
        }

        /// <summary>
        /// Tải các trạm từ tọa độ World Transform đã lưu trong MapData.
        /// Nếu chưa từng lưu hoặc số trạm thay đổi -> tự động Random ngoằn ngoèo và LƯU lại vào MapData & JSON!
        /// </summary>
        private void LoadOrGenerateStations()
        {
            ClearOldStations();

            if (currentMap == null) return;
            int count = Mathf.Max(2, currentMap.stationCount);

            // Kiểm tra xem đã có dữ liệu tọa độ World Transform lưu sẵn hay chưa
            bool hasValidSavedPositions = (!alwaysRandomOnPlay && currentMap.stationPositions != null && currentMap.stationPositions.Count == count);

            if (!hasValidSavedPositions)
            {
                // Tự động sinh ngẫu nhiên các trạm
                GenerateAndSaveRandomPath(count);
            }

            // Tạo các GameObject trạm tại vị trí World Transform đã tính toán
            SpawnStationsFromSavedData();
        }

        /// <summary>
        /// Sinh ngẫu nhiên con đường mòn uốn lượn từ chân núi lên đỉnh núi với các trạm dạt xa trái, phải, trung tâm
        /// </summary>
        public void GenerateAndSaveRandomPath(int count)
        {
            count = Mathf.Max(1, count);
            GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);

            if (currentMap.stationPositions == null)
            {
                currentMap.stationPositions = new List<StationPos>();
            }
            currentMap.stationPositions.Clear();

            if (count == 1)
            {
                // Nếu chỉ có 1 trạm thì đặt tại đỉnh núi
                currentMap.stationPositions.Add(new StationPos(centerX, maxY));
            }
            else
            {
                // 1. Khoảng cách cố định giữa 2 tầng: Mỗi tầng cách tầng trước đúng layerSpacingY (không ngẫu nhiên Y)
                float effectiveStepY = Mathf.Max(0.5f, layerSpacingY);

                // 2. Biên độ trục X: Dùng horizontalSpreadX độc lập với background núi
                float halfSpreadX = (horizontalSpreadX > 0f) ? (horizontalSpreadX * 0.5f) : (halfWidth * widthMarginFactor);

                for (int i = 0; i < count; i++)
                {
                    // 1. Tọa độ Y: Cố định theo khoảng cách tầng layerSpacingY
                    float y = minY + (i * effectiveStepY);

                    // 2. Tọa độ X: Hoàn toàn ngẫu nhiên tự do trong bề ngang X [-halfSpreadX, +halfSpreadX]
                    // Không tuân theo bất kỳ luật làn đường hay ràng buộc nào
                    float x = centerX + UnityEngine.Random.Range(-halfSpreadX, halfSpreadX);

                    currentMap.stationPositions.Add(new StationPos(x, y));
                }
            }

            // LƯU NGAY VÀO FILE SAVE JSON ĐỂ DUY TRÌ TỌA ĐỘ NÀY
            if (onSaveCallback != null)
            {
                onSaveCallback.Invoke();
            }
            else
            {
                GameSaveData currentSave = QuizSaveSystem.Load();
                for (int m = 0; m < currentSave.maps.Count; m++)
                {
                    if (currentSave.maps[m].id == currentMap.id)
                    {
                        currentSave.maps[m].stationPositions = currentMap.stationPositions;
                        break;
                    }
                }
                QuizSaveSystem.Save(currentSave);
            }
            Debug.Log($"[MountainStationManager] Đã random tự do và lưu {count} trạm cho map '{currentMap.mapName}'!");
        }

        /// <summary>
        /// Tạo các Prefab Station đúng ngay tại các vị trí World Transform đã lưu
        /// </summary>
        private void SpawnStationsFromSavedData()
        {
            if (currentMap == null || currentMap.stationPositions == null) return;

            int total = currentMap.stationPositions.Count;
            Vector3[] pathPoints = new Vector3[total];

            for (int i = 0; i < total; i++)
            {
                Vector2 pos = currentMap.stationPositions[i].ToVector2();
                bool isSummit = (i == total - 1);
                bool isBase = (i == 0);

                GameObject stationObj = null;
                float scale = isSummit ? summitScale : stationScale;

                if (stationPrefab != null)
                {
                    stationObj = Instantiate(stationPrefab, mountainRoot);
                    stationObj.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    stationObj = CreateDefaultWorldStation(i, isSummit, isBase);
                    stationObj.transform.SetParent(mountainRoot, false);
                }

                // ĐẶT TỌA ĐỘ WORLD TRANSFORM
                stationObj.transform.position = new Vector3(pos.x, pos.y, 0f);
                stationObj.name = isSummit ? $"Summit_Station_{i + 1}" : $"Station_Layer_{i + 1}";

                // Gắn và thiết lập StationWorldItem (Chỉ hiển thị mốc tầng, không click)
                StationWorldItem item = stationObj.GetComponent<StationWorldItem>();
                if (item == null) item = stationObj.AddComponent<StationWorldItem>();
                item.Setup(i, isSummit, isBase);

                spawnedStations.Add(stationObj);
                pathPoints[i] = new Vector3(pos.x, pos.y, 0f);
            }

            // Cập nhật đường nối LineRenderer trong World Space (uốn cong hoặc thẳng)
            if (pathLineRenderer != null)
            {
                pathLineRenderer.startWidth = trailWidth;
                pathLineRenderer.endWidth = trailWidth;
                pathLineRenderer.numCornerVertices = 5;
                pathLineRenderer.numCapVertices = 5;

                Vector3[] finalLinePoints = (useCurvedPath && total >= 2)
                    ? GenerateCurvedPath(pathPoints, curveResolutionPerSegment)
                    : pathPoints;

                cachedPathPoints = finalLinePoints; // Lưu lại để Intro Camera và Player dùng chung đường di chuyển mượt mà

                pathLineRenderer.positionCount = finalLinePoints.Length;
                pathLineRenderer.SetPositions(finalLinePoints);
            }
        }

        private GameObject CreateDefaultWorldStation(int index, bool isSummit, bool isBase)
        {
            GameObject obj = new GameObject($"Station_{index + 1}", typeof(SpriteRenderer));
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 2; // Nằm trên ảnh núi và LineRenderer

            // Tạo Sprite hình tròn cơ bản
            sr.sprite = CreateCircleSprite(48);
            float scale = isSummit ? summitScale : stationScale;
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            // Text hiển thị nhãn số trạm
            GameObject textObj = new GameObject("StationLabel", typeof(TextMeshPro));
            textObj.transform.SetParent(obj.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);

            TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = isSummit ? 2.4f : 1.8f;
            tmp.sortingOrder = 3;
            tmp.color = Color.white;

            return obj;
        }

        /// <summary>
        /// Tạo đường cong Catmull-Rom mượt mà đi qua tất cả các điểm mốc (waypoints)
        /// </summary>
        private Vector3[] GenerateCurvedPath(Vector3[] waypoints, int resolutionPerSegment)
        {
            if (waypoints == null || waypoints.Length == 0) return new Vector3[0];
            if (waypoints.Length < 2 || resolutionPerSegment <= 1) return waypoints;

            List<Vector3> curvedPoints = new List<Vector3>();
            int count = waypoints.Length;

            for (int i = 0; i < count - 1; i++)
            {
                // 4 điểm kiểm soát cho Catmull-Rom: P0, P1, P2, P3
                Vector3 p0 = (i == 0) ? (waypoints[0] + (waypoints[0] - waypoints[1])) : waypoints[i - 1];
                Vector3 p1 = waypoints[i];
                Vector3 p2 = waypoints[i + 1];
                Vector3 p3 = (i + 2 < count) ? waypoints[i + 2] : (waypoints[count - 1] + (waypoints[count - 1] - waypoints[count - 2]));

                for (int step = 0; step < resolutionPerSegment; step++)
                {
                    float t = (float)step / resolutionPerSegment;
                    curvedPoints.Add(EvaluateCatmullRom(p0, p1, p2, p3, t));
                }
            }

            // Thêm điểm mốc cuối cùng (Đỉnh núi)
            curvedPoints.Add(waypoints[count - 1]);
            return curvedPoints.ToArray();
        }

        private Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + (b * t) + (c * t2) + (d * t3));
        }

        private Sprite CreateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Bấm nút tạo lại đường đi (Re-random)
        /// </summary>
        public void RegenerateMountainPath()
        {
            if (currentMap == null) return;
            currentStationIndex = 0;
            GenerateAndSaveRandomPath(currentMap.stationCount);
            SpawnStationsFromSavedData();
            SpawnOrUpdatePlayer();

            if (playIntroSequence)
            {
                if (introCoroutine != null) StopCoroutine(introCoroutine);
                introCoroutine = StartCoroutine(PlayMountainIntroSequence());
            }
            else
            {
                FinishIntroSequence();
            }
        }

        private ThemeSO FindThemeById(string themeId)
        {
            if (availableThemes == null || availableThemes.Count == 0)
            {
                ThemeSO[] loaded = Resources.FindObjectsOfTypeAll<ThemeSO>();
                if (loaded != null && loaded.Length > 0)
                {
                    if (availableThemes == null) availableThemes = new List<ThemeSO>();
                    availableThemes.AddRange(loaded);
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

            if (availableThemes != null && !string.IsNullOrEmpty(themeId))
            {
                return availableThemes.Find(t => t != null && t.themeId == themeId);
            }

            return null;
        }

        private void ApplyThemeBackground(string themeId)
        {
            if (mountainSprite == null)
            {
                mountainSprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (mountainSprite != null)
            {
                ThemeSO theme = FindThemeById(themeId);
                if (theme != null && theme.mapBg != null)
                {
                    mountainSprite.sprite = theme.mapBg;
                    Debug.Log($"[MountainStationManager] Đã cập nhật ảnh ngọn núi (mountainSprite) theo Theme: '{theme.themeName}'");
                }
            }
        }

        /// <summary>
        /// Lấy thông số chiều cao và bề ngang của ngọn núi trong World Space
        /// </summary>
        public void GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth)
        {
            if (mountainSprite != null && mountainSprite.sprite != null)
            {
                Bounds b = mountainSprite.bounds;
                minY = b.min.y + 0.5f; // Giảm bớt padding chân núi để mở rộng không gian
                maxY = b.max.y - 0.5f; // Giảm bớt padding đỉnh núi để mở rộng không gian
                centerX = b.center.x;
                halfWidth = b.extents.x;
            }
            else
            {
                Vector3 rootPos = mountainRoot != null ? mountainRoot.position : Vector3.zero;
                minY = rootPos.y - (mountainHeight / 2f) + 1f;
                maxY = rootPos.y + (mountainHeight / 2f) - 1f;
                centerX = rootPos.x;
                halfWidth = mountainWidth / 2f;
            }
        }

        /// <summary>
        /// Xử lý kéo chuột để di chuyển camera dọc ngọn núi và cuộn chuột để zoom
        /// </summary>
        private void HandleCameraPanAndZoom()
        {
            if (worldCamera == null || !gameObject.activeInHierarchy) return;

            // 1. Kéo rê chuột để di chuyển (Pan Camera) - Chỉ cho phép khi enableFreeCameraPan = true
            if (enableFreeCameraPan)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    isDraggingCamera = true;
                    isFollowingPlayer = false; // Tạm dừng follow để người chơi tự do khám phá góc nhìn
                    lastPanTime = Time.time;
                    dragOriginMouse = Input.mousePosition;
                    dragOriginCam = worldCamera.transform.position;
                }

                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                {
                    if (isDraggingCamera)
                    {
                        Vector3 diff = Input.mousePosition - dragOriginMouse;
                        float factor = (worldCamera.orthographicSize / Screen.height) * 2f;
                        Vector3 newPos = dragOriginCam - new Vector3(diff.x * factor, diff.y * factor, 0f);

                        GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);
                        newPos.y = Mathf.Clamp(newPos.y, minY - 2f, maxY + 2f);
                        newPos.x = Mathf.Clamp(newPos.x, centerX - halfWidth, centerX + halfWidth);
                        newPos.z = worldCamera.transform.position.z;

                        worldCamera.transform.position = newPos;
                        lastPanTime = Time.time;
                    }
                }
                else
                {
                    if (isDraggingCamera)
                    {
                        isDraggingCamera = false;
                        lastPanTime = Time.time;
                    }
                }
            }

            // 2. Lăn chuột để phóng to / thu nhỏ (Zoom) - Chỉ kích hoạt nếu enableScrollZoom = true
            if (enableScrollZoom && worldCamera.orthographic)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    worldCamera.orthographicSize = Mathf.Clamp(
                        worldCamera.orthographicSize - scroll * zoomSpeed,
                        zoomLimits.x,
                        zoomLimits.y
                    );
                }
            }
        }

        /// <summary>
        /// Camera bám theo vị trí của nhân vật người chơi với độ mượt lerp
        /// </summary>
        private void HandleCameraFollowPlayer()
        {
            if (worldCamera == null || currentPlayerObj == null) return;

            // Nếu người chơi thả chuột sau một khoảng thời gian -> tự động kích hoạt lại chế độ follow Player
            if (!isFollowingPlayer && autoReturnToPlayerAfterPan && !isDraggingCamera)
            {
                if (Time.time - lastPanTime > returnToPlayerDelay)
                {
                    isFollowingPlayer = true;
                }
            }

            if (isFollowingPlayer)
            {
                Vector3 playerPos = currentPlayerObj.transform.position;
                float targetX = playerPos.x;
                float targetY = playerPos.y + playerCameraOffsetY;

                GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);
                targetX = Mathf.Clamp(targetX, centerX - halfWidth * 0.9f, centerX + halfWidth * 0.9f);
                targetY = Mathf.Clamp(targetY, minY - 1f, maxY + 1f);

                Vector3 targetCamPos = new Vector3(targetX, targetY, worldCamera.transform.position.z);

                // Di chuyển mượt mà tới vị trí người chơi
                worldCamera.transform.position = Vector3.Lerp(
                    worldCamera.transform.position,
                    targetCamPos,
                    Time.deltaTime * cameraFollowSpeed
                );

                // Thu nhỏ camera về kích thước focus vào người chơi
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = Mathf.Lerp(
                        worldCamera.orthographicSize,
                        camPlayerSize,
                        Time.deltaTime * (cameraFollowSpeed * 0.8f)
                    );
                }
            }
        }

        /// <summary>
        /// Hiệu ứng máy quay khi vừa mở màn chơi:
        /// Di chuyển mượt mà từ chân núi lên đỉnh núi (lerp chậm dần lúc start và lúc end - Ease In Out)
        /// </summary>
        private IEnumerator PlayMountainIntroSequence()
        {
            if (worldCamera == null || currentMap == null || currentMap.stationPositions == null || currentMap.stationPositions.Count == 0)
            {
                FinishIntroSequence();
                yield break;
            }

            isIntroPlaying = true;
            isFollowingPlayer = false;

            float camZ = worldCamera.transform.position.z;
            Vector3 playerPos = (currentPlayerObj != null)
                ? currentPlayerObj.transform.position
                : new Vector3(currentMap.stationPositions[0].x, currentMap.stationPositions[0].y, 0f);

            // Đặt Camera tại chân núi tập trung vào Player lúc bắt đầu
            worldCamera.transform.position = new Vector3(playerPos.x, playerPos.y + playerCameraOffsetY, camZ);
            if (worldCamera.orthographic)
            {
                worldCamera.orthographicSize = camPlayerSize;
            }

            // Dừng nhẹ 1 nhịp ở chân núi trước khi lướt
            yield return new WaitForSeconds(baseStationDelay);

            // DI CHUYỂN 1 LINE THẲNG TỪ ĐẦU CHÂN NÚI THẲNG TỚI ĐỈNH NÚI:
            // Sử dụng đường cong Smootherstep để:
            // - Xuất phát cực kỳ êm ái, tăng tốc chậm (Soft Start - Ease In)
            // - Gần tới đỉnh núi hãm phanh chậm dần đều (Soft End - Ease Out)
            float panDuration = Mathf.Max(2.0f, cameraPanToSummitDuration);
            float elapsed = 0f;

            Vector3 summitPos = new Vector3(
                currentMap.stationPositions[currentMap.stationPositions.Count - 1].x,
                currentMap.stationPositions[currentMap.stationPositions.Count - 1].y,
                0f
            );

            Vector3 startCamPos = worldCamera.transform.position;
            Vector3 endCamPos = new Vector3(summitPos.x, summitPos.y + playerCameraOffsetY, camZ);

            while (elapsed < panDuration)
            {
                if (!isIntroPlaying) yield break;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / panDuration);

                // Smootherstep (6x^5 - 15x^4 + 10x^3): Chậm êm ái ở start và hãm phanh chậm dần ở end
                float t = SmoothStepCubic(normalizedTime);

                // Di chuyển 1 đường thẳng trực tiếp từ chân núi tới đỉnh núi
                worldCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, t);

                yield return null;
            }

            // Dừng lại ở đỉnh núi 1 nhịp ngắm cảnh
            yield return new WaitForSeconds(summitArrivalDelay);

            // GIAI ĐOẠN 2: CHIẾU FULL MAP (BỨT TỐC NHANH 70% ĐẦU, HÃM PHANH MỀM MẠI 30% CUỐI)
            float zoomOutDuration = Mathf.Max(0.7f, zoomOverviewSpeed);
            float zoomOutElapsed = 0f;
            Vector3 summitCamPos = worldCamera.transform.position;

            GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);

            float actualMinY = minY;
            float actualMaxY = maxY;
            float minStationX = centerX;
            float maxStationX = centerX;

            if (currentMap.stationPositions != null && currentMap.stationPositions.Count > 0)
            {
                actualMinY = currentMap.stationPositions[0].y;
                actualMaxY = currentMap.stationPositions[currentMap.stationPositions.Count - 1].y;

                minStationX = currentMap.stationPositions[0].x;
                maxStationX = currentMap.stationPositions[0].x;
                for (int s = 1; s < currentMap.stationPositions.Count; s++)
                {
                    float sx = currentMap.stationPositions[s].x;
                    if (sx < minStationX) minStationX = sx;
                    if (sx > maxStationX) maxStationX = sx;
                }
            }

            float actualCenterY = (actualMinY + actualMaxY) * 0.5f;
            float actualHeight = Mathf.Max(maxY - minY, actualMaxY - actualMinY);
            float currentSpreadX = (horizontalSpreadX > 0f) ? horizontalSpreadX : (halfWidth * 2f);
            float actualSpreadX = Mathf.Max(currentSpreadX, (maxStationX - minStationX) + 4f);

            Vector3 overviewCamPos = new Vector3(centerX, actualCenterY, camZ);
            float startSize = worldCamera.orthographic ? worldCamera.orthographicSize : camPlayerSize;

            // Camera size bao trùm cả chiều cao và chiều ngang
            float camAspect = (worldCamera != null && worldCamera.aspect > 0.05f) ? worldCamera.aspect : (9f / 16f);
            float sizeRequiredByX = (actualSpreadX * 0.5f) / camAspect;
            float sizeRequiredByY = actualHeight * 0.58f;
            float targetOverviewSize = Mathf.Max(camOverviewSize, Mathf.Max(sizeRequiredByX, sizeRequiredByY));

            while (zoomOutElapsed < zoomOutDuration)
            {
                if (!isIntroPlaying) yield break;
                zoomOutElapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(zoomOutElapsed / zoomOutDuration);
                // Đường cong Ease-Out mạnh: bứt tốc nhanh 70% đầu chặng và hãm phanh êm ái 30% cuối chặng
                float t = FastSnapEaseOut(normalizedTime);

                worldCamera.transform.position = Vector3.Lerp(summitCamPos, overviewCamPos, t);
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = Mathf.Lerp(startSize, targetOverviewSize, t);
                }
                yield return null;
            }

            // Dừng ngắm toàn cảnh ngọn núi
            yield return new WaitForSeconds(overviewHoldDuration);

            // GIAI ĐOẠN 3: QUAY VỀ LẠI PLAYER (BỨT TỐC TỨC THÌ 70% ĐẦU, HÃM PHANH MƯỢT MÀ 30% CUỐI)
            float returnDuration = Mathf.Max(0.6f, returnToPlayerSpeed);
            float returnElapsed = 0f;
            Vector3 returnStartPos = worldCamera.transform.position;
            float returnStartSize = worldCamera.orthographic ? worldCamera.orthographicSize : targetOverviewSize;

            while (returnElapsed < returnDuration)
            {
                if (!isIntroPlaying) yield break;
                returnElapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(returnElapsed / returnDuration);
                // Đường cong Ease-Out mạnh: bứt tốc nhanh 70% đầu chặng và hãm phanh êm ái 30% cuối chặng
                float t = FastSnapEaseOut(normalizedTime);

                Vector3 currentTargetPlayerPos = (currentPlayerObj != null)
                    ? currentPlayerObj.transform.position
                    : playerPos;
                Vector3 targetCamPos = new Vector3(currentTargetPlayerPos.x, currentTargetPlayerPos.y + playerCameraOffsetY, camZ);

                worldCamera.transform.position = Vector3.Lerp(returnStartPos, targetCamPos, t);
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = Mathf.Lerp(returnStartSize, camPlayerSize, t);
                }
                yield return null;
            }

            // Kết thúc Intro và kích hoạt Camera điều khiển / theo dõi người chơi
            FinishIntroSequence();
        }

        /// <summary>
        /// Đường cong chuyển động tức thì có kiểm soát (Fast Snap & Smooth Deceleration):
        /// Đạt khoảng 70-80% quãng đường cực nhanh ở phần đầu, sau đó hãm phanh chậm dần êm ái ở 20-30% cuối.
        /// </summary>
        private float FastSnapEaseOut(float x)
        {
            x = Mathf.Clamp01(x);
            // Ease-Out Quartic / Quintic: 1 - (1 - x)^4
            float inv = 1f - x;
            return 1f - (inv * inv * inv * inv);
        }

        /// <summary>
        /// Hàm nội suy đường cong mượt mà (SmootherStep / Cubic Ease):
        /// Khởi hành êm, lướt đều và hãm phanh chậm dần ở cuối chặng
        /// </summary>
        private float SmoothStepCubic(float x)
        {
            x = Mathf.Clamp01(x);
            // Ken Perlin's Smootherstep: 6x^5 - 15x^4 + 10x^3 (độ mượt cao, hãm phanh cực êm cả đầu và cuối)
            return x * x * x * (x * (x * 6f - 15f) + 10f);
        }

        public void SkipIntroSequence()
        {
            if (!isIntroPlaying) return;
            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
                introCoroutine = null;
            }
            FinishIntroSequence();
        }

        private void FinishIntroSequence()
        {
            isIntroPlaying = false;
            isFollowingPlayer = true;
            lastPanTime = 0f;

            if (currentPlayerObj != null && worldCamera != null)
            {
                float camZ = worldCamera.transform.position.z;
                worldCamera.transform.position = new Vector3(
                    currentPlayerObj.transform.position.x,
                    currentPlayerObj.transform.position.y + playerCameraOffsetY,
                    camZ
                );
                if (worldCamera.orthographic)
                {
                    worldCamera.orthographicSize = camPlayerSize;
                }
            }

            OnStationReached?.Invoke(currentStationIndex);
        }

        /// <summary>
        /// Tạo hoặc cập nhật vị trí nhân vật người chơi ở trạm hiện tại
        /// </summary>
        public void SpawnOrUpdatePlayer()
        {
            if (currentMap == null || currentMap.stationPositions == null || currentMap.stationPositions.Count == 0) return;

            currentStationIndex = Mathf.Clamp(currentStationIndex, 0, currentMap.stationPositions.Count - 1);
            Vector2 targetPos = currentMap.stationPositions[currentStationIndex].ToVector2();

            if (currentPlayerObj == null)
            {
                if (playerPrefab != null)
                {
                    currentPlayerObj = Instantiate(playerPrefab, mountainRoot);
                }
                else
                {
                    currentPlayerObj = CreateDefaultPlayerObject();
                    currentPlayerObj.transform.SetParent(mountainRoot, false);
                }
                currentPlayerObj.name = "Player_Climber";
            }

            currentPlayerObj.transform.localScale = new Vector3(playerScale, playerScale, 1f);
            currentPlayerObj.transform.position = new Vector3(targetPos.x, targetPos.y + 0.22f, -0.1f);
            currentPlayerObj.SetActive(true);
        }

        private GameObject CreateDefaultPlayerObject()
        {
            GameObject root = new GameObject("Player_Climber");

            // 1. Bóng đổ (Shadow)
            GameObject shadow = new GameObject("Shadow", typeof(SpriteRenderer));
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(0f, -0.28f, 0.05f);
            shadow.transform.localScale = new Vector3(0.55f, 0.22f, 1f);
            SpriteRenderer srShadow = shadow.GetComponent<SpriteRenderer>();
            srShadow.sprite = CreateCircleSprite(32);
            srShadow.color = new Color(0f, 0f, 0f, 0.35f);
            srShadow.sortingOrder = 8;

            // 2. Visual holder (chứa badge và icon để làm animation thở / nhún)
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            // Badge tròn nền màu cam năng động
            GameObject token = new GameObject("TokenBadge", typeof(SpriteRenderer));
            token.transform.SetParent(visual.transform, false);
            SpriteRenderer srToken = token.GetComponent<SpriteRenderer>();
            srToken.sprite = CreateCircleSprite(64);
            srToken.color = new Color(1f, 0.45f, 0.1f, 1f); // Vibrant orange climber badge
            srToken.sortingOrder = 9;

            // Viền sáng vàng kim nổi bật
            GameObject ring = new GameObject("GoldenRing", typeof(SpriteRenderer));
            ring.transform.SetParent(visual.transform, false);
            ring.transform.localScale = new Vector3(1.18f, 1.18f, 1f);
            SpriteRenderer srRing = ring.GetComponent<SpriteRenderer>();
            srRing.sprite = CreateRingSprite(64, 6);
            srRing.color = new Color(1f, 0.88f, 0.3f, 0.95f);
            srRing.sortingOrder = 10;

            // Chữ "P" đại diện cho Player (Người chơi leo núi)
            GameObject iconObj = new GameObject("PlayerLabel", typeof(TextMeshPro));
            iconObj.transform.SetParent(visual.transform, false);
            iconObj.transform.localPosition = new Vector3(0f, 0.04f, -0.05f);
            TextMeshPro tmp = iconObj.GetComponent<TextMeshPro>();
            tmp.text = "P";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 2.4f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.sortingOrder = 11;

            return root;
        }

        private Sprite CreateRingSprite(int size, int thickness)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;
            float innerRadius = radius - thickness;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius && dist >= innerRadius)
                    {
                        float outerAlpha = Mathf.Clamp01(radius - dist + 0.5f);
                        float innerAlpha = Mathf.Clamp01(dist - innerRadius + 0.5f);
                        float alpha = Mathf.Min(outerAlpha, innerAlpha);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void UpdatePlayerIdleAnimation()
        {
            if (currentPlayerObj == null || isPlayerMoving) return;

            Transform visualChild = currentPlayerObj.transform.Find("Visual");
            if (visualChild != null)
            {
                float bob = Mathf.Sin(Time.time * 3.5f) * 0.05f;
                visualChild.localPosition = new Vector3(0f, bob, 0f);
            }
        }

        /// <summary>
        /// Di chuyển nhân vật người chơi lên trạm tiếp theo dọc theo đường mòn
        /// </summary>
        public void MovePlayerToNextStation(Action onComplete = null)
        {
            if (isPlayerMoving) return;
            if (currentMap == null || currentMap.stationPositions == null) return;

            if (currentStationIndex >= currentMap.stationPositions.Count - 1)
            {
                Debug.Log("[MountainStationManager] Người chơi đã đứng trên đỉnh núi cao nhất!");
                onComplete?.Invoke();
                return;
            }

            int nextIndex = currentStationIndex + 1;
            StartCoroutine(MovePlayerAlongTrailRoutine(currentStationIndex, nextIndex, onComplete));
        }

        private IEnumerator MovePlayerAlongTrailRoutine(int fromIndex, int toIndex, Action onComplete)
        {
            isPlayerMoving = true;
            isFollowingPlayer = true; // Đảm bảo camera tự động theo sát người chơi khi leo

            if (currentPlayerObj == null)
            {
                SpawnOrUpdatePlayer();
            }

            int segPoints = Mathf.Max(2, curveResolutionPerSegment);
            int startIdx = fromIndex * segPoints;
            int endIdx = toIndex * segPoints;

            bool canUsePathPoints = (cachedPathPoints != null && cachedPathPoints.Length > endIdx);

            float climbDuration = Mathf.Max(0.5f, playerClimbDuration);
            float elapsed = 0f;

            Vector3 startPos = currentPlayerObj.transform.position;
            Vector3 endStationPos = currentMap.stationPositions[toIndex].ToVector2();
            Vector3 targetFinalPos = new Vector3(endStationPos.x, endStationPos.y + 0.22f, -0.1f);

            while (elapsed < climbDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / climbDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 currentTrailPos;
                if (canUsePathPoints)
                {
                    float exactPathIdx = Mathf.Lerp(startIdx, endIdx, smoothT);
                    int idxA = Mathf.FloorToInt(exactPathIdx);
                    int idxB = Mathf.Min(idxA + 1, endIdx);
                    float frac = exactPathIdx - idxA;
                    currentTrailPos = Vector3.Lerp(cachedPathPoints[idxA], cachedPathPoints[idxB], frac);
                }
                else
                {
                    currentTrailPos = Vector3.Lerp(startPos, targetFinalPos, smoothT);
                }

                // Hiệu ứng nhún nhảy bước chân khi leo
                float hop = Mathf.Sin(t * Mathf.PI * 4f) * 0.08f;
                currentPlayerObj.transform.position = new Vector3(currentTrailPos.x, currentTrailPos.y + 0.22f + hop, -0.1f);

                yield return null;
            }

            currentPlayerObj.transform.position = targetFinalPos;
            currentStationIndex = toIndex;
            isPlayerMoving = false;

            Debug.Log($"[MountainStationManager] Người chơi đã leo tới trạm {toIndex + 1} / {currentMap.stationPositions.Count}!");
            OnStationReached?.Invoke(toIndex);
            onComplete?.Invoke();
        }

        private void ResetCameraToBase()
        {
            if (worldCamera == null) return;
            GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);
            worldCamera.transform.position = new Vector3(centerX, minY + 3f, worldCamera.transform.position.z);
            if (worldCamera.orthographic)
            {
                worldCamera.orthographicSize = defaultCameraSize;
            }
        }

        private void ClearOldStations()
        {
            if (currentPlayerObj != null)
            {
                Destroy(currentPlayerObj);
                currentPlayerObj = null;
            }

            foreach (var st in spawnedStations)
            {
                if (st != null) Destroy(st);
            }
            spawnedStations.Clear();

            cachedPathPoints = null;

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
            }
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

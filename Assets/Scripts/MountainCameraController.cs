using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGame
{
    /// <summary>
    /// Điều khiển Camera 2D: Intro máy quay soi đường chân núi lên đỉnh núi,
    /// phóng toàn cảnh (Fast Snap 70% + Decelerate 30%), quay về Player,
    /// bám theo nhân vật (Follow Player) và kéo pan / zoom camera tự do.
    /// </summary>
    public class MountainCameraController : MonoBehaviour
    {
        [Header("--- 1. CẤU HÌNH CAMERA ---")]
        [Tooltip("Camera chính dùng để ngắm và theo dõi ngọn núi")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("Độ lệch trục Y của Camera so với Player (nhìn thoáng phía trước)")]
        [SerializeField] private float playerCameraOffsetY = 0.4f;

        [Tooltip("Tốc độ Camera bám theo Player")]
        [SerializeField] private float cameraFollowSpeed = 4f;

        [Tooltip("Kích thước Camera (Orthographic Size) khi tập trung vào Player")]
        [SerializeField] private float camPlayerSize = 5.2f;

        [Tooltip("Kích thước Camera khi Zoom toàn cảnh ngọn núi")]
        [SerializeField] private float camOverviewSize = 12f;

        [Header("--- 2. HIỆU ỨNG INTRO CINEMATIC ---")]
        [Tooltip("Bật hiệu ứng máy quay soi đường và zoom toàn cảnh khi mở màn")]
        [SerializeField] private bool playIntroSequence = true;

        [Tooltip("Thời gian dừng chờ tại chân núi trước khi lướt lên đỉnh (giây)")]
        [SerializeField] private float baseStationDelay = 1.0f;

        [Tooltip("Thời gian máy quay lướt từ chân núi lên đỉnh núi (giây)")]
        [SerializeField] private float cameraPanToSummitDuration = 4.2f;

        [Tooltip("Thời gian dừng chờ tại đỉnh núi trước khi bắt đầu Zoom toàn cảnh (giây)")]
        [SerializeField] private float summitArrivalDelay = 1.0f;

        [Tooltip("Thời gian chuyển đổi zoom toàn cảnh ngọn núi (giây)")]
        [SerializeField] private float zoomOverviewSpeed = 0.55f;

        [Tooltip("Thời gian dừng ngắm toàn cảnh ngọn núi trong Intro (giây)")]
        [SerializeField] private float overviewHoldDuration = 0.5f;

        [Tooltip("Thời gian bay từ toàn cảnh trở về Player (giây)")]
        [SerializeField] private float returnToPlayerSpeed = 0.65f;

        [Header("--- 3. TƯƠNG TÁC TỰ DO (PAN & ZOOM) ---")]
        [Tooltip("Cho phép kéo chuột để di chuyển camera tự do")]
        [SerializeField] private bool enableFreeCameraPan = false;

        [Tooltip("Tốc độ kéo chuột camera")]
        [SerializeField] private float dragSpeed = 1.2f;

        [Tooltip("Cho phép lăn chuột để phóng to/thu nhỏ")]
        [SerializeField] private bool enableScrollZoom = false;

        [Tooltip("Tốc độ lăn chuột zoom")]
        [SerializeField] private float zoomSpeed = 3f;

        [Tooltip("Kích thước Zoom tối thiểu và tối đa")]
        [SerializeField] private Vector2 zoomLimits = new Vector2(4f, 16f);

        [Tooltip("Tự động quay về bám Player sau khi ngừng kéo chuột")]
        [SerializeField] private bool autoReturnToPlayerAfterPan = true;

        [Tooltip("Thời gian chờ (giây) sau khi ngừng kéo chuột để Camera quay về lại Player")]
        [SerializeField] private float returnToPlayerDelay = 1.8f;

        public Camera TargetCamera => targetCamera;
        public float CamPlayerSize => camPlayerSize;
        public float PlayerCameraOffsetY => playerCameraOffsetY;
        public bool IsIntroPlaying => isIntroPlaying;

        private MountainBackground mountainBg;
        private Transform playerTarget;
        private Coroutine introCoroutine;
        private bool isIntroPlaying = false;
        private bool isFollowingPlayer = true;
        private float lastPanTime = 0f;

        private bool isDraggingCamera = false;
        private Vector3 dragOriginMouse;
        private Vector3 dragOriginCam;

        private Vector3 savedCameraPos = new Vector3(0f, 0f, -10f);
        private float savedCameraSize = 6f;
        private bool hasSavedCameraState = false;

        public void Init(MountainBackground bg)
        {
            mountainBg = bg;
            AutoFindReferences();
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        private void Awake()
        {
            AutoFindReferences();
        }

        public void AutoFindReferences()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) targetCamera = FindFirstObjectByType<Camera>();
            }

            if (mountainBg == null)
            {
                mountainBg = FindFirstObjectByType<MountainBackground>();
            }
        }

        private void Update()
        {
            if (isIntroPlaying) return;

            HandleCameraPanAndZoom();
            HandleCameraFollowPlayer();
        }

        public void SaveCameraState()
        {
            AutoFindReferences();
            if (targetCamera != null && !hasSavedCameraState)
            {
                savedCameraPos = targetCamera.transform.position;
                savedCameraSize = targetCamera.orthographic ? targetCamera.orthographicSize : 6f;
                hasSavedCameraState = true;
            }
        }

        public void RestoreCameraState()
        {
            if (targetCamera != null && hasSavedCameraState)
            {
                targetCamera.transform.position = savedCameraPos;
                if (targetCamera.orthographic) targetCamera.orthographicSize = savedCameraSize;
                hasSavedCameraState = false;
            }
        }

        public void SnapToPlayerImmediate(Vector3 playerPos)
        {
            if (targetCamera == null) return;
            float camZ = targetCamera.transform.position.z;
            targetCamera.transform.position = new Vector3(playerPos.x, playerPos.y + playerCameraOffsetY, camZ);
            if (targetCamera.orthographic)
            {
                targetCamera.orthographicSize = camPlayerSize;
            }
        }

        public void StartIntro(Vector3[] waypoints, Vector3 playerPos, float horizontalSpreadX, Action onIntroComplete)
        {
            if (introCoroutine != null) StopCoroutine(introCoroutine);
            if (playIntroSequence)
            {
                introCoroutine = StartCoroutine(PlayMountainIntroRoutine(waypoints, playerPos, horizontalSpreadX, onIntroComplete));
            }
            else
            {
                SnapToPlayerImmediate(playerPos);
                isIntroPlaying = false;
                isFollowingPlayer = true;
                onIntroComplete?.Invoke();
            }
        }

        public void SkipIntro(Vector3 playerPos, Action onIntroComplete)
        {
            if (!isIntroPlaying) return;
            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
                introCoroutine = null;
            }
            isIntroPlaying = false;
            isFollowingPlayer = true;
            SnapToPlayerImmediate(playerPos);
            onIntroComplete?.Invoke();
        }

        private IEnumerator PlayMountainIntroRoutine(Vector3[] waypoints, Vector3 playerPos, float horizontalSpreadX, Action onComplete)
        {
            if (targetCamera == null || waypoints == null || waypoints.Length == 0)
            {
                isIntroPlaying = false;
                isFollowingPlayer = true;
                onComplete?.Invoke();
                yield break;
            }

            isIntroPlaying = true;
            isFollowingPlayer = false;

            float camZ = targetCamera.transform.position.z;
            SnapToPlayerImmediate(playerPos);

            yield return new WaitForSeconds(baseStationDelay);

            // Giai đoạn 1: Lướt mượt mà từ chân núi lên đỉnh núi
            float panDuration = Mathf.Max(2.0f, cameraPanToSummitDuration);
            float elapsed = 0f;

            Vector3 summitPos = waypoints[waypoints.Length - 1];
            Vector3 startCamPos = targetCamera.transform.position;
            Vector3 endCamPos = new Vector3(summitPos.x, summitPos.y + playerCameraOffsetY, camZ);

            while (elapsed < panDuration)
            {
                if (!isIntroPlaying) yield break;
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / panDuration);
                float t = SmoothStepCubic(normalized);
                targetCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, t);
                yield return null;
            }

            yield return new WaitForSeconds(summitArrivalDelay);

            // Giai đoạn 2: Chiếu toàn cảnh (Fast Snap 70% + Decelerate 30%)
            float zoomOutDuration = Mathf.Max(0.7f, zoomOverviewSpeed);
            float zoomOutElapsed = 0f;
            Vector3 summitCamPos = targetCamera.transform.position;

            float minY = waypoints[0].y;
            float maxY = waypoints[waypoints.Length - 1].y;
            float centerX = (waypoints[0].x + waypoints[waypoints.Length - 1].x) * 0.5f;
            float halfWidth = 6f;

            if (mountainBg != null)
            {
                mountainBg.GetMountainBounds(out minY, out maxY, out centerX, out halfWidth);
            }

            float minStationX = centerX;
            float maxStationX = centerX;
            for (int s = 0; s < waypoints.Length; s++)
            {
                if (waypoints[s].x < minStationX) minStationX = waypoints[s].x;
                if (waypoints[s].x > maxStationX) maxStationX = waypoints[s].x;
            }

            float actualCenterY = (waypoints[0].y + waypoints[waypoints.Length - 1].y) * 0.5f;
            float actualHeight = Mathf.Max(maxY - minY, waypoints[waypoints.Length - 1].y - waypoints[0].y);
            float currentSpreadX = (horizontalSpreadX > 0f) ? horizontalSpreadX : (halfWidth * 2f);
            float actualSpreadX = Mathf.Max(currentSpreadX, (maxStationX - minStationX) + 4f);

            Vector3 overviewCamPos = new Vector3(centerX, actualCenterY, camZ);
            float startSize = targetCamera.orthographic ? targetCamera.orthographicSize : camPlayerSize;

            float camAspect = (targetCamera.aspect > 0.05f) ? targetCamera.aspect : (9f / 16f);
            float sizeRequiredByX = (actualSpreadX * 0.5f) / camAspect;
            float sizeRequiredByY = actualHeight * 0.58f;
            float targetOverviewSize = Mathf.Max(camOverviewSize, Mathf.Max(sizeRequiredByX, sizeRequiredByY));

            while (zoomOutElapsed < zoomOutDuration)
            {
                if (!isIntroPlaying) yield break;
                zoomOutElapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(zoomOutElapsed / zoomOutDuration);
                float t = FastSnapEaseOut(normalized);

                targetCamera.transform.position = Vector3.Lerp(summitCamPos, overviewCamPos, t);
                if (targetCamera.orthographic)
                {
                    targetCamera.orthographicSize = Mathf.Lerp(startSize, targetOverviewSize, t);
                }
                yield return null;
            }

            yield return new WaitForSeconds(overviewHoldDuration);

            // Giai đoạn 3: Quay về lại Player (Fast Snap 70% + Decelerate 30%)
            float returnDuration = Mathf.Max(0.6f, returnToPlayerSpeed);
            float returnElapsed = 0f;
            Vector3 returnStartPos = targetCamera.transform.position;
            float returnStartSize = targetCamera.orthographic ? targetCamera.orthographicSize : targetOverviewSize;

            while (returnElapsed < returnDuration)
            {
                if (!isIntroPlaying) yield break;
                returnElapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(returnElapsed / returnDuration);
                float t = FastSnapEaseOut(normalized);

                Vector3 currentTargetPlayerPos = (playerTarget != null) ? playerTarget.position : playerPos;
                Vector3 targetCamPos = new Vector3(currentTargetPlayerPos.x, currentTargetPlayerPos.y + playerCameraOffsetY, camZ);

                targetCamera.transform.position = Vector3.Lerp(returnStartPos, targetCamPos, t);
                if (targetCamera.orthographic)
                {
                    targetCamera.orthographicSize = Mathf.Lerp(returnStartSize, camPlayerSize, t);
                }
                yield return null;
            }

            isIntroPlaying = false;
            isFollowingPlayer = true;
            onComplete?.Invoke();
        }

        private float FastSnapEaseOut(float x)
        {
            x = Mathf.Clamp01(x);
            float inv = 1f - x;
            return 1f - (inv * inv * inv * inv);
        }

        private float SmoothStepCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x * (x * (x * 6f - 15f) + 10f);
        }

        private void HandleCameraPanAndZoom()
        {
            if (targetCamera == null || !gameObject.activeInHierarchy) return;

            if (enableFreeCameraPan)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    isDraggingCamera = true;
                    isFollowingPlayer = false;
                    lastPanTime = Time.time;
                    dragOriginMouse = Input.mousePosition;
                    dragOriginCam = targetCamera.transform.position;
                }

                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                {
                    if (isDraggingCamera)
                    {
                        Vector3 diff = Input.mousePosition - dragOriginMouse;
                        float factor = (targetCamera.orthographicSize / Screen.height) * 2f;
                        Vector3 newPos = dragOriginCam - new Vector3(diff.x * factor, diff.y * factor, 0f);

                        if (mountainBg != null)
                        {
                            mountainBg.GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);
                            newPos.y = Mathf.Clamp(newPos.y, minY - 2f, maxY + 2f);
                            newPos.x = Mathf.Clamp(newPos.x, centerX - halfWidth, centerX + halfWidth);
                        }
                        newPos.z = targetCamera.transform.position.z;
                        targetCamera.transform.position = newPos;
                        lastPanTime = Time.time;
                    }
                }
                else if (isDraggingCamera)
                {
                    isDraggingCamera = false;
                    lastPanTime = Time.time;
                }
            }

            if (enableScrollZoom && targetCamera.orthographic)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    targetCamera.orthographicSize = Mathf.Clamp(
                        targetCamera.orthographicSize - scroll * zoomSpeed,
                        zoomLimits.x,
                        zoomLimits.y
                    );
                }
            }
        }

        private void HandleCameraFollowPlayer()
        {
            if (targetCamera == null || playerTarget == null) return;

            if (!isFollowingPlayer && autoReturnToPlayerAfterPan && !isDraggingCamera)
            {
                if (Time.time - lastPanTime > returnToPlayerDelay)
                {
                    isFollowingPlayer = true;
                }
            }

            if (isFollowingPlayer)
            {
                Vector3 playerPos = playerTarget.position;
                float targetX = playerPos.x;
                float targetY = playerPos.y + playerCameraOffsetY;

                if (mountainBg != null)
                {
                    mountainBg.GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);
                    targetX = Mathf.Clamp(targetX, centerX - halfWidth * 0.9f, centerX + halfWidth * 0.9f);
                    targetY = Mathf.Clamp(targetY, minY - 1f, maxY + 1f);
                }

                Vector3 targetCamPos = new Vector3(targetX, targetY, targetCamera.transform.position.z);
                targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, targetCamPos, Time.deltaTime * cameraFollowSpeed);

                if (targetCamera.orthographic)
                {
                    targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, camPlayerSize, Time.deltaTime * (cameraFollowSpeed * 0.8f));
                }
            }
        }
    }
}

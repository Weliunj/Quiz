using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace QuizGame
{
    /// <summary>
    /// Quản lý nhân vật người chơi leo núi (Player):
    /// Tạo visual badge cam vàng rực rỡ, animation thở/nhún khi đứng yên (Idle),
    /// và hoạt ảnh di chuyển nhún nhảy dọc theo đường mòn uốn lượn khi leo trạm.
    /// </summary>
    public class MountainPlayerController : MonoBehaviour
    {
        [Header("--- 1. CẤU HÌNH NHÂN VẬT ---")]
        [Tooltip("Prefab nhân vật người chơi (nếu để trống sẽ tự sinh visual badge mặc định)")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("Kích thước nhân vật người chơi (Scale)")]
        [SerializeField] private float playerScale = 0.28f;

        [Tooltip("Thời gian nhân vật người chơi leo giữa 2 trạm (giây)")]
        [SerializeField] private float playerClimbDuration = 2.8f;

        [Tooltip("Độ lệch trục Y của nhân vật so với mốc trạm")]
        [SerializeField] private float yOffset = 0.22f;

        [Header("--- 2. HIỆU ỨNG CHUYỂN ĐỘNG / NHÚN NHẢY (BOBBING ANIMATION) ---")]
        [Tooltip("Bật/tắt hiệu ứng icon nhún nhảy lên xuống khi đứng yên tại trạm (Idle)")]
        [SerializeField] private bool enableIdleBobbing = true;

        [Tooltip("Tốc độ chuyển động lên xuống khi đứng yên (tần số nhún)")]
        [SerializeField] private float idleBobSpeed = 3.5f;

        [Tooltip("Biên độ (độ cao) chuyển động lên xuống khi đứng yên")]
        [SerializeField] private float idleBobHeight = 0.12f;

        [Tooltip("Độ nảy (bước chân) khi đang di chuyển leo giữa 2 trạm")]
        [SerializeField] private float moveHopHeight = 0.08f;

        [Tooltip("Hiển thị bóng đổ dưới chân nhân vật")]
        [SerializeField] private bool showShadow = true;

        private GameObject currentPlayerObj;
        private bool isPlayerMoving = false;
        private Vector3 currentBasePos;

        public GameObject CurrentPlayerObj => currentPlayerObj;
        public bool IsPlayerMoving => isPlayerMoving;
        public Transform PlayerTransform => (currentPlayerObj != null) ? currentPlayerObj.transform : transform;

        private void Update()
        {
            UpdatePlayerIdleAnimation();
        }

        public void SpawnOrUpdatePlayer(Transform parentRoot, Vector3 targetPos)
        {
            currentBasePos = new Vector3(targetPos.x, targetPos.y + yOffset, -0.1f);

            if (currentPlayerObj == null)
            {
                if (playerPrefab != null)
                {
                    // Tạo root object cho Player để camera bám theo vị trí cố định của trạm
                    currentPlayerObj = new GameObject("Player_Climber");
                    currentPlayerObj.transform.SetParent(parentRoot, false);

                    if (showShadow)
                    {
                        CreateShadowObject(currentPlayerObj.transform, 1);
                    }

                    // Tạo container Visual để chứa Icon và thực hiện hoạt ảnh nhún nhảy
                    GameObject visual = new GameObject("Visual");
                    visual.transform.SetParent(currentPlayerObj.transform, false);

                    GameObject iconInstance = Instantiate(playerPrefab, visual.transform);
                    iconInstance.name = playerPrefab.name;
                    iconInstance.transform.localPosition = Vector3.zero;
                    iconInstance.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    currentPlayerObj = CreateDefaultPlayerObject();
                    currentPlayerObj.transform.SetParent(parentRoot, false);
                }
            }

            currentPlayerObj.transform.localScale = new Vector3(playerScale, playerScale, 1f);
            currentPlayerObj.transform.position = currentBasePos;

            Transform visualChild = currentPlayerObj.transform.Find("Visual");
            if (visualChild != null)
            {
                visualChild.localPosition = Vector3.zero;
            }

            currentPlayerObj.SetActive(true);
        }

        public void ClearPlayer()
        {
            if (currentPlayerObj != null)
            {
                Destroy(currentPlayerObj);
                currentPlayerObj = null;
            }
            isPlayerMoving = false;
        }

        [Header("--- 2. CHUYỂN CẢNH TRÊN ĐƯỜNG LEO ---")]
        [Tooltip("Tỷ lệ quãng đường đi được trước khi kích hoạt che màn (0.10 = vừa nhấc chân đi 10% chặng đường là xuất hiện màn đen ngay)")]
        [Range(0.02f, 0.4f)]
        [SerializeField] private float departThreshold = 0.10f;

        [Tooltip("Tỷ lệ quãng đường khi gần tới trạm tiếp theo để mở màn (0.78 = tới 78% chặng đường thì mở dần lộ scene)")]
        [Range(0.5f, 0.95f)]
        [SerializeField] private float nearThreshold = 0.78f;

        public void MovePlayerAlongPath(
            int fromIndex, 
            int toIndex, 
            Vector3[] cachedPathPoints, 
            Vector3 targetStationPos, 
            int curveResolution, 
            Action onComplete, 
            Action onNearComplete = null, 
            float customNearThreshold = -1f,
            Action onDepart = null,
            float customDepartThreshold = -1f,
            Action<Action> onMidwayEvent = null)
        {
            if (isPlayerMoving) return;
            float finalDepartThresh = (customDepartThreshold >= 0f) ? customDepartThreshold : departThreshold;
            float finalNearThresh   = (customNearThreshold >= 0f)   ? customNearThreshold   : nearThreshold;

            StartCoroutine(MovePlayerAlongTrailRoutine(
                fromIndex, toIndex, cachedPathPoints, targetStationPos, curveResolution, 
                onComplete, onNearComplete, finalNearThresh, onDepart, finalDepartThresh, onMidwayEvent));
        }

        private IEnumerator MovePlayerAlongTrailRoutine(
            int fromIndex, 
            int toIndex, 
            Vector3[] cachedPathPoints, 
            Vector3 endStationPos, 
            int curveResolution, 
            Action onComplete, 
            Action onNearComplete, 
            float targetNearThreshold,
            Action onDepart,
            float targetDepartThreshold,
            Action<Action> onMidwayEvent = null)
        {
            isPlayerMoving = true;
            bool departFired = false;
            bool midwayFired = false;
            bool nearFired = false;

            int segPoints = Mathf.Max(2, curveResolution);
            int startIdx = fromIndex * segPoints;
            int endIdx = toIndex * segPoints;

            bool canUsePathPoints = (cachedPathPoints != null && cachedPathPoints.Length > endIdx);
            float climbDuration = Mathf.Max(0.5f, playerClimbDuration);
            float elapsed = 0f;

            Vector3 startPos = currentPlayerObj != null ? currentPlayerObj.transform.position : transform.position;
            Vector3 targetFinalPos = new Vector3(endStationPos.x, endStationPos.y + yOffset, -0.1f);

            while (elapsed < climbDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / climbDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // 1. Kích hoạt onDepart khi người chơi đã rời trạm cũ đi được 1 ít
                if (!departFired && t >= targetDepartThreshold)
                {
                    departFired = true;
                    onDepart?.Invoke();
                }

                // 2. Kích hoạt sự kiện giữa chặng (Thiên tai) nếu có
                if (!midwayFired && onMidwayEvent != null && t >= 0.5f)
                {
                    midwayFired = true;

                    // Đưa nhân vật đứng chính xác ở mốc giữa 2 trạm
                    Vector3 midTrailPos;
                    if (canUsePathPoints)
                    {
                        float midIdxFloat = Mathf.Lerp(startIdx, endIdx, 0.5f);
                        int mA = Mathf.FloorToInt(midIdxFloat);
                        int mB = Mathf.Min(mA + 1, endIdx);
                        float mFrac = midIdxFloat - mA;
                        midTrailPos = Vector3.Lerp(cachedPathPoints[mA], cachedPathPoints[mB], mFrac);
                    }
                    else
                    {
                        midTrailPos = Vector3.Lerp(startPos, targetFinalPos, 0.5f);
                    }

                    currentBasePos = new Vector3(midTrailPos.x, midTrailPos.y + yOffset, -0.1f);
                    if (currentPlayerObj != null)
                    {
                        currentPlayerObj.transform.position = currentBasePos;
                    }

                    bool resumeClimb = false;
                    onMidwayEvent?.Invoke(() => resumeClimb = true);

                    while (!resumeClimb)
                    {
                        yield return null;
                    }

                    // Reset lại mốc elapsed để tiếp tục di chuyển nửa chặng còn lại
                    elapsed = climbDuration * 0.5f;
                }

                // 3. Kích hoạt onNearComplete khi gần đến trạm kế tiếp
                if (!nearFired && t >= targetNearThreshold)
                {
                    nearFired = true;
                    onNearComplete?.Invoke();
                }

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
                float hop = Mathf.Sin(t * Mathf.PI * 4f) * moveHopHeight;
                if (currentPlayerObj != null)
                {
                    currentPlayerObj.transform.position = new Vector3(currentTrailPos.x, currentTrailPos.y + yOffset + hop, -0.1f);
                }

                yield return null;
            }

            if (!departFired)
            {
                departFired = true;
                onDepart?.Invoke();
            }
            if (!nearFired)
            {
                nearFired = true;
                onNearComplete?.Invoke();
            }

            currentBasePos = targetFinalPos;
            if (currentPlayerObj != null)
            {
                currentPlayerObj.transform.position = targetFinalPos;
                Transform visualChild = currentPlayerObj.transform.Find("Visual");
                if (visualChild != null)
                {
                    visualChild.localPosition = Vector3.zero;
                }
            }

            isPlayerMoving = false;
            onComplete?.Invoke();
        }

        private void UpdatePlayerIdleAnimation()
        {
            if (currentPlayerObj == null || isPlayerMoving || !enableIdleBobbing) return;

            float bob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight;

            Transform visualChild = currentPlayerObj.transform.Find("Visual");
            if (visualChild != null)
            {
                visualChild.localPosition = new Vector3(0f, bob, 0f);
            }
            else
            {
                // Fallback nếu không có child "Visual": di chuyển trực tiếp quanh mốc tọa độ gốc
                currentPlayerObj.transform.position = new Vector3(currentBasePos.x, currentBasePos.y + bob, currentBasePos.z);
            }
        }

        private void CreateShadowObject(Transform parent, int sortingOrder = 1)
        {
            GameObject shadow = new GameObject("Shadow", typeof(SpriteRenderer));
            shadow.transform.SetParent(parent, false);
            shadow.transform.localPosition = new Vector3(0f, -0.28f, 0.05f);
            shadow.transform.localScale = new Vector3(0.55f, 0.22f, 1f);
            SpriteRenderer srShadow = shadow.GetComponent<SpriteRenderer>();
            srShadow.sprite = CreateCircleSprite(32);
            srShadow.color = new Color(0f, 0f, 0f, 0.35f);
            srShadow.sortingOrder = sortingOrder;
        }

        private GameObject CreateDefaultPlayerObject()
        {
            GameObject root = new GameObject("Player_Climber");

            // 1. Bóng đổ (Shadow)
            if (showShadow)
            {
                CreateShadowObject(root.transform, 8);
            }

            // 2. Visual holder
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            // Badge tròn nền màu cam năng động
            GameObject token = new GameObject("TokenBadge", typeof(SpriteRenderer));
            token.transform.SetParent(visual.transform, false);
            SpriteRenderer srToken = token.GetComponent<SpriteRenderer>();
            srToken.sprite = CreateCircleSprite(64);
            srToken.color = new Color(1f, 0.45f, 0.1f, 1f);
            srToken.sortingOrder = 9;

            // Viền sáng vàng kim nổi bật
            GameObject ring = new GameObject("GoldenRing", typeof(SpriteRenderer));
            ring.transform.SetParent(visual.transform, false);
            ring.transform.localScale = new Vector3(1.18f, 1.18f, 1f);
            SpriteRenderer srRing = ring.GetComponent<SpriteRenderer>();
            srRing.sprite = CreateRingSprite(64, 6);
            srRing.color = new Color(1f, 0.88f, 0.3f, 0.95f);
            srRing.sortingOrder = 10;

            // Chữ "P" đại diện cho Player
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
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Hiệu ứng chuyển cảnh kiểu Fade (Hiện rõ / Mờ dần) dùng duy nhất 1 tấm màn đen.
    /// - Hiện rõ (Close / Fade In): Màn đen hiện dần lên che toàn màn hình.
    /// - Mờ dần (Open / Fade Out): Màn đen mờ dần để lộ cảnh game.
    /// 
    /// Gọi từ bất kỳ đâu qua các shortcut tĩnh:
    /// - CutSceneTransition.Close(onComplete)
    /// - CutSceneTransition.Open(onComplete)
    /// - CutSceneTransition.DelayedClose(delay, onComplete)
    /// - CutSceneTransition.CrossFade(midAction, onComplete)
    /// - CutSceneTransition.DelayedCrossFade(delay, midAction, onComplete)
    /// </summary>
    public class CutSceneTransition : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────────────
        public static CutSceneTransition Instance { get; private set; }

        // ─── Inspector ────────────────────────────────────────────────────────────
        [Header("--- PANEL & COMPONENTS ---")]
        [Tooltip("GameObject cha chứa màn chuyển cảnh (CutScreenPanel)")]
        [SerializeField] private GameObject cutScreenPanel;

        [Tooltip("CanvasGroup điều khiển độ mờ (tự tìm hoặc tự thêm nếu để trống)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Tấm hình đen duy nhất (tự tìm hoặc căn full screen)")]
        [SerializeField] private Image fadeImage;

        [Header("--- NHÂN VẬT BƯỚC ĐI (CUTSCENE ANIMATION) ---")]
        [Tooltip("GameObject hiển thị nhân vật bước đi giữa màn hình khi màn đen đang bật (CutScreenPanel/WalkingCharacter)")]
        [SerializeField] private GameObject walkingCharacter;

        [Header("--- TIMING ---")]
        [Tooltip("Thời gian hiện rõ dần (fade in - đóng màn che đen)")]
        [SerializeField] private float closeDuration = 0.22f;

        [Tooltip("Thời gian mờ dần (fade out - mở màn lộ scene)")]
        [SerializeField] private float openDuration = 0.35f;

        [Tooltip("Thời gian giữ màn đen ở giữa khi dùng CrossFade")]
        [SerializeField] private float holdDuration = 0.15f;

        [Header("--- STARTUP ---")]
        [Tooltip("Tự động mở màn (mờ dần) khi game khởi động")]
        [SerializeField] private bool openOnStart = true;

        [Tooltip("Delay (giây) trước khi tự mở màn lúc khởi động")]
        [SerializeField] private float autoOpenDelay = 0.1f;

        [Header("--- CURVE ---")]
        [Tooltip("Curve khi hiện rõ dần (Fade In - đóng màn)")]
        [SerializeField] private AnimationCurve closeEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Curve khi mờ dần (Fade Out - mở màn)")]
        [SerializeField] private AnimationCurve openEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float CloseDuration
        {
            get => closeDuration;
            set => closeDuration = Mathf.Max(0.01f, value);
        }

        public float OpenDuration
        {
            get => openDuration;
            set => openDuration = Mathf.Max(0.01f, value);
        }

        // ─── Private ──────────────────────────────────────────────────────────────
        private Coroutine _currentCoroutine;
        private bool _isClosed = false;

        // ─── Unity ───────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoFindReferences();
            InitPanel();
        }

        private void Start()
        {
            if (openOnStart)
            {
                SetClosedImmediate();
                if (autoOpenDelay > 0f)
                    StartCoroutine(CoDelayedOpen(autoOpenDelay));
                else
                    PlayOpen();
            }
        }

        private IEnumerator CoDelayedOpen(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            PlayOpen();
        }

        private void AutoFindReferences()
        {
            if (cutScreenPanel == null)
            {
                Transform t = transform.Find("CutScreenPanel");
                if (t == null) t = GameObject.Find("CutScreenPanel")?.transform;
                if (t != null) cutScreenPanel = t.gameObject;
            }

            if (cutScreenPanel != null)
            {
                // CanvasGroup
                if (canvasGroup == null)
                {
                    canvasGroup = cutScreenPanel.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                        canvasGroup = cutScreenPanel.AddComponent<CanvasGroup>();
                }

                // 1 tấm Image duy nhất
                if (fadeImage == null)
                {
                    // Thử tìm Image ngay trên cutScreenPanel
                    fadeImage = cutScreenPanel.GetComponent<Image>();

                    // Nếu không có, tìm tấm con LeftBlackImg hoặc bất kỳ Image con nào
                    if (fadeImage == null)
                    {
                        Transform left = cutScreenPanel.transform.Find("LeftBlackImg");
                        if (left != null) fadeImage = left.GetComponent<Image>();
                    }

                    if (fadeImage == null)
                    {
                        fadeImage = cutScreenPanel.GetComponentInChildren<Image>(true);
                    }
                }

                // Tìm walkingCharacter trong cutScreenPanel nếu chưa gán
                if (walkingCharacter == null)
                {
                    Transform charT = cutScreenPanel.transform.Find("WalkingCharacter");
                    if (charT == null) charT = cutScreenPanel.transform.Find("Character");
                    if (charT != null) walkingCharacter = charT.gameObject;
                }

                // Nếu còn tấm RightBlackImg cũ, tắt đi để chỉ dùng duy nhất 1 tấm
                Transform right = cutScreenPanel.transform.Find("RightBlackImg");
                if (right != null && (fadeImage == null || right.gameObject != fadeImage.gameObject))
                {
                    right.gameObject.SetActive(false);
                }
            }
        }

        private void InitPanel()
        {
            // Căn tấm fadeImage phủ full màn hình
            if (fadeImage != null)
            {
                RectTransform rt = fadeImage.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;

                // Đảm bảo alpha của ảnh gốc là 1 để CanvasGroup quản lý độ mờ
                Color c = fadeImage.color;
                c.a = 1f;
                fadeImage.color = c;

                fadeImage.gameObject.SetActive(true);
            }

            if (walkingCharacter != null)
            {
                walkingCharacter.SetActive(false);
            }

            // Mặc định ở trạng thái mở (tàng hình)
            SetAlpha(0f);
            if (cutScreenPanel != null)
            {
                cutScreenPanel.SetActive(false);
            }
        }

        // ─── Public Static Shortcuts ─────────────────────────────────────────────

        /// <summary>Hiện rõ màn đen dần dần (Đóng màn). Gọi onComplete khi đã che kín.</summary>
        public static void Close(Action onComplete = null, float duration = -1f)
        {
            if (Instance != null) Instance.PlayClose(onComplete, duration);
            else onComplete?.Invoke();
        }

        /// <summary>Mờ dần màn đen (Mở màn lộ scene). Gọi onComplete khi mờ hẳn.</summary>
        public static void Open(Action onComplete = null, float duration = -1f)
        {
            if (Instance != null) Instance.PlayOpen(onComplete, duration);
            else onComplete?.Invoke();
        }

        /// <summary>
        /// Hiện rõ (che đen) → chờ → chạy midAction → mờ dần (lộ scene).
        /// Dùng khi chuyển cảnh hoặc đổi dữ liệu.
        /// </summary>
        public static void CrossFade(Action midAction = null, Action onComplete = null)
        {
            if (Instance != null) Instance.PlayCrossFade(midAction, onComplete);
            else { midAction?.Invoke(); onComplete?.Invoke(); }
        }

        /// <summary>
        /// Chờ delay giây rồi CrossFade.
        /// </summary>
        public static void DelayedCrossFade(float delay, Action midAction = null, Action onComplete = null)
        {
            if (Instance != null) Instance.PlayDelayedCrossFade(delay, midAction, onComplete);
            else { midAction?.Invoke(); onComplete?.Invoke(); }
        }

        /// <summary>
        /// Chờ delay giây rồi hiện rõ màn đen (chỉ đóng, không tự mở).
        /// </summary>
        public static void DelayedClose(float delay, Action onComplete = null, float duration = -1f)
        {
            if (Instance != null) Instance.PlayDelayedClose(delay, onComplete, duration);
            else onComplete?.Invoke();
        }

        // ─── Public Instance Methods ──────────────────────────────────────────────

        public void PlayClose(Action onComplete = null, float duration = -1f)
        {
            StopCurrentTransition();
            _currentCoroutine = StartCoroutine(CoClose(onComplete, duration));
        }

        public void PlayOpen(Action onComplete = null, float duration = -1f)
        {
            StopCurrentTransition();
            _currentCoroutine = StartCoroutine(CoOpen(onComplete, duration));
        }

        public void PlayCrossFade(Action midAction = null, Action onComplete = null)
        {
            StopCurrentTransition();
            _currentCoroutine = StartCoroutine(CoCrossFade(midAction, onComplete));
        }

        public void PlayDelayedCrossFade(float delay, Action midAction = null, Action onComplete = null)
        {
            StopCurrentTransition();
            _currentCoroutine = StartCoroutine(CoDelayedCrossFade(delay, midAction, onComplete));
        }

        public void PlayDelayedClose(float delay, Action onComplete = null, float duration = -1f)
        {
            StopCurrentTransition();
            _currentCoroutine = StartCoroutine(CoDelayedCloseOnly(delay, onComplete, duration));
        }

        /// <summary>Đặt ngay về trạng thái hiện rõ đen hoàn toàn (không animation)</summary>
        public void SetClosedImmediate()
        {
            if (cutScreenPanel != null) cutScreenPanel.SetActive(true);
            if (walkingCharacter != null) walkingCharacter.SetActive(true);
            SetAlpha(1f);
            _isClosed = true;
        }

        /// <summary>Đặt ngay về trạng thái mờ hẳn / ẩn hoàn toàn (không animation)</summary>
        public void SetOpenImmediate()
        {
            SetAlpha(0f);
            if (walkingCharacter != null) walkingCharacter.SetActive(false);
            if (cutScreenPanel != null) cutScreenPanel.SetActive(false);
            _isClosed = false;
        }

        // ─── Coroutines ───────────────────────────────────────────────────────────

        private IEnumerator CoClose(Action onComplete, float duration = -1f)
        {
            float dur = (duration > 0f) ? duration : closeDuration;
            if (cutScreenPanel != null) cutScreenPanel.SetActive(true);
            if (walkingCharacter != null) walkingCharacter.SetActive(true);
            yield return StartCoroutine(CoFade(isClosing: true, dur));
            _isClosed = true;
            onComplete?.Invoke();
        }

        private IEnumerator CoOpen(Action onComplete, float duration = -1f)
        {
            float dur = (duration > 0f) ? duration : openDuration;
            if (cutScreenPanel != null) cutScreenPanel.SetActive(true);
            yield return StartCoroutine(CoFade(isClosing: false, dur));
            _isClosed = false;
            if (walkingCharacter != null) walkingCharacter.SetActive(false);
            if (cutScreenPanel != null) cutScreenPanel.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator CoCrossFade(Action midAction, Action onComplete)
        {
            // 1. Hiện rõ dần (Fade In che đen)
            if (cutScreenPanel != null) cutScreenPanel.SetActive(true);
            yield return StartCoroutine(CoFade(isClosing: true, closeDuration));
            _isClosed = true;

            // 2. Thực thi logic giữa màn đen
            midAction?.Invoke();

            // 3. Giữ màn đen một lúc
            if (holdDuration > 0f) yield return new WaitForSecondsRealtime(holdDuration);

            // 4. Mờ dần (Fade Out lộ scene)
            yield return StartCoroutine(CoFade(isClosing: false, openDuration));
            _isClosed = false;
            if (cutScreenPanel != null) cutScreenPanel.SetActive(false);

            onComplete?.Invoke();
        }

        private IEnumerator CoDelayedCrossFade(float delay, Action midAction, Action onComplete)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return StartCoroutine(CoCrossFade(midAction, onComplete));
        }

        private IEnumerator CoDelayedCloseOnly(float delay, Action onComplete, float duration = -1f)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return StartCoroutine(CoClose(onComplete, duration));
        }

        /// <summary>
        /// Fade animation cốt lõi.
        /// isClosing = true  → Alpha từ 0 lên 1 (hiện rõ che đen)
        /// isClosing = false → Alpha từ 1 về 0 (mờ dần lộ màn hình)
        /// </summary>
        private IEnumerator CoFade(bool isClosing, float duration)
        {
            float fromAlpha = isClosing ? 0f : 1f;
            float toAlpha   = isClosing ? 1f : 0f;

            AnimationCurve curve = isClosing ? closeEase : openEase;
            float elapsed = 0f;

            SetAlpha(fromAlpha);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = curve.Evaluate(t);

                float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                SetAlpha(currentAlpha);

                yield return null;
            }

            SetAlpha(toAlpha);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.blocksRaycasts = (alpha > 0.01f);
            }
            else if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
                fadeImage.raycastTarget = (alpha > 0.01f);
            }
        }

        private void StopCurrentTransition()
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
        }
    }
}


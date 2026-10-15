using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Chạy hoạt ảnh sprite (Sprite Animation) trực tiếp trên UnityEngine.UI.Image trong Canvas.
    /// - Hoàn toàn tương thích với Canvas/UI.
    /// - Không cần tạo Animator Controller hay file .anim phức tạp.
    /// - Tự động căn chỉnh hình ảnh, khắc phục hoàn toàn vấn đề lệch tâm (Pivot).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UISpriteAnimation : MonoBehaviour
    {
        [Header("--- DANH SÁCH KHUNG HÌNH (FRAMES) ---")]
        [Tooltip("Kéo các frame sprite bước đi của nhân vật vào đây")]
        [SerializeField] private List<Sprite> frames = new List<Sprite>();

        [Header("--- CÀI ĐẶT HOẠT ẢNH ---")]
        [Tooltip("Tốc độ khung hình (khung hình / giây, ví dụ 8 - 12 fps)")]
        [SerializeField] private float frameRate = 8f;

        [Tooltip("Lặp lại hoạt ảnh liên tục")]
        [SerializeField] private bool loop = true;

        [Tooltip("Tự động chạy hoạt ảnh khi đối tượng được bật (Active)")]
        [SerializeField] private bool playOnEnable = true;

        [Tooltip("Giữ nguyên tỷ lệ chuẩn của nhân vật (Preserve Aspect)")]
        [SerializeField] private bool preserveAspect = true;

        private Image _image;
        private float _timer = 0f;
        private int _currentFrameIndex = 0;
        private bool _isPlaying = false;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null && preserveAspect)
            {
                _image.preserveAspect = true;
            }

            // Tự động nạp các sprite từ Character.png nếu danh sách trống
#if UNITY_EDITOR
            if (frames == null || frames.Count == 0)
            {
                LoadCharacterSpritesFromEditor();
            }
#endif
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!_isPlaying || frames == null || frames.Count <= 1) return;

            _timer += Time.unscaledDeltaTime;
            float frameInterval = 1f / Mathf.Max(1f, frameRate);

            if (_timer >= frameInterval)
            {
                _timer -= frameInterval;
                _currentFrameIndex++;

                if (_currentFrameIndex >= frames.Count)
                {
                    if (loop)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        _currentFrameIndex = frames.Count - 1;
                        _isPlaying = false;
                        return;
                    }
                }

                UpdateSpriteFrame();
            }
        }

        public void Play()
        {
            _timer = 0f;
            _currentFrameIndex = 0;
            _isPlaying = true;
            UpdateSpriteFrame();
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void SetFrames(List<Sprite> newFrames)
        {
            frames = newFrames;
            _currentFrameIndex = 0;
            UpdateSpriteFrame();
        }

        private void UpdateSpriteFrame()
        {
            if (_image == null || frames == null || frames.Count == 0) return;
            if (_currentFrameIndex >= 0 && _currentFrameIndex < frames.Count)
            {
                _image.sprite = frames[_currentFrameIndex];
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Load Character Sprites")]
        public void LoadCharacterSpritesFromEditor()
        {
            string path = "Assets/Sprite/Character.png";
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                frames.Clear();
                foreach (var a in assets)
                {
                    if (a is Sprite s)
                    {
                        frames.Add(s);
                    }
                }
                if (_image != null && frames.Count > 0)
                {
                    _image.sprite = frames[0];
                }
            }
        }
#endif
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizGame
{
    /// <summary>
    /// Quản lý hình ảnh nền ngọn núi (MountainBg), các điểm mốc Start/End (BottomPos / TopPos),
    /// và cung cấp thông tin ranh giới (bounds) cho việc rải trạm và điều khiển máy quay.
    /// </summary>
    public class MountainBackground : MonoBehaviour
    {
        [Header("--- 1. CÀI ĐẶT ẢNH NỀN (SPRITE RENDERER) ---")]
        [Tooltip("SpriteRenderer hiển thị ảnh nền ngọn núi theo Theme")]
        [SerializeField] private SpriteRenderer mountainSprite;

        [Header("--- 2. HAI ĐIỂM START VÀ END TRONG BG ---")]
        [Tooltip("Điểm xuất phát chân núi (Start / BottomPos)")]
        [SerializeField] private Transform bottomPos;

        [Tooltip("Điểm kết thúc đỉnh núi (End / TopPos)")]
        [SerializeField] private Transform topPos;

        [Header("--- 3. KÍCH THƯỚC DỰ PHÒNG (KHI CHƯA GẮN SPRITE HOẶC TRANSFORM) ---")]
        [Tooltip("Chiều rộng ngọn núi trong World units")]
        [SerializeField] private float fallbackWidth = 12f;

        [Tooltip("Chiều cao ngọn núi trong World units")]
        [SerializeField] private float fallbackHeight = 25f;

        [Tooltip("Tỷ lệ lề an toàn 2 bên sườn núi (0.88 = 88% bề ngang núi để trạm không rớt ra ngoài)")]
        [Range(0.2f, 0.98f)]
        [SerializeField] private float widthMarginFactor = 0.88f;

        [Header("--- 4. CẤU HÌNH THEME (THEME SETTINGS) ---")]
        [Tooltip("Danh sách ThemeSO dùng để tra cứu ảnh nền ngọn núi mapBg")]
        [SerializeField] private List<ThemeSO> availableThemes = new List<ThemeSO>();

        public SpriteRenderer MountainSprite => mountainSprite;
        public Transform BottomPos => bottomPos;
        public Transform TopPos => topPos;
        public float WidthMarginFactor => widthMarginFactor;
        public List<ThemeSO> AvailableThemes => availableThemes;

        private void Awake()
        {
            AutoFindReferences();
        }

        private void OnValidate()
        {
            AutoFindReferences();
        }

        /// <summary>
        /// Tự động tìm kiếm SpriteRenderer và các điểm mốc con (BottomPos, TopPos)
        /// theo đúng hierarchy GameObject:
        /// GameManager -> MountainBg -> TopPos, BottomPos
        /// </summary>
        public void AutoFindReferences()
        {
            if (mountainSprite == null)
            {
                mountainSprite = GetComponent<SpriteRenderer>();
                if (mountainSprite == null) mountainSprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (bottomPos == null)
            {
                bottomPos = FindChild(transform, "BottomPos", "Bottom", "StartPos", "Start", "BasePos", "Base", "GameObject (1)");
            }

            if (topPos == null)
            {
                topPos = FindChild(transform, "TopPos", "Top", "EndPos", "End", "SummitPos", "Summit", "GameObject");
            }
        }

        private Transform FindChild(Transform parent, params string[] names)
        {
            foreach (var n in names)
            {
                Transform t = parent.Find(n);
                if (t != null) return t;
            }
            return null;
        }

        /// <summary>
        /// Cập nhật Sprite ngọn núi theo ThemeSO (mapBg)
        /// </summary>
        public void ApplyTheme(string themeId)
        {
            if (mountainSprite == null)
            {
                AutoFindReferences();
            }

            ThemeSO theme = FindThemeById(themeId);
            if (theme != null && theme.mapBg != null && mountainSprite != null)
            {
                mountainSprite.sprite = theme.mapBg;
                Debug.Log($"[MountainBackground] Đã cập nhật ảnh ngọn núi (MountainBg) theo Theme: '{theme.themeName}'");
            }
        }

        public ThemeSO FindThemeById(string themeId)
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

        /// <summary>
        /// Lấy tọa độ điểm xuất phát chân núi (Start) và điểm kết thúc đỉnh núi (End)
        /// </summary>
        public void GetStartAndEndPositions(out Vector3 startPos, out Vector3 endPos)
        {
            AutoFindReferences();

            GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth);

            if (bottomPos != null)
            {
                startPos = bottomPos.position;
            }
            else
            {
                startPos = new Vector3(centerX, minY, 0f);
            }

            if (topPos != null)
            {
                endPos = topPos.position;
            }
            else
            {
                endPos = new Vector3(centerX, maxY, 0f);
            }
        }

        /// <summary>
        /// Lấy ranh giới chiều cao và bề ngang của ngọn núi trong World Space
        /// </summary>
        public void GetMountainBounds(out float minY, out float maxY, out float centerX, out float halfWidth)
        {
            if (bottomPos != null && topPos != null)
            {
                float y1 = bottomPos.position.y;
                float y2 = topPos.position.y;
                minY = Mathf.Min(y1, y2);
                maxY = Mathf.Max(y1, y2);
                centerX = (bottomPos.position.x + topPos.position.x) * 0.5f;

                if (mountainSprite != null && mountainSprite.sprite != null)
                {
                    halfWidth = mountainSprite.bounds.extents.x;
                }
                else
                {
                    halfWidth = fallbackWidth * 0.5f;
                }
                return;
            }

            if (mountainSprite != null && mountainSprite.sprite != null)
            {
                Bounds b = mountainSprite.bounds;
                minY = b.min.y + 0.5f;
                maxY = b.max.y - 0.5f;
                centerX = b.center.x;
                halfWidth = b.extents.x;
            }
            else
            {
                Vector3 rootPos = transform.position;
                minY = rootPos.y - (fallbackHeight / 2f) + 1f;
                maxY = rootPos.y + (fallbackHeight / 2f) - 1f;
                centerX = rootPos.x;
                halfWidth = fallbackWidth / 2f;
            }
        }

        /// <summary>
        /// Tự động scale đồng đều X = Y của MountainBg (transform gốc) để khoảng cách BottomPos → TopPos
        /// đạt đủ chiều cao requiredHeight. Scale uniform giữ tỷ lệ ảnh không bị méo.
        /// Trả về true nếu đã scale, false nếu đã đủ hoặc không cần.
        /// </summary>
        public bool AutoScaleToFitHeight(float requiredHeight)
        {
            if (bottomPos == null || topPos == null) return false;

            float currentHeight = Mathf.Abs(topPos.position.y - bottomPos.position.y);
            if (currentHeight >= requiredHeight - 0.01f)
            {
                // Chiều cao hiện tại đã đủ, không cần scale
                return false;
            }

            // Tính tỷ lệ scale cần thiết (uniform: X = Y)
            // Vì TopPos/BottomPos là con của MountainBg, khi scale thì khoảng cách giữa chúng cũng scale theo
            float scaleFactor = requiredHeight / currentHeight;

            Vector3 oldScale = transform.localScale;
            float newScale = Mathf.Abs(oldScale.y) * scaleFactor;
            if (newScale < 0.001f) newScale = 1f;

            transform.localScale = new Vector3(newScale, newScale, oldScale.z);

            Debug.Log($"[MountainBackground] Auto-Scale uniform X=Y: {oldScale.x:F2} → {newScale:F2} " +
                      $"(chiều cao {currentHeight:F1} → {requiredHeight:F1} world units)");
            return true;
        }

        /// <summary>
        /// Lấy khoảng cách hiện tại từ BottomPos đến TopPos (world Y)
        /// </summary>
        public float GetCurrentTrailHeight()
        {
            if (bottomPos != null && topPos != null)
            {
                return Mathf.Abs(topPos.position.y - bottomPos.position.y);
            }
            return fallbackHeight;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            if (bottomPos != null)
            {
                Gizmos.DrawWireSphere(bottomPos.position, 0.4f);
            }

            Gizmos.color = Color.red;
            if (topPos != null)
            {
                Gizmos.DrawWireSphere(topPos.position, 0.4f);
            }

            if (bottomPos != null && topPos != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(bottomPos.position, topPos.position);
            }
        }
    }
}

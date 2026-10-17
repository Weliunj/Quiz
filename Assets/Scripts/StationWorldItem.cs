using TMPro;
using UnityEngine;

namespace QuizGame
{
    /// <summary>
    /// Component hiển thị Trạm (Station) trong không gian World Transform 2D.
    /// Trạm chỉ dùng làm mốc trực quan trên sườn núi, không nhận tương tác click chuột.
    /// </summary>
    public class StationWorldItem : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN HIỂN THỊ TRẠM ---")]
        [Tooltip("Renderer hình ảnh của trạm")]
        [SerializeField] private SpriteRenderer iconRenderer;

        [Tooltip("Text 3D/World hiển thị số trạm hoặc đỉnh núi")]
        [SerializeField] private TMP_Text labelText;

        [Header("--- CÀI ĐẶT MÀU SẮC TRẠM ---")]
        [Tooltip("Màu của trạm thường khi bật applyColorTint")]
        [SerializeField] private Color normalColor = Color.white;

        [Tooltip("Màu của trạm đỉnh núi khi bật applyColorTint")]
        [SerializeField] private Color summitColor = Color.white;

        [Tooltip("Màu của trạm xuất phát khi bật applyColorTint")]
        [SerializeField] private Color baseColor = Color.white;

        public int StationIndex { get; private set; }
        public bool IsSummit { get; private set; }

        private void Awake()
        {
            AutoFindReferences();
        }

        private void AutoFindReferences()
        {
            if (iconRenderer == null) iconRenderer = GetComponent<SpriteRenderer>();
            if (labelText == null) labelText = GetComponentInChildren<TMP_Text>(true);
        }

        public void Setup(int stationNumber, Sprite customSprite = null, bool randomFlipX = true, bool showLabel = true, bool applyColorTint = false)
        {
            StationIndex = stationNumber;
            IsSummit = false;

            AutoFindReferences();

            if (iconRenderer != null)
            {
                // 1. Gán Sprite nếu được truyền vào
                if (customSprite != null)
                {
                    iconRenderer.sprite = customSprite;
                }

                // 2. Random lật ngang (Flip X)
                if (randomFlipX)
                {
                    iconRenderer.flipX = (UnityEngine.Random.value > 0.5f);
                }

                // 3. Màu sắc tint (nếu bật, nếu tắt thì giữ nguyên màu gốc của ảnh)
                if (applyColorTint)
                {
                    iconRenderer.color = normalColor;
                }
                else
                {
                    iconRenderer.color = Color.white;
                }
            }

            // 4. Nhãn text hiển thị rõ ràng: Trạm [n]
            if (labelText != null)
            {
                labelText.gameObject.SetActive(showLabel);
                if (showLabel)
                {
                    labelText.text = $"Trạm {stationNumber}";
                }
            }
        }

        public void Setup(int index, bool isSummit, bool isBase, Sprite customSprite = null, bool randomFlipX = true, bool showLabel = true, bool applyColorTint = false)
        {
            Setup(index, customSprite, randomFlipX, showLabel, applyColorTint);
        }
    }
}

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

        [Header("--- MÀU SẮC TRẠM ---")]
        [Tooltip("Màu của trạm thường")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.7f, 1f, 1f);

        [Tooltip("Màu của trạm đỉnh núi")]
        [SerializeField] private Color summitColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Tooltip("Màu của trạm xuất phát")]
        [SerializeField] private Color baseColor = new Color(0.3f, 0.85f, 0.4f, 1f);

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

        public void Setup(int index, bool isSummit, bool isBase)
        {
            StationIndex = index;
            IsSummit = isSummit;

            AutoFindReferences();

            if (iconRenderer != null)
            {
                if (isSummit) iconRenderer.color = summitColor;
                else if (isBase) iconRenderer.color = baseColor;
                else iconRenderer.color = normalColor;
            }

            if (labelText != null)
            {
                if (isSummit) labelText.text = "🏔️ Đỉnh";
                else labelText.text = $"{index + 1}";
            }
        }
    }
}

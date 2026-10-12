using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    public class MapManager : MonoBehaviour
    {
        [Header("--- PANELS ---")]
        [Tooltip("Panel màn hình chính")]
        [SerializeField] private GameObject homeMenuPanel;

        [Tooltip("Panel quản lý bản đồ (chứa danh sách map và nút +)")]
        [SerializeField] private GameObject mapManagerPanel;

        [Header("--- BUTTONS ---")]
        [Tooltip("Nút Start ở HomeMenuPanel")]
        [SerializeField] private Button startBtn;

        [Tooltip("Nút dấu cộng '+' để tạo/thêm map")]
        [SerializeField] private Button addMapBtn;

        [Tooltip("Nút quay lại Home (tùy chọn)")]
        [SerializeField] private Button backBtn;

        [Header("--- MAP CONTENT ---")]
        [Tooltip("Vùng chứa các map (LevelContent)")]
        [SerializeField] private Transform levelContent;

        [Tooltip("Prefab hoặc GameObject mẫu thẻ Map")]
        [SerializeField] private GameObject mapItemPrefab;

        [Header("--- SCROLL & CĂN GIỮA ---")]
        [Tooltip("Component hỗ trợ cuộn ngang và tự động căn giữa")]
        [SerializeField] private CenterSnapScroll centerSnapScroll;

        private int mapCount = 0;

        private void Awake()
        {
            if (centerSnapScroll == null && mapManagerPanel != null)
                centerSnapScroll = mapManagerPanel.GetComponentInChildren<CenterSnapScroll>(true);

            // Tự động gán sự kiện click cho các nút nếu đã kéo vào Inspector
            if (startBtn != null)
                startBtn.onClick.AddListener(OnStartButtonClicked);

            if (addMapBtn != null)
                addMapBtn.onClick.AddListener(OnAddMapButtonClicked);

            if (backBtn != null)
                backBtn.onClick.AddListener(OnBackButtonClicked);
        }

        private void Start()
        {
            // Khởi đầu: Luôn hiện HomeMenuPanel và ẩn MapManagerPanel
            ShowHomeMenu();
        }

        /// <summary>
        /// Khi bấm nút Start: Ẩn HomeMenuPanel, mở MapManagerPanel
        /// </summary>
        public void OnStartButtonClicked()
        {
            if (homeMenuPanel != null) homeMenuPanel.SetActive(false);
            if (mapManagerPanel != null) mapManagerPanel.SetActive(true);

            StartCoroutine(InitSnapRoutine());
            Debug.Log("[MapManager] Đã mở MapManagerPanel!");
        }

        private System.Collections.IEnumerator InitSnapRoutine()
        {
            yield return null;
            if (centerSnapScroll != null)
            {
                centerSnapScroll.UpdatePadding();
                centerSnapScroll.SnapToNearestChild(true);
            }
        }

        /// <summary>
        /// Khi bấm nút Back: Quay trở lại HomeMenuPanel
        /// </summary>
        public void OnBackButtonClicked()
        {
            if (mapManagerPanel != null) mapManagerPanel.SetActive(false);
            if (homeMenuPanel != null) homeMenuPanel.SetActive(true);
        }

        /// <summary>
        /// Khi bấm nút Dấu Cộng (+): Tạo thêm một thẻ Map mới
        /// </summary>
        public void OnAddMapButtonClicked()
        {
            mapCount++;
            Debug.Log($"[MapManager] Đã bấm nút dấu '+' -> Tạo Map #{mapCount}");

            // Nếu có gắn mapItemPrefab và levelContent, sinh ra thẻ Map mới
            if (mapItemPrefab != null && levelContent != null)
            {
                GameObject newMap = Instantiate(mapItemPrefab, levelContent);
                newMap.name = $"Map_{mapCount}";
                newMap.SetActive(true);

                // Đảm bảo nút dấu cộng luôn nằm ở cuối (bên phải của các map)
                if (addMapBtn != null)
                {
                    addMapBtn.transform.SetAsLastSibling();
                }

                StartCoroutine(SnapToTargetRoutine(newMap.GetComponent<RectTransform>()));
            }
        }

        private System.Collections.IEnumerator SnapToTargetRoutine(RectTransform target)
        {
            yield return null;
            if (centerSnapScroll != null && target != null)
            {
                centerSnapScroll.UpdatePadding();
                centerSnapScroll.SnapTo(target, false);
            }
        }

        public void ShowHomeMenu()
        {
            if (homeMenuPanel != null) homeMenuPanel.SetActive(true);
            if (mapManagerPanel != null) mapManagerPanel.SetActive(false);
        }
    }
}

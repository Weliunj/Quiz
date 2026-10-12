using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    public class MountainStationManager : MonoBehaviour
    {
        [Header("--- GIAO DIỆN MÀN HÌNH LEO NÚI ---")]
        [Tooltip("Bảng giao diện chính của màn chơi leo núi")]
        [SerializeField] private GameObject mountainPanel;

        [Tooltip("ScrollRect chứa toàn bộ con đường leo núi")]
        [SerializeField] private ScrollRect mountainScrollRect;

        [Tooltip("Container (Content) chứa các trạm được sinh ra")]
        [SerializeField] private RectTransform stationsContainer;

        [Tooltip("Text hiển thị tiêu đề và tên bản đồ")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Nút quay lại danh sách bản đồ (BackBtn)")]
        [SerializeField] private Button backBtn;

        [Header("--- CÀI ĐẶT TẦNG TRẠM LEO NÚI ---")]
        [Tooltip("Khoảng cách chiều cao giữa mỗi tầng (layer)")]
        [SerializeField] private float layerHeight = 180f;

        [Tooltip("Độ lệch ngang ngẫu nhiên tối đa sang 2 bên (X offset)")]
        [SerializeField] private float maxHorizontalSpread = 260f;

        [Tooltip("Prefab hiển thị một trạm (nếu để trống script sẽ tự tạo giao diện)")]
        [SerializeField] private GameObject stationPrefab;

        private MapData currentMap;
        private Action onBackCallback;
        private List<GameObject> spawnedStations = new List<GameObject>();
        private List<GameObject> spawnedPaths = new List<GameObject>();

        private void Awake()
        {
            AutoFindReferences();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(CloseMountain);
            }
        }

        private void AutoFindReferences()
        {
            if (mountainPanel == null)
            {
                mountainPanel = gameObject;
            }

            if (mountainScrollRect == null)
            {
                mountainScrollRect = GetComponentInChildren<ScrollRect>(true);
            }

            if (stationsContainer == null && mountainScrollRect != null && mountainScrollRect.content != null)
            {
                stationsContainer = mountainScrollRect.content;
            }

            if (titleText == null)
            {
                Transform t = transform.Find("TitleText");
                if (t == null) t = transform.Find("Header/TitleText");
                if (t != null) titleText = t.GetComponent<TMP_Text>();
            }

            if (backBtn == null)
            {
                Transform t = transform.Find("BackBtn");
                if (t == null) t = transform.Find("Header/BackBtn");
                if (t != null) backBtn = t.GetComponent<Button>();
            }
        }

        /// <summary>
        /// Mở màn hình leo núi và sinh các trạm theo từng tầng đến đỉnh núi
        /// </summary>
        public void OpenMountain(MapData map, Action onBack = null)
        {
            currentMap = map;
            onBackCallback = onBack;

            // Đảm bảo có UI hoàn chỉnh để hiển thị
            EnsureUIHierarchy();

            if (mountainPanel != null)
            {
                mountainPanel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = $"🏔️ {currentMap.mapName} ({currentMap.stationCount} Trạm Đến Đỉnh)";
            }

            // Sinh các tầng trạm lên đỉnh núi
            SpawnMountainLayers();
        }

        public void CloseMountain()
        {
            if (mountainPanel != null)
            {
                mountainPanel.SetActive(false);
            }

            onBackCallback?.Invoke();
        }

        /// <summary>
        /// Sinh các trạm: Mỗi index là 1 tầng (layer) leo dần từ chân núi lên đỉnh núi.
        /// Chiều cao Y tăng dần theo từng tầng, vị trí X được random tự nhiên tạo đường mòn uốn lượn.
        /// </summary>
        private void SpawnMountainLayers()
        {
            ClearOldStations();

            if (stationsContainer == null || currentMap == null) return;

            int stationCount = Mathf.Max(2, currentMap.stationCount);
            float totalHeight = (stationCount - 1) * layerHeight + 350f;

            // Mở rộng chiều cao Container để cuộn mượt mà
            stationsContainer.sizeDelta = new Vector2(stationsContainer.sizeDelta.x, totalHeight);

            List<Vector2> stationPositions = new List<Vector2>();

            float startY = 120f;
            float previousX = 0f;

            for (int i = 0; i < stationCount; i++)
            {
                bool isSummit = (i == stationCount - 1); // Trạm cuối cùng là Đỉnh núi
                bool isBase = (i == 0);                 // Trạm đầu tiên là Chân núi

                // 1. Tọa độ Y: Tăng dần theo từng tầng (Layer)
                float layerJitter = UnityEngine.Random.Range(-layerHeight * 0.1f, layerHeight * 0.1f);
                float y = startY + (i * layerHeight) + layerJitter;

                // 2. Tọa độ X: Random khoảng cách sang 2 bên tạo con đường mòn uốn lượn
                float x = 0f;
                if (isSummit)
                {
                    // Đỉnh núi: Nằm ngay chính giữa trên cùng
                    x = UnityEngine.Random.Range(-30f, 30f);
                }
                else if (isBase)
                {
                    // Chân núi: Xuất phát gần trung tâm
                    x = UnityEngine.Random.Range(-80f, 80f);
                    previousX = x;
                }
                else
                {
                    // Các trạm ở giữa: Đảo hướng và lệch ngẫu nhiên tạo hình zigzag leo dốc
                    float stepOffset = UnityEngine.Random.Range(80f, maxHorizontalSpread);
                    float targetX = (previousX > 0) ? -stepOffset : stepOffset;
                    targetX += UnityEngine.Random.Range(-40f, 40f);
                    x = Mathf.Clamp(targetX, -maxHorizontalSpread, maxHorizontalSpread);
                    previousX = x;
                }

                Vector2 pos = new Vector2(x, y);
                stationPositions.Add(pos);

                // 3. Tạo GameObject trạm
                GameObject stationObj = CreateStationObject(i, stationCount, isSummit, isBase);
                stationObj.transform.SetParent(stationsContainer, false);

                RectTransform rt = stationObj.GetComponent<RectTransform>();
                rt.anchoredPosition = pos;

                spawnedStations.Add(stationObj);
            }

            // 4. Vẽ đường nối (đường mòn) giữa các tầng trạm
            for (int i = 0; i < stationPositions.Count - 1; i++)
            {
                CreatePathBetweenStations(stationPositions[i], stationPositions[i + 1]);
            }

            // Cuộn xuống chân núi (vị trí ban đầu)
            StartCoroutine(ScrollToBottomRoutine());
        }

        private GameObject CreateStationObject(int index, int total, bool isSummit, bool isBase)
        {
            if (stationPrefab != null)
            {
                GameObject obj = Instantiate(stationPrefab);
                SetupStationData(obj, index, isSummit);
                return obj;
            }

            // Tự động tạo giao diện thẻ trạm bằng mã lệnh
            GameObject station = new GameObject($"Station_Layer_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rt = station.GetComponent<RectTransform>();
            rt.sizeDelta = isSummit ? new Vector2(160f, 100f) : new Vector2(120f, 80f);

            Image img = station.GetComponent<Image>();
            // Màu sắc theo từng độ cao: Chân núi xanh -> Sườn núi đất/đá -> Đỉnh núi vàng kim tuyết trắng
            if (isSummit)
            {
                img.color = new Color(1f, 0.85f, 0.2f, 1f); // Vàng kim rực rỡ
            }
            else if (isBase)
            {
                img.color = new Color(0.2f, 0.75f, 0.35f, 1f); // Xanh lá chân núi
            }
            else
            {
                float t = (float)index / total;
                img.color = Color.Lerp(new Color(0.3f, 0.6f, 0.9f, 1f), new Color(0.85f, 0.5f, 0.25f, 1f), t);
            }

            // Nhãn hiển thị tên trạm
            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(station.transform, false);

            RectTransform labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = isSummit ? 20 : 16;
            label.color = Color.white;
            label.fontStyle = FontStyles.Bold;

            if (isSummit)
            {
                label.text = $"🏔️ ĐỈNH NÚI\nTầng {index + 1}";
                label.color = Color.black;
            }
            else if (isBase)
            {
                label.text = $"🏕️ Trạm 1\n(Khởi hành)";
            }
            else
            {
                label.text = $"Trạm {index + 1}\n(Tầng {index + 1})";
            }

            // Sự kiện khi bấm vào trạm
            Button btn = station.GetComponent<Button>();
            int stationIdx = index;
            btn.onClick.AddListener(() => OnStationClicked(stationIdx, isSummit));

            return station;
        }

        private void SetupStationData(GameObject obj, int index, bool isSummit)
        {
            TMP_Text txt = obj.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = isSummit ? $"🏔️ ĐỈNH NÚI" : $"Trạm {index + 1}";
            }

            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                int stationIdx = index;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnStationClicked(stationIdx, isSummit));
            }
        }

        /// <summary>
        /// Vẽ một nét đường nối giữa 2 trạm liên tiếp
        /// </summary>
        private void CreatePathBetweenStations(Vector2 from, Vector2 to)
        {
            GameObject lineObj = new GameObject("Trail_Line", typeof(RectTransform), typeof(Image));
            lineObj.transform.SetParent(stationsContainer, false);
            lineObj.transform.SetAsFirstSibling(); // Đưa đường đi xuống dưới để trạm đè lên trên

            RectTransform rt = lineObj.GetComponent<RectTransform>();
            Vector2 dir = to - from;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.sizeDelta = new Vector2(distance, 6f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = from;
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            Image img = lineObj.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.45f); // Màu mờ uốn lượn

            spawnedPaths.Add(lineObj);
        }

        private void OnStationClicked(int index, bool isSummit)
        {
            if (currentMap == null) return;

            string stationName = isSummit ? "ĐỈNH NÚI CHIẾN THẮNG" : $"Trạm {index + 1}";
            Debug.Log($"[MountainStationManager] Người chơi đã bấm vào {stationName} (Tầng {index + 1}/{currentMap.stationCount})!");

            // Bốc câu hỏi tương ứng trong danh sách câu hỏi của map nếu có
            if (currentMap.questions != null && currentMap.questions.Count > 0)
            {
                int qIdx = index % currentMap.questions.Count;
                QuestionData q = currentMap.questions[qIdx];
                Debug.Log($"[MountainStationManager] Câu hỏi tại trạm: '{q.questionText}' | Đáp án đúng: {(char)('A' + q.correctIndex)}");
            }
        }

        private void ClearOldStations()
        {
            foreach (var st in spawnedStations)
            {
                if (st != null) Destroy(st);
            }
            spawnedStations.Clear();

            foreach (var p in spawnedPaths)
            {
                if (p != null) Destroy(p);
            }
            spawnedPaths.Clear();
        }

        private IEnumerator ScrollToBottomRoutine()
        {
            yield return null;
            if (mountainScrollRect != null)
            {
                mountainScrollRect.verticalNormalizedPosition = 0f; // Cuộn về chân núi ban đầu
            }
        }

        /// <summary>
        /// Tự động sinh cấu trúc UI nếu chưa có sẵn trong Scene
        /// </summary>
        private void EnsureUIHierarchy()
        {
            AutoFindReferences();

            if (mountainScrollRect == null)
            {
                // Tạo Scroll View hoàn chỉnh
                GameObject svObj = new GameObject("MountainScrollView", typeof(RectTransform), typeof(ScrollRect));
                svObj.transform.SetParent(transform, false);

                RectTransform svRt = svObj.GetComponent<RectTransform>();
                svRt.anchorMin = Vector2.zero;
                svRt.anchorMax = Vector2.one;
                svRt.sizeDelta = Vector2.zero;

                mountainScrollRect = svObj.GetComponent<ScrollRect>();
                mountainScrollRect.horizontal = false;
                mountainScrollRect.vertical = true;

                // Viewport
                GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
                vpObj.transform.SetParent(svObj.transform, false);

                RectTransform vpRt = vpObj.GetComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;
                mountainScrollRect.viewport = vpRt;

                // Content
                GameObject contentObj = new GameObject("StationsContent", typeof(RectTransform));
                contentObj.transform.SetParent(vpObj.transform, false);

                RectTransform cRt = contentObj.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0f);
                cRt.anchorMax = new Vector2(0.5f, 0f);
                cRt.pivot = new Vector2(0.5f, 0f);
                cRt.anchoredPosition = Vector2.zero;
                cRt.sizeDelta = new Vector2(900f, 1200f);

                mountainScrollRect.content = cRt;
                stationsContainer = cRt;
            }

            if (titleText == null)
            {
                GameObject tObj = new GameObject("MountainTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                tObj.transform.SetParent(transform, false);

                RectTransform trt = tObj.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(0.5f, 1f);
                trt.anchorMax = new Vector2(0.5f, 1f);
                trt.pivot = new Vector2(0.5f, 1f);
                trt.anchoredPosition = new Vector2(0, -30f);
                trt.sizeDelta = new Vector2(800f, 60f);

                titleText = tObj.GetComponent<TextMeshProUGUI>();
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontSize = 28;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.white;
            }

            if (backBtn == null)
            {
                GameObject bObj = new GameObject("MountainBackBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                bObj.transform.SetParent(transform, false);

                RectTransform brt = bObj.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0f, 1f);
                brt.anchorMax = new Vector2(0f, 1f);
                brt.pivot = new Vector2(0f, 1f);
                brt.anchoredPosition = new Vector2(40f, -30f);
                brt.sizeDelta = new Vector2(140f, 55f);

                bObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
                backBtn = bObj.GetComponent<Button>();
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(CloseMountain);

                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(bObj.transform, false);
                RectTransform trt = txtObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.sizeDelta = Vector2.zero;

                TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
                txt.text = "← Quay Lại";
                txt.alignment = TextAlignmentOptions.Center;
                txt.fontSize = 18;
                txt.color = Color.white;
            }
        }
    }
}

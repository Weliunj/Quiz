using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame
{
    /// <summary>
    /// Quản lý việc sinh các mốc trạm (StationWorldItem) và vẽ đường mòn (LineRenderer) uốn cong Catmull-Rom.
    /// Tính toán phân bố vị trí từ điểm Start (BottomPos) lên đỉnh End (TopPos).
    /// </summary>
    public class MountainTrailGenerator : MonoBehaviour
    {
        [Header("--- 1. CÀI ĐẶT TRẠM (STATIONS) ---")]
        [Tooltip("Prefab của trạm trong World Space (nếu để trống sẽ tự tạo vòng tròn mặc định)")]
        [SerializeField] private GameObject stationPrefab;

        [Tooltip("Tỷ lệ kích thước trạm thường (Scale)")]
        [SerializeField] private float stationScale = 0.35f;

        [Tooltip("Tỷ lệ kích thước trạm đỉnh núi (Scale)")]
        [SerializeField] private float summitScale = 0.5f;

        [Tooltip("Khoảng cách cố định giữa 2 tầng trạm trên trục Y")]
        [SerializeField] private float layerSpacingY = 2.2f;

        [Header("--- HÌNH DÁNG NÚI CHÓP NÓN (TAPERED CONE SPREAD) ---")]
        [Tooltip("Bề ngang tối đa ở chân núi (Start / BottomPos). Càng lên đỉnh càng thu hẹp lại.")]
        [SerializeField] private float baseSpreadX = 8.0f;

        [Tooltip("Bề ngang tối đa ở sát chóp đỉnh núi (TopPos). Nên để nhỏ (0.3 - 0.8) để trạm không rớt ra ngoài khoảng không.")]
        [SerializeField] private float summitSpreadX = 0.5f;

        [Tooltip("Độ dốc thu hẹp sườn núi (1.0: thu hẹp thẳng tam giác, >1.0: thu hẹp nhanh hơn theo hình chóp nhọn)")]
        [Range(0.5f, 2.5f)]
        [SerializeField] private float mountainSlopePower = 1.1f;

        [Header("--- 2. ĐƯỜNG MÒN NỐI TRẠM (LINE RENDERER) ---")]
        [Tooltip("LineRenderer dùng để vẽ con đường mòn")]
        [SerializeField] private LineRenderer pathLineRenderer;

        [Tooltip("Bật chế độ đường mòn uốn cong mềm mại thay vì đường thẳng")]
        [SerializeField] private bool useCurvedPath = true;

        [Tooltip("Độ phân giải làm mịn đường cong cho mỗi đoạn")]
        [Range(4, 25)]
        [SerializeField] private int curveResolutionPerSegment = 12;

        [Tooltip("Độ dày nét vẽ đường mòn")]
        [SerializeField] private float trailWidth = 0.12f;

        public float HorizontalSpreadX => baseSpreadX;
        public int CurveResolutionPerSegment => curveResolutionPerSegment;
        public Vector3[] CachedPathPoints { get; private set; }

        private List<GameObject> spawnedStations = new List<GameObject>();

        public void AutoFindReferences(Transform defaultParent)
        {
            if (pathLineRenderer == null)
            {
                pathLineRenderer = GetComponentInChildren<LineRenderer>();
                if (pathLineRenderer == null && defaultParent != null)
                {
                    GameObject lineObj = new GameObject("MountainTrail_LineRenderer", typeof(LineRenderer));
                    lineObj.transform.SetParent(defaultParent, false);
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
            lr.sortingOrder = 1;
        }

        /// <summary>
        /// Sinh ngẫu nhiên danh sách tọa độ các trạm từ điểm Start lên đỉnh End (BottomPos -> TopPos)
        /// Nếu số trạm * layerSpacingY vượt quá chiều cao hiện tại → tự động scale MountainBg.
        /// </summary>
        public void GenerateRandomStations(MapData currentMap, MountainBackground bg, Action onSave)
        {
            if (currentMap == null) return;
            int count = Mathf.Max(1, currentMap.stationCount);

            if (currentMap.stationPositions == null)
            {
                currentMap.stationPositions = new List<StationPos>();
            }
            currentMap.stationPositions.Clear();

            // ── Auto-Scale: đảm bảo khoảng cách giữa các trạm luôn = layerSpacingY ──
            if (bg != null && count > 1)
            {
                float requiredHeight = (count - 1) * layerSpacingY;
                bg.AutoScaleToFitHeight(requiredHeight);
            }

            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;

            if (bg != null)
            {
                bg.GetStartAndEndPositions(out startPos, out endPos);
            }

            if (count == 1)
            {
                currentMap.stationPositions.Add(new StationPos(endPos.x, endPos.y));
            }
            else
            {
                // Dùng layerSpacingY cố định. stepY = khoảng cách thực tế giữa 2 trạm
                // Nếu BG đã được auto-scale, totalHeight sẽ >= requiredHeight
                float totalHeight = endPos.y - startPos.y;
                float stepY = totalHeight / (count - 1);
                float centerX = (startPos.x + endPos.x) * 0.5f;

                float scaleFactorX = 1f;
                if (bg != null)
                {
                    scaleFactorX = Mathf.Abs(bg.transform.lossyScale.x);
                }

                // Bề ngang tối đa ở chân núi được nhân với scale của ngọn núi (MountainBg)
                float effectiveBaseSpreadX = baseSpreadX * scaleFactorX;
                float effectiveSummitSpreadX = summitSpreadX * scaleFactorX;

                float maxBaseHalfX = effectiveBaseSpreadX * 0.5f;
                float minTopHalfX = Mathf.Max(0.1f, effectiveSummitSpreadX * 0.5f);

                for (int i = 0; i < count; i++)
                {
                    if (i == 0)
                    {
                        // Trạm đầu tiên xuất phát chính xác tại điểm Start (BottomPos)
                        currentMap.stationPositions.Add(new StationPos(startPos.x, startPos.y));
                    }
                    else if (i == count - 1)
                    {
                        // Trạm cuối cùng kết thúc chính xác tại đỉnh (TopPos)
                        currentMap.stationPositions.Add(new StationPos(endPos.x, endPos.y));
                    }
                    else
                    {
                        // Tỷ lệ tiến độ từ chân núi (0.0) lên đỉnh núi (1.0)
                        float progress = (float)i / (count - 1);

                        // Trọng tâm trục X di chuyển dần theo đường thẳng nối từ startPos.x đến endPos.x
                        float currentCenterX = Mathf.Lerp(startPos.x, endPos.x, progress);

                        // Thu hẹp bề ngang sườn núi theo hình chóp nón
                        float taperFactor = Mathf.Pow(1f - progress, Mathf.Max(0.5f, mountainSlopePower));
                        float currentHalfSpreadX = Mathf.Lerp(minTopHalfX, maxBaseHalfX, taperFactor);

                        float y = startPos.y + (i * stepY);
                        float x = currentCenterX + UnityEngine.Random.Range(-currentHalfSpreadX, currentHalfSpreadX);
                        currentMap.stationPositions.Add(new StationPos(x, y));
                    }
                }
            }

            if (onSave != null)
            {
                onSave.Invoke();
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
            Debug.Log($"[MountainTrailGenerator] Đã tạo {count} trạm | layerSpacingY = {layerSpacingY} | Tổng cao = {(endPos.y - startPos.y):F1} units");
        }

        /// <summary>
        /// Tạo các đối tượng trạm và vẽ đường LineRenderer theo tọa độ đã lưu
        /// </summary>
        public void SpawnStations(MapData currentMap, Transform parentRoot)
        {
            ClearStations();

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
                    stationObj = Instantiate(stationPrefab, parentRoot);
                    stationObj.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    stationObj = CreateDefaultWorldStation(i, isSummit, isBase);
                    stationObj.transform.SetParent(parentRoot, false);
                }

                stationObj.transform.position = new Vector3(pos.x, pos.y, 0f);
                stationObj.name = isSummit ? $"Summit_Station_{i + 1}" : $"Station_Layer_{i + 1}";

                StationWorldItem item = stationObj.GetComponent<StationWorldItem>();
                if (item == null) item = stationObj.AddComponent<StationWorldItem>();
                item.Setup(i, isSummit, isBase);

                spawnedStations.Add(stationObj);
                pathPoints[i] = new Vector3(pos.x, pos.y, 0f);
            }

            // Vẽ đường LineRenderer
            if (pathLineRenderer != null)
            {
                pathLineRenderer.startWidth = trailWidth;
                pathLineRenderer.endWidth = trailWidth;
                pathLineRenderer.numCornerVertices = 5;
                pathLineRenderer.numCapVertices = 5;

                Vector3[] finalLinePoints = (useCurvedPath && total >= 2)
                    ? GenerateCurvedPath(pathPoints, curveResolutionPerSegment)
                    : pathPoints;

                CachedPathPoints = finalLinePoints;
                pathLineRenderer.positionCount = finalLinePoints.Length;
                pathLineRenderer.SetPositions(finalLinePoints);
            }
        }

        public void ClearStations()
        {
            foreach (var st in spawnedStations)
            {
                if (st != null) Destroy(st);
            }
            spawnedStations.Clear();
            CachedPathPoints = null;

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
            }
        }

        private GameObject CreateDefaultWorldStation(int index, bool isSummit, bool isBase)
        {
            GameObject obj = new GameObject($"Station_{index + 1}", typeof(SpriteRenderer));
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            sr.sprite = CreateCircleSprite(48);

            float scale = isSummit ? summitScale : stationScale;
            obj.transform.localScale = new Vector3(scale, scale, 1f);

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

        private Vector3[] GenerateCurvedPath(Vector3[] waypoints, int resolutionPerSegment)
        {
            if (waypoints == null || waypoints.Length == 0) return new Vector3[0];
            if (waypoints.Length < 2 || resolutionPerSegment <= 1) return waypoints;

            List<Vector3> curvedPoints = new List<Vector3>();
            int count = waypoints.Length;

            for (int i = 0; i < count - 1; i++)
            {
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

            curvedPoints.Add(waypoints[count - 1]);
            return curvedPoints.ToArray();
        }

        private Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
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

        private void OnDrawGizmosSelected()
        {
            MountainBackground bg = GetComponent<MountainBackground>();
            if (bg == null) bg = GetComponentInChildren<MountainBackground>();
            if (bg == null) bg = FindFirstObjectByType<MountainBackground>();

            if (bg != null)
            {
                bg.GetStartAndEndPositions(out Vector3 startPos, out Vector3 endPos);
                float scaleFactorX = Mathf.Abs(bg.transform.lossyScale.x);

                float maxBaseHalfX = (baseSpreadX * scaleFactorX) * 0.5f;
                float minTopHalfX = Mathf.Max(0.1f, (summitSpreadX * scaleFactorX) * 0.5f);

                Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
                // Vẽ sườn bên trái
                Vector3 baseLeft = new Vector3(startPos.x - maxBaseHalfX, startPos.y, 0f);
                Vector3 topLeft = new Vector3(endPos.x - minTopHalfX, endPos.y, 0f);
                Gizmos.DrawLine(baseLeft, topLeft);

                // Vẽ sườn bên phải
                Vector3 baseRight = new Vector3(startPos.x + maxBaseHalfX, startPos.y, 0f);
                Vector3 topRight = new Vector3(endPos.x + minTopHalfX, endPos.y, 0f);
                Gizmos.DrawLine(baseRight, topRight);

                // Vẽ đường nối đáy và đỉnh
                Gizmos.DrawLine(baseLeft, baseRight);
                Gizmos.DrawLine(topLeft, topRight);
            }
        }
    }
}

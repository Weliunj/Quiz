using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuizGame
{
    [RequireComponent(typeof(ScrollRect))]
    public class CenterSnapScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [Header("--- CÀI ĐẶT CĂN GIỮA (SNAP) ---")]
        [Tooltip("Tốc độ trượt mượt mà về chính giữa")]
        [SerializeField] private float snapSpeed = 10f;

        [Tooltip("Độ nhạy con lăn chuột khi cuộn ngang trên PC")]
        [SerializeField] private float mouseScrollSensitivity = 0.08f;

        [Header("--- THÀNH PHẦN (TỰ ĐỘNG NHẬN DIỆN) ---")]
        [Tooltip("Component ScrollRect điều khiển cuộn")]
        [SerializeField] private ScrollRect scrollRect;

        [Tooltip("Vùng hiển thị Viewport của ScrollRect")]
        [SerializeField] private RectTransform viewportRect;

        [Tooltip("Content chứa các thẻ map và nút +")]
        [SerializeField] private RectTransform contentRect;

        [Tooltip("Layout tự động sắp xếp hàng ngang")]
        [SerializeField] private HorizontalLayoutGroup layoutGroup;

        private bool isDragging = false;
        private bool isSnapping = false;
        private float targetContentPosX = 0f;
        private RectTransform currentFocusedChild;

        private void Awake()
        {
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                if (viewportRect == null) viewportRect = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
                if (contentRect == null) contentRect = scrollRect.content;
            }

            if (contentRect != null && layoutGroup == null)
            {
                layoutGroup = contentRect.GetComponent<HorizontalLayoutGroup>();
            }
        }

        private void Start()
        {
            UpdatePadding();
            SnapToNearestChild(true);
        }

        private void Update()
        {
            if (isSnapping && !isDragging && contentRect != null)
            {
                Vector2 pos = contentRect.anchoredPosition;
                pos.x = Mathf.Lerp(pos.x, targetContentPosX, Time.unscaledDeltaTime * snapSpeed);
                contentRect.anchoredPosition = pos;

                if (Mathf.Abs(pos.x - targetContentPosX) < 1f)
                {
                    pos.x = targetContentPosX;
                    contentRect.anchoredPosition = pos;
                    isSnapping = false;
                }
            }
            else if (!isDragging && !isSnapping && scrollRect != null && Mathf.Abs(scrollRect.velocity.x) < 50f)
            {
                if (Mathf.Abs(scrollRect.velocity.x) > 0.01f)
                {
                    SnapToNearestChild(false);
                }
            }
        }

        public void UpdatePadding()
        {
            if (viewportRect == null || contentRect == null || layoutGroup == null) return;

            Canvas.ForceUpdateCanvases();

            float viewportWidth = viewportRect.rect.width;
            if (viewportWidth <= 0 && transform.parent is RectTransform p)
            {
                viewportWidth = p.rect.width;
            }

            float firstItemWidth = 300f;
            float lastItemWidth = 200f;

            if (contentRect.childCount > 0)
            {
                for (int i = 0; i < contentRect.childCount; i++)
                {
                    Transform c = contentRect.GetChild(i);
                    if (c.gameObject.activeSelf && c is RectTransform rt && rt.rect.width > 10f)
                    {
                        firstItemWidth = rt.rect.width;
                        break;
                    }
                }

                for (int i = contentRect.childCount - 1; i >= 0; i--)
                {
                    Transform c = contentRect.GetChild(i);
                    if (c.gameObject.activeSelf && c is RectTransform rt && rt.rect.width > 10f)
                    {
                        lastItemWidth = rt.rect.width;
                        break;
                    }
                }
            }

            int leftPad = Mathf.Max(0, Mathf.RoundToInt((viewportWidth - firstItemWidth) * 0.5f));
            int rightPad = Mathf.Max(0, Mathf.RoundToInt((viewportWidth - lastItemWidth) * 0.5f));

            layoutGroup.padding.left = leftPad;
            layoutGroup.padding.right = rightPad;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        public void SnapTo(RectTransform child, bool instant = false)
        {
            if (child == null || viewportRect == null || contentRect == null) return;

            UpdatePadding();
            currentFocusedChild = child;

            Vector3 childWorldCenter = child.TransformPoint(child.rect.center);
            Vector3 childInViewport = viewportRect.InverseTransformPoint(childWorldCenter);
            float diffX = childInViewport.x - viewportRect.rect.center.x;

            targetContentPosX = contentRect.anchoredPosition.x - diffX;

            if (scrollRect != null)
            {
                scrollRect.velocity = Vector2.zero;
            }

            if (instant)
            {
                Vector2 pos = contentRect.anchoredPosition;
                pos.x = targetContentPosX;
                contentRect.anchoredPosition = pos;
                isSnapping = false;
            }
            else
            {
                isSnapping = true;
            }
        }

        public void SnapToNearestChild(bool instant = false)
        {
            if (contentRect == null || viewportRect == null || contentRect.childCount == 0) return;

            RectTransform closestChild = null;
            float minDistance = float.MaxValue;
            Vector3 viewportCenterWorld = viewportRect.TransformPoint(viewportRect.rect.center);

            for (int i = 0; i < contentRect.childCount; i++)
            {
                Transform t = contentRect.GetChild(i);
                if (!t.gameObject.activeSelf || !(t is RectTransform rt)) continue;

                Vector3 childCenterWorld = rt.TransformPoint(rt.rect.center);
                float dist = Mathf.Abs(childCenterWorld.x - viewportCenterWorld.x);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestChild = rt;
                }
            }

            if (closestChild != null)
            {
                SnapTo(closestChild, instant);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            isSnapping = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            StartCoroutine(WaitAndSnap());
        }

        private IEnumerator WaitAndSnap()
        {
            yield return new WaitForSeconds(0.08f);
            SnapToNearestChild(false);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (scrollRect == null) return;

            float delta = eventData.scrollDelta.y;
            if (Mathf.Abs(delta) > 0.01f)
            {
                isSnapping = false;
                scrollRect.horizontalNormalizedPosition -= delta * mouseScrollSensitivity;
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);

                StopAllCoroutines();
                StartCoroutine(WaitAfterScroll());
            }
        }

        private IEnumerator WaitAfterScroll()
        {
            yield return new WaitForSeconds(0.18f);
            SnapToNearestChild(false);
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    public class MapItemUI : MonoBehaviour
    {
        [Header("--- NÚT BẤM (BUTTONS) ---")]
        [Tooltip("Nút bắt đầu chơi bản đồ này")]
        [SerializeField] private Button playBtn;

        [Tooltip("Nút mở bảng tạo và chỉnh sửa câu hỏi cho map")]
        [SerializeField] private Button editBtn;

        [Tooltip("Nút xóa bản đồ")]
        [SerializeField] private Button deleteBtn;

        [Header("--- CHỮ HIỂN THỊ (TEXTS) ---")]
        [Tooltip("Text hiển thị tên bản đồ")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Text hiển thị tổng số lượng câu hỏi trong map")]
        [SerializeField] private TMP_Text countText;

        public MapData Data { get; private set; }
        private Action<MapData> onEditAction;
        private Action<MapData> onPlayAction;
        private Action<MapData> onDeleteAction;

        private void Awake()
        {
            AutoFindReferences();

            if (playBtn != null) playBtn.onClick.AddListener(() => onPlayAction?.Invoke(Data));
            if (editBtn != null) editBtn.onClick.AddListener(() => onEditAction?.Invoke(Data));
            if (deleteBtn != null) deleteBtn.onClick.AddListener(() => onDeleteAction?.Invoke(Data));
        }

        private void AutoFindReferences()
        {
            if (playBtn == null)
            {
                Transform t = FindChild(transform, "PlayBtn", "PlayButton", "Play");
                if (t != null) playBtn = t.GetComponent<Button>();
            }

            if (editBtn == null)
            {
                Transform t = FindChild(transform, "EditBtn", "EditButton", "Edit");
                if (t != null) editBtn = t.GetComponent<Button>();
            }

            if (deleteBtn == null)
            {
                Transform t = FindChild(transform, "DeleteBtn", "DeleteButton", "Delete", "DelBtn", "RemoveBtn");
                if (t != null) deleteBtn = t.GetComponent<Button>();
            }

            if (titleText == null)
            {
                Transform t = FindChild(transform, "TitleText", "MapTitleText", "MapTitle", "Title", "NameText");
                if (t != null) titleText = t.GetComponent<TMP_Text>();
            }

            if (countText == null)
            {
                Transform t = FindChild(transform, "CountText", "QuestionCountText", "QuestionCount");
                if (t != null) countText = t.GetComponent<TMP_Text>();
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

        public void Setup(MapData data, Action<MapData> onEdit, Action<MapData> onPlay, Action<MapData> onDelete = null)
        {
            Data = data;
            onEditAction = onEdit;
            onPlayAction = onPlay;
            onDeleteAction = onDelete;

            AutoFindReferences();

            if (playBtn != null)
            {
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => onPlayAction?.Invoke(Data));
            }

            if (editBtn != null)
            {
                editBtn.onClick.RemoveAllListeners();
                editBtn.onClick.AddListener(() => onEditAction?.Invoke(Data));
            }

            if (deleteBtn != null)
            {
                deleteBtn.onClick.RemoveAllListeners();
                deleteBtn.onClick.AddListener(() => onDeleteAction?.Invoke(Data));
            }

            if (titleText != null)
            {
                titleText.text = data.mapName;
            }

            if (countText != null)
            {
                int count = data.questions != null ? data.questions.Count : 0;
                countText.text = $"{count} câu hỏi";
            }
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    public class QuestionItemUI : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN HIỂN THỊ ---")]
        [Tooltip("Text hiển thị nội dung câu hỏi")]
        [SerializeField] private TMP_Text questionText;

        [Tooltip("Text hiển thị đáp án đúng")]
        [SerializeField] private TMP_Text correctText;

        [Tooltip("Đối tượng Selection hiển thị viền/nền khi câu hỏi được chọn")]
        [SerializeField] private GameObject highlight;

        [Tooltip("Nút bấm trên thẻ để chọn câu hỏi này")]
        [SerializeField] private Button selectBtn;

        public QuestionData Data { get; private set; }
        private Action<QuestionData, QuestionItemUI> onClickCallback;

        private void Awake()
        {
            AutoFindReferences();

            if (selectBtn != null)
            {
                selectBtn.onClick.AddListener(HandleClicked);
            }
        }

        private void AutoFindReferences()
        {
            if (questionText == null)
            {
                Transform t = FindChild(transform, "NameQuestText", "QuestionTitleText", "QuestionText", "TitleText");
                if (t != null) questionText = t.GetComponent<TMP_Text>();
            }

            if (correctText == null)
            {
                Transform t = FindChild(transform, "CorrectText", "CorrectAnswerText", "AnswerText");
                if (t != null) correctText = t.GetComponent<TMP_Text>();
            }

            if (highlight == null)
            {
                Transform t = FindChild(transform, "Selection", "SelectionHighlight", "Highlight", "Selected");
                if (t != null) highlight = t.gameObject;
            }

            // Đảm bảo hình ảnh Selection không chặn sự kiện click chuột
            if (highlight != null)
            {
                Image img = highlight.GetComponent<Image>();
                if (img != null) img.raycastTarget = false;
            }

            if (selectBtn == null)
            {
                selectBtn = GetComponent<Button>();
                if (selectBtn == null) selectBtn = gameObject.AddComponent<Button>();
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

        public void Setup(QuestionData data, Action<QuestionData, QuestionItemUI> onClick)
        {
            Data = data;
            onClickCallback = onClick;

            if (questionText != null)
            {
                questionText.text = string.IsNullOrEmpty(data.questionText) ? "Câu hỏi chưa có nội dung" : data.questionText;
            }

            if (correctText != null)
            {
                string[] opts = new string[] { data.option1, data.option2, data.option3, data.option4 };
                int idx = Mathf.Clamp(data.correctIndex, 0, 3);
                string correctAns = opts[idx];
                correctText.text = $"Đáp án đúng: {(char)('A' + idx)}. {correctAns}";
            }

            SetSelected(false);
        }

        /// <summary>
        /// Bật/tắt đối tượng Selection và chỉnh độ trong suốt (Alpha = 1 khi chọn, 0 khi bỏ chọn)
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (highlight != null)
            {
                highlight.SetActive(isSelected);

                // Chỉnh độ trong suốt (Alpha = 1 khi được chọn)
                Image img = highlight.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = false;
                    Color c = img.color;
                    c.a = isSelected ? 1f : 0f;
                    img.color = c;
                }

                CanvasGroup cg = highlight.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = isSelected ? 1f : 0f;
                    cg.blocksRaycasts = false;
                }
            }
        }

        private void HandleClicked()
        {
            onClickCallback?.Invoke(Data, this);
        }
    }
}

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

        [Tooltip("Text hiển thị thời gian làm bài của câu hỏi (TimeText)")]
        [SerializeField] private TMP_Text timeText;

        [Tooltip("Đối tượng Selection hiển thị viền/nền khi câu hỏi được chọn")]
        [SerializeField] private GameObject highlight;

        [Tooltip("Nút bấm trên thẻ để chọn câu hỏi này")]
        [SerializeField] private Button selectBtn;

        [Header("--- GIỚI HẠN HIỂN THỊ DẤU BA CHẤM (...) ---")]
        [Tooltip("Số ký tự tối đa của câu hỏi trên thẻ trước khi cắt thành '...' (0 là không giới hạn bằng code, để TextMeshPro tự xử lý)")]
        [SerializeField] private int maxQuestionLength = 0;

        [Tooltip("Số ký tự tối đa của đáp án đúng trên thẻ trước khi cắt thành '...' (0 là không giới hạn bằng code, để TextMeshPro tự xử lý)")]
        [SerializeField] private int maxAnswerLength = 0;

        [Tooltip("Tự động bật chế độ Overflow = Ellipsis trên TextMeshPro để tự hiện '...' khi chữ dài quá khung")]
        [SerializeField] private bool autoEnableTmpEllipsis = true;

        [Header("--- HIỂN THỊ ĐÁP ÁN ĐÚNG ---")]
        [Tooltip("Hiển thị thêm chữ cái A/B/C/D trước đáp án đúng (Mặc định false: chỉ hiện 'Đáp án: [Nội dung]')")]
        [SerializeField] private bool showOptionLetter = false;

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoFindReferences();
        }
#endif

        private void AutoFindReferences()
        {
            if (questionText == null)
            {
                Transform t = FindChild(transform, "NameQuestText", "QuestionTitleText", "QuestionText", "TitleText");
                if (t != null) questionText = t.GetComponent<TMP_Text>();
            }

            if (timeText == null)
            {
                Transform t = FindChild(transform, "TimeText", "TimerText", "Time", "DurationText", "Duration");
                if (t != null) timeText = t.GetComponent<TMP_Text>();
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

            if (autoEnableTmpEllipsis)
            {
                if (questionText != null) questionText.overflowMode = TextOverflowModes.Ellipsis;
                if (correctText != null) correctText.overflowMode = TextOverflowModes.Ellipsis;
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

            if (questionText == null || timeText == null || correctText == null)
            {
                AutoFindReferences();
            }

            if (questionText != null)
            {
                string qContent = string.IsNullOrEmpty(data.questionText) ? "Câu hỏi chưa có nội dung" : data.questionText;
                if (maxQuestionLength > 0 && qContent.Length > maxQuestionLength)
                {
                    qContent = qContent.Substring(0, maxQuestionLength) + "...";
                }
                questionText.text = qContent;
            }

            float qTime = (data.timeLimit > 0f) ? data.timeLimit : 30f;

            if (timeText != null)
            {
                timeText.text = $"{qTime:0}s";
                timeText.gameObject.SetActive(true);
            }

            if (correctText != null)
            {
                string[] opts = new string[] { data.option1, data.option2, data.option3, data.option4 };
                int idx = Mathf.Clamp(data.correctIndex, 0, 3);
                string correctAns = opts[idx];

                // Nếu đáp án tại idx bị rỗng, tự động tìm đáp án đầu tiên có nội dung
                if (string.IsNullOrWhiteSpace(correctAns))
                {
                    for (int k = 0; k < opts.Length; k++)
                    {
                        if (!string.IsNullOrWhiteSpace(opts[k]))
                        {
                            correctAns = opts[k];
                            idx = k;
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(correctAns))
                {
                    correctAns = "Chưa có";
                }

                if (maxAnswerLength > 0 && !string.IsNullOrEmpty(correctAns) && correctAns.Length > maxAnswerLength)
                {
                    correctAns = correctAns.Substring(0, maxAnswerLength) + "...";
                }

                string ansDisplay = showOptionLetter ? $"{(char)('A' + idx)}. {correctAns}" : correctAns;
                if (timeText != null)
                {
                    correctText.text = $"Đáp án: {ansDisplay}";
                }
                else
                {
                    correctText.text = $"Đáp án: {ansDisplay} | <color=#FFD54F>⏱ {qTime:0}s</color>";
                }
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

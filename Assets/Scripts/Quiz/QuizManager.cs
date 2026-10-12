using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MountainQuest.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("UI References (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] answerButtons; // 4 Nút đáp án
        [SerializeField] private TextMeshProUGUI[] answerTexts; // 4 Text bên trong nút
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillBar;
        [SerializeField] private GameObject quizPanel;

        [Header("Colors (Kahoot Style)")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color correctColor = new Color(0.1f, 0.8f, 0.3f);
        [SerializeField] private Color wrongColor = new Color(0.9f, 0.2f, 0.2f);

        // Events thông báo kết quả ra bên ngoài (Observer Pattern)
        public event Action<bool, QuestionSO> OnQuestionCompleted;

        private QuestionSO currentQuestion;
        private float currentTimer;
        private bool isAnswering;
        private Coroutine timerCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Gán sự kiện click cho 4 nút đáp án
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
        }

        public void DisplayQuestion(QuestionSO questionSO)
        {
            if (questionSO == null) return;

            currentQuestion = questionSO;
            isAnswering = true;
            quizPanel.SetActive(true);

            // Hiển thị nội dung câu hỏi
            questionText.text = questionSO.QuestionText;

            // Hiển thị 4 lựa chọn và reset màu
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < questionSO.Answers.Count)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].interactable = true;
                    answerTexts[i].text = questionSO.Answers[i].AnswerText;
                    answerButtons[i].image.color = normalColor;
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            // Bắt đầu đếm ngược thời gian
            currentTimer = questionSO.TimeLimit;
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(TimerRoutine());
        }

        private IEnumerator TimerRoutine()
        {
            while (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
                if (timerText != null) timerText.text = Mathf.CeilToInt(currentTimer).ToString();
                if (timerFillBar != null) timerFillBar.fillAmount = currentTimer / currentQuestion.TimeLimit;
                yield return null;
            }

            // Hết giờ
            if (isAnswering)
            {
                OnTimeout();
            }
        }

        private void OnAnswerSelected(int selectedIndex)
        {
            if (!isAnswering) return;
            isAnswering = false;

            if (timerCoroutine != null) StopCoroutine(timerCoroutine);

            bool isCorrect = currentQuestion.Answers[selectedIndex].IsCorrect;

            // Khóa các nút và đổi màu phản hồi
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].interactable = false;
                if (currentQuestion.Answers[i].IsCorrect)
                {
                    answerButtons[i].image.color = correctColor;
                }
                else if (i == selectedIndex)
                {
                    answerButtons[i].image.color = wrongColor;
                }
            }

            StartCoroutine(FinishQuestionRoutine(isCorrect));
        }

        private void OnTimeout()
        {
            isAnswering = false;

            // Làm nổi bật đáp án đúng khi hết giờ
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].interactable = false;
                if (currentQuestion.Answers[i].IsCorrect)
                {
                    answerButtons[i].image.color = correctColor;
                }
            }

            StartCoroutine(FinishQuestionRoutine(false));
        }

        private IEnumerator FinishQuestionRoutine(bool isCorrect)
        {
            yield return new WaitForSeconds(1.8f);
            quizPanel.SetActive(false);
            OnQuestionCompleted?.Invoke(isCorrect, currentQuestion);
        }
    }
}

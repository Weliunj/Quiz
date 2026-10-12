using System;
using System.Collections.Generic;
using UnityEngine;

namespace MountainQuest.Quiz
{
    [System.Serializable]
    public class AnswerData
    {
        [TextArea(1, 3)]
        [SerializeField] private string answerText;
        [SerializeField] private bool isCorrect;

        public string AnswerText => answerText;
        public bool IsCorrect => isCorrect;

        public AnswerData(string text, bool correct)
        {
            answerText = text;
            isCorrect = correct;
        }
    }

    [CreateAssetMenu(fileName = "NewQuestionData", menuName = "MountainQuest/Quiz/Question Data", order = 1)]
    public class QuestionSO : ScriptableObject
    {
        [Header("Nội dung câu hỏi")]
        [TextArea(2, 5)]
        [SerializeField] private string questionText;

        [Header("Danh sách 4 đáp án (Chỉ 1 đáp án đúng)")]
        [SerializeField] private List<AnswerData> answers = new List<AnswerData>(4);

        [Header("Thời gian làm bài (Giây)")]
        [Range(5, 60)]
        [SerializeField] private float timeLimit = 15f;

        [Header("Vật phẩm thưởng khi trả lời đúng (Tùy chọn)")]
        [SerializeField] private string rewardItemId;

        public string QuestionText => questionText;
        public IReadOnlyList<AnswerData> Answers => answers;
        public float TimeLimit => timeLimit;
        public string RewardItemId => rewardItemId;

        // Tự động kiểm tra tính hợp lệ trong Inspector (chỉ đúng 1 đáp án)
        private void OnValidate()
        {
            if (answers == null) return;

            // Đảm bảo tối đa 4 đáp án theo quy chuẩn
            if (answers.Count > 4)
            {
                Debug.LogWarning($"[QuestionSO] Câu hỏi '{name}' có hơn 4 đáp án. Khuyến nghị chỉ để đúng 4 lựa chọn!");
            }

            int correctCount = 0;
            foreach (var ans in answers)
            {
                if (ans.IsCorrect) correctCount++;
            }

            if (correctCount != 1)
            {
                Debug.LogWarning($"[QuestionSO] Câu hỏi '{name}' đang có {correctCount} đáp án đúng! Phải có chính xác 1 đáp án đúng.");
            }
        }
    }
}

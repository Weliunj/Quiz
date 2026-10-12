using System.Collections.Generic;
using UnityEngine;

namespace MountainQuest.Quiz
{
    [CreateAssetMenu(fileName = "StationData_01", menuName = "MountainQuest/Levels/Station Data", order = 2)]
    public class StationDataSO : ScriptableObject
    {
        [Header("Thông tin trạm")]
        [SerializeField] private int stationIndex = 1; // Trạm 1, 2, 3...
        [SerializeField] private string stationName = "Trạm 1";
        [TextArea(2, 4)]
        [SerializeField] private string stationDescription;

        [Header("Kho câu hỏi riêng của trạm này")]
        [Tooltip("Kéo thả các QuestionSO phù hợp với trạm này vào đây")]
        [SerializeField] private List<QuestionSO> questionPool = new List<QuestionSO>();

        public int StationIndex => stationIndex;
        public string StationName => stationName;
        public string StationDescription => stationDescription;
        public IReadOnlyList<QuestionSO> QuestionPool => questionPool;

        /// <summary>
        /// Lấy ngẫu nhiên 1 câu hỏi từ kho câu hỏi của trạm, tránh lặp lại câu đã trả lời.
        /// </summary>
        public QuestionSO GetRandomQuestion(HashSet<string> excludedQuestionNames = null)
        {
            if (questionPool == null || questionPool.Count == 0) return null;

            List<QuestionSO> available = new List<QuestionSO>();
            foreach (var q in questionPool)
            {
                if (q == null) continue;
                if (excludedQuestionNames == null || !excludedQuestionNames.Contains(q.name))
                {
                    available.Add(q);
                }
            }

            // Nếu đã trả lời hết kho câu hỏi, reset lại kho để người chơi tiếp tục có câu hỏi
            if (available.Count == 0)
            {
                available.AddRange(questionPool);
            }

            int randomIndex = Random.Range(0, available.Count);
            return available[randomIndex];
        }
    }
}

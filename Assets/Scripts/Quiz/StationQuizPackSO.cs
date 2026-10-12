using System.Collections.Generic;
using UnityEngine;

namespace MountainQuest.Quiz
{
    [CreateAssetMenu(fileName = "NewStationQuizPack", menuName = "MountainQuest/Quiz/Station Quiz Pack", order = 2)]
    public class StationQuizPackSO : ScriptableObject
    {
        [Header("Thông tin trạm")]
        [SerializeField] private int stationId;
        [SerializeField] private string stationName;

        [Header("Danh sách câu hỏi của trạm")]
        [SerializeField] private List<QuestionSO> questions = new List<QuestionSO>();

        public int StationId => stationId;
        public string StationName => stationName;
        public IReadOnlyList<QuestionSO> Questions => questions;

        // Trả về 1 câu hỏi ngẫu nhiên hoặc theo thứ tự
        public QuestionSO GetRandomQuestion()
        {
            if (questions == null || questions.Count == 0) return null;
            int randomIndex = Random.Range(0, questions.Count);
            return questions[randomIndex];
        }
    }
}

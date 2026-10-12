using System.Collections.Generic;
using UnityEngine;

namespace MountainQuest.Quiz
{
    [CreateAssetMenu(fileName = "MountainLevel_01", menuName = "MountainQuest/Levels/Mountain Level", order = 3)]
    public class MountainLevelSO : ScriptableObject
    {
        [Header("Thông tin ngọn núi (Level)")]
        [SerializeField] private int mountainLevelIndex = 1;
        [SerializeField] private string mountainName = "Ngọn Núi 1: Đỉnh Sương Khói";
        [TextArea(2, 4)]
        [SerializeField] private string mountainStory;
        [SerializeField] private Sprite mountainMapBackground;

        [Header("Danh sách các trạm của ngọn núi này")]
        [Tooltip("Kéo thả các StationDataSO của ngọn núi này theo thứ tự 1, 2, 3...")]
        [SerializeField] private List<StationDataSO> stations = new List<StationDataSO>();

        public int MountainLevelIndex => mountainLevelIndex;
        public string MountainName => mountainName;
        public string MountainStory => mountainStory;
        public Sprite MountainMapBackground => mountainMapBackground;
        public IReadOnlyList<StationDataSO> Stations => stations;

        public StationDataSO GetStation(int index)
        {
            if (index >= 0 && index < stations.Count)
            {
                return stations[index];
            }
            return null;
        }
    }
}

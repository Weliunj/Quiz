using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MountainQuest.Quiz
{
    [Serializable]
    public class PlayerProgressData
    {
        public int currentMountainLevel = 1;
        public int currentStationIndex = 0;
        public int currentHearts = 5;
        public int totalScore = 0;
        public List<string> answeredQuestionNames = new List<string>();
        public List<string> inventoryItemIds = new List<string>();
    }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "MountainQuestProgress.json");

        public static void SaveProgress(PlayerProgressData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Đã lưu tiến trình thành công tại: {SavePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi lưu dữ liệu: {ex.Message}");
            }
        }

        public static PlayerProgressData LoadProgress()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);
                    Debug.Log("[SaveSystem] Đã tải tiến trình chơi đã lưu.");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Lỗi đọc dữ liệu lưu: {ex.Message}");
            }

            // Trả về dữ liệu mới mặc định nếu chưa có file lưu
            return new PlayerProgressData();
        }

        public static void ClearSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Đã reset dữ liệu chơi.");
            }
        }
    }
}

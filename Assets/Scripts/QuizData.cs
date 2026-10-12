using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuizGame
{
    [Serializable]
    public class QuestionData
    {
        public string id;
        public string questionText;
        public string option1;
        public string option2;
        public string option3;
        public string option4;
        public int correctIndex; // 0 = A, 1 = B, 2 = C, 3 = D

        public QuestionData()
        {
            id = Guid.NewGuid().ToString();
            questionText = "";
            option1 = "";
            option2 = "";
            option3 = "";
            option4 = "";
            correctIndex = 0;
        }

        public QuestionData(string question, string opt1, string opt2, string opt3, string opt4, int correct = 0)
        {
            id = Guid.NewGuid().ToString();
            questionText = question;
            option1 = opt1;
            option2 = opt2;
            option3 = opt3;
            option4 = opt4;
            correctIndex = correct;
        }
    }

    [Serializable]
    public class MapData
    {
        public string id;
        public string mapName;
        public int stationCount = 5; // Số lượng trạm / tầng leo đến đỉnh núi
        public List<QuestionData> questions = new List<QuestionData>();

        public MapData()
        {
            id = Guid.NewGuid().ToString();
            mapName = "Bản đồ mới";
            stationCount = 5;
            questions = new List<QuestionData>();
        }

        public MapData(string name, int stations = 5)
        {
            id = Guid.NewGuid().ToString();
            mapName = name;
            stationCount = Mathf.Max(1, stations);
            questions = new List<QuestionData>();
        }
    }

    [Serializable]
    public class GameSaveData
    {
        public List<MapData> maps = new List<MapData>();
    }

    public static class QuizSaveSystem
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "quiz_save.json");

        public static GameSaveData Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    return new GameSaveData();
                }

                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new GameSaveData();
                }

                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                return data != null ? data : new GameSaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuizSaveSystem] Lỗi khi tải dữ liệu: {ex.Message}");
                return new GameSaveData();
            }
        }

        public static void Save(GameSaveData data)
        {
            try
            {
                if (data == null) data = new GameSaveData();
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[QuizSaveSystem] Đã lưu dữ liệu thành công vào: {SavePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuizSaveSystem] Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        public static void ClearAllSaveData()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                string oldPath = Path.Combine(Application.persistentDataPath, "quiz_data.json");
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }

                Debug.Log("[QuizSaveSystem] Đã xóa toàn bộ file save thành công!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuizSaveSystem] Lỗi khi xóa file save: {ex.Message}");
            }
        }
    }

#if UNITY_EDITOR
    public static class QuizSaveSystemEditor
    {
        [MenuItem("Tools/Quiz/Clear All Save Data (Xóa toàn bộ dữ liệu lưu để test lại)")]
        public static void ClearSaveData()
        {
            QuizSaveSystem.ClearAllSaveData();
            EditorUtility.DisplayDialog("Thông Báo", "Đã xóa sạch toàn bộ file save!\nBây giờ bạn có thể nhấn Play để test như lần đầu tiên tạo map.", "OK");
        }

        [MenuItem("Tools/Quiz/Open Save Data Folder (Mở thư mục lưu dữ liệu)")]
        public static void OpenSaveFolder()
        {
            if (!Directory.Exists(Application.persistentDataPath))
            {
                Directory.CreateDirectory(Application.persistentDataPath);
            }
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
#endif
}

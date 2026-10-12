using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using MountainQuest.Quiz;

namespace MountainQuest.EditorTools
{
    public static class SampleDataGenerator
    {
        [MenuItem("MountainQuest/Tools/Tự Động Tạo Dữ Liệu Mẫu (Núi - Trạm - Câu Hỏi)", false, 1)]
        public static void GenerateAllSampleData()
        {
            EnsureDirectories();

            // 1. Tạo các câu hỏi cho Trạm 1 (Chân núi)
            QuestionSO q1_1 = CreateQuestion(
                "Assets/ScriptableObjects/Questions/M1_Station_01/Q_DongGoiBalo.asset",
                "Khi đóng gói ba lô leo núi, nguyên tắc đặt đồ vật nặng là gì?",
                new List<AnswerData>()
                {
                    new AnswerData("Đặt gần lưng và ở giữa/trên", true),
                    new AnswerData("Đặt ở tận đáy ba lô", false),
                    new AnswerData("Treo lủng lẳng bên ngoài túi", false),
                    new AnswerData("Càng nhiều đồ sắt càng tốt", false)
                },
                15f, "item_shield"
            );

            QuestionSO q1_2 = CreateQuestion(
                "Assets/ScriptableObjects/Questions/M1_Station_01/Q_LaBanHuongBac.asset",
                "Khi kim la bàn đứng yên tự do, đầu kim chữ 'N' chỉ về hướng nào?",
                new List<AnswerData>()
                {
                    new AnswerData("Hướng Bắc", true),
                    new AnswerData("Hướng Nam", false),
                    new AnswerData("Hướng Đông", false),
                    new AnswerData("Hướng Tây", false)
                },
                15f, "item_food"
            );

            // 2. Tạo các câu hỏi cho Trạm 2 (Vách suối)
            QuestionSO q2_1 = CreateQuestion(
                "Assets/ScriptableObjects/Questions/M1_Station_02/Q_NuocDongBang.asset",
                "Nước nguyên chất đóng băng ở nhiệt độ nào dưới áp suất tiêu chuẩn?",
                new List<AnswerData>()
                {
                    new AnswerData("0°C", true),
                    new AnswerData("100°C", false),
                    new AnswerData("-10°C", false),
                    new AnswerData("37°C", false)
                },
                15f, "item_water"
            );

            QuestionSO q2_2 = CreateQuestion(
                "Assets/ScriptableObjects/Questions/M1_Station_02/Q_NuocTrongCoThe.asset",
                "Cơ thể con người chiếm trung bình khoảng bao nhiêu phần trăm là nước?",
                new List<AnswerData>()
                {
                    new AnswerData("Khoảng 60% - 70%", true),
                    new AnswerData("Khoảng 20% - 30%", false),
                    new AnswerData("Khoảng 90% - 95%", false),
                    new AnswerData("Khoảng 40% - 45%", false)
                },
                15f, "item_meat"
            );

            // 3. Tạo các câu hỏi cho Trạm 3 (Đỉnh băng)
            QuestionSO q3_1 = CreateQuestion(
                "Assets/ScriptableObjects/Questions/M1_Station_03/Q_NhietDoSoiTrenCao.asset",
                "Tại sao nước sôi ở nhiệt độ dưới 100°C khi lên núi cao?",
                new List<AnswerData>()
                {
                    new AnswerData("Do áp suất khí quyển giảm", true),
                    new AnswerData("Do trời quá lạnh", false),
                    new AnswerData("Do nước tinh khiết hơn", false),
                    new AnswerData("Do có tuyết rơi", false)
                },
                15f, "item_campfire"
            );

            // 4. Tạo các Trạm (StationDataSO) và gán câu hỏi vào
            StationDataSO station1 = CreateStation(
                "Assets/ScriptableObjects/Stations/M1_Station_01.asset",
                1, "Trạm 1: Chân Núi Rừng Xanh",
                "Cổng xuất phát hành trình leo núi với rừng thông đại thụ.",
                new List<QuestionSO> { q1_1, q1_2 }
            );

            StationDataSO station2 = CreateStation(
                "Assets/ScriptableObjects/Stations/M1_Station_02.asset",
                2, "Trạm 2: Vách Đá Ven Suối",
                "Dòng suối nước chảy xiết và vách đá dốc hiểm trở.",
                new List<QuestionSO> { q2_1, q2_2 }
            );

            StationDataSO station3 = CreateStation(
                "Assets/ScriptableObjects/Stations/M1_Station_03.asset",
                3, "Trạm 3: Đỉnh Mây Băng Giá",
                "Độ cao 3.000m nơi biển mây bao la và gió tuyết cuồn cuộn.",
                new List<QuestionSO> { q3_1 }
            );

            // 5. Tạo Ngọn Núi 1 (MountainLevelSO) và gán các Trạm vào
            MountainLevelSO mountain1 = CreateMountain(
                "Assets/ScriptableObjects/Mountains/Mountain_01.asset",
                1, "Ngọn Núi 1: Đỉnh Mây Huyền Thoại",
                "Hành trình chinh phục ngọn núi đầu tiên dành cho các tân binh thám hiểm.",
                new List<StationDataSO> { station1, station2, station3 }
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành công!", 
                "Đã tự động tạo và sắp xếp hoàn chỉnh dữ liệu mẫu cho:\n" +
                "- Ngọn Núi: Mountain_01\n" +
                "- 3 Trạm: M1_Station_01, M1_Station_02, M1_Station_03\n" +
                "- 5 Câu hỏi chuẩn 1/4 tương ứng với từng trạm!\n\n" +
                "Bạn có thể mở thư mục ScriptableObjects để kiểm tra.", "Tuyệt vời!");
        }

        private static void EnsureDirectories()
        {
            CreateDirIfNeeded("Assets/ScriptableObjects");
            CreateDirIfNeeded("Assets/ScriptableObjects/Mountains");
            CreateDirIfNeeded("Assets/ScriptableObjects/Stations");
            CreateDirIfNeeded("Assets/ScriptableObjects/Questions");
            CreateDirIfNeeded("Assets/ScriptableObjects/Questions/M1_Station_01");
            CreateDirIfNeeded("Assets/ScriptableObjects/Questions/M1_Station_02");
            CreateDirIfNeeded("Assets/ScriptableObjects/Questions/M1_Station_03");
        }

        private static void CreateDirIfNeeded(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static QuestionSO CreateQuestion(string path, string text, List<AnswerData> answers, float timeLimit, string rewardId)
        {
            QuestionSO so = AssetDatabase.LoadAssetAtPath<QuestionSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<QuestionSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            SetField(so, "questionText", text);
            SetField(so, "answers", answers);
            SetField(so, "timeLimit", timeLimit);
            SetField(so, "rewardItemId", rewardId);

            EditorUtility.SetDirty(so);
            return so;
        }

        private static StationDataSO CreateStation(string path, int index, string name, string desc, List<QuestionSO> questions)
        {
            StationDataSO so = AssetDatabase.LoadAssetAtPath<StationDataSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<StationDataSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            SetField(so, "stationIndex", index);
            SetField(so, "stationName", name);
            SetField(so, "stationDescription", desc);
            SetField(so, "questionPool", questions);

            EditorUtility.SetDirty(so);
            return so;
        }

        private static MountainLevelSO CreateMountain(string path, int index, string name, string story, List<StationDataSO> stations)
        {
            MountainLevelSO so = AssetDatabase.LoadAssetAtPath<MountainLevelSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<MountainLevelSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            SetField(so, "mountainLevelIndex", index);
            SetField(so, "mountainName", name);
            SetField(so, "mountainStory", story);
            SetField(so, "stations", stations);

            EditorUtility.SetDirty(so);
            return so;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}

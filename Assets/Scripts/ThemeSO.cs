using System.Collections.Generic;
using UnityEngine;

namespace QuizGame
{
    [CreateAssetMenu(fileName = "NewTheme", menuName = "Quiz/Theme Data", order = 2)]
    public class ThemeSO : ScriptableObject
    {
        [Tooltip("Mã định danh Theme (ví dụ: theme_forest, theme_desert...)")]
        public string themeId = "theme_forest";

        [Tooltip("Tên hiển thị của Theme (ví dụ: Rừng Xanh, Sa Mạc, Băng Tuyết...)")]
        public string themeName = "Rừng Xanh";

        [Tooltip("Danh sách các ItemSO thuộc theme này")]
        public List<ItemSO> themeItems = new List<ItemSO>();

        [Tooltip("Ảnh nền bản đồ / ngọn núi (Map Background) theo theme này")]
        public Sprite mapBg;

        [Tooltip("Màu nhận diện đặc trưng của theme (tùy chọn)")]
        public Color themeColor = Color.white;
    }
}

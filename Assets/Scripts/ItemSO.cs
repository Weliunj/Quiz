using UnityEngine;

namespace QuizGame
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Quiz/Item Data", order = 1)]
    public class ItemSO : ScriptableObject
    {
        [Tooltip("Mã định danh của Item (ví dụ: leaf, potion, gem...)")]
        public string itemId = "item_1";

        [Tooltip("Tên hiển thị của Item")]
        public string itemName = "Item Mới";

        [Tooltip("Hình ảnh Icon hiển thị trên ItemImg")]
        public Sprite icon;
    }
}

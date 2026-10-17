using System.Collections.Generic;
using UnityEngine;

namespace QuizGame
{
    [CreateAssetMenu(fileName = "NewDisaster", menuName = "Quiz/Disaster Data", order = 3)]
    public class DisasterSO : ScriptableObject
    {
        [Tooltip("Mã định danh thiên tai (ví dụ: avalanche, sleet, bear...)")]
        public string disasterId = "disaster_1";

        [Tooltip("Tên hiển thị thiên tai")]
        public string disasterName = "Thiên Tai Mới";

        [Tooltip("Hình ảnh thiên tai hiển thị trên NaturalDisastersPanel/Image")]
        public Sprite disasterSprite;

        [Header("--- VẬT PHẨM ĐỐI PHÓ (KÉO THẢ ITEM SO) ---")]
        [Tooltip("Danh sách các vật phẩm khắc chế được thiên tai này (chọn 1 trong các item này đều thành công và không bị trừ HP)")]
        public List<ItemSO> counterItems = new List<ItemSO>();

        [Header("--- MÃ ID DỰ PHÒNG (NẾU DÙNG CHUỖI STRING) ---")]
        [Tooltip("Mã itemId dự phòng (ví dụ: item_bearspray)")]
        public string counterItemId;

        [Tooltip("Danh sách mã itemId dự phòng")]
        public List<string> counterItemIds = new List<string>();

        [Header("--- TỶ LỆ THÀNH CÔNG (0.0 -> 1.0) ---")]
        [Tooltip("Cơ hội thành công khi người chơi dùng đúng vật phẩm đối phó (0.0 = 0%, 1.0 = 100%, ví dụ 0.8 là 80% thành công)")]
        [Range(0f, 1f)]
        public float successChance = 1f;

        [Header("--- THÔNG ĐIỆP DIALOG CHO THIÊN TAI NÀY ---")]
        [TextArea(2, 4)]
        [Tooltip("Nội dung cảnh báo khi thiên tai xuất hiện (để trống sẽ dùng mặc định. Hỗ trợ thẻ {disaster})")]
        public string warningMessage = "";

        [TextArea(2, 4)]
        [Tooltip("Nội dung khi đối phó THÀNH CÔNG (để trống sẽ dùng mặc định. Hỗ trợ thẻ {item}, {disaster}, {hp}, {maxhp})")]
        public string successMessage = "";

        [TextArea(2, 4)]
        [Tooltip("Nội dung khi đối phó THẤT BẠI (để trống sẽ dùng mặc định. Hỗ trợ thẻ {item}, {disaster}, {hp}, {maxhp})")]
        public string failureMessage = "";

        [TextArea(2, 4)]
        [Tooltip("Nội dung khi dùng ĐÚNG vật phẩm nhưng KHÔNG MAY MẮN thất bại do xác suất rủi ro (để trống sẽ dùng thông báo mặc định)")]
        public string badLuckFailureMessage = "";

        /// <summary>
        /// Lấy toàn bộ danh sách mã item khắc chế hợp lệ (kết hợp cả counterItems dạng ScriptableObject và chuỗi string)
        /// </summary>
        public List<string> GetAllValidCounterItems()
        {
            List<string> list = new List<string>();

            // 1. Lấy từ danh sách ItemSO kéo thả
            if (counterItems != null)
            {
                foreach (var item in counterItems)
                {
                    if (item != null)
                    {
                        string id = !string.IsNullOrEmpty(item.itemId) ? item.itemId : item.name;
                        if (!list.Contains(id)) list.Add(id);
                    }
                }
            }

            // 2. Lấy từ danh sách string dự phòng
            if (counterItemIds != null)
            {
                foreach (var id in counterItemIds)
                {
                    if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                        list.Add(id);
                }
            }

            // 3. Lấy từ counterItemId đơn lẻ
            if (!string.IsNullOrEmpty(counterItemId) && !list.Contains(counterItemId))
            {
                list.Add(counterItemId);
            }

            return list;
        }
    }
}

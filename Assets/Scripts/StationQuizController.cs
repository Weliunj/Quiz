using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame
{
    /// <summary>
    /// Quản lý giao diện trạm dừng (StationPanel) và câu hỏi (QuestPanel).
    /// </summary>
    public class StationQuizController : MonoBehaviour
    {
        [Header("--- 1. CÁC PANEL CHÍNH ---")]
        [Tooltip("Panel trạm (StationPanel) chứa danh sách item")]
        [SerializeField] private GameObject stationPanel;

        [Tooltip("Panel hiển thị câu hỏi (QuestPanel)")]
        [SerializeField] private GameObject questPanel;

        [Tooltip("Vùng chứa các Item trong StationPanel")]
        [SerializeField] private Transform itemContainer;

        [Header("--- 2. NÚT ĐIỀU HƯỚNG TRẠM ---")]
        [Tooltip("Nút Join để vào trạm (hiển thị khi vừa tới trạm)")]
        [SerializeField] private Button joinBtn;

        [Tooltip("Nút rời khỏi trạm (ExitBtn)")]
        [SerializeField] private Button exitBtn;

        [Tooltip("Nút bỏ qua thời gian làm câu hỏi (SkipTimeBtn)")]
        [SerializeField] private Button skipTimeBtn;

        [Tooltip("Text hiển thị số câu hỏi / item còn lại (RemainText)")]
        [SerializeField] private TMP_Text remainText;

        [Header("--- 3. THÀNH PHẦN CÂU HỎI (QUEST PANEL) ---")]
        [Tooltip("Text nội dung câu hỏi")]
        [SerializeField] private TMP_Text questionText;

        [Tooltip("Text đếm ngược thời gian")]
        [SerializeField] private TMP_Text timeText;

        [Tooltip("Danh sách 4 nút đáp án")]
        [SerializeField] private Button[] answerButtons = new Button[4];

        [Header("--- 4. THÔNG SỐ ĐẾM GIỜ & THỜI GIAN ---")]
        [Tooltip("Thời gian làm bài cho mỗi câu hỏi (giây)")]
        [SerializeField] private float questionTimeLimit = 20f;

        private MapData currentMap;
        private List<QuestionData> poolQuestions = new List<QuestionData>();
        private List<QuestionData> unusedQuestions = new List<QuestionData>();

        private List<Button> spawnedItems = new List<Button>();
        private Button currentActiveItemBtn;

        private Coroutine timerCoroutine;
        private float currentTimeLeft = 0f;
        private bool isAnswering = false;

        // Lưu danh sách đáp án random của câu hỏi hiện tại
        private int currentCorrectShuffledIndex = 0;

        private void Awake()
        {
            AutoFindReferences();
            BindEvents();
        }

        public void AutoFindReferences()
        {
            Transform root = transform;

            if (stationPanel == null)
            {
                Transform t = FindChildRecursive(root, "StationPanel");
                if (t != null) stationPanel = t.gameObject;
            }

            if (stationPanel != null)
            {
                if (itemContainer == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "ItemContainer");
                    if (t != null) itemContainer = t;
                }

                if (questPanel == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "QuestPanel");
                    if (t != null) questPanel = t.gameObject;
                }

                if (exitBtn == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "ExitBtn", "ExitButton", "CloseBtn");
                    if (t != null) exitBtn = t.GetComponent<Button>();
                }

                if (remainText == null)
                {
                    Transform t = FindChildRecursive(stationPanel.transform, "RemainText", "Remain", "StatusText");
                    if (t != null) remainText = t.GetComponent<TMP_Text>();
                }
            }

            if (questPanel != null)
            {
                if (questionText == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "QuestionText", "Question");
                    if (t != null) questionText = t.GetComponent<TMP_Text>();
                }

                if (timeText == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "TimeText", "TimerText", "Time");
                    if (t != null) timeText = t.GetComponent<TMP_Text>();
                }

                if (skipTimeBtn == null)
                {
                    Transform t = FindChildRecursive(questPanel.transform, "SkipTimeBtn", "SkipBtn", "Skip");
                    if (t != null) skipTimeBtn = t.GetComponent<Button>();
                }

                // Tìm 4 nút D1Btn, D1Btn (1), D1Btn (2), D1Btn (3)
                if (answerButtons == null || answerButtons.Length != 4 || answerButtons[0] == null)
                {
                    answerButtons = new Button[4];
                    string[] btnNames = new string[] { "D1Btn", "D1Btn (1)", "D1Btn (2)", "D1Btn (3)" };
                    for (int i = 0; i < 4; i++)
                    {
                        Transform t = FindChildRecursive(questPanel.transform, btnNames[i], $"AnswerBtn_{i + 1}", $"Option_{i + 1}");
                        if (t != null) answerButtons[i] = t.GetComponent<Button>();
                    }
                }
            }

            // Tìm nút JoinBtn (có thể nằm ở GamePanel hoặc MainPanel)
            if (joinBtn == null)
            {
                Transform t = FindChildRecursive(root, "JoinBtn", "JoinButton", "Join");
                if (t != null) joinBtn = t.GetComponent<Button>();
            }
        }

        private Transform FindChildRecursive(Transform parent, params string[] names)
        {
            if (parent == null) return null;
            foreach (var n in names)
            {
                Transform t = parent.Find(n);
                if (t != null) return t;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), names);
                if (found != null) return found;
            }
            return null;
        }

        private void BindEvents()
        {
            if (joinBtn != null)
            {
                joinBtn.onClick.RemoveAllListeners();
                joinBtn.onClick.AddListener(OnJoinButtonClicked);
            }

            if (exitBtn != null)
            {
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(OnExitButtonClicked);
            }

            if (skipTimeBtn != null)
            {
                skipTimeBtn.onClick.RemoveAllListeners();
                skipTimeBtn.onClick.AddListener(OnSkipTimeClicked);
            }

            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    int index = i;
                    if (answerButtons[i] != null)
                    {
                        answerButtons[i].onClick.RemoveAllListeners();
                        answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                    }
                }
            }
        }

        /// <summary>
        /// Khởi tạo khi vào một Map mới
        /// </summary>
        public void InitForMap(MapData map)
        {
            currentMap = map;
            poolQuestions.Clear();
            unusedQuestions.Clear();

            if (map != null && map.questions != null)
            {
                poolQuestions.AddRange(map.questions);
                unusedQuestions.AddRange(map.questions);
            }

            HideAllStationPanels();
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);
        }

        /// <summary>
        /// Khi người chơi di chuyển đến trạm bất kỳ -> Hiển thị nút Join
        /// </summary>
        public void ShowJoinPrompt(int stationIndex)
        {
            AutoFindReferences();
            if (joinBtn != null)
            {
                joinBtn.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Ẩn toàn bộ khi rời khỏi map
        /// </summary>
        public void HideAllStationPanels()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            if (stationPanel != null) stationPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);
            isAnswering = false;
        }

        /// <summary>
        /// Bấm nút Join để mở StationPanel
        /// </summary>
        public void OnJoinButtonClicked()
        {
            if (joinBtn != null) joinBtn.gameObject.SetActive(false);

            if (stationPanel != null)
            {
                stationPanel.SetActive(true);
            }

            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }

            SetupStationItems();
            UpdateRemainText();
        }

        /// <summary>
        /// Bấm nút ExitBtn để rời khỏi StationPanel
        /// </summary>
        public void OnExitButtonClicked()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            if (questPanel != null) questPanel.SetActive(false);
            if (stationPanel != null) stationPanel.SetActive(false);

            // Cho phép hiển thị lại nút Join nếu người chơi vẫn ở trạm đó
            if (joinBtn != null) joinBtn.gameObject.SetActive(true);
        }

        /// <summary>
        /// Khởi tạo các item có sẵn trong ItemContainer (tự tìm các Button item có sẵn)
        /// </summary>
        private void SetupStationItems()
        {
            spawnedItems.Clear();

            if (itemContainer != null)
            {
                Button[] childButtons = itemContainer.GetComponentsInChildren<Button>(true);
                foreach (var btn in childButtons)
                {
                    spawnedItems.Add(btn);
                    btn.gameObject.SetActive(true);
                    SetButtonOpacity(btn, 1f);
                    btn.interactable = true;

                    btn.onClick.RemoveAllListeners();
                    Button capturedBtn = btn;
                    btn.onClick.AddListener(() => OnItemClicked(capturedBtn));
                }
            }
        }

        /// <summary>
        /// Bấm vào 1 Item trong StationPanel để mở QuestPanel và hiển thị câu hỏi ngẫu nhiên
        /// </summary>
        public void OnItemClicked(Button itemBtn)
        {
            currentActiveItemBtn = itemBtn;

            QuestionData q = PickNextQuestion();
            if (q == null)
            {
                Debug.LogWarning("[StationQuizController] Không có câu hỏi nào trong danh sách!");
                return;
            }

            if (questPanel != null)
            {
                questPanel.SetActive(true);
            }

            DisplayQuestion(q);
        }

        /// <summary>
        /// Lấy ngẫu nhiên câu hỏi từ danh sách, tránh trùng lặp cho đến khi hết danh sách thì xáo lại
        /// </summary>
        private QuestionData PickNextQuestion()
        {
            if (poolQuestions.Count == 0) return null;

            if (unusedQuestions.Count == 0)
            {
                // Đã trả lời hết danh sách -> Reset lại danh sách để tiếp tục
                unusedQuestions.AddRange(poolQuestions);
            }

            int randomIndex = UnityEngine.Random.Range(0, unusedQuestions.Count);
            QuestionData picked = unusedQuestions[randomIndex];
            unusedQuestions.RemoveAt(randomIndex);
            return picked;
        }

        /// <summary>
        /// Hiển thị câu hỏi và xáo ngẫu nhiên 4 đáp án
        /// </summary>
        private void DisplayQuestion(QuestionData q)
        {
            isAnswering = true;

            if (questionText != null)
            {
                questionText.text = q.questionText;
            }

            // Chuẩn bị 4 đáp án và vị trí đúng
            string[] rawOptions = new string[] { q.option1, q.option2, q.option3, q.option4 };
            int originalCorrect = Mathf.Clamp(q.correctIndex, 0, 3);

            // Xáo ngẫu nhiên thứ tự 4 đáp án
            List<int> shuffledIndices = new List<int> { 0, 1, 2, 3 };
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                int rnd = UnityEngine.Random.Range(i, shuffledIndices.Count);
                int tmp = shuffledIndices[i];
                shuffledIndices[i] = shuffledIndices[rnd];
                shuffledIndices[rnd] = tmp;
            }

            currentSelectedIndex = -1;

            for (int i = 0; i < 4; i++)
            {
                int optIdx = shuffledIndices[i];
                if (optIdx == originalCorrect)
                {
                    currentCorrectShuffledIndex = i;
                }

                if (answerButtons != null && i < answerButtons.Length && answerButtons[i] != null)
                {
                    answerButtons[i].interactable = true;

                    Image btnImg = answerButtons[i].GetComponent<Image>();
                    if (btnImg != null) btnImg.color = defaultButtonColor;

                    TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>(true);
                    if (btnText != null)
                    {
                        btnText.text = rawOptions[optIdx];
                    }
                }
            }

            // Khởi động đếm ngược thời gian
            currentTimeLeft = questionTimeLimit;
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(CountdownRoutine());
        }

        private Color defaultButtonColor = Color.white;
        private Color selectedHighlightColor = new Color(0.65f, 0.85f, 1f, 1f); // Màu xanh dương nhạt khi đang chọn
        private Color correctColor = new Color(0.2f, 0.85f, 0.3f, 1f);          // Màu xanh lá khi đúng
        private Color wrongColor = new Color(0.95f, 0.28f, 0.28f, 1f);          // Màu đỏ khi sai
        private int currentSelectedIndex = -1;

        private IEnumerator CountdownRoutine()
        {
            while (currentTimeLeft > 0f)
            {
                if (timeText != null)
                {
                    timeText.text = Mathf.CeilToInt(currentTimeLeft).ToString();
                }

                yield return new WaitForSeconds(1f);
                currentTimeLeft -= 1f;
            }

            // Hết giờ
            if (timeText != null) timeText.text = "0";
            StartCoroutine(OnQuestionEndRoutine());
        }

        /// <summary>
        /// Bấm nút Skip: giảm thời gian xuống còn đúng 3 giây
        /// </summary>
        public void OnSkipTimeClicked()
        {
            if (!isAnswering) return;

            if (currentTimeLeft > 3f)
            {
                currentTimeLeft = 3f;
                if (timeText != null) timeText.text = "3";
                Debug.Log("[StationQuizController] Đã bấm SkipTimeBtn: Thời gian giảm còn 3s!");
            }
        }

        /// <summary>
        /// Khi người chơi bấm chọn 1 đáp án:
        /// - Chưa biết đúng sai!
        /// - Có thể tự do đổi sang đáp án khác bất kỳ lúc nào khi còn thời gian.
        /// - Đổi màu highlight cho đáp án đang chọn.
        /// </summary>
        public void OnAnswerSelected(int selectedIndex)
        {
            if (!isAnswering) return;

            currentSelectedIndex = selectedIndex;

            // Highlight nút đang chọn và reset lại các nút khác
            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    if (answerButtons[i] != null)
                    {
                        Image img = answerButtons[i].GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = (i == currentSelectedIndex) ? selectedHighlightColor : defaultButtonColor;
                        }
                    }
                }
            }

            Debug.Log($"[StationQuizController] Người chơi đã chọn đáp án #{selectedIndex + 1} (chưa kiểm tra đúng sai, có thể đổi tiếp)!");
        }

        /// <summary>
        /// Khi hết thời gian (hoặc sau khi bấm Skip đếm hết 3s về 0):
        /// - Khóa đáp án.
        /// - Đổi màu đáp án (Xanh nếu đúng, Đỏ nếu sai).
        /// - Nếu không chọn gì: hiện đáp án đúng nhưng hiển thị màu Đỏ.
        /// - Chờ 1 giây để người chơi nhìn rõ kết quả rồi mới đóng QuestPanel!
        /// </summary>
        private IEnumerator OnQuestionEndRoutine()
        {
            if (!isAnswering) yield break;
            isAnswering = false;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            // Khóa tương tác toàn bộ nút đáp án
            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    if (answerButtons[i] != null) answerButtons[i].interactable = false;
                }
            }

            bool hasAnswered = (currentSelectedIndex >= 0);
            bool isCorrect = (hasAnswered && currentSelectedIndex == currentCorrectShuffledIndex);

            if (!hasAnswered)
            {
                // Người chơi KHÔNG CHỌN GÌ mà hết giờ -> show đáp án đúng nhưng tô màu ĐỎ
                Debug.Log("[StationQuizController] Không chọn đáp án: Show đáp án đúng nhưng tô màu ĐỎ!");
                if (answerButtons != null && currentCorrectShuffledIndex >= 0 && currentCorrectShuffledIndex < answerButtons.Length)
                {
                    Image correctImg = answerButtons[currentCorrectShuffledIndex].GetComponent<Image>();
                    if (correctImg != null) correctImg.color = wrongColor;
                }
            }
            else
            {
                if (isCorrect)
                {
                    // Trả lời ĐÚNG -> tô màu Xanh
                    Debug.Log("[StationQuizController] Trả lời ĐÚNG: Tô màu XANH!");
                    if (answerButtons != null && currentSelectedIndex < answerButtons.Length)
                    {
                        Image img = answerButtons[currentSelectedIndex].GetComponent<Image>();
                        if (img != null) img.color = correctColor;
                    }
                }
                else
                {
                    // Trả lời SAI -> đáp án người chọn tô Đỏ, đáp án đúng tô Xanh
                    Debug.Log("[StationQuizController] Trả lời SAI: Đáp án chọn tô ĐỎ, đáp án đúng tô XANH!");
                    if (answerButtons != null && currentSelectedIndex < answerButtons.Length)
                    {
                        Image img = answerButtons[currentSelectedIndex].GetComponent<Image>();
                        if (img != null) img.color = wrongColor;
                    }
                    if (answerButtons != null && currentCorrectShuffledIndex >= 0 && currentCorrectShuffledIndex < answerButtons.Length)
                    {
                        Image correctImg = answerButtons[currentCorrectShuffledIndex].GetComponent<Image>();
                        if (correctImg != null) correctImg.color = correctColor;
                    }
                }
            }

            // Đợi 1 giây hiển thị kết quả cho người chơi xem
            yield return new WaitForSeconds(1.0f);

            // Thực hiện kết quả và đóng QuestPanel
            if (isCorrect)
            {
                HandleCorrectAnswer();
            }
            else if (!hasAnswered)
            {
                HandleTimeout();
            }
            else
            {
                HandleWrongAnswer();
            }
        }

        /// <summary>
        /// Trả lời ĐÚNG:
        /// - Giảm số remain còn 2/3 (hoặc giảm số item yêu cầu)
        /// - Item (nút đó) biến mất (SetActive = false)
        /// - Đóng QuestPanel
        /// </summary>
        private void HandleCorrectAnswer()
        {
            if (currentActiveItemBtn != null)
            {
                currentActiveItemBtn.gameObject.SetActive(false);
                spawnedItems.Remove(currentActiveItemBtn);
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            ReduceRemainingCount();
            UpdateRemainText();
        }

        /// <summary>
        /// Trả lời SAI:
        /// - Làm mờ nút item đó
        /// - Đóng QuestPanel
        /// </summary>
        private void HandleWrongAnswer()
        {
            if (currentActiveItemBtn != null)
            {
                SetButtonOpacity(currentActiveItemBtn, 0.45f);
                currentActiveItemBtn.interactable = false;
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            UpdateRemainText();
        }

        /// <summary>
        /// Hết thời gian:
        /// - Giảm remain còn 2/3
        /// - Làm mờ nút item
        /// </summary>
        private void HandleTimeout()
        {
            if (currentActiveItemBtn != null)
            {
                SetButtonOpacity(currentActiveItemBtn, 0.45f);
                currentActiveItemBtn.interactable = false;
                currentActiveItemBtn = null;
            }

            CloseQuestPanel();
            ReduceRemainingCount();
            UpdateRemainText();
        }

        private void CloseQuestPanel()
        {
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Giảm giá trị remain còn 2/3
        /// </summary>
        private void ReduceRemainingCount()
        {
            int currentActiveCount = 0;
            foreach (var item in spawnedItems)
            {
                if (item != null && item.gameObject.activeSelf && item.interactable)
                {
                    currentActiveCount++;
                }
            }

            // Áp dụng giảm tỷ lệ 2/3
            Debug.Log($"[StationQuizController] Cập nhật số item còn lại: {currentActiveCount}");
        }

        private void UpdateRemainText()
        {
            if (remainText == null) return;

            int activeCount = 0;
            foreach (var item in spawnedItems)
            {
                if (item != null && item.gameObject.activeSelf)
                {
                    activeCount++;
                }
            }

            remainText.text = $"Còn lại: {activeCount} item";
        }

        private void SetButtonOpacity(Button btn, float alpha)
        {
            if (btn == null) return;

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }
    }
}

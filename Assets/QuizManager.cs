using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI questionIndexText;
    public TextMeshProUGUI[] answerTexts;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Questions")]
    public Question[] questions;

    [Header("Return Scene")]
    [SerializeField] private string returnSceneName = "GameScene";

    private int currentQuestion = 0;
    private int correctCount = 0;
    private bool gameEnded = false;

    void Start()
    {
        // ===== FIX UI KHÔNG BẤM ĐƯỢC =====
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ===== TẮT PANEL =====
        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        // ===== HIỆN CÂU HỎI =====
        ShowQuestion();
    }

    void ShowQuestion()
    {
        // Hết câu hỏi
        if (currentQuestion >= questions.Length)
        {
            EndGame();
            return;
        }

        Question q = questions[currentQuestion];

        // Hiện nội dung câu hỏi
        questionText.text = q.question;

        // Hiện số câu
        questionIndexText.text =
            "Question " + (currentQuestion + 1) + "/" + questions.Length;

        // Hiện đáp án
        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = q.answers[i];
        }
    }

    public void ChooseAnswer(int index)
    {
        // Nếu game kết thúc thì không cho bấm nữa
        if (gameEnded)
            return;

        // Đúng đáp án
        if (index == questions[currentQuestion].correctAnswerIndex)
        {
            correctCount++;
            Debug.Log("✅ Correct");
        }
        else
        {
            Debug.Log("❌ Wrong");
        }

        // Sang câu tiếp theo
        currentQuestion++;

        ShowQuestion();
    }

    void EndGame()
    {
        gameEnded = true;

        Debug.Log("Correct Count: " + correctCount);

        // ===== THẮNG =====
        if (correctCount >= 3)
        {
            Debug.Log("🏆 WIN");

            if (winPanel != null)
                winPanel.SetActive(true);
        }
        // ===== THUA =====
        else
        {
            Debug.Log("💀 LOSE");

            if (losePanel != null)
                losePanel.SetActive(true);
        }

        // Quay về map sau 2 giây
        Invoke(nameof(ReturnToMap), 2f);
    }

    void ReturnToMap()
    {
        SceneManager.LoadScene(returnSceneName);
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI questionIndexText;
    public TextMeshProUGUI[] answerTexts;

    public GameObject winPanel;
    public GameObject losePanel;

    public Question[] questions;

    private int currentQuestion = 0;
    private int correctCount = 0;
    private bool gameEnded = false;

    void Start()
    {
        // HIỆN CHUỘT
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ẩn panel thắng thua
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        // Hiện câu hỏi đầu
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestion >= questions.Length)
        {
            EndGame();
            return;
        }

        Question q = questions[currentQuestion];

        questionText.text = q.question;

        questionIndexText.text =
            "Question " + (currentQuestion + 1) + "/" + questions.Length;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].text = q.answers[i];
        }
    }

    public void ChooseAnswer(int index)
    {
        if (gameEnded) return;

        if (index == questions[currentQuestion].correctAnswerIndex)
        {
            correctCount++;
        }

        currentQuestion++;

        ShowQuestion();
    }

    void EndGame()
    {
        gameEnded = true;

        if (correctCount >= 3)
        {
            winPanel.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
        }

        Invoke(nameof(ReturnToMap), 2f);
    }

    void ReturnToMap()
    {
        // KHÓA CHUỘT LẠI KHI VỀ GAME
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("GameScene");
    }
}
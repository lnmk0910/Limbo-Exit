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
        winPanel.SetActive(false);
        losePanel.SetActive(false);
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
        questionIndexText.text = "Question " + (currentQuestion + 1) + "/5";

        for (int i = 0; i < answerTexts.Length; i++)
            answerTexts[i].text = q.answers[i];
    }

    public void ChooseAnswer(int index)
    {
        if (gameEnded) return;

        if (index == questions[currentQuestion].correctAnswerIndex)
            correctCount++;

        currentQuestion++;
        ShowQuestion();
    }

    void EndGame()
    {
        gameEnded = true;

        if (correctCount >= 3)
            winPanel.SetActive(true);
        else
            losePanel.SetActive(true);

        Invoke(nameof(ReturnToMap), 2f);
    }

    void ReturnToMap()
    {
        SceneManager.LoadScene("GameScene"); // 👉 sửa đúng tên map của bạn
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BasketballMinigame : MonoBehaviour
{
    [Header("Minigame Settings")]
    public float gameTime = 20f;
    public int targetScore = 10;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Result Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private float currentTime;
    private int currentScore = 0;
    private bool gameEnded = false;
    private bool daHenGioVeMap = false; // ✅ tránh gọi nhiều lần

    private MinigameManager minigameManager;

    // Khoi tao thoi gian, tham chieu va UI
    void Start()
    {
        currentTime = gameTime;
        minigameManager = FindObjectOfType<MinigameManager>();

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        UpdateUI();
    }

    // Dem nguoc thoi gian va ket thuc neu het gio
    void Update()
    {
        if (gameEnded) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateUI();
            EndGame();
            return;
        }

        UpdateUI();
    }

    // 👉 GỌI HÀM NÀY KHI BÓNG VÀO RỔ
    // Cong diem va kiem tra dieu kien thang
    public void AddScore(int points = 1)
    {
        if (gameEnded) return;

        currentScore += points;
        UpdateUI();

        Debug.Log("Score: " + currentScore);

        if (currentScore >= targetScore)
            WinGame();
    }

    // Cap nhat text thoi gian va diem
    private void UpdateUI()
    {
        if (timerText)
            timerText.text = "Time: " + Mathf.Ceil(currentTime) + "s";

        if (scoreText)
            scoreText.text = "Score: " + currentScore + " / " + targetScore;
    }

    // Ket thuc game va hien ket qua
    private void EndGame()
    {
        if (gameEnded) return;

        if (currentScore >= targetScore)
            WinGame();
        else
            LoseGame();
    }

    // Xu ly thang va hen gio ve map
    private void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (winPanel) winPanel.SetActive(true);

        Debug.Log("=== WIN BASKETBALL ===");

        if (!daHenGioVeMap)
        {
            daHenGioVeMap = true;
            Invoke(nameof(ReturnToMap), 2f);
        }
    }

    // Xu ly thua va hen gio ve map
    private void LoseGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (losePanel) losePanel.SetActive(true);

        Debug.Log("=== LOSE BASKETBALL ===");

        if (!daHenGioVeMap)
        {
            daHenGioVeMap = true;
            Invoke(nameof(ReturnToMap), 2f);
        }
    }

    // Quay ve map chinh (qua MinigameManager neu co)
    private void ReturnToMap()
    {
        if (minigameManager != null)
            minigameManager.ReturnToMainMap();
        else
            SceneManager.LoadScene("GameScene"); // 👉 nhớ đúng tên scene
    }
}

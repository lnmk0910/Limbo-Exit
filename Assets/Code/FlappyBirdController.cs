using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FlappyBirdController : MonoBehaviour
{

    public GameObject Bird;
    public GameObject PipePrefab;
    public GameObject WingsLeft;
    public GameObject WingsRight;
    public Text ScoreText;

    // Win Screen
    public GameObject WinPanel;
    public Button RestartButton;

    public float Gravity = 30f;
    public float Jump = 10f;
    public float PipeSpawnInterval = 2f;
    public float PipesSpeed = 5f;

    private float VerticalSpeed;
    private float PipeSpawnCountdown;
    private GameObject PipesHolder;
    private int PipeCount;
    private int Score;

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        // Dừng game khi thắng
        if (WinPanel.activeSelf) return;

        // Movement
        VerticalSpeed += -Gravity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            VerticalSpeed = Jump;
        }

        Bird.transform.position += Vector3.up * VerticalSpeed * Time.deltaTime;

        // Spawn Pipes
        PipeSpawnCountdown -= Time.deltaTime;
        if (PipeSpawnCountdown <= 0)
        {
            PipeSpawnCountdown = PipeSpawnInterval;

            GameObject pipe = Instantiate(PipePrefab, PipesHolder.transform);
            pipe.name = (++PipeCount).ToString();
            pipe.transform.position = new Vector3(12f, Random.Range(4f, 9f), 0f);
        }

        // Move Pipes
        PipesHolder.transform.position += Vector3.left * PipesSpeed * Time.deltaTime;

        // Bird Animation
        float speedTo01 = Mathf.InverseLerp(-10, 10, VerticalSpeed);
        float noseAngle = Mathf.Lerp(-30, 30, speedTo01);
        Bird.transform.rotation = Quaternion.Euler(0, 20, noseAngle);

        float flapSpeed = (VerticalSpeed > 0) ? 30 : 5;
        float angle = Mathf.Sin(Time.time * flapSpeed) * 45;
        WingsLeft.transform.localRotation = Quaternion.Euler(-angle, 0, 0);
        WingsRight.transform.localRotation = Quaternion.Euler(angle, 0, 0);

        // Score
        foreach (Transform pipe in PipesHolder.transform)
        {
            if (pipe.position.x < 0)
            {
                int pipeId = int.Parse(pipe.name);
                if (pipeId > Score)
                {
                    Score = pipeId;
                    ScoreText.text = "SCORE: " + Score;

                    // Khi đạt 5 điểm → Thắng
                    if (Score >= 5)
                    {
                        WinGame();
                        return;
                    }
                }
            }

            if (pipe.position.x < -15)
            {
                Destroy(pipe.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ResetGame();   // Va chạm = thua
    }

    // ====================== PHẦN THẮNG ======================
    private void WinGame()
    {
        Time.timeScale = 0f;
        WinPanel.SetActive(true);

        // Hiện nút "Chơi lại" khi thắng
        if (RestartButton != null)
        {
            RestartButton.gameObject.SetActive(true);
            RestartButton.onClick.RemoveAllListeners();
            RestartButton.onClick.AddListener(RestartGame);
        }
    }

    private void ResetGame()
    {
        Score = 0;
        ScoreText.text = "SCORE: 0";

        PipeCount = 0;
        if (PipesHolder != null) Destroy(PipesHolder);
        PipesHolder = new GameObject("PipesHolder");
        PipesHolder.transform.parent = transform;

        VerticalSpeed = 0;
        Bird.transform.position = Vector3.up * 5;
        Bird.transform.rotation = Quaternion.Euler(0, 20, 0);

        PipeSpawnCountdown = 0;
        Time.timeScale = 1f;

        WinPanel.SetActive(false);

        // Ẩn nút chơi lại cho lần sau
        if (RestartButton != null)
            RestartButton.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
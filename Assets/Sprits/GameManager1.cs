using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager1 : MonoBehaviour
{
    public float thoiGianChoPhepVeDich = 30f;
    public bool gameDaKetThuc = false;
    public bool winGame = false;

    private static GameManager1 instance;

    public GameObject gameOverObject;
    public GameObject timeGameObject;
    public GameObject winGameObject;

    [SerializeField]
    private float thoiGianCongThem = 5f;

    private bool daHenGioVeMap = false; // tránh gọi nhiều lần

    public static GameManager1 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager1>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager1");
                    instance = obj.AddComponent<GameManager1>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        if (gameOverObject != null)
            gameOverObject.SetActive(false);

        if (winGameObject != null)
            winGameObject.SetActive(false);

        if (timeGameObject != null)
            timeGameObject.SetActive(true);
    }

    void Update()
    {
        // ===== WIN =====
        if (winGame)
        {
            if (timeGameObject != null)
                timeGameObject.SetActive(false);

            if (winGameObject != null)
                winGameObject.SetActive(true);

            if (!daHenGioVeMap)
            {
                daHenGioVeMap = true;
                Invoke(nameof(ReturnToMap), 2f);
            }

            return;
        }

        // Nếu game đã kết thúc thì dừng
        if (gameDaKetThuc)
            return;

        // ===== ĐẾM NGƯỢC =====
        thoiGianChoPhepVeDich -= Time.deltaTime;

        // ===== HẾT GIỜ =====
        if (thoiGianChoPhepVeDich <= 0)
        {
            thoiGianChoPhepVeDich = 0;

            if (timeGameObject != null)
                timeGameObject.SetActive(false);

            if (gameOverObject != null)
                gameOverObject.SetActive(true);

            KetThucGame();

            if (!daHenGioVeMap)
            {
                daHenGioVeMap = true;
                Invoke(nameof(ReturnToMap), 2f);
            }
        }
    }

    public void KetThucGame()
    {
        gameDaKetThuc = true;
        Debug.Log("GAME OVER");
    }

    public void QuaCheckPoint()
    {
        if (!gameDaKetThuc && !winGame)
        {
            thoiGianChoPhepVeDich += thoiGianCongThem;
            Debug.Log("✅ Qua CheckPoint + " + thoiGianCongThem + " giây");
        }
    }

    public void QuaWinPoint()
    {
        if (!gameDaKetThuc)
        {
            winGame = true;
            gameDaKetThuc = true;
            Debug.Log("🏆 WIN GAME!");
        }
    }

    void ReturnToMap()
    {
        SceneManager.LoadScene("GameScene"); // 👉 sửa tên nếu map bạn khác
    }
}
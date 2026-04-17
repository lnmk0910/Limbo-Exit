using UnityEngine;

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

    public static GameManager1 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager1>();           // ← Sửa ở đây
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager1");
                    instance = obj.AddComponent<GameManager1>();       // ← Sửa ở đây
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
        // Khởi tạo UI an toàn
        if (gameOverObject != null) gameOverObject.SetActive(false);
        if (winGameObject != null) winGameObject.SetActive(false);
        if (timeGameObject != null) timeGameObject.SetActive(true);
    }

    void Update()
    {
        if (gameDaKetThuc) return;

        thoiGianChoPhepVeDich -= Time.deltaTime;

        // Debug.Log("Time: " + thoiGianChoPhepVeDich);   // ← Tạm comment để tránh spam

        if (thoiGianChoPhepVeDich <= 0)
        {
            if (timeGameObject != null) timeGameObject.SetActive(false);
            if (gameOverObject != null) gameOverObject.SetActive(true);
            KetThucGame();
        }

        if (winGame && winGameObject != null)
        {
            if (timeGameObject != null) timeGameObject.SetActive(false);
            winGameObject.SetActive(true);
        }
    }

    public void KetThucGame()
    {
        gameDaKetThuc = true;
        Debug.Log("GAME OVER");
    }

    public void QuaCheckPoint()
    {
        if (!gameDaKetThuc)
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
            KetThucGame();
            Debug.Log("🏆 WIN GAME!");
        }
    }
}
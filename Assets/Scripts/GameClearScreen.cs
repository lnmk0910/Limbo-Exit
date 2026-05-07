// GameClearScreen.cs
// Man hinh CHUC MUNG khi nguoi choi hoan thanh ca 4 tang
// Dem nguoc bang Update + Time.unscaledDeltaTime (khong bi anh huong timeScale)
// Sau 30 giay tu dong load MainMenu
// GAN vao: Empty GameObject "GameClearScreen" trong GameScene (luon Active)

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameClearScreen : MonoBehaviour
{
    public static GameClearScreen Instance { get; private set; }

    [Header("=== PANELS ===")]
    public GameObject panelGameClear;

    [Header("=== TEXT ===")]
    public TMP_Text txtTieuDe;
    public TMP_Text txtThongKe;
    public TMP_Text txtDemNguoc;

    [Header("=== CAU HINH ===")]
    public string tenSceneMenu      = "MainMenu";
    public float  thoiGianChoVeMenu = 30f;

    private bool  dangHien        = false;
    private bool  daLoadScene     = false;   // Guard chong load nhieu lan
    private float thoiGianChoi    = 0f;      // Tinh khi con dang choi
    private float demNguoc        = 0f;      // Dem nguoc ve Menu

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelGameClear != null) panelGameClear.SetActive(false);
    }

    void Update()
    {
        // Khi chua thang: dem thoi gian choi
        if (!dangHien)
        {
            thoiGianChoi += Time.unscaledDeltaTime;
            return;
        }

        // Da load roi thi khong lam gi nua
        if (daLoadScene) return;

        // Dem nguoc (unscaled - hoat dong ca khi timeScale = 0)
        demNguoc -= Time.unscaledDeltaTime;

        // Cap nhat text
        int giayConLai = Mathf.CeilToInt(demNguoc);
        if (txtDemNguoc != null)
            txtDemNguoc.text = giayConLai > 0
                ? $"Tu dong ve Menu trong: {giayConLai}s"
                : "Dang chuyen ve Menu...";

        // Het dem nguoc -> ve MainMenu
        if (demNguoc <= 0f)
        {
            daLoadScene    = true;
            Time.timeScale = 1f;

            // MainMenu luon la scene index 0 trong Build Settings
            // (uu tien dung index de tranh loi ten scene)
            Debug.Log($"[GameClear] >>> LoadScene index 0 (MainMenu)");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // -----------------------------------------------
    // GOI TU VictoryScreen khi phat hien tang cuoi
    // -----------------------------------------------
    public void HienGameClear()
    {
        if (dangHien) return;
        dangHien    = true;
        daLoadScene = false;
        demNguoc    = thoiGianChoVeMenu;

        // Mo cursor, chuyen UI state de ngan Pause
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        UIManager.Mo(UIManager.TrangThaiUI.ChienThang);

        // Hien panel
        if (panelGameClear != null) panelGameClear.SetActive(true);

        // Thong ke
        PlayerData data = SaveSystem.LoadGame();
        int phut = Mathf.FloorToInt(thoiGianChoi / 60f);
        int giay = Mathf.FloorToInt(thoiGianChoi % 60f);

        if (txtTieuDe != null)
            txtTieuDe.text =
                "CHUC MUNG!\n" +
                "BAN DA THOAT KHOI LIMBO!";

        if (txtThongKe != null)
            txtThongKe.text =
                $"Tang hoan thanh : 4 / 4\n" +
                $"Manh Hon con lai : {data.soManhHon} MH\n" +
                $"Toc do nang cap  : Cap {data.capTocDo}\n" +
                $"Thoi gian choi   : {phut:00}:{giay:00}";

        if (txtDemNguoc != null)
            txtDemNguoc.text = $"Tu dong ve Menu trong: {Mathf.CeilToInt(demNguoc)}s";

        AudioManager.PhatPhaDao();

        Debug.Log($"[GameClear] Hien! Dem nguoc {thoiGianChoVeMenu}s -> {tenSceneMenu}");
    }
}

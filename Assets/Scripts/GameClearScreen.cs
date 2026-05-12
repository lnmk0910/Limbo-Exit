// GameClearScreen.cs
// Màn hình CHÚC MỪNG khi hoàn thành cả 4 tầng
// PHIÊN BẢN MỚI: Phát video MP4 ending thay vì đếm ngược text
// - Gán VideoClip vào Inspector → phát toàn màn hình
// - Video kết thúc → tự động về MainMenu
// - Nhấn Esc/Enter/Space để skip video
// - Fallback: nếu không có video → đếm ngược 30s như cũ
// GẮN vào: Empty GameObject "GameClearScreen" trong GameScene (luôn Active)

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class GameClearScreen : MonoBehaviour
{
    public static GameClearScreen Instance { get; private set; }

    [Header("=== VIDEO ENDING ===")]
    [Tooltip("Kéo file video (.mp4) vào đây. Nếu để trống → dùng fallback đếm ngược.")]
    public VideoClip videoEnding;

    [Tooltip("RawImage toàn màn hình để hiện video. Nếu để trống → tự tạo.")]
    public RawImage imgVideo;

    [Header("=== PANELS ===")]
    public GameObject panelGameClear;       // Panel text thống kê (fallback)

    [Header("=== TEXT (dùng cho fallback khi không có video) ===")]
    public TMP_Text txtTieuDe;
    public TMP_Text txtThongKe;
    public TMP_Text txtDemNguoc;

    [Header("=== TEXT SKIP (hiện trên video) ===")]
    [Tooltip("Text gợi ý skip. Để trống → tự tạo.")]
    public TMP_Text txtSkipHint;

    [Header("=== CẤU HÌNH ===")]
    public string tenSceneMenu      = "MainMenu";
    public float  thoiGianChoVeMenu = 30f;      // Fallback nếu không có video

    // --- Internal ---
    private bool  dangHien        = false;
    private bool  daLoadScene     = false;
    private float thoiGianChoi    = 0f;
    private float demNguoc        = 0f;
    private bool  dangPhatVideo   = false;

    // Video components (tự tạo nếu cần)
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private Canvas canvasVideo;
    private GameObject goCanvasVideo;   // Để destroy khi không cần

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelGameClear != null) panelGameClear.SetActive(false);
        if (imgVideo != null) imgVideo.gameObject.SetActive(false);

        // Ẩn skip hint ban đầu
        if (txtSkipHint != null) txtSkipHint.gameObject.SetActive(false);
    }

    void Update()
    {
        // Khi chưa thắng: đếm thời gian chơi
        if (!dangHien)
        {
            thoiGianChoi += Time.unscaledDeltaTime;
            return;
        }

        if (daLoadScene) return;

        // === ĐANG PHÁT VIDEO ===
        if (dangPhatVideo)
        {
            // Skip video bằng Esc / Enter / Space
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                SkipVideo();
            }
            return; // Không xử lý đếm ngược khi đang phát video
        }

        // === FALLBACK: ĐẾM NGƯỢC (khi không có video) ===
        demNguoc -= Time.unscaledDeltaTime;

        int giayConLai = Mathf.CeilToInt(demNguoc);
        if (txtDemNguoc != null)
            txtDemNguoc.text = giayConLai > 0
                ? $"Tu dong ve Menu trong: {giayConLai}s"
                : "Dang chuyen ve Menu...";

        // Hết đếm ngược → về MainMenu
        if (demNguoc <= 0f)
            VeMainMenu();
    }

    // -----------------------------------------------
    // GỌI TỪ VictoryScreen khi phát hiện tầng cuối
    // -----------------------------------------------
    public void HienGameClear()
    {
        if (dangHien) return;
        dangHien    = true;
        daLoadScene = false;

        // Mở cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        UIManager.Mo(UIManager.TrangThaiUI.ChienThang);

        // Phát nhạc chiến thắng
        AudioManager.PhatPhaDao();

        // Log thống kê
        PlayerData data = SaveSystem.LoadGame();
        int phut = Mathf.FloorToInt(thoiGianChoi / 60f);
        int giay = Mathf.FloorToInt(thoiGianChoi % 60f);
        Debug.Log($"[GameClear] Hoàn thành! MH={data.soManhHon} | Tốc độ cấp {data.capTocDo} | Thời gian: {phut:00}:{giay:00}");

        // === CÓ VIDEO → PHÁT VIDEO ===
        if (videoEnding != null)
        {
            BatDauPhatVideo();
        }
        // === KHÔNG CÓ VIDEO → FALLBACK ĐẾM NGƯỢC ===
        else
        {
            HienFallbackText(data, phut, giay);
        }
    }

    // -----------------------------------------------
    // PHÁT VIDEO TOÀN MÀN HÌNH
    // -----------------------------------------------
    void BatDauPhatVideo()
    {
        dangPhatVideo = true;

        // Game đã kết thúc → giữ timeScale = 1 để VideoPlayer chạy mượt
        Time.timeScale = 1f;

        // --- Tạo VideoPlayer ---
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = videoEnding;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        // Render vào RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // --- Tạo Canvas + RawImage nếu chưa có ---
        if (imgVideo == null)
        {
            TaoCanvasVideo();
        }
        else
        {
            imgVideo.texture = renderTexture;
            imgVideo.gameObject.SetActive(true);
        }

        // --- Tạo text skip hint ---
        if (txtSkipHint != null)
        {
            txtSkipHint.text = "[Esc / Enter / Space] Bo qua";
            txtSkipHint.gameObject.SetActive(true);
        }

        // Ẩn panel text fallback
        if (panelGameClear != null) panelGameClear.SetActive(false);

        // Lắng nghe sự kiện video kết thúc
        videoPlayer.loopPointReached += OnVideoKetThuc;

        // Prepare trước rồi play
        videoPlayer.prepareCompleted += (vp) =>
        {
            vp.Play();
            Debug.Log($"[GameClear] Video bắt đầu phát! Thời lượng: {vp.clip.length:F1}s");
        };
        videoPlayer.Prepare();
    }

    // -----------------------------------------------
    // TẠO CANVAS + RAWIMAGE TOÀN MÀN HÌNH (tự động)
    // -----------------------------------------------
    void TaoCanvasVideo()
    {
        // Canvas overlay
        goCanvasVideo = new GameObject("Canvas_VideoEnding");
        canvasVideo = goCanvasVideo.AddComponent<Canvas>();
        canvasVideo.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasVideo.sortingOrder = 999; // Trên tất cả UI khác

        var scaler = goCanvasVideo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        goCanvasVideo.AddComponent<GraphicRaycaster>();

        // RawImage toàn màn hình
        GameObject goImg = new GameObject("RawImage_Video");
        goImg.transform.SetParent(goCanvasVideo.transform, false);

        imgVideo = goImg.AddComponent<RawImage>();
        imgVideo.texture = renderTexture;
        imgVideo.color = Color.white;

        // Stretch toàn màn hình
        RectTransform rt = imgVideo.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // --- Text skip hint trên video ---
        GameObject goSkip = new GameObject("Txt_SkipHint");
        goSkip.transform.SetParent(goCanvasVideo.transform, false);

        txtSkipHint = goSkip.AddComponent<TextMeshProUGUI>();
        txtSkipHint.text = "[Esc / Enter / Space] Bo qua";
        txtSkipHint.fontSize = 24;
        txtSkipHint.color = new Color(1f, 1f, 1f, 0.6f);
        txtSkipHint.alignment = TextAlignmentOptions.BottomRight;

        RectTransform rtSkip = txtSkipHint.rectTransform;
        rtSkip.anchorMin = new Vector2(0.7f, 0f);
        rtSkip.anchorMax = new Vector2(1f, 0.08f);
        rtSkip.offsetMin = Vector2.zero;
        rtSkip.offsetMax = new Vector2(-20f, 0f);
    }

    // -----------------------------------------------
    // VIDEO KẾT THÚC → VỀ MENU
    // -----------------------------------------------
    void OnVideoKetThuc(VideoPlayer vp)
    {
        Debug.Log("[GameClear] Video kết thúc → Về MainMenu");
        DonDepVideo();
        VeMainMenu();
    }

    // -----------------------------------------------
    // SKIP VIDEO
    // -----------------------------------------------
    void SkipVideo()
    {
        Debug.Log("[GameClear] Người chơi skip video → Về MainMenu");
        DonDepVideo();
        VeMainMenu();
    }

    // -----------------------------------------------
    // DỌN DẸP TÀI NGUYÊN VIDEO
    // -----------------------------------------------
    void DonDepVideo()
    {
        dangPhatVideo = false;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnVideoKetThuc;
            Destroy(videoPlayer);
            videoPlayer = null;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        // Destroy canvas tự tạo (nếu có)
        if (goCanvasVideo != null)
        {
            Destroy(goCanvasVideo);
            goCanvasVideo = null;
            canvasVideo = null;
            imgVideo = null;
            txtSkipHint = null;
        }
        else if (imgVideo != null)
        {
            imgVideo.gameObject.SetActive(false);
        }
    }

    // -----------------------------------------------
    // VỀ MAIN MENU
    // -----------------------------------------------
    void VeMainMenu()
    {
        if (daLoadScene) return;
        daLoadScene    = true;
        Time.timeScale = 1f;

        Debug.Log($"[GameClear] >>> LoadScene: {tenSceneMenu}");
        SceneManager.LoadScene(tenSceneMenu);
    }

    // -----------------------------------------------
    // FALLBACK: HIỆN TEXT ĐẾM NGƯỢC (khi không có video)
    // -----------------------------------------------
    void HienFallbackText(PlayerData data, int phut, int giay)
    {
        demNguoc = thoiGianChoVeMenu;

        if (panelGameClear != null) panelGameClear.SetActive(true);

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

        Debug.Log($"[GameClear] Fallback: Đếm ngược {thoiGianChoVeMenu}s → {tenSceneMenu}");
    }

    void OnDestroy()
    {
        // Đảm bảo dọn dẹp khi bị destroy
        DonDepVideo();
    }
}

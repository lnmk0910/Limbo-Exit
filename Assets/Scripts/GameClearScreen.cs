// GameClearScreen.cs — Màn hình phá đảo: random video ending + fallback text
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

[System.Serializable]
public class VideoEndingData
{
    [Tooltip("File video (.mp4)")]
    public VideoClip clip;
    [Tooltip("Thời gian chờ sau video trước khi về menu (giây)")]
    public float delayVeMenu = 2f;
    [Tooltip("Mô tả (ghi chú)")]
    public string moTa = "";
}

public class GameClearScreen : MonoBehaviour
{
    public static GameClearScreen Instance { get; private set; }

    [Header("=== DANH SÁCH VIDEO ENDING ===")]
    public VideoEndingData[] danhSachVideo;

    [Header("=== VIDEO UI ===")]
    public RawImage imgVideo;

    [Header("=== PANELS ===")]
    public GameObject panelGameClear;

    [Header("=== TEXT (fallback) ===")]
    public TMP_Text txtTieuDe;
    public TMP_Text txtThongKe;
    public TMP_Text txtDemNguoc;

    [Header("=== TEXT SKIP ===")]
    public TMP_Text txtSkipHint;

    [Header("=== CẤU HÌNH ===")]
    public string tenSceneMenu      = "MainMenu";
    public float  thoiGianChoVeMenu = 30f;

    private bool  dangHien        = false;
    private bool  daLoadScene     = false;
    private float thoiGianChoi    = 0f;
    private float demNguoc        = 0f;
    private bool  dangPhatVideo   = false;
    private float delayHienTai    = 2f;
    private bool  dangChoDelay    = false;
    private float demDelay        = 0f;

    private static int indexVideoTruoc = -1;

    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private Canvas canvasVideo;
    private GameObject goCanvasVideo;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // An UI khi bat dau
    void Start()
    {
        if (panelGameClear != null) panelGameClear.SetActive(false);
        if (imgVideo != null) imgVideo.gameObject.SetActive(false);
        if (txtSkipHint != null) txtSkipHint.gameObject.SetActive(false);
    }

    // Xu ly trang thai: choi video, delay, fallback text
    void Update()
    {
        if (!dangHien)
        {
            thoiGianChoi += Time.unscaledDeltaTime;
            return;
        }

        if (daLoadScene) return;

        // Chờ delay sau video
        if (dangChoDelay)
        {
            demDelay -= Time.unscaledDeltaTime;
            if (demDelay <= 0f) VeMainMenu();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                VeMainMenu();
            return;
        }

        // Đang phát video
        if (dangPhatVideo)
        {
            // Kiểm tra video kết thúc tự nhiên
            if (videoPlayer != null && videoPlayer.isPrepared && !videoPlayer.isPlaying && videoPlayer.frame > 0)
            {
                OnVideoKetThuc(videoPlayer);
                return;
            }
            if (videoPlayer == null)
            {
                dangPhatVideo = false;
                VeMainMenu();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                SkipVideo();
            return;
        }

        // Fallback: đếm ngược text
        demNguoc -= Time.unscaledDeltaTime;
        int giayConLai = Mathf.CeilToInt(demNguoc);
        if (txtDemNguoc != null)
            txtDemNguoc.text = giayConLai > 0
                ? $"Tự động về Menu trong: {giayConLai}s"
                : "Đang chuyển về Menu...";
        if (demNguoc <= 0f) VeMainMenu();
    }

    // Gọi từ VictoryScreen khi tầng cuối
    // Hien man hinh game clear va chon video
    public void HienGameClear()
    {
        if (dangHien) return;
        dangHien    = true;
        daLoadScene = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        UIManager.Mo(UIManager.TrangThaiUI.ChienThang);
        AudioManager.PhatPhaDao();

        PlayerData data = SaveSystem.LoadGame();
        int phut = Mathf.FloorToInt(thoiGianChoi / 60f);
        int giay = Mathf.FloorToInt(thoiGianChoi % 60f);

        VideoEndingData videoChon = ChonVideoNgauNhien();
        if (videoChon != null && videoChon.clip != null)
        {
            delayHienTai = videoChon.delayVeMenu;
            BatDauPhatVideo(videoChon.clip);
        }
        else
        {
            HienFallbackText(data, phut, giay);
        }
    }

    // Chon video ngau nhien (tranh lap lien tuc)
    VideoEndingData ChonVideoNgauNhien()
    {
        if (danhSachVideo == null || danhSachVideo.Length == 0) return null;
        if (danhSachVideo.Length == 1) { indexVideoTruoc = 0; return danhSachVideo[0]; }

        int index, soLanThu = 0;
        do { index = Random.Range(0, danhSachVideo.Length); soLanThu++; }
        while (index == indexVideoTruoc && soLanThu < 20);
        indexVideoTruoc = index;
        return danhSachVideo[index];
    }

    // Tao VideoPlayer va bat dau phat
    void BatDauPhatVideo(VideoClip clip)
    {
        dangPhatVideo = true;
        Time.timeScale = 1f;

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        renderTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        if (imgVideo == null) TaoCanvasVideo();
        else { imgVideo.texture = renderTexture; imgVideo.gameObject.SetActive(true); }

        if (txtSkipHint != null)
        {
            txtSkipHint.text = "[Esc / Enter / Space] Bỏ qua";
            txtSkipHint.gameObject.SetActive(true);
        }

        if (panelGameClear != null) panelGameClear.SetActive(false);

        videoPlayer.loopPointReached += OnVideoKetThuc;
        videoPlayer.prepareCompleted += (vp) => { vp.Play(); };
        videoPlayer.Prepare();
    }

    // Tao canvas hien thi video neu chua co RawImage
    void TaoCanvasVideo()
    {
        goCanvasVideo = new GameObject("Canvas_VideoEnding");
        canvasVideo = goCanvasVideo.AddComponent<Canvas>();
        canvasVideo.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasVideo.sortingOrder = 999;

        var scaler = goCanvasVideo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        goCanvasVideo.AddComponent<GraphicRaycaster>();

        GameObject goImg = new GameObject("RawImage_Video");
        goImg.transform.SetParent(goCanvasVideo.transform, false);
        imgVideo = goImg.AddComponent<RawImage>();
        imgVideo.texture = renderTexture;
        imgVideo.color = Color.white;
        RectTransform rt = imgVideo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        GameObject goSkip = new GameObject("Txt_SkipHint");
        goSkip.transform.SetParent(goCanvasVideo.transform, false);
        txtSkipHint = goSkip.AddComponent<TextMeshProUGUI>();
        txtSkipHint.text = "[Esc / Enter / Space] Bỏ qua";
        txtSkipHint.fontSize = 24;
        txtSkipHint.color = new Color(1f, 1f, 1f, 0.6f);
        txtSkipHint.alignment = TextAlignmentOptions.BottomRight;
        RectTransform rtSkip = txtSkipHint.rectTransform;
        rtSkip.anchorMin = new Vector2(0.7f, 0f);
        rtSkip.anchorMax = new Vector2(1f, 0.08f);
        rtSkip.offsetMin = Vector2.zero;
        rtSkip.offsetMax = new Vector2(-20f, 0f);
    }

    // Xu ly khi video ket thuc
    void OnVideoKetThuc(VideoPlayer vp)
    {
        if (!dangPhatVideo) return;
        DonDepVideo();
        if (delayHienTai <= 0f) VeMainMenu();
        else { dangChoDelay = true; demDelay = delayHienTai; }
    }

    // Bo qua video va ve menu
    void SkipVideo()
    {
        DonDepVideo();
        VeMainMenu();
    }

    // Don dep VideoPlayer, RenderTexture va UI
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
        if (renderTexture != null) { renderTexture.Release(); Destroy(renderTexture); renderTexture = null; }
        if (goCanvasVideo != null)
        {
            Destroy(goCanvasVideo);
            goCanvasVideo = null; canvasVideo = null; imgVideo = null; txtSkipHint = null;
        }
        else if (imgVideo != null) imgVideo.gameObject.SetActive(false);
    }

    // Load ve menu chinh (chi 1 lan)
    void VeMainMenu()
    {
        if (daLoadScene) return;
        daLoadScene   = true;
        dangHien      = false;
        dangChoDelay  = false;
        dangPhatVideo = false;
        Time.timeScale = 1f;
        UIManager.DongVeGame();
        SceneManager.LoadScene(tenSceneMenu);
    }

    // Hien text fallback khi khong co video
    void HienFallbackText(PlayerData data, int phut, int giay)
    {
        demNguoc = thoiGianChoVeMenu;
        if (panelGameClear != null) panelGameClear.SetActive(true);

        if (txtTieuDe != null)
            txtTieuDe.text = "CHÚC MỪNG!\nBẠN ĐÃ THOÁT KHỎI LIMBO!";

        if (txtThongKe != null)
            txtThongKe.text =
                $"Tầng hoàn thành : 4 / 4\n" +
                $"Mảnh Hồn còn lại : {data.soManhHon} MH\n" +
                $"Tốc độ nâng cấp  : Cấp {data.capTocDo}\n" +
                $"Thời gian chơi   : {phut:00}:{giay:00}";

        if (txtDemNguoc != null)
            txtDemNguoc.text = $"Tự động về Menu trong: {Mathf.CeilToInt(demNguoc)}s";
    }

    // Dam bao don dep video khi destroy
    void OnDestroy() { DonDepVideo(); }
}

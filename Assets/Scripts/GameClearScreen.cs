

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

    [Tooltip("Thời gian chờ sau khi video kết thúc trước khi về menu (giây)")]
    public float delayVeMenu = 2f;

    [Tooltip("Mô tả (chỉ để ghi chú, không dùng trong code)")]
    public string moTa = "";
}

public class GameClearScreen : MonoBehaviour
{
    public static GameClearScreen Instance { get; private set; }

    [Header("=== DANH SÁCH VIDEO ENDING ===")]
    [Tooltip("Kéo nhiều video vào đây. Mỗi lần thắng sẽ random 1 video.")]
    public VideoEndingData[] danhSachVideo;

    [Header("=== VIDEO UI ===")]
    [Tooltip("RawImage toàn màn hình để hiện video. Để trống → tự tạo.")]
    public RawImage imgVideo;

    [Header("=== PANELS ===")]
    public GameObject panelGameClear;

    [Header("=== TEXT (fallback khi không có video) ===")]
    public TMP_Text txtTieuDe;
    public TMP_Text txtThongKe;
    public TMP_Text txtDemNguoc;

    [Header("=== TEXT SKIP ===")]
    public TMP_Text txtSkipHint;

    [Header("=== CẤU HÌNH ===")]
    public string tenSceneMenu      = "MainMenu";
    public float  thoiGianChoVeMenu = 30f;

    // --- Internal ---
    private bool  dangHien        = false;
    private bool  daLoadScene     = false;
    private float thoiGianChoi    = 0f;
    private float demNguoc        = 0f;
    private bool  dangPhatVideo   = false;
    private float delayHienTai    = 2f;
    private bool  dangChoDelay    = false;
    private float demDelay        = 0f;

    // Lưu index video vừa phát để không lặp lại liên tiếp
    private static int indexVideoTruoc = -1;

    // Video components
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private Canvas canvasVideo;
    private GameObject goCanvasVideo;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelGameClear != null) panelGameClear.SetActive(false);
        if (imgVideo != null) imgVideo.gameObject.SetActive(false);
        if (txtSkipHint != null) txtSkipHint.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!dangHien)
        {
            thoiGianChoi += Time.unscaledDeltaTime;
            return;
        }

        if (daLoadScene) return;

        // === ĐANG CHỜ DELAY SAU VIDEO ===
        if (dangChoDelay)
        {
            demDelay -= Time.unscaledDeltaTime;
            if (demDelay <= 0f)
                VeMainMenu();

            // Vẫn cho skip trong lúc delay
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space))
                VeMainMenu();

            return;
        }

        // === ĐANG PHÁT VIDEO ===
        if (dangPhatVideo)
        {
            // Kiểm tra video đã kết thúc tự nhiên (loopPointReached có thể không fire)
            if (videoPlayer != null && videoPlayer.isPrepared && !videoPlayer.isPlaying && videoPlayer.frame > 0)
            {
                OnVideoKetThuc(videoPlayer);
                return;
            }

            // VideoPlayer bị null bất thường → về menu luôn
            if (videoPlayer == null)
            {
                dangPhatVideo = false;
                VeMainMenu();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                SkipVideo();
            }
            return;
        }

        // === FALLBACK: ĐẾM NGƯỢC ===
        demNguoc -= Time.unscaledDeltaTime;
        int giayConLai = Mathf.CeilToInt(demNguoc);
        if (txtDemNguoc != null)
            txtDemNguoc.text = giayConLai > 0
                ? $"Tự động về Menu trong: {giayConLai}s"
                : "Đang chuyển về Menu...";

        if (demNguoc <= 0f)
            VeMainMenu();
    }

    // -----------------------------------------------
    // GỌI TỪ VictoryScreen khi tầng cuối
    // -----------------------------------------------
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
        Debug.Log($"[GameClear] Hoàn thành! MH={data.soManhHon} | Thời gian: {phut:00}:{giay:00}");

        // === CHỌN VIDEO RANDOM ===
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

    // -----------------------------------------------
    // CHỌN VIDEO NGẪU NHIÊN (không lặp lại liên tiếp)
    // -----------------------------------------------
    VideoEndingData ChonVideoNgauNhien()
    {
        if (danhSachVideo == null || danhSachVideo.Length == 0)
            return null;

        // Chỉ 1 video → luôn chọn nó
        if (danhSachVideo.Length == 1)
        {
            indexVideoTruoc = 0;
            return danhSachVideo[0];
        }

        // Nhiều video → random nhưng tránh lặp lại video vừa phát
        int index;
        int soLanThu = 0;
        do
        {
            index = Random.Range(0, danhSachVideo.Length);
            soLanThu++;
        }
        while (index == indexVideoTruoc && soLanThu < 20);

        indexVideoTruoc = index;

        VideoEndingData chon = danhSachVideo[index];
        Debug.Log($"[GameClear] Chọn video #{index}: \"{chon.moTa}\" | Delay: {chon.delayVeMenu}s | " +
                  $"Clip: {chon.clip?.name ?? "NULL"}");

        return chon;
    }

    // -----------------------------------------------
    // PHÁT VIDEO
    // -----------------------------------------------
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

        if (imgVideo == null)
            TaoCanvasVideo();
        else
        {
            imgVideo.texture = renderTexture;
            imgVideo.gameObject.SetActive(true);
        }

        if (txtSkipHint != null)
        {
            txtSkipHint.text = "[Esc / Enter / Space] Bỏ qua";
            txtSkipHint.gameObject.SetActive(true);
        }

        if (panelGameClear != null) panelGameClear.SetActive(false);

        videoPlayer.loopPointReached += OnVideoKetThuc;

        videoPlayer.prepareCompleted += (vp) =>
        {
            vp.Play();
            // Backup: coroutine kiểm tra video kết thúc phòng loopPointReached không fire
            StartCoroutine(KiemTraVideoKetThuc());
        };
        videoPlayer.Prepare();
    }

    // Backup: kiểm tra mỗi 0.5s video đã phát xong chưa
    System.Collections.IEnumerator KiemTraVideoKetThuc()
    {
        yield return new WaitForSeconds(1f);
        while (dangPhatVideo && videoPlayer != null)
        {
            if (!videoPlayer.isPlaying && videoPlayer.frame > 0)
            {
                OnVideoKetThuc(videoPlayer);
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // -----------------------------------------------
    // TẠO CANVAS VIDEO TỰ ĐỘNG
    // -----------------------------------------------
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

        // RawImage toàn màn hình
        GameObject goImg = new GameObject("RawImage_Video");
        goImg.transform.SetParent(goCanvasVideo.transform, false);

        imgVideo = goImg.AddComponent<RawImage>();
        imgVideo.texture = renderTexture;
        imgVideo.color = Color.white;

        RectTransform rt = imgVideo.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Skip hint
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

    // -----------------------------------------------
    // VIDEO KẾT THÚC → CHỜ DELAY → VỀ MENU
    // -----------------------------------------------
    void OnVideoKetThuc(VideoPlayer vp)
    {
        if (!dangPhatVideo) return; // Đã xử lý rồi
        DonDepVideo();

        if (delayHienTai <= 0f)
        {
            VeMainMenu();
        }
        else
        {
            dangChoDelay = true;
            demDelay = delayHienTai;
        }
    }

    // -----------------------------------------------
    // SKIP VIDEO
    // -----------------------------------------------
    void SkipVideo()
    {
        Debug.Log("[GameClear] Skip video → Về MainMenu");
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
        dangHien       = false;
        dangChoDelay   = false;
        dangPhatVideo  = false;
        Time.timeScale = 1f;
        UIManager.DongVeGame();
        SceneManager.LoadScene(tenSceneMenu);
    }

    // -----------------------------------------------
    // FALLBACK: TEXT ĐẾM NGƯỢC
    // -----------------------------------------------
    void HienFallbackText(PlayerData data, int phut, int giay)
    {
        demNguoc = thoiGianChoVeMenu;

        if (panelGameClear != null) panelGameClear.SetActive(true);

        if (txtTieuDe != null)
            txtTieuDe.text =
                "CHÚC MỪNG!\n" +
                "BẠN ĐÃ THOÁT KHỎI LIMBO!";

        if (txtThongKe != null)
            txtThongKe.text =
                $"Tầng hoàn thành : 4 / 4\n" +
                $"Mảnh Hồn còn lại : {data.soManhHon} MH\n" +
                $"Tốc độ nâng cấp  : Cấp {data.capTocDo}\n" +
                $"Thời gian chơi   : {phut:00}:{giay:00}";

        if (txtDemNguoc != null)
            txtDemNguoc.text = $"Tự động về Menu trong: {Mathf.CeilToInt(demNguoc)}s";

        Debug.Log($"[GameClear] Fallback: Đếm ngược {thoiGianChoVeMenu}s → {tenSceneMenu}");
    }

    void OnDestroy()
    {
        DonDepVideo();
    }
}

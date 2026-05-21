// DeathScreen.cs — Màn hình chết: trừ Mảnh Hồn + vật phẩm ngay khi chết
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [Header("=== UI ===")]
    public GameObject panelChet;
    public TMP_Text txtThongBao;
    public TMP_Text txtManhHonConLai;

    [Header("=== SCENE ===")]
    public string tenSceneMenu = "MainMenu";

    [Header("=== THỜI GIAN CHỜ ===")]
    public float thoiGianCho = 1.5f;

    private bool dangHien = false;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // An panel khi bat dau
    void Start()
    {
        if (panelChet != null) panelChet.SetActive(false);
    }

    // Lang nghe phim tat khi dang chet
    void Update()
    {
        if (!dangHien || !UIManager.DangO(UIManager.TrangThaiUI.ChetChoc)) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnClick_TiepTuc();
        if (Input.GetKeyDown(KeyCode.Escape))
            OnClick_TuBo();
    }

    // Goi coroutine hien man hinh chet
    public void HienManHinhChet()
    {
        if (dangHien) return;
        StartCoroutine(TrinhTuHienThi());
    }

    // Trinh tu: khoa game, tru tai san, hien UI
    IEnumerator TrinhTuHienThi()
    {
        dangHien = true;
        UIManager.Mo(UIManager.TrangThaiUI.ChetChoc);
        DDAManager.GhiNhanChet();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale = 0f;

        AudioManager.PhatChet();
        AudioManager.PhatManhHonRoi();
        AudioManager.PhatBGM(AudioManager.Instance?.bgmGameOver);
        AudioManager.TatAmThanhQuai();

        // Trừ Mảnh Hồn + vật phẩm ngay khi chết
        float phanTram = (RespawnManager.Instance != null) ? RespawnManager.Instance.phanTramGiuManhHon : 0.5f;
        PlayerData data = SaveSystem.LoadGame();
        data.soManhHon    = Mathf.FloorToInt(data.soManhHon * phanTram);
        data.soDaPhatSang = 0;
        data.soDongHo     = 0;
        data.soLaBan      = 0;
        SaveSystem.SaveGame(data);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();
        GameHUD.LamMoi();

        yield return new WaitForSecondsRealtime(thoiGianCho);

        int phanTramHienThi = Mathf.RoundToInt(phanTram * 100f);

        if (txtThongBao != null)
            txtThongBao.text = "[CHẾT] Bóng tối đã nuốt chửng bạn...";

        if (txtManhHonConLai != null)
            txtManhHonConLai.text =
                $"Mảnh Hồn còn lại: {data.soManhHon} MH (mất {100 - phanTramHienThi}%)\n" +
                $"Toàn bộ vật phẩm bị tịch thu\n\n" +
                $"[Enter/Space] Tiếp tục   |   [Esc] Từ bỏ";

        if (panelChet != null) panelChet.SetActive(true);
    }

    // Tiep tuc choi tu checkpoint/respawn
    public void OnClick_TiepTuc()
    {
        if (!dangHien) return;
        dangHien = false;
        UIManager.DongVeGame();

        if (panelChet != null) panelChet.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.HoiSinhPlayer();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Tu bo va ve menu
    public void OnClick_TuBo()
    {
        if (!dangHien) return;
        dangHien = false;
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

// VictoryScreen.cs — Màn hình chiến thắng: Shop / Nâng cấp / Tầng tiếp / Về Menu
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    public static VictoryScreen Instance { get; private set; }

    [Header("=== UI ===")]
    public GameObject panelVictory;
    public TMP_Text txtTieuDe;
    public TMP_Text txtThongKe;

    [Header("=== CẤU HÌNH ===")]
    public string tenSceneGame = "GameScene";
    public string tenSceneMenu = "MainMenu";

    private bool dangHien = false;
    private bool laPhaDao = false;
    private bool choPhimFrame = false;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // An panel khi bat dau
    void Start()
    {
        if (panelVictory != null) panelVictory.SetActive(false);
    }

    // Lang nghe phim tat khi dang hien
    void Update()
    {
        if (!dangHien) return;
        if (!UIManager.DangO(UIManager.TrangThaiUI.ChienThang)) return;
        if (laPhaDao) return;
        if (choPhimFrame) { choPhimFrame = false; return; }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.S)) OnClick_MoShop();
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.U)) OnClick_NangCap();
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Return)) OnClick_SangTangTiep();
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Q)) OnClick_VeMenu();
    }

    // Gọi từ ExitGate
    // Hien man hinh thang va khoa thoi gian
    public void HienManHinhThang()
    {
        if (dangHien) return;

        PlayerData data = SaveSystem.LoadGame();
        bool conTang = data.biomeSequence != null && data.mapHienTai < data.biomeSequence.Length;
        laPhaDao = !conTang;

        // Tầng cuối → GameClearScreen
        if (laPhaDao)
        {
            GameClearScreen gc = GameClearScreen.Instance ?? FindFirstObjectByType<GameClearScreen>();
            if (gc != null) { gc.HienGameClear(); return; }
        }

        // Tầng thường → VictoryScreen
        dangHien = true;
        UIManager.Mo(UIManager.TrangThaiUI.ChienThang);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;

        if (txtTieuDe != null)
            txtTieuDe.text = $"[OK] TẦNG {data.mapHienTai} HOÀN THÀNH!";

        string goiY = "[S/1] Cửa Hàng   [U/2] Nâng Cấp   [Enter/3] Tầng Tiếp   [Q/4] Về Menu";
        if (txtThongKe != null)
            txtThongKe.text = $"+10 Mảnh Hồn  |  Tổng: {data.soManhHon}\n\n{goiY}";

        if (panelVictory != null) panelVictory.SetActive(true);
        AudioManager.PhatBGM(AudioManager.Instance?.bgmVictory);
    }

    // Gọi từ Shop / UpgradeScreen để quay lại HUB
    // Khoi phuc panel HUB sau khi dong shop/upgrade
    public void KhoiPhucHienThiHUB()
    {
        if (!dangHien) return;
        choPhimFrame = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;
        if (panelVictory != null) panelVictory.SetActive(true);
    }

    // Mo cua hang vat pham
    public void OnClick_MoShop()
    {
        if (!dangHien) return;
        if (panelVictory != null) panelVictory.SetActive(false);
        ItemShopUI shop = ItemShopUI.Instance ?? FindFirstObjectByType<ItemShopUI>();
        if (shop != null) shop.MoShop();
    }

    // Mo man hinh nang cap
    public void OnClick_NangCap()
    {
        if (!dangHien) return;
        if (panelVictory != null) panelVictory.SetActive(false);
        UpgradeScreen us = UpgradeScreen.Instance ?? FindFirstObjectByType<UpgradeScreen>();
        if (us != null) us.MoUpgrade();
    }

    // Chuyen sang tang tiep va cap nhat save
    public void OnClick_SangTangTiep()
    {
        if (!dangHien) return;
        dangHien = false;
        UIManager.DongVeGame();

        PlayerData data = SaveSystem.LoadGame();
        if (laPhaDao) { data.mapHienTai = 1; data.biomeSequence = null; data.seed = 0; }
        else          { data.mapHienTai++; data.seed = 0; }
        SaveSystem.SaveGame(data);
        Time.timeScale = 1f;

        int biomeHienTai = -1;
        if (data.biomeSequence != null && data.biomeSequence.Length > 0)
        {
            int idx = Mathf.Clamp((data.mapHienTai - 1) % data.biomeSequence.Length, 0, data.biomeSequence.Length - 1);
            biomeHienTai = data.biomeSequence[idx];
        }
        LoadingScreen.LoadScene(tenSceneGame, biomeHienTai);
    }

    // Ve menu chinh
    public void OnClick_VeMenu()
    {
        if (!dangHien) return;
        dangHien = false;
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

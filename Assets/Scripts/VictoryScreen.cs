// VictoryScreen.cs
// Hiện ra SAU KHI người chơi chạm ExitGate, cho người chơi lựa chọn:
//   1. Mở Cửa Hàng (mua vật phẩm)
//   2. Nâng Cấp Năng Lực
//   3. Sang Tầng Tiếp (không mua gì)
//   4. Về Menu Chính
// GẮN vào: GameObject riêng, LUÔN ACTIVE (không tắt)

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
    private bool laPhaDao = false; // True neu day la man cuoi cung
    private bool choPhimFrame = false; // Bo qua input 1 frame (chong chain tu Shop/Upgrade)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelVictory != null) panelVictory.SetActive(false);
        else Debug.LogError("[LOI] Panel_Victory chua gan vao VictoryScreen!");
    }

    void Update()
    {
        if (!dangHien) return;
        if (!UIManager.DangO(UIManager.TrangThaiUI.ChienThang)) return;
        if (laPhaDao) return;

        // Bo qua 1 frame sau khi Shop/Upgrade dong (chong key chain)
        if (choPhimFrame) { choPhimFrame = false; return; }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.S)) OnClick_MoShop();
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.U)) OnClick_NangCap();
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Return)) OnClick_SangTangTiep();
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Q)) OnClick_VeMenu();
    }

    // -----------------------------------------------
    // GỌI TỪ ExitGate — luôn hiện màn hình này trước
    // -----------------------------------------------
    public void HienManHinhThang()
    {
        if (dangHien) return;

        PlayerData data = SaveSystem.LoadGame();

        // Kiem tra co phai tang cuoi khong
        bool conTang = data.biomeSequence != null
                       && data.mapHienTai < data.biomeSequence.Length;
        laPhaDao = !conTang;

        // --- PHA DAO → Chuyen sang GameClearScreen ---
        if (laPhaDao)
        {
            GameClearScreen gc = GameClearScreen.Instance
                              ?? FindFirstObjectByType<GameClearScreen>();
            if (gc != null)
            {
                gc.HienGameClear();
                return; // Khong mo VictoryScreen binh thuong
            }
            else
            {
                Debug.LogWarning("[!] Khong tim thay GameClearScreen trong Scene!");
                // Fallback: van hien VictoryScreen binh thuong
            }
        }

        // --- TANG THUONG → VictoryScreen binh thuong ---
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

        Debug.Log($"[WIN] VictoryScreen hien! Tang {data.mapHienTai}");
    }

    // -----------------------------------------------
    // GOI TU Shop / UpgradeScreen de tra lai HUB
    // -----------------------------------------------
    public void KhoiPhucHienThiHUB()
    {
        if (!dangHien) return;
        choPhimFrame = true; // Bo qua input frame nay de tranh key chain
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;
        if (panelVictory != null) panelVictory.SetActive(true);
    }

    // -----------------------------------------------
    // CÁC NÚT LỰA CHỌN
    // -----------------------------------------------

    // [1] Mở Cửa Hàng
    public void OnClick_MoShop()
    {
        if (!dangHien) return;
        if (panelVictory != null) panelVictory.SetActive(false);

        ItemShopUI shop = ItemShopUI.Instance ?? FindFirstObjectByType<ItemShopUI>();
        if (shop != null)
            shop.MoShop(); // Mở bình thường, đóng shop sẽ quay về VictoryScreen
        else
            Debug.LogError("[LOI] Không tìm thấy ItemShopUI!");
    }

    // [2] Mở Nâng Cấp
    public void OnClick_NangCap()
    {
        if (!dangHien) return;
        if (panelVictory != null) panelVictory.SetActive(false);

        UpgradeScreen us = UpgradeScreen.Instance ?? FindFirstObjectByType<UpgradeScreen>();
        if (us != null)
            us.MoUpgrade();
        else
            Debug.LogError("[LOI] Không tìm thấy UpgradeScreen!");
    }

    // [3] Sang tầng tiếp / Chơi lại (nếu phá đảo)
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

        // Lấy biome index hiện tại để hiện ảnh Loading Screen đúng
        int biomeHienTai = -1;
        if (data.biomeSequence != null && data.biomeSequence.Length > 0)
        {
            int idx = Mathf.Clamp((data.mapHienTai - 1) % data.biomeSequence.Length,
                                  0, data.biomeSequence.Length - 1);
            biomeHienTai = data.biomeSequence[idx];
        }

        LoadingScreen.LoadScene(tenSceneGame, biomeHienTai);
    }

    public void OnClick_VeMenu()
    {
        if (!dangHien) return;
        dangHien       = false;
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

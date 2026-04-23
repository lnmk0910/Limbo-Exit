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
    private bool laPhaDao = false; // True nếu đây là màn cuối cùng

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelVictory != null) panelVictory.SetActive(false);
        else Debug.LogError("❌ Panel_Victory chưa gán vào VictoryScreen!");
    }

    void Update()
    {
        if (!dangHien) return;

        // Phím tắt cho người chơi
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.S)) OnClick_MoShop();
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.U)) OnClick_NangCap();
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Return)) OnClick_SangTangTiep();
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Escape)) OnClick_VeMenu();
    }

    // -----------------------------------------------
    // GỌI TỪ ExitGate — luôn hiện màn hình này trước
    // -----------------------------------------------
    public void HienManHinhThang()
    {
        if (dangHien) return;
        dangHien = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;

        PlayerData data = SaveSystem.LoadGame();

        // Kiểm tra xem còn tầng tiếp hay đã phá đảo
        bool conTang = data.biomeSequence != null
                       && data.mapHienTai < data.biomeSequence.Length;
        laPhaDao = !conTang;

        // Cập nhật tiêu đề
        if (txtTieuDe != null)
            txtTieuDe.text = laPhaDao
                ? "🏆 PHÁ ĐẢO TOÀN CỤC! 🏆"
                : $"✅ TẦNG {data.mapHienTai} HOÀN THÀNH!";

        // Cập nhật thống kê + hướng dẫn phím
        string gợiY = laPhaDao
            ? "[Enter/3] Chơi Lại   [Esc/4] Về Menu"
            : "[S/1] Cửa Hàng   [U/2] Nâng Cấp   [Enter/3] Sang Tầng Tiếp   [Esc/4] Về Menu";

        if (txtThongKe != null)
            txtThongKe.text = $"+10 💎 Mảnh Hồn  |  Tổng: {data.soManhHon}\n\n{gợiY}";

        if (panelVictory != null) panelVictory.SetActive(true);

        Debug.Log($"🏆 VictoryScreen hiện! Tầng {data.mapHienTai} | Phá đảo: {laPhaDao}");
    }

    // -----------------------------------------------
    // GỌI TỪ Shop / UpgradeScreen để trả lại HUB
    // -----------------------------------------------
    public void KhoiPhucHienThiHUB()
    {
        if (!dangHien) return;
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
            Debug.LogError("❌ Không tìm thấy ItemShopUI!");
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
            Debug.LogError("❌ Không tìm thấy UpgradeScreen!");
    }

    // [3] Sang tầng tiếp / Chơi lại (nếu phá đảo)
    public void OnClick_SangTangTiep()
    {
        if (!dangHien) return;
        dangHien = false;

        PlayerData data = SaveSystem.LoadGame();

        if (laPhaDao)
        {
            // Phá đảo → reset về tầng 1, lộ trình mới
            data.mapHienTai    = 1;
            data.biomeSequence = null;
            data.seed          = 0;
        }
        else
        {
            // Còn tầng → tăng mapHienTai, reset seed
            data.mapHienTai++;
            data.seed = 0;
        }

        SaveSystem.SaveGame(data);
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneGame);
    }

    // [4] Về Menu
    public void OnClick_VeMenu()
    {
        if (!dangHien) return;
        dangHien       = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

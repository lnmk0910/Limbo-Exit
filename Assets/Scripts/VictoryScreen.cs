// VictoryScreen.cs - Fix nút không phản hồi
// Thêm phím tắt + đảm bảo cursor unlock trước khi hiện panel

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
    public int thuongManhHon   = 10;

    private bool dangHien = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Debug.Log("✅ VictoryScreen sẵn sàng");
    }

    void Start()
    {
        if (panelVictory != null) panelVictory.SetActive(false);
        else Debug.LogError("❌ Panel_Victory chưa gán vào VictoryScreen!");
    }

    void Update()
    {
        if (!dangHien) return;

        // ⭐ Phím tắt (phòng khi click UI không hoạt động)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnClick_ChoisTiep();

        if (Input.GetKeyDown(KeyCode.U))
            OnClick_NangCap();

        if (Input.GetKeyDown(KeyCode.Escape))
            OnClick_NghiNgoi();
    }

    // -----------------------------------------------
    // GỌI TỪ ExitGate
    // -----------------------------------------------
    public void HienManHinhThang()
    {
        if (dangHien) return;
        dangHien = true;

        // ⭐ Mở chuột TRƯỚC rồi mới dừng thời gian
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;

        // Lưu tiến trình
        PlayerData data = SaveSystem.LoadGame();
        data.mapHienTai += 1;
        data.soManhHon  += thuongManhHon;
        data.seed        = 0;
        SaveSystem.SaveGame(data);

        // Cập nhật UI
        if (txtTieuDe != null)
            txtTieuDe.text = $"🏆 THOÁT THÀNH CÔNG!\nHoàn thành Tầng {data.mapHienTai - 1}!";

        if (txtThongKe != null)
            txtThongKe.text = $"💎 +{thuongManhHon} Mảnh Hồn → Tổng: {data.soManhHon}\n" +
                              $"🗺️ Chuẩn bị chinh phục Tầng {data.mapHienTai}...\n\n" +
                              $"[U] Nâng Cấp  |  [Enter] Chơi Tiếp  |  [Esc] Về Menu";

        if (panelVictory != null) panelVictory.SetActive(true);
        Debug.Log($"🏆 Thắng! Màn {data.mapHienTai} | 💎 {data.soManhHon}");
    }

    // -----------------------------------------------
    // 3 NÚT
    // -----------------------------------------------
    public void OnClick_NangCap()
    {
        if (!dangHien) return;
        if (panelVictory != null) panelVictory.SetActive(false);

        UpgradeScreen us = UpgradeScreen.Instance
                        ?? FindFirstObjectByType<UpgradeScreen>();
        if (us != null)
            us.MoUpgrade();
        else
            Debug.LogError("❌ Không tìm thấy UpgradeScreen!");
    }

    public void OnClick_ChoisTiep()
    {
        if (!dangHien) return;
        dangHien       = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneGame);
    }

    public void OnClick_NghiNgoi()
    {
        if (!dangHien) return;
        dangHien       = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

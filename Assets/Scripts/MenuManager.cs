// MenuManager.cs
// Điều khiển Main Menu: HomePanel, SettingsPanel, SaveSlotPanel
// BiomePanel đã được xoá — Biome được random tự động khi vào game
// GẮN vào: Canvas_MainMenu

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("=== PANELS ===")]
    public GameObject homePanel;
    public GameObject settingsPanel;

    [Header("=== GIAO DIỆN SAVE SLOT ===")]
    public GameObject btnContinue;      // Nút Continue ngoài sảnh chính
    public GameObject saveSlotPanel;    // Bảng liệt kê 3 hồ sơ
    public GameObject slotActionPanel;  // Cảnh báo khi lưu đè

    [Header("=== TEXT CỦA 3 KHUNG SLOT ===")]
    public TMP_Text txtSlot1;
    public TMP_Text txtSlot2;
    public TMP_Text txtSlot3;

    [Header("=== SETTINGS — KÍCH THƯỚC MÊ CUNG ===")]
    public TMPro.TMP_InputField inputRong;
    public TMPro.TMP_InputField inputDai;
    public UnityEngine.UI.Slider sliderChieuCao;
    public UnityEngine.UI.Slider sliderDoDay;

    [Header("=== SCENE ===")]
    public string tenSceneGame = "GameScene";

    private int  dangChonSlot       = 0;
    private bool dangChonDeContinue = true;

    // -----------------------------------------------
    void Start()
    {
        AudioManager.PhatBGMTheoScene("MenuScene");

        homePanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (saveSlotPanel   != null) saveSlotPanel.SetActive(false);
        if (slotActionPanel != null) slotActionPanel.SetActive(false);

        dangChonSlot = 0;

        // Hiện nút Continue nếu có ít nhất 1 hồ sơ
        if (btnContinue != null)
        {
            btnContinue.SetActive(
                SaveSystem.KiemTraSlotTonTai(1) ||
                SaveSystem.KiemTraSlotTonTai(2) ||
                SaveSystem.KiemTraSlotTonTai(3)
            );
        }

        // Đồng bộ Settings UI
        if (inputRong      != null) inputRong.text       = GameSettings.rong.ToString();
        if (inputDai       != null) inputDai.text        = GameSettings.dai.ToString();
        if (sliderChieuCao != null) sliderChieuCao.value = GameSettings.chieuCaoTuong;
        if (sliderDoDay    != null) sliderDoDay.value    = GameSettings.doDayTuong;
    }

    void Update()
    {
        // 1. Đang hỏi cảnh báo lưu đè
        if (slotActionPanel != null && slotActionPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Y)) OnClick_NewGameThucSu();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.N))
            {
                dangChonSlot = 0;
                OnClick_DongActionPanel();
            }
        }
        // 2. Đang chọn Slot
        else if (saveSlotPanel != null && saveSlotPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnClick_ChonSlot1();
            if (Input.GetKeyDown(KeyCode.Alpha2)) OnClick_ChonSlot2();
            if (Input.GetKeyDown(KeyCode.Alpha3)) OnClick_ChonSlot3();
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_DongBangSlot();
        }
        // 3. Đang trong Settings
        else if (settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_Back();
        }
        // 4. Sảnh chính
        else if (homePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Return)) OnClick_MenuContinue();
            if (Input.GetKeyDown(KeyCode.N)) OnClick_MenuNewGame();
            if (Input.GetKeyDown(KeyCode.S)) OnClick_Settings();
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_Exit();
        }
    }

    // -----------------------------------------------
    // CONTINUE / NEW GAME
    // -----------------------------------------------
    public void OnClick_MenuContinue()
    {
        dangChonDeContinue = true;
        MoBangGiaoDienSlot();
    }

    public void OnClick_MenuNewGame()
    {
        dangChonDeContinue = false;
        MoBangGiaoDienSlot();
    }

    void MoBangGiaoDienSlot()
    {
        homePanel.SetActive(false);
        if (saveSlotPanel != null) saveSlotPanel.SetActive(true);

        CapNhatChuTrenSlot(1, txtSlot1);
        CapNhatChuTrenSlot(2, txtSlot2);
        CapNhatChuTrenSlot(3, txtSlot3);
    }

    void CapNhatChuTrenSlot(int slot, TMP_Text txt)
    {
        if (txt == null) return;
        if (SaveSystem.KiemTraSlotTonTai(slot))
        {
            PlayerData data = SaveSystem.XemTruocDataSlot(slot);
            txt.text = $"HỒ SƠ {slot}\nTầng {data.mapHienTai} — {data.soManhHon} 💎";
        }
        else
        {
            txt.text = $"HỒ SƠ {slot}\n(Trống)";
        }
    }

    // -----------------------------------------------
    // CHỌN SLOT
    // -----------------------------------------------
    public void OnClick_ChonSlot1() { XuLyBamSlotVatLy(1); }
    public void OnClick_ChonSlot2() { XuLyBamSlotVatLy(2); }
    public void OnClick_ChonSlot3() { XuLyBamSlotVatLy(3); }

    void XuLyBamSlotVatLy(int slot)
    {
        dangChonSlot = slot;
        SaveSystem.currentSlotIndex = dangChonSlot;

        if (dangChonDeContinue)
        {
            if (SaveSystem.KiemTraSlotTonTai(slot))
                OnClick_TiepTucThucSu();
            else
                Debug.Log($"[!]️ Slot {slot} đang trống!");
        }
        else
        {
            if (SaveSystem.KiemTraSlotTonTai(slot))
            {
                if (slotActionPanel != null) slotActionPanel.SetActive(true);
            }
            else
            {
                OnClick_NewGameThucSu();
            }
        }
    }

    // -----------------------------------------------
    // HÀNH ĐỘNG THỰC SỰ
    // -----------------------------------------------
    public void OnClick_TiepTucThucSu()
    {
        if (SaveSystem.CoFileSave())
        {
            PlayerData data = SaveSystem.LoadGame();
            Debug.Log($"[GAME] Tiếp tục Hồ sơ {dangChonSlot} (Tầng {data.mapHienTai})");
            SceneManager.LoadScene(tenSceneGame);
        }
    }

    public void OnClick_NewGameThucSu()
    {
        SaveSystem.currentSlotIndex = dangChonSlot;
        SaveSystem.DeleteSave();
        SaveSystem.SaveGame(new PlayerData());
        Debug.Log($"[MOI] Tạo mới Hồ sơ {dangChonSlot} — Biome sẽ được random khi vào game!");
        SceneManager.LoadScene(tenSceneGame);
    }

    // -----------------------------------------------
    // BACK / ĐÓNG
    // -----------------------------------------------
    public void OnClick_DongBangSlot()
    {
        if (saveSlotPanel   != null) saveSlotPanel.SetActive(false);
        if (slotActionPanel != null) slotActionPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void OnClick_DongActionPanel()
    {
        if (slotActionPanel != null) slotActionPanel.SetActive(false);
    }

    // -----------------------------------------------
    // SETTINGS
    // -----------------------------------------------
    public void OnClick_Settings()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnClick_Back()
    {
        if (inputRong != null && int.TryParse(inputRong.text, out int rong) && rong > 0)
            GameSettings.rong = rong;
        if (inputDai != null && int.TryParse(inputDai.text, out int dai) && dai > 0)
            GameSettings.dai = dai;

        if (sliderChieuCao != null) GameSettings.chieuCaoTuong = sliderChieuCao.value;
        if (sliderDoDay    != null) GameSettings.doDayTuong    = sliderDoDay.value;

        settingsPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void OnClick_Exit() => Application.Quit();
}

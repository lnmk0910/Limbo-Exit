// MenuManager.cs
// Điều khiển Main Menu: HomePanel, SettingsPanel, BiomePanel
// GẮN vào: Canvas_MainMenu

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("=== PANELS ===")]
    public GameObject homePanel;
    public GameObject settingsPanel;
    public GameObject biomePanel;       // Panel chọn Biome (mới)

    [Header("=== GIAO DIỆN SAVE SLOT ===")]
    public GameObject btnContinue;      // Cục nút Continue ngoài sảnh chính
    public GameObject saveSlotPanel;    // Bảng liệt kê 3 khung Hồ sơ
    public GameObject slotActionPanel;  // Bảng hỏi Cảnh báo (chỉ dùng khi ấn Phím NewGame mà lỡ chọn Slot đã có dữ liệu)
    
    [Header("=== TEXT CỦA 3 KHUNG SLOT ===")]
    public TMP_Text txtSlot1;
    public TMP_Text txtSlot2;
    public TMP_Text txtSlot3;

    private int dangChonSlot = 0;
    private bool dangChonDeContinue = true; // Cờ kiểm tra Workflow

    [Header("=== INPUT FIELDS ===")]
    public TMP_InputField inputRong;
    public TMP_InputField inputDai;

    [Header("=== SLIDERS ===")]
    public UnityEngine.UI.Slider sliderChieuCao;
    public UnityEngine.UI.Slider sliderDoDay;

    [Header("=== BIOME BUTTONS ===")]
    // Mảng 4 button chọn biome (highlight button đang chọn)
    public Button[] danhSachBtnBiome;
    private int biomeChon = 0;

    [Header("=== BIOME INFO DISPLAY ===")]
    public TMP_Text txtTenBiome;        // Hiện tên biome đang chọn
    public TMP_Text txtMoTaBiome;       // Hiện mô tả biome
    public Image imgAnhBiome;           // Ảnh đại diện biome

    [Header("=== SCENE ===")]
    public string tenSceneGame = "GameScene";

    // -----------------------------------------------
    void Start()
    {
        AudioManager.PhatBGMTheoScene("MenuScene");

        homePanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (biomePanel != null) biomePanel.SetActive(false);
        if (saveSlotPanel != null) saveSlotPanel.SetActive(false);
        if (slotActionPanel != null) slotActionPanel.SetActive(false);

        dangChonSlot = 0;

        // Hiện nút Continue ở màn hình gốc chừng nào còn có ÍT NHẤT 1 hồ sơ lưu
        if (btnContinue != null)
        {
            btnContinue.SetActive(
                SaveSystem.KiemTraSlotTonTai(1) ||
                SaveSystem.KiemTraSlotTonTai(2) ||
                SaveSystem.KiemTraSlotTonTai(3)
            );
        }

        if (inputRong != null) inputRong.text = GameSettings.rong.ToString();
        if (inputDai != null) inputDai.text  = GameSettings.dai.ToString();
        if (sliderChieuCao != null) sliderChieuCao.value = GameSettings.chieuCaoTuong;
        if (sliderDoDay != null) sliderDoDay.value    = GameSettings.doDayTuong;

        biomeChon = GameSettings.biomeIndex;
        CapNhatHienThiBiome();
    }

    void Update()
    {
        // 1. KHI ĐANG HỎI CẢNH BÁO LƯU ĐÈ (Action Panel)
        if (slotActionPanel != null && slotActionPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Y)) OnClick_NewGameThucSu();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.N))
            {
                dangChonSlot = 0;
                OnClick_DongActionPanel();
            }
        }
        // 2. KHI ĐANG HIỆN BẢNG CHỌN SLOT 1, 2, 3
        else if (saveSlotPanel != null && saveSlotPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnClick_ChonSlot1();
            if (Input.GetKeyDown(KeyCode.Alpha2)) OnClick_ChonSlot2();
            if (Input.GetKeyDown(KeyCode.Alpha3)) OnClick_ChonSlot3();
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_DongBangSlot();
        }
        // 3. KHI ĐANG TRONG SETTINGS
        else if (settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_Back();
        }
        // 4. KHI ĐANG MỞ BIOME PANEL (NẾU CÒN DÙNG)
        else if (biomePanel != null && biomePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnClick_ChonBiomeIndex(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) OnClick_ChonBiomeIndex(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) OnClick_ChonBiomeIndex(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) OnClick_ChonBiomeIndex(3);
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_BiomeBack();
        }
        // 5. KHI ĐANG Ở SẢNH CHÍNH
        else if (homePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Return)) OnClick_MenuContinue();
            if (Input.GetKeyDown(KeyCode.N)) OnClick_MenuNewGame();
            if (Input.GetKeyDown(KeyCode.S)) OnClick_Settings();
            if (Input.GetKeyDown(KeyCode.Escape)) OnClick_Exit();
        }
    }

    // -----------------------------------------------
    // 1. MỞ BẢNG QUẢN LÝ HỒ SƠ 
    // -----------------------------------------------

    // Gắn vào Nút CONTINUE ngoài sảnh chính
    public void OnClick_MenuContinue()
    {
        dangChonDeContinue = true;
        MoBangGiaoDienSlot();
    }

    // Gắn vào Nút NEW GAME ngoài sảnh chính
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
            txt.text = $"HỒ SƠ {slot}\nTầng {data.mapHienTai} - {data.soManhHon} 💎";
        }
        else
        {
            txt.text = $"HỒ SƠ {slot}\n(Trống)";
        }
    }

    // -----------------------------------------------
    // 2. KHI NGƯỜI CHƠI BẤM VÀO TỪNG CÁI KHUNG SLOT VẬT LÝ
    // -----------------------------------------------
    public void OnClick_ChonSlot1() { XuLyBamSlotVatLy(1); }
    public void OnClick_ChonSlot2() { XuLyBamSlotVatLy(2); }
    public void OnClick_ChonSlot3() { XuLyBamSlotVatLy(3); }

    void XuLyBamSlotVatLy(int slot)
    {
        dangChonSlot = slot;
        SaveSystem.currentSlotIndex = dangChonSlot; 

        if (dangChonDeContinue) // WORKFLOW: CONTINUE
        {
            if (SaveSystem.KiemTraSlotTonTai(slot))
            {
                // BAY THẲNG VÀO GAME NẾU ĐÃ CÓ DATA
                OnClick_TiepTucThucSu();
            }
            else
            {
                Debug.Log($"⚠️ Slot {slot} này đang trống, làm sao mà Continue!");
            }
        }
        else // WORKFLOW: NEW GAME
        {
            if (SaveSystem.KiemTraSlotTonTai(slot))
            {
                // CẢNH BÁO BẰNG BẢNG SLOT ACTION PANEL NẾU MUỐN LƯU ĐÈ
                if (slotActionPanel != null) slotActionPanel.SetActive(true);
            }
            else
            {
                // KHÔNG CÓ DATA THÌ QUẤT THẲNG NEW GAME LUÔN
                OnClick_NewGameThucSu();
            }
        }
    }

    // -----------------------------------------------
    // 3. THAO TÁC ACTION (Hành động Thực sự)
    // -----------------------------------------------
    public void OnClick_TiepTucThucSu()
    {
        if (SaveSystem.CoFileSave())
        {
            PlayerData data = SaveSystem.LoadGame();
            Debug.Log($"🎮 Tiếp tục Hồ sơ {dangChonSlot} (Tầng {data.mapHienTai})");
            SceneManager.LoadScene(tenSceneGame);
        }
    }

    // Được gọi khi nhấn Nút [Xoá tạo mới] trong Bảng Cảnh Báo LƯU ĐÈ
    // Hoặc gọi thẳng nếu slot Trống
    public void OnClick_NewGameThucSu()
    {
        SaveSystem.currentSlotIndex = dangChonSlot;
        SaveSystem.DeleteSave();
        PlayerData dataMoi = new PlayerData();
        SaveSystem.SaveGame(dataMoi);
        Debug.Log($"🆕 Xây mới dữ liệu Hồ sơ {dangChonSlot}!");
        SceneManager.LoadScene(tenSceneGame);
    }

    // -----------------------------------------------
    // 4. QUAY ĐẦU (BACK)
    // -----------------------------------------------
    public void OnClick_DongBangSlot()
    {
        if (saveSlotPanel != null) saveSlotPanel.SetActive(false);
        if (slotActionPanel != null) slotActionPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void OnClick_DongActionPanel()
    {
        if (slotActionPanel != null) slotActionPanel.SetActive(false);
    }

    public void OnClick_Settings()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnClick_ChonBiome()
    {
        homePanel.SetActive(false);
        if (biomePanel != null) biomePanel.SetActive(true);
    }

    public void OnClick_Exit() => Application.Quit();

    // -----------------------------------------------
    // SETTINGS PANEL
    // -----------------------------------------------
    public void OnClick_Back()
    {
        if (int.TryParse(inputRong.text, out int rong) && rong > 0)
            GameSettings.rong = rong;
        if (int.TryParse(inputDai.text, out int dai) && dai > 0)
            GameSettings.dai = dai;

        GameSettings.chieuCaoTuong = sliderChieuCao.value;
        GameSettings.doDayTuong    = sliderDoDay.value;

        settingsPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    // -----------------------------------------------
    // BIOME PANEL
    // -----------------------------------------------
    // Gọi từ mỗi Button biome: OnClick_ChonBiome(0), (1), (2), (3)
    public void OnClick_ChonBiomeIndex(int index)
    {
        biomeChon = index;
        GameSettings.biomeIndex = index;
        CapNhatHienThiBiome();
        Debug.Log($"🌍 Đã chọn Biome Index: {index}");
    }

    public void OnClick_BiomeBack()
    {
        if (biomePanel != null) biomePanel.SetActive(false);
        homePanel.SetActive(true);
    }

    // Cập nhật tên, mô tả biome theo index đang chọn
    void CapNhatHienThiBiome()
    {
        string[] tenBiome = {
            "🗿 Mê Cung Đá Cổ",
            "📚 Thư Viện Vô Tận",
            "🌿 Đầm Lầy Sương Mù",
            "💎 Mê Cung Tinh Thể"
        };
        string[] moTa = {
            "Tường đá rêu phong, bẫy chông ẩn trong bóng tối cổ đại.",
            "Kệ sách cao ngất, sàn gỗ kêu cọt kẹt. Cẩn thận tạo tiếng ồn!",
            "Sương mù dày đặc, nước ngập mắt cá chân làm bạn chậm lại.",
            "Vách tinh thể phản chiếu ánh sáng huyền ảo, mê hoặc và nguy hiểm."
        };

        if (txtTenBiome  != null) txtTenBiome.text  = tenBiome[Mathf.Clamp(biomeChon, 0, 3)];
        if (txtMoTaBiome != null) txtMoTaBiome.text = moTa[Mathf.Clamp(biomeChon, 0, 3)];

        // Highlight button đang chọn
        for (int i = 0; i < danhSachBtnBiome.Length; i++)
        {
            if (danhSachBtnBiome[i] == null) continue;
            var colors = danhSachBtnBiome[i].colors;
            colors.normalColor = (i == biomeChon) ? new Color(1f, 0.8f, 0f) : Color.white;
            danhSachBtnBiome[i].colors = colors;
        }
    }
}

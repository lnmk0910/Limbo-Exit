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
        homePanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (biomePanel != null) biomePanel.SetActive(false);

        inputRong.text = GameSettings.rong.ToString();
        inputDai.text  = GameSettings.dai.ToString();
        sliderChieuCao.value = GameSettings.chieuCaoTuong;
        sliderDoDay.value    = GameSettings.doDayTuong;

        biomeChon = GameSettings.biomeIndex;
        CapNhatHienThiBiome();
    }

    // -----------------------------------------------
    // HOME PANEL
    // -----------------------------------------------
    public void OnClick_Continue()
    {
        if (SaveSystem.CoFileSave())
        {
            PlayerData data = SaveSystem.LoadGame();
            Debug.Log($"🎮 Tiếp tục Màn {data.mapHienTai} | Mảnh Hồn: {data.soManhHon}");
            SceneManager.LoadScene(tenSceneGame);
        }
        else
            Debug.Log("⚠️ Chưa có save! Hãy chọn 'Chơi Mới'.");
    }

    public void OnClick_NewGame()
    {
        SaveSystem.DeleteSave();
        PlayerData dataMoi = new PlayerData();
        SaveSystem.SaveGame(dataMoi);
        Debug.Log("🆕 Bắt đầu game mới!");
        SceneManager.LoadScene(tenSceneGame);
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

// MenuManager.cs
// Script chính điều khiển toàn bộ Main Menu
// GẮN vào: GameObject Canvas_MainMenu
// Kéo thả các UI element vào đúng slot trong Inspector

using UnityEngine;
using UnityEngine.SceneManagement;  // Dùng để chuyển Scene
using TMPro;                        // Dùng TextMeshPro InputField

public class MenuManager : MonoBehaviour
{
    [Header("=== PANELS ===")]
    public GameObject homePanel;        // Kéo HomePanel vào đây
    public GameObject settingsPanel;    // Kéo SettingsPanel vào đây

    [Header("=== INPUT FIELDS (Settings) ===")]
    public TMP_InputField inputRong;           // Kéo InputField_Width
    public TMP_InputField inputDai;            // Kéo InputField_Height

    [Header("=== SLIDERS (Settings) ===")]
    public UnityEngine.UI.Slider sliderChieuCao;   // Kéo Slider_WallHeight
    public UnityEngine.UI.Slider sliderDoDay;      // Kéo Slider_WallThickness

    [Header("=== TÊN SCENE GAME ===")]
    public string tenSceneGame = "GameScene";  // Tên Scene game sẽ tạo sau

    // =============================================
    // Chạy khi Scene được load lần đầu
    // =============================================
    void Start()
    {
        // Hiện HomePanel, ẩn SettingsPanel
        homePanel.SetActive(true);
        settingsPanel.SetActive(false);

        // Điền giá trị mặc định vào các Input/Slider
        inputRong.text = GameSettings.rong.ToString();
        inputDai.text = GameSettings.dai.ToString();
        sliderChieuCao.value = GameSettings.chieuCaoTuong;
        sliderDoDay.value = GameSettings.doDayTuong;
    }

    // =============================================
    // NÚT: CHƠI TIẾP (btn_Continue)
    // Đọc save JSON → Load vào Scene Game
    // =============================================
    public void OnClick_Continue()
    {
        if (SaveSystem.CoFileSave())
        {
            // Có file save → đọc dữ liệu
            PlayerData data = SaveSystem.LoadGame();
            Debug.Log("🎮 Tiếp tục từ Map: " + data.mapHienTai
                      + " | Mảnh Hồn: " + data.soManhHon);

            // Chuyển sang Scene Game
            SceneManager.LoadScene(tenSceneGame);
        }
        else
        {
            // Chưa có save → thông báo và không làm gì
            Debug.Log("⚠️ Chưa có dữ liệu lưu! Hãy chọn 'Chơi Mới'.");
        }
    }

    // =============================================
    // NÚT: CHƠI MỚI (btn_NewGame)
    // Cảnh báo → Xóa save → Load Scene Game
    // =============================================
    public void OnClick_NewGame()
    {
        // TODO Giai đoạn sau: thêm popup xác nhận trước khi xóa
        // Hiện tại: xóa save cũ và bắt đầu game mới luôn
        SaveSystem.DeleteSave();

        // Tạo dữ liệu mới và lưu ngay
        PlayerData dataMoi = new PlayerData();
        SaveSystem.SaveGame(dataMoi);

        Debug.Log("🆕 Bắt đầu game mới!");
        SceneManager.LoadScene(tenSceneGame);
    }

    // =============================================
    // NÚT: CÀI ĐẶT MAP (btn_Settings)
    // Ẩn HomePanel → Hiện SettingsPanel
    // =============================================
    public void OnClick_Settings()
    {
        homePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // =============================================
    // NÚT: THOÁT (btn_Exit)
    // =============================================
    public void OnClick_Exit()
    {
        Debug.Log("👋 Thoát game.");
        Application.Quit();
    }

    // =============================================
    // NÚT: QUAY LẠI (btn_Back trong SettingsPanel)
    // Đọc giá trị từ UI → lưu vào GameSettings → về HomePanel
    // =============================================
    public void OnClick_Back()
    {
        // Đọc giá trị từ InputField, dùng TryParse để tránh lỗi chữ
        if (int.TryParse(inputRong.text, out int rong) && rong > 0)
            GameSettings.rong = rong;

        if (int.TryParse(inputDai.text, out int dai) && dai > 0)
            GameSettings.dai = dai;

        // Đọc giá trị Slider
        GameSettings.chieuCaoTuong = sliderChieuCao.value;
        GameSettings.doDayTuong = sliderDoDay.value;

        Debug.Log($"⚙️ Cài đặt Map: {GameSettings.rong}x{GameSettings.dai}" +
                  $" | Tường: cao={GameSettings.chieuCaoTuong}, dày={GameSettings.doDayTuong}");

        // Quay về HomePanel
        settingsPanel.SetActive(false);
        homePanel.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MenuTamDung : MonoBehaviour
{
    [Header("=== GIAO DIỆN CHÍNH ===")]
    public GameObject hubPanel;      // Bảng gộp chung cho PauseMenu
    public GameObject pausePanel;    // Bảng Tạm Dừng hiển thị đầu tiên
    public GameObject settingsPanel; // Bảng Setting tùy chỉnh

    [Header("=== INPUT SETTINGS (Tương tự MenuManager) ===")]
    public TMP_InputField inputRong;
    public TMP_InputField inputDai;
    public Slider sliderChieuCao;
    public Slider sliderDoDay;

    private bool isPaused = false;

    void Start()
    {
        // Ẩn menu khi mới vào game
        if (hubPanel != null) hubPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (!isPaused)
        {
            // Nhấn ESC khi đang chơi → Mở Pause
            if (Input.GetKeyDown(KeyCode.Escape))
                PauseGame();
            return;
        }

        // ── ĐANG Ở BẢNG SETTINGS ──
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            // ESC hoặc X → Đóng Settings, quay về Pause
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
                DongSettings();

            // Enter hoặc S → Lưu & Đóng Settings
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.S))
                LuuVaDongSettings();

            return; // Không xử lý phím Pause khi đang ở Settings
        }

        // ── ĐANG Ở BẢNG PAUSE CHÍNH ──

        // ESC hoặc C hoặc Enter → Chơi Tiếp (Resume)
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.C)       ||
            Input.GetKeyDown(KeyCode.Return))
        {
            OnClick_ChoiTiep();
            return;
        }

        // G → Mở Cài Đặt (Settings)
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnClick_MoSettings();
            return;
        }

        // M → Về Menu Chính
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnClick_VeMenu();
            return;
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Nếu có Hub thì bật Hub, không thì bật từng Panel
        if (hubPanel != null) hubPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Nếu có Hub thì tắt Hub, không thì tắt từng Panel
        if (hubPanel != null) hubPanel.SetActive(false);
        else
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }

    // -----------------------------------------------
    // CÁC NÚT BẤM TRONG BẢNG TẠM DỪNG
    // -----------------------------------------------
    public void OnClick_ChoiTiep()
    {
        ResumeGame();
    }

    public void OnClick_VeMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClick_MoSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // Nạp dữ liệu hiện tại vào nút
        if (inputRong != null) inputRong.text = GameSettings.rong.ToString();
        if (inputDai != null) inputDai.text = GameSettings.dai.ToString();
        if (sliderChieuCao != null) sliderChieuCao.value = GameSettings.chieuCaoTuong;
        if (sliderDoDay != null) sliderDoDay.value = GameSettings.doDayTuong;
    }

    // -----------------------------------------------
    // CÁC NÚT NẰM TRONG BẢNG SETTING (LƯU & ĐÓNG)
    // -----------------------------------------------
    public void DongSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void LuuVaDongSettings()
    {
        // Lưu thông số UI vào GameSettings
        if (inputRong != null && int.TryParse(inputRong.text, out int rong)) GameSettings.rong = rong;
        if (inputDai != null && int.TryParse(inputDai.text, out int dai)) GameSettings.dai = dai;
        if (sliderChieuCao != null) GameSettings.chieuCaoTuong = sliderChieuCao.value;
        if (sliderDoDay != null) GameSettings.doDayTuong = sliderDoDay.value;

        // Trở về bảng Pause
        DongSettings();
    }
}

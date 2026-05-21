// MenuTamDung.cs — Menu Pause: tạm dừng, cài đặt, về menu
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MenuTamDung : MonoBehaviour
{
    [Header("=== GIAO DIỆN ===")]
    public GameObject hubPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("=== INPUT SETTINGS ===")]
    public TMP_InputField inputRong;
    public TMP_InputField inputDai;
    public Slider sliderChieuCao;
    public Slider sliderDoDay;

    [Header("=== ÂM THANH ===")]
    public Toggle toggleAmThanh;
    public TMP_Text txtTrangThaiAmThanh;

    private bool isPaused = false;

    // Tat het panel luc khoi dong
    void Start()
    {
        if (hubPanel != null) hubPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Lang nghe phim tat khi dang pause
    void Update()
    {
        if (!isPaused)
        {
            bool coThePause = UIManager.DangTrongGame() && !UIManager.DangO(UIManager.TrangThaiUI.ChienThang);
            if (Input.GetKeyDown(KeyCode.Escape) && coThePause) PauseGame();
            return;
        }

        if (!UIManager.DangO(UIManager.TrangThaiUI.Pause)) return;

        // Settings panel
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X)) DongSettings();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.S)) LuuVaDongSettings();
            if (Input.GetKeyDown(KeyCode.T)) ToggleAmThanh();
            return;
        }

        // Pause panel
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Return))
        { OnClick_ChoiTiep(); return; }
        if (Input.GetKeyDown(KeyCode.G)) { OnClick_MoSettings(); return; }
        if (Input.GetKeyDown(KeyCode.M)) { OnClick_VeMenu(); return; }
    }

    // Mo menu pause va khoa thoi gian
    public void PauseGame()
    {
        isPaused = true;
        UIManager.Mo(UIManager.TrangThaiUI.Pause);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (hubPanel != null) hubPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Dong pause va tra lai trang thai game
    public void ResumeGame()
    {
        isPaused = false;
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (hubPanel != null) hubPanel.SetActive(false);
        else
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }

    public void OnClick_ChoiTiep() => ResumeGame();

    public void OnClick_VeMenu()
    {
        isPaused = false;
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Mo bang cai dat va nap du lieu hien tai
    public void OnClick_MoSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (inputRong != null) inputRong.text = GameSettings.rong.ToString();
        if (inputDai != null) inputDai.text = GameSettings.dai.ToString();
        if (sliderChieuCao != null) sliderChieuCao.value = GameSettings.chieuCaoTuong;
        if (sliderDoDay != null) sliderDoDay.value = GameSettings.doDayTuong;
        if (toggleAmThanh != null)
        {
            toggleAmThanh.onValueChanged.RemoveAllListeners();
            toggleAmThanh.isOn = GameSettings.coAmThanh;
            toggleAmThanh.onValueChanged.AddListener(OnToggleAmThanh);
        }
        CapNhatTextAmThanh();
    }

    // Dong bang settings va quay lai pause
    public void DongSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    // Luu cai dat va dong settings
    public void LuuVaDongSettings()
    {
        if (inputRong != null && int.TryParse(inputRong.text, out int rong)) GameSettings.rong = rong;
        if (inputDai != null && int.TryParse(inputDai.text, out int dai)) GameSettings.dai = dai;
        if (sliderChieuCao != null) GameSettings.chieuCaoTuong = sliderChieuCao.value;
        if (sliderDoDay != null) GameSettings.doDayTuong = sliderDoDay.value;
        DongSettings();
    }

    // Xu ly toggle am thanh tu UI
    void OnToggleAmThanh(bool batTieng)
    {
        GameSettings.coAmThanh = batTieng;
        CapNhatTextAmThanh();
    }

    // Phim tat bat/tat am thanh
    public void ToggleAmThanh()
    {
        if (toggleAmThanh != null) toggleAmThanh.isOn = !toggleAmThanh.isOn;
        else { GameSettings.coAmThanh = !GameSettings.coAmThanh; CapNhatTextAmThanh(); }
    }

    // Cap nhat text hien thi trang thai am thanh
    void CapNhatTextAmThanh()
    {
        if (txtTrangThaiAmThanh != null)
            txtTrangThaiAmThanh.text = GameSettings.coAmThanh ? "Âm thanh: BẬT" : "Âm thanh: TẮT";
    }
}

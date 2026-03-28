// DeathScreen.cs - Phiên bản sửa lỗi
// Thêm phím tắt, xử lý null an toàn hơn

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [Header("=== UI ===")]
    public GameObject panelChet;
    public TMP_Text txtThongBao;
    public TMP_Text txtManhHonConLai;

    [Header("=== SCENE ===")]
    public string tenSceneMenu = "MainMenu";

    [Header("=== THỜI GIAN CHỜ (giây) ===")]
    public float thoiGianCho = 1.5f;

    private bool dangHien = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelChet != null) panelChet.SetActive(false);
    }

    void Update()
    {
        if (!dangHien) return;

        // Phím tắt: Enter = Tiếp tục | Escape = Từ bỏ (phòng khi click UI lỗi)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnClick_TiepTuc();

        if (Input.GetKeyDown(KeyCode.Escape))
            OnClick_TuBo();
    }

    // -----------------------------------------------
    // GỌI KHI BỊ BẮT
    // -----------------------------------------------
    public void HienManHinhChet()
    {
        if (dangHien) return;
        StartCoroutine(TrinhTuHienThi());
    }

    IEnumerator TrinhTuHienThi()
    {
        dangHien = true;

        // Mở chuột TRƯỚC để đảm bảo click được
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Dừng game
        Time.timeScale = 0f;

        // Chờ thật (không bị ảnh hưởng bởi timeScale)
        yield return new WaitForSecondsRealtime(thoiGianCho);

        // Cập nhật text
        PlayerData data = SaveSystem.LoadGame();
        int conLai = Mathf.FloorToInt(data.soManhHon * 0.5f);

        if (txtThongBao != null)
            txtThongBao.text = "🌑 Bóng tối đã nuốt chửng bạn...";

        if (txtManhHonConLai != null)
            txtManhHonConLai.text =
                $"💎 Mảnh Hồn còn lại: {conLai} (mất 50%)\n" +
                $"🎒 Toàn bộ vật phẩm bị tịch thu\n\n" +
                $"[Enter/Space] Tiếp tục   |   [Esc] Từ bỏ";

        if (panelChet != null) panelChet.SetActive(true);
    }

    // -----------------------------------------------
    // TIẾP TỤC → Hồi sinh ngẫu nhiên
    // -----------------------------------------------
    public void OnClick_TiepTuc()
    {
        if (!dangHien) return;
        dangHien = false;

        // Ẩn panel
        if (panelChet != null) panelChet.SetActive(false);

        // Phục hồi game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Hồi sinh qua RespawnManager
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.HoiSinhPlayer();
        else
        {
            // Fallback: reload Scene nếu không tìm thấy RespawnManager
            Debug.LogWarning("⚠️ Không có RespawnManager → Reload Scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // -----------------------------------------------
    // TỪ BỎ → Về Main Menu
    // -----------------------------------------------
    public void OnClick_TuBo()
    {
        if (!dangHien) return;
        dangHien = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMenu);
    }
}

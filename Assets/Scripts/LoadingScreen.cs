// LoadingScreen.cs
// Màn hình chuyển cảnh giữa các Biome — che lấp delay tạo mê cung
// Hiện fade-in → text "Đang tạo mê cung..." → fade-out sau khi scene load xong
// GẮN vào: Canvas_Loading (DontDestroyOnLoad)

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("=== UI ===")]
    public CanvasGroup canvasGroup;      // CanvasGroup trên Panel_Loading
    public TMP_Text   txtTrangThai;      // "Đang tạo mê cung..."
    public TMP_Text   txtTangSo;         // "Tầng 2 / 4"
    public Image      imgNenBiome;       // Ảnh nền theo biome (tùy chọn)

    [Header("=== BIOME BACKGROUNDS (tùy chọn) ===")]
    public Sprite[] anhBiome;            // 4 sprite theo biome 0-3

    [Header("=== THỜI GIAN ===")]
    public float thoiGianFade     = 0.4f;
    public float thoiGianHienToiThieu = 1.2f; // Tối thiểu bao lâu phải hiện

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; }
    }

    // -----------------------------------------------
    // GỌI TỪ BÊN NGOÀI: Load scene với loading screen
    // -----------------------------------------------
    public static void LoadScene(string tenScene, int biomeIndex = -1)
    {
        if (Instance == null)
        {
            // Fallback nếu không có LoadingScreen
            SceneManager.LoadScene(tenScene);
            return;
        }
        Instance.StartCoroutine(Instance.ThucHienLoad(tenScene, biomeIndex));
    }

    // -----------------------------------------------
    // COROUTINE CHÍNH
    // -----------------------------------------------
    IEnumerator ThucHienLoad(string tenScene, int biomeIndex)
    {
        // ---- BƯỚC 1: Cập nhật text & ảnh ----
        CapNhatThongTin(biomeIndex);

        // ---- BƯỚC 2: Fade IN ----
        yield return StartCoroutine(Fade(0f, 1f));
        canvasGroup.blocksRaycasts = true;

        // ---- BƯỚC 3: Load scene async ----
        float tBatDau = Time.realtimeSinceStartup;
        AsyncOperation op = SceneManager.LoadSceneAsync(tenScene);
        op.allowSceneActivation = false;

        // Chờ load + đảm bảo tối thiểu thoiGianHienToiThieu giây
        while (!op.isDone)
        {
            float elapsed = Time.realtimeSinceStartup - tBatDau;
            if (op.progress >= 0.9f && elapsed >= thoiGianHienToiThieu)
                op.allowSceneActivation = true;
            yield return null;
        }

        // ---- BƯỚC 4: Fade OUT ----
        canvasGroup.blocksRaycasts = false;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    void CapNhatThongTin(int biomeIndex)
    {
        string[] tenBiome = { "Đá Cổ", "Thư Viện Cấm", "Đầm Lầy Tối", "Tinh Thể Hư Vô" };

        if (txtTrangThai != null)
            txtTrangThai.text = "Đang tạo mê cung...";

        if (txtTangSo != null)
        {
            PlayerData data = SaveSystem.LoadGame();
            int tong = data.biomeSequence != null ? data.biomeSequence.Length : 4;
            string tenB = (biomeIndex >= 0 && biomeIndex < tenBiome.Length) ? tenBiome[biomeIndex] : "";
            txtTangSo.text = $"Tầng {data.mapHienTai} / {tong}  {(tenB != "" ? $"— {tenB}" : "")}";
        }

        if (imgNenBiome != null && anhBiome != null
            && biomeIndex >= 0 && biomeIndex < anhBiome.Length
            && anhBiome[biomeIndex] != null)
        {
            imgNenBiome.sprite = anhBiome[biomeIndex];
        }
    }

    IEnumerator Fade(float tu, float den)
    {
        float t = 0f;
        while (t < thoiGianFade)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(tu, den, t / thoiGianFade);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = den;
    }
}

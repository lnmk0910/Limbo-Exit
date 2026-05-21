// LoadingScreen.cs — Màn hình chuyển cảnh giữa các Biome
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("=== UI ===")]
    public CanvasGroup canvasGroup;
    public TMP_Text   txtTrangThai;
    public TMP_Text   txtTangSo;
    public Image      imgNenBiome;

    [Header("=== BIOME BACKGROUNDS ===")]
    public Sprite[] anhBiome;

    [Header("=== THỜI GIAN ===")]
    public float thoiGianFade          = 0.4f;
    public float thoiGianHienToiThieu  = 1.2f;

    // Khoi tao singleton va an man hinh
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; }
    }

    // API static de load scene co hieu ung
    public static void LoadScene(string tenScene, int biomeIndex = -1)
    {
        if (Instance == null) { SceneManager.LoadScene(tenScene); return; }
        Instance.StartCoroutine(Instance.ThucHienLoad(tenScene, biomeIndex));
    }

    // Fade in -> load async -> fade out
    IEnumerator ThucHienLoad(string tenScene, int biomeIndex)
    {
        CapNhatThongTin(biomeIndex);
        yield return StartCoroutine(Fade(0f, 1f));
        canvasGroup.blocksRaycasts = true;

        float tBatDau = Time.realtimeSinceStartup;
        AsyncOperation op = SceneManager.LoadSceneAsync(tenScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float elapsed = Time.realtimeSinceStartup - tBatDau;
            if (op.progress >= 0.9f && elapsed >= thoiGianHienToiThieu)
                op.allowSceneActivation = true;
            yield return null;
        }

        canvasGroup.blocksRaycasts = false;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    // Cap nhat thong tin tang va anh nen biome
    void CapNhatThongTin(int biomeIndex)
    {
        string[] tenBiome = { "Đá Cổ", "Thư Viện Cấm", "Đầm Lầy Tối", "Tinh Thể Hư Vô" };

        if (txtTrangThai != null) txtTrangThai.text = "Đang tạo mê cung...";

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
            imgNenBiome.sprite = anhBiome[biomeIndex];
    }

    // Fade canvas group theo thoi gian unscaled
    IEnumerator Fade(float tu, float den)
    {
        float t = 0f;
        while (t < thoiGianFade)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(tu, den, t / thoiGianFade);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = den;
    }
}

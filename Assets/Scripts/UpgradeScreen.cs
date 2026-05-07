// UpgradeScreen.cs
// Man hinh Nang Cap Vinh Vien giua cac man
// 4 loai nang cap dung Manh Hon (MH)
// GAN vao: GameObject "UpgradeScreen"

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeScreen : MonoBehaviour
{
    public static UpgradeScreen Instance { get; private set; }

    [Header("=== PANEL ===")]
    public GameObject panelUpgrade;

    [Header("=== HIEN THI TONG ===")]
    public TMP_Text txtManhHon;

    [Header("=== TOC DO DI CHUYEN ===")]
    public TMP_Text txtCapTocDo;
    public TMP_Text txtGiaTocDo;
    public int giaTocDo     = 8;
    public int capToiDaTocDo = 5;

    [Header("=== LA BAN (them thoi gian) ===")]
    public TMP_Text txtCapLaBan;
    public TMP_Text txtGiaLaBan;
    public int giaLaBan      = 6;
    public int capToiDaLaBan = 3;

    [Header("=== GIAM GIA SHOP ===")]
    public TMP_Text txtCapGiaShop;
    public TMP_Text txtGiaGiaShop;
    public int giaGiamShop      = 5;
    public int capToiDaGiamShop = 3;

    [Header("=== GIAM TAM PHAT HIEN ===")]
    public TMP_Text txtCapTamPH;
    public TMP_Text txtGiaTamPH;
    public int giaTamPhatHien   = 7;
    public int capToiDaTamPH    = 3;

    [Header("=== SCENE ===")]
    public string tenSceneGame = "GameScene";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
    }

    void Update()
    {
        if (panelUpgrade != null && panelUpgrade.activeSelf)
        {
            if (!UIManager.DangO(UIManager.TrangThaiUI.NangCap)) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) NangCapTocDo_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha2)) NangCapLaBan_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha3)) NangCapShop_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha4)) NangCapTamPH_Btn();
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape)) OnClick_ThuNho();
        }
    }

    // -----------------------------------------------
    // MO man hinh nang cap
    // -----------------------------------------------
    public void MoUpgrade()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(true);
        UIManager.Mo(UIManager.TrangThaiUI.NangCap);
        AudioManager.PhatMoMenu();
        CapNhatUI();
    }

    void CapNhatUI()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (txtManhHon != null)
            txtManhHon.text = $"Manh Hon: {data.soManhHon} MH";

        // Toc do
        if (txtCapTocDo != null)
            txtCapTocDo.text = CapString(data.capTocDo, capToiDaTocDo, "+0.5 toc do");
        if (txtGiaTocDo != null)
            txtGiaTocDo.text = data.capTocDo >= capToiDaTocDo ? "DA TOI DA" : $"{giaTocDo} MH";

        // La ban
        if (txtCapLaBan != null)
            txtCapLaBan.text = CapString(data.capLaBan, capToiDaLaBan, "+1s la ban");
        if (txtGiaLaBan != null)
            txtGiaLaBan.text = data.capLaBan >= capToiDaLaBan ? "DA TOI DA" : $"{giaLaBan} MH";

        // Giam gia shop
        if (txtCapGiaShop != null)
            txtCapGiaShop.text = CapString(data.capGiamGiaShop, capToiDaGiamShop, "-1 gia shop");
        if (txtGiaGiaShop != null)
            txtGiaGiaShop.text = data.capGiamGiaShop >= capToiDaGiamShop ? "DA TOI DA" : $"{giaGiamShop} MH";

        // Giam tam phat hien
        if (txtCapTamPH != null)
            txtCapTamPH.text = CapString(data.capTamPhatHien, capToiDaTamPH, "-1 tam quai");
        if (txtGiaTamPH != null)
            txtGiaTamPH.text = data.capTamPhatHien >= capToiDaTamPH ? "DA TOI DA" : $"{giaTamPhatHien} MH";
    }

    string CapString(int hienTai, int toiDa, string moTa) =>
        $"{moTa}\n[{new string('*', hienTai)}{new string('-', toiDa - hienTai)}] {hienTai}/{toiDa}";

    // ---- Ham nang cap rieng tung loai ----
    void ThucHienNangCap(string loai)
    {
        PlayerData data = SaveSystem.LoadGame();
        int cap = 0, toiDa = 0, gia = 0;
        string ten = "";

        switch (loai)
        {
            case "tocdo":  cap=data.capTocDo;       toiDa=capToiDaTocDo;   gia=giaTocDo;       ten="[TOC DO]";  break;
            case "laban":  cap=data.capLaBan;        toiDa=capToiDaLaBan;   gia=giaLaBan;       ten="[LA BAN]";  break;
            case "shop":   cap=data.capGiamGiaShop;  toiDa=capToiDaGiamShop;gia=giaGiamShop;    ten="[SHOP]";    break;
            case "tamph":  cap=data.capTamPhatHien;  toiDa=capToiDaTamPH;   gia=giaTamPhatHien;  ten="[TAM PH]"; break;
        }

        if (cap >= toiDa) { AudioManager.PhatKhongDuTien(); Debug.Log($"[LOI] {ten} da toi da!"); return; }
        if (data.soManhHon < gia) { AudioManager.PhatKhongDuTien(); Debug.Log($"[LOI] Khong du Manh Hon! Can {gia}"); return; }

        data.soManhHon -= gia;
        switch (loai)
        {
            case "tocdo":  data.capTocDo++;       break;
            case "laban":  data.capLaBan++;        break;
            case "shop":   data.capGiamGiaShop++;  break;
            case "tamph":  data.capTamPhatHien++;  break;
        }

        SaveSystem.SaveGame(data);
        Debug.Log($"[OK] Nang cap {ten} - Cap {cap+1}! Con {data.soManhHon} Manh Hon");
        AudioManager.PhatNangCap();
        GameHUD.LamMoi();
        CapNhatUI();
    }

    // Goi tu Button trong Inspector
    public void NangCapTocDo_Btn()    => ThucHienNangCap("tocdo");
    public void NangCapLaBan_Btn()    => ThucHienNangCap("laban");
    public void NangCapShop_Btn()     => ThucHienNangCap("shop");
    public void NangCapTamPH_Btn()    => ThucHienNangCap("tamph");

    // -----------------------------------------------
    // NUT: THU NHO LAI HUB HOAC TIEP TUC
    // -----------------------------------------------
    public void OnClick_ThuNho()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        UIManager.DongVePanel();

        if (VictoryScreen.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.ChienThang))
            VictoryScreen.Instance.KhoiPhucHienThiHUB();
    }

    public void OnClick_TiepTuc()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        UIManager.DongVeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneGame);
    }
}

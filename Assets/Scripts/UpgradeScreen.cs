// UpgradeScreen.cs
// Màn hình Nâng Cấp Vĩnh Viễn giữa các màn
// 4 loại nâng cấp dùng Mảnh Hồn
// GẮN vào: GameObject "UpgradeScreen"

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeScreen : MonoBehaviour
{
    public static UpgradeScreen Instance { get; private set; }

    [Header("=== PANEL ===")]
    public GameObject panelUpgrade;

    [Header("=== HIỂN THỊ TỔNG ===")]
    public TMP_Text txtManhHon;

    [Header("=== TỐC ĐỘ DI CHUYỂN ===")]
    public TMP_Text txtCapTocDo;
    public TMP_Text txtGiaTocDo;
    public int giaTocDo     = 8;
    public int capToiDaTocDo = 5;

    [Header("=== LA BÀN (thêm thời gian) ===")]
    public TMP_Text txtCapLaBan;
    public TMP_Text txtGiaLaBan;
    public int giaLaBan      = 6;
    public int capToiDaLaBan = 3;

    [Header("=== GIẢM GIÁ SHOP ===")]
    public TMP_Text txtCapGiaShop;
    public TMP_Text txtGiaGiaShop;
    public int giaGiamShop      = 5;
    public int capToiDaGiamShop = 3;

    [Header("=== GIẢM TẦM PHÁT HIỆN ===")]
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
            if (Input.GetKeyDown(KeyCode.Alpha1)) NangCapTocDo_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha2)) NangCapLaBan_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha3)) NangCapShop_Btn();
            if (Input.GetKeyDown(KeyCode.Alpha4)) NangCapTamPH_Btn();
            
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape)) OnClick_ThuNho();
        }
    }

    // -----------------------------------------------
    // MỞ màn hình nâng cấp
    // -----------------------------------------------
    public void MoUpgrade()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(true);
        CapNhatUI();
    }

    void CapNhatUI()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (txtManhHon != null)
            txtManhHon.text = $"💎 {data.soManhHon} Mảnh Hồn";

        // Tốc độ
        if (txtCapTocDo != null)
            txtCapTocDo.text = CapString(data.capTocDo, capToiDaTocDo, "+0.5 tốc độ");
        if (txtGiaTocDo != null)
            txtGiaTocDo.text = data.capTocDo >= capToiDaTocDo ? "ĐÃ TỐI ĐA" : $"{giaTocDo} 💎";

        // La bàn
        if (txtCapLaBan != null)
            txtCapLaBan.text = CapString(data.capLaBan, capToiDaLaBan, "+1s la bàn");
        if (txtGiaLaBan != null)
            txtGiaLaBan.text = data.capLaBan >= capToiDaLaBan ? "ĐÃ TỐI ĐA" : $"{giaLaBan} 💎";

        // Giảm giá shop
        if (txtCapGiaShop != null)
            txtCapGiaShop.text = CapString(data.capGiamGiaShop, capToiDaGiamShop, "-1 giá shop");
        if (txtGiaGiaShop != null)
            txtGiaGiaShop.text = data.capGiamGiaShop >= capToiDaGiamShop ? "ĐÃ TỐI ĐA" : $"{giaGiamShop} 💎";

        // Giảm tầm phát hiện
        if (txtCapTamPH != null)
            txtCapTamPH.text = CapString(data.capTamPhatHien, capToiDaTamPH, "-1 tầm quái");
        if (txtGiaTamPH != null)
            txtGiaTamPH.text = data.capTamPhatHien >= capToiDaTamPH ? "ĐÃ TỐI ĐA" : $"{giaTamPhatHien} 💎";
    }

    string CapString(int hienTai, int toiDa, string moTa) =>
        $"{moTa}\n[{new string('●', hienTai)}{new string('○', toiDa - hienTai)}] {hienTai}/{toiDa}";

    // -----------------------------------------------
    // NÚT NÂNG CẤP
    // -----------------------------------------------
    public void NangCapTocDo()     => NangCap(ref GetData().capTocDo,       capToiDaTocDo, giaTocDo,      "⚡ Tốc độ");
    public void NangCapLaBan()     => NangCap(ref GetData().capLaBan,        capToiDaLaBan, giaLaBan,      "🧭 La Bàn");
    public void NangCapGiamShop()  => NangCap(ref GetData().capGiamGiaShop,  capToiDaGiamShop, giaGiamShop,"🏪 Giá Shop");
    public void NangCapTamPH()     => NangCap(ref GetData().capTamPhatHien,  capToiDaTamPH, giaTamPhatHien,"👁️ Tầm Quái");

    PlayerData GetData() => SaveSystem.LoadGame();

    void NangCap(ref int cap, int toiDa, int gia, string ten)
    {
        PlayerData data = SaveSystem.LoadGame();
        // Phải map lại vì ref không hoạt động với property trả về object mới
        // → Xử lý trực tiếp dưới dạng tên nâng cấp
        Debug.Log("Gọi qua hàm chuyên biệt bên dưới");
    }

    // ---- Hàm nâng cấp riêng từng loại ----
    void ThucHienNangCap(string loai)
    {
        PlayerData data = SaveSystem.LoadGame();
        int cap = 0, toiDa = 0, gia = 0;
        string ten = "";

        switch (loai)
        {
            case "tocdo":   cap=data.capTocDo;       toiDa=capToiDaTocDo;   gia=giaTocDo;      ten="⚡ Tốc Độ"; break;
            case "laban":   cap=data.capLaBan;        toiDa=capToiDaLaBan;   gia=giaLaBan;      ten="🧭 La Bàn"; break;
            case "shop":    cap=data.capGiamGiaShop;  toiDa=capToiDaGiamShop;gia=giaGiamShop;   ten="🏪 Giảm Giá"; break;
            case "tamph":   cap=data.capTamPhatHien;  toiDa=capToiDaTamPH;   gia=giaTamPhatHien; ten="👁️ Tầm Quái"; break;
        }

        if (cap >= toiDa) { Debug.Log($"❌ {ten} đã tối đa!"); return; }
        if (data.soManhHon < gia) { Debug.Log($"❌ Không đủ Mảnh Hồn! Cần {gia}"); return; }

        data.soManhHon -= gia;
        switch (loai)
        {
            case "tocdo":  data.capTocDo++;       break;
            case "laban":  data.capLaBan++;        break;
            case "shop":   data.capGiamGiaShop++;  break;
            case "tamph":  data.capTamPhatHien++;  break;
        }

        SaveSystem.SaveGame(data);
        Debug.Log($"✅ Nâng cấp {ten} → Cấp {cap+1}! Còn {data.soManhHon} Mảnh Hồn");
        CapNhatUI();
    }

    // Gọi từ Button trong Inspector
    public void NangCapTocDo_Btn()    => ThucHienNangCap("tocdo");
    public void NangCapLaBan_Btn()    => ThucHienNangCap("laban");
    public void NangCapShop_Btn()     => ThucHienNangCap("shop");
    public void NangCapTamPH_Btn()    => ThucHienNangCap("tamph");

    // -----------------------------------------------
    // NÚT: THU NHỎ LẠI HUB HOẶC TIẾP TỤC
    // -----------------------------------------------
    public void OnClick_ThuNho()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);

        // NẾU VICTORY SCREEN HIỆN TẠI ĐANG MỞ NGẦM -> PHỤC HỒI NÓ
        if (VictoryScreen.Instance != null && VictoryScreen.Instance.panelVictory.activeSelf == false)
        {
            VictoryScreen.Instance.KhoiPhucHienThiHUB();
        }
    }

    public void OnClick_TiepTuc()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneGame);
    }
}

// GameHUD.cs — HUD: Mảnh Hồn, vật phẩm, tầng hiện tại (cập nhật mỗi 0.5s)
using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("=== MẢNH HỒN ===")]
    public TMP_Text txtManhHon;

    [Header("=== VẬT PHẨM ===")]
    public TMP_Text txtDaPhatSang;
    public TMP_Text txtDongHo;
    public TMP_Text txtLaBan;

    [Header("=== THÔNG TIN TẦNG ===")]
    public TMP_Text txtTang;

    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianCapNhat = 0.5f;

    private float _dem = 0f;
    private static bool _canCapNhat = true;

    // Khoi tao HUD va cap nhat lan dau
    void Start() { _canCapNhat = true; CapNhatHUD(); }

    // Cap nhat HUD theo chu ky (unscaled time)
    void Update()
    {
        _dem += Time.unscaledDeltaTime;
        if (_dem >= thoiGianCapNhat) { _dem = 0f; CapNhatHUD(); }
    }

    // Doc save/inventory va ghi ra UI
    void CapNhatHUD()
    {
        PlayerData data = _canCapNhat ? SaveSystem.LoadGame() : null;
        _canCapNhat = false;

        if (txtManhHon != null && data != null)
            txtManhHon.text = $"Mảnh Hồn: {data.soManhHon}";

        int da     = PlayerInventory.Instance != null ? PlayerInventory.Instance.daPhatSang : 0;
        int dongHo = PlayerInventory.Instance != null ? PlayerInventory.Instance.dongHo     : 0;
        int laBan  = PlayerInventory.Instance != null ? PlayerInventory.Instance.laBan      : 0;

        if (txtDaPhatSang != null) txtDaPhatSang.text = $"Đá x{da}";
        if (txtDongHo     != null) txtDongHo.text     = $"ĐH x{dongHo}";
        if (txtLaBan      != null) txtLaBan.text      = $"LB x{laBan}";

        if (txtTang != null && data != null)
        {
            int tong = (data.biomeSequence != null && data.biomeSequence.Length > 0) ? data.biomeSequence.Length : 4;
            txtTang.text = $"Tầng {data.mapHienTai} / {tong}";
        }
    }

    // Cho phep cap nhat ngay khi co thay doi nhanh
    public static void LamMoi()
    {
        _canCapNhat = true;
        GameHUD hud = FindFirstObjectByType<GameHUD>();
        if (hud != null) hud.CapNhatHUD();
    }
}

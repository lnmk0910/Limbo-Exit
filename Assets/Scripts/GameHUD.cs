// GameHUD.cs
// Hiển thị thông tin HUD trong game: Mảnh Hồn, vật phẩm, tầng hiện tại
// Tự động cập nhật mỗi 0.5 giây thay vì mỗi frame để tối ưu hiệu suất
// GẮN vào: Canvas_GameUI (hoặc bất kỳ GameObject nào luôn Active)

using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("=== MẢNH HỒN ===")]
    public TMP_Text txtManhHon;     // Hiện: 💎 12

    [Header("=== VẬT PHẨM ===")]
    public TMP_Text txtDaPhatSang;  // Hiện: Da x3
    public TMP_Text txtDongHo;      // Hiện: DH x1
    public TMP_Text txtLaBan;       // Hiện: LB x2

    [Header("=== THÔNG TIN TẦNG ===")]
    public TMP_Text txtTang;        // Hiện: Tầng 2 / 4

    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianCapNhat = 0.5f;  // Cập nhật mỗi 0.5 giây

    private float _dem = 0f;
    private static bool _canCapNhat = true; // Flag de chi doc file khi can

    void Start()
    {
        _canCapNhat = true;
        CapNhatHUD();
    }

    void Update()
    {
        _dem += Time.unscaledDeltaTime;
        if (_dem >= thoiGianCapNhat)
        {
            _dem = 0f;
            CapNhatHUD();
        }
    }

    void CapNhatHUD()
    {
        // Chi doc file JSON khi co thay doi (goi LamMoi)
        // Ngoai ra dung PlayerInventory runtime (khong can doc file)
        PlayerData data = _canCapNhat ? SaveSystem.LoadGame() : null;
        _canCapNhat = false;

        // Manh Hon — doc tu file khi can
        if (txtManhHon != null && data != null)
            txtManhHon.text = $"Mảnh Hồn: {data.soManhHon}";

        // Vat pham — doc tu PlayerInventory runtime (khong can file IO)
        int da     = PlayerInventory.Instance != null ? PlayerInventory.Instance.daPhatSang : 0;
        int dongHo = PlayerInventory.Instance != null ? PlayerInventory.Instance.dongHo      : 0;
        int laBan  = PlayerInventory.Instance != null ? PlayerInventory.Instance.laBan       : 0;

        if (txtDaPhatSang != null) txtDaPhatSang.text = $"Đá x{da}";
        if (txtDongHo     != null) txtDongHo.text     = $"ĐH x{dongHo}";
        if (txtLaBan      != null) txtLaBan.text      = $"LB x{laBan}";

        // Tang hien tai
        if (txtTang != null && data != null)
        {
            int tong = (data.biomeSequence != null && data.biomeSequence.Length > 0)
                       ? data.biomeSequence.Length : 4;
            txtTang.text = $"Tầng {data.mapHienTai} / {tong}";
        }
    }

    // Goi tu ben ngoai khi can cap nhat ngay (sau khi mua do, nhan Manh Hon, v.v.)
    public static void LamMoi()
    {
        _canCapNhat = true; // Lan cap nhat ke tiep se doc lai file
        GameHUD hud = FindFirstObjectByType<GameHUD>();
        if (hud != null) hud.CapNhatHUD();
    }
}

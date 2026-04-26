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
    public TMP_Text txtDaPhatSang;  // Hiện: 🪨 x3
    public TMP_Text txtDongHo;      // Hiện: ⏱ x1
    public TMP_Text txtLaBan;       // Hiện: 🧭 x2

    [Header("=== THÔNG TIN TẦNG ===")]
    public TMP_Text txtTang;        // Hiện: Tầng 2 / 4

    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianCapNhat = 0.5f;  // Cập nhật mỗi 0.5 giây

    private float _dem = 0f;

    void Start()
    {
        CapNhatHUD(); // Cập nhật ngay lúc vào game
    }

    void Update()
    {
        _dem += Time.unscaledDeltaTime; // Dùng unscaled để HUD vẫn cập nhật khi game pause
        if (_dem >= thoiGianCapNhat)
        {
            _dem = 0f;
            CapNhatHUD();
        }
    }

    void CapNhatHUD()
    {
        PlayerData data = SaveSystem.LoadGame();

        // Mảnh Hồn
        if (txtManhHon != null)
            txtManhHon.text = $"💎 {data.soManhHon}";

        // Vật phẩm — đọc từ PlayerInventory nếu có, fallback về Save
        int da     = PlayerInventory.Instance != null ? PlayerInventory.Instance.daPhatSang : data.soDaPhatSang;
        int dongHo = PlayerInventory.Instance != null ? PlayerInventory.Instance.dongHo      : data.soDongHo;
        int laBan  = PlayerInventory.Instance != null ? PlayerInventory.Instance.laBan       : data.soLaBan;

        if (txtDaPhatSang != null) txtDaPhatSang.text = $"🪨 x{da}";
        if (txtDongHo     != null) txtDongHo.text     = $"⏱ x{dongHo}";
        if (txtLaBan      != null) txtLaBan.text      = $"🧭 x{laBan}";

        // Tầng hiện tại / tổng
        if (txtTang != null)
        {
            int tong = (data.biomeSequence != null && data.biomeSequence.Length > 0)
                       ? data.biomeSequence.Length : 4;
            txtTang.text = $"Tầng {data.mapHienTai} / {tong}";
        }
    }

    // Gọi từ bên ngoài khi cần cập nhật ngay (VD: sau khi mua đồ)
    public static void LamMoi()
    {
        GameHUD hud = FindFirstObjectByType<GameHUD>();
        if (hud != null) hud.CapNhatHUD();
    }
}

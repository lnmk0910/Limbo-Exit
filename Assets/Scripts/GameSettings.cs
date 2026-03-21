// GameSettings.cs
// Static Class: lưu thông số cài đặt Map từ UI Menu
// Dữ liệu ở đây tồn tại trong suốt session (khi chuyển Scene không mất)
// Không cần gắn vào GameObject, gọi trực tiếp: GameSettings.rong

public static class GameSettings
{
    // ---- Kích thước Plane (mặc định 10x10) ----
    public static int rong = 10;       // Chiều rộng map theo trục X
    public static int dai = 10;        // Chiều dài map theo trục Z

    // ---- Kích thước Tường ----
    public static float chieuCaoTuong = 3f;    // Chiều cao tường (đơn vị Unity)
    public static float doDayTuong = 0.5f;     // Độ dày tường

    // ---- Reset về mặc định (gọi khi cần) ----
    public static void ResetMacDinh()
    {
        rong = 10;
        dai = 10;
        chieuCaoTuong = 3f;
        doDayTuong = 0.5f;
    }
}

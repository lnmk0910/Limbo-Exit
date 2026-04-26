// GameSettings.cs
// Static Class: lưu thông số Map + Biome để truyền giữa các Scene
// Không gắn vào GameObject, gọi trực tiếp: GameSettings.rong

public static class GameSettings
{
    // ---- Kích thước Plane ----
    public static int rong = 10;
    public static int dai  = 10;

    // ---- Kích thước Tường ----
    public static float chieuCaoTuong = 3f;
    public static float doDayTuong    = 0.5f;

    // ---- Biome Index ----
    // 0 = Mê Cung Đá Cổ
    // 1 = Thư Viện Vô Tận
    // 2 = Đầm Lầy Sương Mù
    // 3 = Mê Cung Tinh Thể
    public static int biomeIndex = 0;

    // ---- Reset về mặc định ----
    public static void ResetMacDinh()
    {
        rong           = 10;
        dai            = 10;
        chieuCaoTuong  = 3f;
        doDayTuong     = 0.5f;
        biomeIndex     = 0;
    }
}

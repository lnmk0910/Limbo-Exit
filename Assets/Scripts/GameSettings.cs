// GameSettings.cs
// Static Class: lưu thông số Map để truyền giữa các Scene
// Biome được random tự động trong MazeGenerator — không lưu ở đây nữa
// Không gắn vào GameObject, gọi trực tiếp: GameSettings.rong

public static class GameSettings
{
    // ---- Kích thước lưới mê cung ----
    public static int rong = 50;
    public static int dai  = 50;

    // ---- Kích thước Tường ----
    public static float chieuCaoTuong = 5f;
    public static float doDayTuong    = 1f;

    // ---- Reset về mặc định ----
    public static void ResetMacDinh()
    {
        rong          = 50;
        dai           = 50;
        chieuCaoTuong = 5f;
        doDayTuong    = 1f;
    }
}

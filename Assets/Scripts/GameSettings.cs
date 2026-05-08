// GameSettings.cs
// Static Class: luu thong so Map de truyen giua cac Scene
// Khong gan vao GameObject, goi truc tiep: GameSettings.rong

public static class GameSettings
{
    // ---- Kich thuoc luoi me cung ----
    public static int rong = 50;
    public static int dai  = 50;

    // ---- Kich thuoc Tuong ----
    public static float chieuCaoTuong = 8f;     // Tang tu 5 len 8 (cao gap 1.6x)
    public static float doDayTuong    = 1.5f;    // Tang tu 1 len 1.5 (day hon 50%)

    // ---- Kich thuoc O (san) ----
    public static float kichThuocO = 6f;         // Tang tu 4 len 6 (rong hon 50%)

    // ---- Reset ve mac dinh ----
    public static void ResetMacDinh()
    {
        rong          = 50;
        dai           = 50;
        chieuCaoTuong = 8f;
        doDayTuong    = 1.5f;
        kichThuocO    = 6f;
    }
}

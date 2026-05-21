// UIManager.cs — Quản lý trạng thái UI toàn cục, chống xung đột phím
public static class UIManager
{
    public enum TrangThaiUI
    {
        TrongGame,
        Pause,
        Shop,
        NangCap,
        ChienThang,
        ChetChoc,
        HoiThoai,
    }

    public static TrangThaiUI PanelHienTai { get; private set; } = TrangThaiUI.TrongGame;
    private static TrangThaiUI trangThaiTruoc = TrangThaiUI.TrongGame;

    // Chuyen trang thai UI va luu trang thai truoc do
    public static void Mo(TrangThaiUI trangThai)
    {
        trangThaiTruoc = PanelHienTai;
        PanelHienTai   = trangThai;
    }

    // Dong panel va quay ve trang thai truoc
    public static void DongVePanel()
    {
        PanelHienTai   = trangThaiTruoc;
        trangThaiTruoc = TrangThaiUI.TrongGame;
    }

    // Reset toan bo UI ve che do trong game
    public static void DongVeGame()
    {
        trangThaiTruoc = TrangThaiUI.TrongGame;
        PanelHienTai   = TrangThaiUI.TrongGame;
    }

    public static bool DangO(TrangThaiUI trangThai) => PanelHienTai == trangThai;
    public static bool DangTrongGame() => PanelHienTai == TrangThaiUI.TrongGame;
}

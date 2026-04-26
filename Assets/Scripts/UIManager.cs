// UIManager.cs
// Quản lý trạng thái UI toàn cục — chống xung đột phím giữa các màn hình
// Không gắn vào GameObject, gọi trực tiếp UIManager.PanelHienTai

public static class UIManager
{
    public enum TrangThaiUI
    {
        TrongGame,   // Đang chơi bình thường
        Pause,       // Pause Menu đang mở
        Shop,        // Cửa hàng đang mở
        NangCap,     // Màn hình nâng cấp đang mở
        ChienThang,  // VictoryScreen đang mở
        ChetChoc,    // DeathScreen đang mở
        HoiThoai,    // Khung hội thoại NPC đang mở
    }

    public static TrangThaiUI PanelHienTai { get; private set; } = TrangThaiUI.TrongGame;

    // Trạng thái trước đó — để khi đóng panel biết quay về đâu
    private static TrangThaiUI trangThaiTruoc = TrangThaiUI.TrongGame;

    public static void Mo(TrangThaiUI trangThai)
    {
        trangThaiTruoc = PanelHienTai; // Lưu lại trước khi mở
        PanelHienTai   = trangThai;
    }

    // Đóng panel hiện tại → quay về trạng thái trước đó
    public static void DongVePanel()
    {
        PanelHienTai   = trangThaiTruoc;
        trangThaiTruoc = TrangThaiUI.TrongGame; // Reset để tránh chain lỗi
    }

    // Đóng hẳn về game (không qua trạng thái trung gian)
    public static void DongVeGame()
    {
        trangThaiTruoc = TrangThaiUI.TrongGame;
        PanelHienTai   = TrangThaiUI.TrongGame;
    }

    // Kiểm tra: chỉ xử lý input nếu đúng trạng thái
    public static bool DangO(TrangThaiUI trangThai) => PanelHienTai == trangThai;
    public static bool DangTrongGame() => PanelHienTai == TrangThaiUI.TrongGame;
}

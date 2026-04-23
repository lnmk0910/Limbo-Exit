// PlayerData.cs
[System.Serializable]
public class PlayerData
{
    // === TIỀN TỆ ===
    public int soManhHon;
    public int mapHienTai;
    public int seed;

    // === VẬT PHẨM ===
    public int soDaPhatSang;
    public int soDongHo;
    public int soLaBan;

    // === NÂNG CẤP VĨNH VIỄN ===
    public int capTocDo;        // +0.5 tốc độ mỗi cấp (tối đa 5)
    public int capLaBan;        // +1s thời gian la bàn mỗi cấp (tối đa 3)
    public int capGiamGiaShop;  // -1 Mảnh Hồn giá shop mỗi cấp (tối đa 3)
    public int capTamPhatHien;  // -1 tầm phát hiện quái mỗi cấp (tối đa 3)

    // === CỐT TRUYỆN ===
    public int[] biomeSequence; // Chuỗi ngẫu nhiên các Map

    public PlayerData()
    {
        soManhHon   = 0;
        mapHienTai  = 1;
        seed        = 0;
        soDaPhatSang = 0;
        soDongHo    = 0;
        soLaBan     = 0;
        capTocDo        = 0;
        capLaBan        = 0;
        capGiamGiaShop  = 0;
        capTamPhatHien  = 0;
        
        // Cốt truyện (Khởi tạo mảng rỗng, MazeGenerator tráo ngẫu nhiên sau)
        biomeSequence   = new int[] { 0, 1, 2, 3 }; // Mặc định 4 Biome (Đá Cổ, Thư Viện, Đầm Lầy, Tinh Thể)
    }
}

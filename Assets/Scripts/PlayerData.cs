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
    }
}

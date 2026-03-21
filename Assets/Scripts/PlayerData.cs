// PlayerData.cs
// Dữ liệu người chơi – serialize ra JSON

[System.Serializable]
public class PlayerData
{
    public int soManhHon;    // Tiền tệ
    public int mapHienTai;   // Màn hiện tại
    public int seed;         // Seed map hiện tại

    // Kho vật phẩm
    public int soDaPhatSang;    // Đá phát sáng
    public int soDongHo;        // Đồng hồ thời gian
    public int soLaBan;         // La bàn cổ

    public PlayerData()
    {
        soManhHon   = 0;
        mapHienTai  = 1;
        seed        = 0;
        soDaPhatSang = 0;
        soDongHo    = 0;
        soLaBan     = 0;
    }
}

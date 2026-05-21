// PlayerData.cs — Dữ liệu lưu trữ người chơi
[System.Serializable]
public class PlayerData
{
    public int soManhHon;
    public int mapHienTai;
    public int seed;

    public int soDaPhatSang;
    public int soDongHo;
    public int soLaBan;

    // Nâng cấp vĩnh viễn
    public int capTocDo;
    public int capLaBan;
    public int capGiamGiaShop;
    public int capTamPhatHien;

    public int[] biomeSequence;

    // DDA
    public int soLanChetTong;
    public int soLanChetTangNay;
    public float thoiGianBatDauTang;
    public float thoiGianVuotTangTruoc;
    public int soLanDungVatPham;
    public float diemDoKho;

    // Khoi tao gia tri mac dinh cho save moi
    public PlayerData()
    {
        soManhHon  = 0;
        mapHienTai = 1;
        seed = 0;
        soDaPhatSang = 0;
        soDongHo = 0;
        soLaBan  = 0;
        capTocDo       = 0;
        capLaBan       = 0;
        capGiamGiaShop = 0;
        capTamPhatHien = 0;
        biomeSequence = new int[] { 0, 1, 2, 3 };
        soLanChetTong      = 0;
        soLanChetTangNay   = 0;
        thoiGianBatDauTang = 0f;
        thoiGianVuotTangTruoc = 0f;
        soLanDungVatPham    = 0;
        diemDoKho = 0.5f;
    }
}

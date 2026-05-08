// PlayerData.cs
[System.Serializable]
public class PlayerData
{
    // === TIEN TE ===
    public int soManhHon;
    public int mapHienTai;
    public int seed;

    // === VAT PHAM ===
    public int soDaPhatSang;
    public int soDongHo;
    public int soLaBan;

    // === NANG CAP VINH VIEN ===
    public int capTocDo;        // +0.5 toc do moi cap (toi da 5)
    public int capLaBan;        // +1s thoi gian la ban moi cap (toi da 3)
    public int capGiamGiaShop;  // -1 Manh Hon gia shop moi cap (toi da 3)
    public int capTamPhatHien;  // -1 tam phat hien quai moi cap (toi da 3)

    // === COT TRUYEN ===
    public int[] biomeSequence;

    // === DDA — DYNAMIC DIFFICULTY ADJUSTMENT ===
    public int soLanChetTong;       // Tong so lan chet tu dau den cuoi
    public int soLanChetTangNay;    // So lan chet o tang hien tai (reset khi sang tang)
    public float thoiGianBatDauTang; // Time.realtimeSinceStartup luc bat dau tang
    public float thoiGianVuotTangTruoc; // Thoi gian (giay) da vuot tang truoc do
    public int soLanDungVatPham;    // So lan su dung vat pham (do kho hay de?)
    public float diemDoKho;         // DDA score hien tai (0.0 = de nhat, 1.0 = kho nhat)

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

        biomeSequence   = new int[] { 0, 1, 2, 3 };

        // DDA defaults
        soLanChetTong       = 0;
        soLanChetTangNay    = 0;
        thoiGianBatDauTang  = 0f;
        thoiGianVuotTangTruoc = 0f;
        soLanDungVatPham    = 0;
        diemDoKho           = 0.5f; // Trung binh
    }
}

// DDAManager.cs
// Dynamic Difficulty Adjustment — He thong AI dieu chinh do kho
// Doc du lieu hanh vi nguoi choi tu SaveSystem va tinh diem do kho
// TAT CA script khac doc tu day de dieu chinh gameplay
// KHONG can gan vao GameObject — goi truc tiep DDAManager.LayXxx()

using UnityEngine;

public static class DDAManager
{
    // ===================================================
    // DIEM DO KHO: 0.0 (de nhat) → 0.5 (trung binh) → 1.0 (kho nhat)
    // ===================================================
    // Input signals:
    //   - soLanChetTangNay: chet nhieu = dang kho → giam do kho
    //   - thoiGianVuotTangTruoc: vuot nhanh = de → tang do kho
    //   - soManhHon: nhieu tien = choi gioi → tang do kho
    //   - soLanDungVatPham: dung nhieu item = dang kho → giam do kho
    //   - mapHienTai: tang cao hon = nen kho hon (baseline)

    // Cache ket qua tinh toan
    private static float _diemDoKho = 0.5f;
    private static bool  _daTinh    = false;

    // -----------------------------------------------
    // TINH DIEM DO KHO — goi 1 lan moi dau tang
    // -----------------------------------------------
    public static float TinhDiemDoKho()
    {
        PlayerData data = SaveSystem.LoadGame();

        // === Signal 1: So lan chet tang nay (chet nhieu = kho qua) ===
        // 0 chet → +0.2 (tang do kho)
        // 1-2 chet → 0 (binh thuong)
        // 3-5 chet → -0.1
        // 6+ chet → -0.25 (giam nhieu)
        float signalChet = 0f;
        if (data.soLanChetTangNay == 0)      signalChet = 0.2f;
        else if (data.soLanChetTangNay <= 2) signalChet = 0f;
        else if (data.soLanChetTangNay <= 5) signalChet = -0.1f;
        else                                 signalChet = -0.25f;

        // === Signal 2: Thoi gian vuot tang truoc ===
        // < 60s → +0.15 (qua nhanh, tang do kho)
        // 60-180s → 0 (binh thuong)
        // > 180s → -0.1 (mat lau, giam do kho)
        // > 300s → -0.2 (rat lau)
        float signalThoiGian = 0f;
        float tg = data.thoiGianVuotTangTruoc;
        if (tg > 0f) // Chi tinh khi co du lieu
        {
            if (tg < 60f)       signalThoiGian = 0.15f;
            else if (tg < 180f) signalThoiGian = 0f;
            else if (tg < 300f) signalThoiGian = -0.1f;
            else                signalThoiGian = -0.2f;
        }

        // === Signal 3: So Manh Hon hien co ===
        // > 30 → +0.1 (nhieu tien = choi gioi)
        // 10-30 → 0
        // < 10 → -0.1 (it tien = dang kho)
        float signalTien = 0f;
        if (data.soManhHon > 30)      signalTien = 0.1f;
        else if (data.soManhHon >= 10) signalTien = 0f;
        else                           signalTien = -0.1f;

        // === Signal 4: Su dung vat pham ===
        // 0 item → +0.05 (khong can dung = de)
        // 1-3 → 0
        // 4+ → -0.1 (dung nhieu = dang kho)
        float signalItem = 0f;
        if (data.soLanDungVatPham == 0)       signalItem = 0.05f;
        else if (data.soLanDungVatPham <= 3)  signalItem = 0f;
        else                                  signalItem = -0.1f;

        // === Signal 5: Tang hien tai (baseline) ===
        // Tang 1 → 0.3 (de)
        // Tang 2 → 0.45
        // Tang 3 → 0.6
        // Tang 4 → 0.75
        float baseline = 0.15f + data.mapHienTai * 0.15f;

        // === Tong hop ===
        float diem = baseline + signalChet + signalThoiGian + signalTien + signalItem;
        diem = Mathf.Clamp(diem, 0f, 1f);

        // Smooth: khong thay doi qua nhieu so voi lan truoc
        float diemCu = data.diemDoKho;
        if (diemCu > 0f)
            diem = Mathf.Lerp(diemCu, diem, 0.6f); // 60% diem moi, 40% diem cu

        // Luu lai
        data.diemDoKho = diem;
        SaveSystem.SaveGame(data);

        _diemDoKho = diem;
        _daTinh = true;

        Debug.Log($"[DDA] === DIEM DO KHO: {diem:F2} ===" +
                  $"\n  Chet tang nay: {data.soLanChetTangNay} ({signalChet:+0.00;-0.00})" +
                  $"\n  Thoi gian tang truoc: {tg:F0}s ({signalThoiGian:+0.00;-0.00})" +
                  $"\n  Manh Hon: {data.soManhHon} ({signalTien:+0.00;-0.00})" +
                  $"\n  Dung item: {data.soLanDungVatPham} ({signalItem:+0.00;-0.00})" +
                  $"\n  Baseline tang {data.mapHienTai}: {baseline:F2}");

        return diem;
    }

    // -----------------------------------------------
    // CAC HAM LAY GIA TRI DIEU CHINH
    // -----------------------------------------------

    static float LayDiem()
    {
        if (!_daTinh) TinhDiemDoKho();
        return _diemDoKho;
    }

    // Toc do quai: 0.7x (de) → 1.0x (TB) → 1.3x (kho)
    public static float LayHeSoTocDoQuai()
    {
        return Mathf.Lerp(0.7f, 1.3f, LayDiem());
    }

    // So luong quai: -1 (de) → 0 (TB) → +2 (kho)
    public static int LaySoQuaiBonus()
    {
        float d = LayDiem();
        if (d < 0.3f) return -1;
        if (d < 0.6f) return 0;
        if (d < 0.8f) return 1;
        return 2;
    }

    // Kich thuoc map: -2 (de) → 0 (TB) → +2 (kho)
    public static int LayMapSizeBonus()
    {
        float d = LayDiem();
        if (d < 0.25f) return -2;
        if (d < 0.4f)  return -1;
        if (d < 0.6f)  return 0;
        if (d < 0.8f)  return 1;
        return 2;
    }

    // Ti le checkpoint: 8% (de) → 5% (TB) → 3% (kho)
    public static float LayTiLeCheckpoint()
    {
        return Mathf.Lerp(0.08f, 0.03f, LayDiem());
    }

    // Ti le NPC: 5% (de) → 3% (TB) → 2% (kho)
    public static float LayTiLeNPC()
    {
        return Mathf.Lerp(0.05f, 0.02f, LayDiem());
    }

    // Tam phat hien quai: 0.8x (de) → 1.0x (TB) → 1.2x (kho)
    public static float LayHeSoTamPhatHien()
    {
        return Mathf.Lerp(0.8f, 1.2f, LayDiem());
    }

    // -----------------------------------------------
    // RESET — goi khi bat dau tang moi
    // -----------------------------------------------
    public static void ResetChoTangMoi()
    {
        _daTinh = false;
    }

    // -----------------------------------------------
    // GHI NHAN SU KIEN
    // -----------------------------------------------
    public static void GhiNhanChet()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soLanChetTong++;
        data.soLanChetTangNay++;
        SaveSystem.SaveGame(data);
        _daTinh = false; // Can tinh lai
        Debug.Log($"[DDA] Ghi nhan chet: tong={data.soLanChetTong}, tang nay={data.soLanChetTangNay}");
    }

    public static void GhiNhanDungItem()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soLanDungVatPham++;
        SaveSystem.SaveGame(data);
        _daTinh = false;
    }

    public static void GhiNhanVuotTang(float thoiGian)
    {
        PlayerData data = SaveSystem.LoadGame();
        data.thoiGianVuotTangTruoc = thoiGian;
        data.soLanChetTangNay = 0;    // Reset chet cho tang moi
        data.soLanDungVatPham = 0;    // Reset item cho tang moi
        SaveSystem.SaveGame(data);
        _daTinh = false;
        Debug.Log($"[DDA] Vuot tang! Thoi gian: {thoiGian:F1}s → Reset counter tang moi");
    }

    public static void GhiNhanBatDauTang()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.thoiGianBatDauTang = Time.realtimeSinceStartup;
        SaveSystem.SaveGame(data);
        Debug.Log("[DDA] Bat dau tinh thoi gian tang moi");
    }
}

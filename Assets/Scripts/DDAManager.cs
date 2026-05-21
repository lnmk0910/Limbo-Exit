// DDAManager.cs — Dynamic Difficulty Adjustment: tính điểm độ khó từ hành vi người chơi
using UnityEngine;

public static class DDAManager
{
    private static float _diemDoKho = 0.5f;
    private static bool  _daTinh    = false;

    // Tính điểm độ khó: 0.0 (dễ) → 0.5 (trung bình) → 1.0 (khó)
    // Tong hop tin hieu nguoi choi de tinh diem do kho
    public static float TinhDiemDoKho()
    {
        PlayerData data = SaveSystem.LoadGame();

        // Signal 1: Số lần chết tầng này
        float signalChet = 0f;
        if (data.soLanChetTangNay == 0)      signalChet = 0.2f;
        else if (data.soLanChetTangNay <= 2) signalChet = 0f;
        else if (data.soLanChetTangNay <= 5) signalChet = -0.1f;
        else                                 signalChet = -0.25f;

        // Signal 2: Thời gian vượt tầng trước
        float signalThoiGian = 0f;
        float tg = data.thoiGianVuotTangTruoc;
        if (tg > 0f)
        {
            if (tg < 60f)       signalThoiGian = 0.15f;
            else if (tg < 180f) signalThoiGian = 0f;
            else if (tg < 300f) signalThoiGian = -0.1f;
            else                signalThoiGian = -0.2f;
        }

        // Signal 3: Số Mảnh Hồn hiện có
        float signalTien = 0f;
        if (data.soManhHon > 30)      signalTien = 0.1f;
        else if (data.soManhHon >= 10) signalTien = 0f;
        else                           signalTien = -0.1f;

        // Signal 4: Sử dụng vật phẩm
        float signalItem = 0f;
        if (data.soLanDungVatPham == 0)      signalItem = 0.05f;
        else if (data.soLanDungVatPham <= 3) signalItem = 0f;
        else                                 signalItem = -0.1f;

        // Signal 5: Tầng hiện tại (baseline)
        float baseline = 0.15f + data.mapHienTai * 0.15f;

        // Tổng hợp + smooth
        float diem = baseline + signalChet + signalThoiGian + signalTien + signalItem;
        diem = Mathf.Clamp(diem, 0f, 1f);

        float diemCu = data.diemDoKho;
        if (diemCu > 0f) diem = Mathf.Lerp(diemCu, diem, 0.6f);

        data.diemDoKho = diem;
        SaveSystem.SaveGame(data);

        _diemDoKho = diem;
        _daTinh = true;
        return diem;
    }

    // Lay diem do kho hien tai (tinh neu chua co)
    static float LayDiem()
    {
        if (!_daTinh) TinhDiemDoKho();
        return _diemDoKho;
    }

    // Tốc độ quái: 0.7x (dễ) → 1.3x (khó)
    public static float LayHeSoTocDoQuai() => Mathf.Lerp(0.7f, 1.3f, LayDiem());

    // Số lượng quái bonus: -1 → +2
    // Bonus so luong quai theo diem DDA
    public static int LaySoQuaiBonus()
    {
        float d = LayDiem();
        if (d < 0.3f) return -1;
        if (d < 0.6f) return 0;
        if (d < 0.8f) return 1;
        return 2;
    }

    // Kích thước map bonus: -2 → +2
    // Bonus kich thuoc map theo diem DDA
    public static int LayMapSizeBonus()
    {
        float d = LayDiem();
        if (d < 0.25f) return -2;
        if (d < 0.4f)  return -1;
        if (d < 0.6f)  return 0;
        if (d < 0.8f)  return 1;
        return 2;
    }

    public static float LayTiLeCheckpoint() => Mathf.Lerp(0.08f, 0.03f, LayDiem());
    public static float LayTiLeNPC()        => Mathf.Lerp(0.05f, 0.02f, LayDiem());
    public static float LayHeSoTamPhatHien() => Mathf.Lerp(0.8f, 1.2f, LayDiem());

    // Reset trang thai tinh diem cho tang moi
    public static void ResetChoTangMoi() { _daTinh = false; }

    // Ghi nhan 1 lan chet (cap nhat save)
    public static void GhiNhanChet()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soLanChetTong++;
        data.soLanChetTangNay++;
        SaveSystem.SaveGame(data);
        _daTinh = false;
    }

    // Ghi nhan da dung vat pham (cap nhat save)
    public static void GhiNhanDungItem()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soLanDungVatPham++;
        SaveSystem.SaveGame(data);
        _daTinh = false;
    }

    // Ghi nhan vuot tang va reset thong ke tang
    public static void GhiNhanVuotTang(float thoiGian)
    {
        PlayerData data = SaveSystem.LoadGame();
        data.thoiGianVuotTangTruoc = thoiGian;
        data.soLanChetTangNay = 0;
        data.soLanDungVatPham = 0;
        SaveSystem.SaveGame(data);
        _daTinh = false;
    }

    // Ghi thoi diem bat dau tang (de tinh thoi gian)
    public static void GhiNhanBatDauTang()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.thoiGianBatDauTang = Time.realtimeSinceStartup;
        SaveSystem.SaveGame(data);
    }
}

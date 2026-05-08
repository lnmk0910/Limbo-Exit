// AccountManager.cs
// He thong tai khoan local — dang ky/dang nhap bang ten + mat khau
// Mat khau duoc hash SHA256 truoc khi luu (khong luu mat khau goc)
// KHONG can server — tat ca luu tren may nguoi choi
// Goi truc tiep: AccountManager.DangNhap(), AccountManager.DangKy()

using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AccountManager
{
    // Tai khoan dang dang nhap
    public static string TenDangNhap { get; private set; } = "";
    public static bool DaDangNhap => !string.IsNullOrEmpty(TenDangNhap);

    // File luu danh sach tai khoan
    private static string FilePath => Application.persistentDataPath + "/accounts.json";

    // -----------------------------------------------
    // DANG KY TAI KHOAN MOI
    // Tra ve: "" neu thanh cong, hoac thong bao loi
    // -----------------------------------------------
    public static string DangKy(string ten, string matKhau)
    {
        // Kiem tra dau vao
        if (string.IsNullOrWhiteSpace(ten))
            return "Ten dang nhap khong duoc de trong!";
        if (ten.Length < 3)
            return "Ten dang nhap phai co it nhat 3 ky tu!";
        if (ten.Length > 20)
            return "Ten dang nhap toi da 20 ky tu!";
        if (string.IsNullOrWhiteSpace(matKhau))
            return "Mat khau khong duoc de trong!";
        if (matKhau.Length < 4)
            return "Mat khau phai co it nhat 4 ky tu!";

        // Doc database hien tai
        AccountDatabase db = DocDatabase();

        // Kiem tra trung ten
        foreach (var acc in db.danhSach)
        {
            if (acc.tenDangNhap.ToLower() == ten.ToLower())
                return "Ten dang nhap da ton tai!";
        }

        // Tao tai khoan moi
        AccountData accMoi = new AccountData
        {
            tenDangNhap    = ten,
            matKhauHash    = HashMatKhau(matKhau),
            ngayTao        = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            lanDangNhapCuoi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        db.danhSach.Add(accMoi);
        LuuDatabase(db);

        // Tu dong dang nhap
        TenDangNhap = ten;
        Debug.Log($"[ACCOUNT] Dang ky thanh cong: {ten}");
        return "";
    }

    // -----------------------------------------------
    // DANG NHAP
    // Tra ve: "" neu thanh cong, hoac thong bao loi
    // -----------------------------------------------
    public static string DangNhap(string ten, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(ten))
            return "Nhap ten dang nhap!";
        if (string.IsNullOrWhiteSpace(matKhau))
            return "Nhap mat khau!";

        AccountDatabase db = DocDatabase();
        string hash = HashMatKhau(matKhau);

        foreach (var acc in db.danhSach)
        {
            if (acc.tenDangNhap.ToLower() == ten.ToLower())
            {
                if (acc.matKhauHash == hash)
                {
                    // Dang nhap thanh cong
                    TenDangNhap = acc.tenDangNhap;
                    acc.lanDangNhapCuoi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LuuDatabase(db);
                    Debug.Log($"[ACCOUNT] Dang nhap thanh cong: {TenDangNhap}");
                    return "";
                }
                else
                {
                    return "Sai mat khau!";
                }
            }
        }

        return "Tai khoan khong ton tai!";
    }

    // -----------------------------------------------
    // DANG XUAT
    // -----------------------------------------------
    public static void DangXuat()
    {
        Debug.Log($"[ACCOUNT] Dang xuat: {TenDangNhap}");
        TenDangNhap = "";
    }

    // -----------------------------------------------
    // LAY DUONG DAN SAVE THEO TAI KHOAN
    // VD: playerdata_minhtuan_slot1.json
    // -----------------------------------------------
    public static string LayDuongDanSave(int slot)
    {
        string tenFile = DaDangNhap
            ? $"playerdata_{TenDangNhap.ToLower()}_slot{slot}.json"
            : $"playerdata_slot{slot}.json";
        return Application.persistentDataPath + "/" + tenFile;
    }

    // -----------------------------------------------
    // KIEM TRA CO TAI KHOAN NAO CHUA
    // -----------------------------------------------
    public static bool CoTaiKhoan()
    {
        AccountDatabase db = DocDatabase();
        return db.danhSach.Count > 0;
    }

    // Lay danh sach ten tai khoan (cho dropdown/chon nhanh)
    public static List<string> LayDanhSachTen()
    {
        AccountDatabase db = DocDatabase();
        List<string> ds = new List<string>();
        foreach (var acc in db.danhSach)
            ds.Add(acc.tenDangNhap);
        return ds;
    }

    // -----------------------------------------------
    // HASH MAT KHAU BANG SHA256
    // -----------------------------------------------
    private static string HashMatKhau(string matKhau)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(matKhau + "_LimboExit_Salt"));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    // -----------------------------------------------
    // DOC / LUU DATABASE
    // -----------------------------------------------
    private static AccountDatabase DocDatabase()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            AccountDatabase db = JsonUtility.FromJson<AccountDatabase>(json);
            if (db != null) return db;
        }
        return new AccountDatabase();
    }

    private static void LuuDatabase(AccountDatabase db)
    {
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(FilePath, json);
    }
}

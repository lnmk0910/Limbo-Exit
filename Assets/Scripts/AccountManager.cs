// AccountManager.cs — Hệ thống tài khoản local (SHA256, không cần server)
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AccountManager
{
    public static string TenDangNhap { get; private set; } = "";
    public static bool DaDangNhap => !string.IsNullOrEmpty(TenDangNhap);

    private static string FilePath => Application.persistentDataPath + "/accounts.json";

    // Dang ky tai khoan moi va luu vao file
    public static string DangKy(string ten, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(ten))
            return "Tên đăng nhập không được để trống!";
        if (ten.Length < 3)
            return "Tên đăng nhập phải có ít nhất 3 ký tự!";
        if (ten.Length > 20)
            return "Tên đăng nhập tối đa 20 ký tự!";
        if (string.IsNullOrWhiteSpace(matKhau))
            return "Mật khẩu không được để trống!";
        if (matKhau.Length < 4)
            return "Mật khẩu phải có ít nhất 4 ký tự!";

        AccountDatabase db = DocDatabase();
        foreach (var acc in db.danhSach)
            if (acc.tenDangNhap.ToLower() == ten.ToLower())
                return "Tên đăng nhập đã tồn tại!";

        AccountData accMoi = new AccountData
        {
            tenDangNhap    = ten,
            matKhauHash    = HashMatKhau(matKhau),
            ngayTao        = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            lanDangNhapCuoi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        db.danhSach.Add(accMoi);
        LuuDatabase(db);
        TenDangNhap = ten;
        return "";
    }

    // Dang nhap va cap nhat lan dang nhap cuoi
    public static string DangNhap(string ten, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(ten))
            return "Nhập tên đăng nhập!";
        if (string.IsNullOrWhiteSpace(matKhau))
            return "Nhập mật khẩu!";

        AccountDatabase db = DocDatabase();
        string hash = HashMatKhau(matKhau);

        foreach (var acc in db.danhSach)
        {
            if (acc.tenDangNhap.ToLower() == ten.ToLower())
            {
                if (acc.matKhauHash == hash)
                {
                    TenDangNhap = acc.tenDangNhap;
                    acc.lanDangNhapCuoi = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LuuDatabase(db);
                    return "";
                }
                return "Sai mật khẩu!";
            }
        }
        return "Tài khoản không tồn tại!";
    }

    // Xoa trang thai dang nhap
    public static void DangXuat() { TenDangNhap = ""; }

    // Tao duong dan file save theo user + slot
    public static string LayDuongDanSave(int slot)
    {
        string tenFile = DaDangNhap
            ? $"playerdata_{TenDangNhap.ToLower()}_slot{slot}.json"
            : $"playerdata_slot{slot}.json";
        return Application.persistentDataPath + "/" + tenFile;
    }

    // Kiem tra he thong co it nhat 1 tai khoan
    public static bool CoTaiKhoan()
    {
        return DocDatabase().danhSach.Count > 0;
    }

    // Lay danh sach ten dang nhap de hien thi
    public static List<string> LayDanhSachTen()
    {
        AccountDatabase db = DocDatabase();
        List<string> ds = new List<string>();
        foreach (var acc in db.danhSach) ds.Add(acc.tenDangNhap);
        return ds;
    }

    // Hash mat khau voi salt co dinh (SHA256)
    private static string HashMatKhau(string matKhau)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(matKhau + "_LimboExit_Salt"));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    // Doc database tai khoan tu file JSON
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

    // Ghi database tai khoan ra file JSON
    private static void LuuDatabase(AccountDatabase db)
    {
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(FilePath, json);
    }
}

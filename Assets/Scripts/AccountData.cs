// AccountData.cs
// Du lieu tai khoan — luu vao file JSON local
// KHONG can gan vao GameObject

using System.Collections.Generic;

[System.Serializable]
public class AccountData
{
    public string tenDangNhap;    // Username
    public string matKhauHash;    // SHA256 hash cua mat khau
    public string ngayTao;        // Ngay tao tai khoan
    public string lanDangNhapCuoi; // Lan dang nhap gan nhat
}

[System.Serializable]
public class AccountDatabase
{
    public List<AccountData> danhSach = new List<AccountData>();
}

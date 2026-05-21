// AccountData.cs — Dữ liệu tài khoản local
using System.Collections.Generic;

[System.Serializable]
public class AccountData
{
    public string tenDangNhap;
    public string matKhauHash;
    public string ngayTao;
    public string lanDangNhapCuoi;
}

[System.Serializable]
public class AccountDatabase
{
    public List<AccountData> danhSach = new List<AccountData>();
}

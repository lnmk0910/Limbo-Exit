// SaveSystem.cs
// He thong luu/doc du lieu nguoi choi bang file JSON
// HO TRO TAI KHOAN: khi dang nhap, save file duoc tach rieng theo username
// VD: playerdata_minhtuan_slot1.json
// KHONG ke thua MonoBehaviour — goi truc tiep qua SaveSystem.SaveGame()

using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // Bien luu slot hien tai dang phuc vu
    public static int currentSlotIndex = 0;

    // Tra ve duong dan lay theo Slot + Tai khoan
    private static string GetFilePath()
    {
        if (currentSlotIndex <= 0) currentSlotIndex = 1;

        // Neu da dang nhap: luu theo ten tai khoan
        if (AccountManager.DaDangNhap)
            return AccountManager.LayDuongDanSave(currentSlotIndex);

        // Chua dang nhap: luu binh thuong (tuong thich nguoc)
        return Application.persistentDataPath + $"/playerdata_slot{currentSlotIndex}.json";
    }

    // =============================================
    // HAM LUU
    // =============================================
    public static void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log($"[OK] Da luu game ({LayTenNguoiChoi()} Ho so {currentSlotIndex}) tai: {GetFilePath()}");
    }

    // =============================================
    // HAM DOC
    // =============================================
    public static PlayerData LoadGame()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"[OK] Da doc save {LayTenNguoiChoi()} Ho so {currentSlotIndex} thanh cong!");
            return data;
        }
        else
        {
            Debug.Log($"[!] Chua co file save cho {LayTenNguoiChoi()} Ho so {currentSlotIndex}. Tao du lieu moi.");
            return new PlayerData();
        }
    }

    // =============================================
    // HAM XOA SAVE
    // =============================================
    public static void DeleteSave()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[XOA] Da xoa file save: {path}");
        }
    }

    public static bool CoFileSave()
    {
        return File.Exists(GetFilePath());
    }

    // =============================================
    // HAM KHAM PHA (Danh rieng cho MenuManager de Load Text Slot)
    // =============================================
    public static bool KiemTraSlotTonTai(int slot)
    {
        string path;
        if (AccountManager.DaDangNhap)
            path = AccountManager.LayDuongDanSave(slot);
        else
            path = Application.persistentDataPath + $"/playerdata_slot{slot}.json";
        return File.Exists(path);
    }

    public static PlayerData XemTruocDataSlot(int slot)
    {
        string path;
        if (AccountManager.DaDangNhap)
            path = AccountManager.LayDuongDanSave(slot);
        else
            path = Application.persistentDataPath + $"/playerdata_slot{slot}.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData();
    }

    // =============================================
    // HELPER
    // =============================================
    static string LayTenNguoiChoi()
    {
        return AccountManager.DaDangNhap ? AccountManager.TenDangNhap : "Guest";
    }
}

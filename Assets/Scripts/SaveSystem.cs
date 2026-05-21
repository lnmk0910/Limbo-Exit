// SaveSystem.cs — Lưu/đọc dữ liệu người chơi bằng JSON, hỗ trợ tài khoản + slot
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static int currentSlotIndex = 0;

    // Xac dinh duong dan file theo slot va tai khoan
    private static string GetFilePath()
    {
        if (currentSlotIndex <= 0) currentSlotIndex = 1;
        if (AccountManager.DaDangNhap)
            return AccountManager.LayDuongDanSave(currentSlotIndex);
        return Application.persistentDataPath + $"/playerdata_slot{currentSlotIndex}.json";
    }

    // Luu PlayerData ra file JSON
    public static void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);
    }

    // Doc PlayerData tu file JSON (neu khong co thi tra data mac dinh)
    public static PlayerData LoadGame()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData();
    }

    // Xoa file save hien tai
    public static void DeleteSave()
    {
        string path = GetFilePath();
        if (File.Exists(path)) File.Delete(path);
    }

    // Kiem tra save hien tai co ton tai hay khong
    public static bool CoFileSave() => File.Exists(GetFilePath());

    // Kiem tra 1 slot co ton tai hay khong (khong phu thuoc slot dang chon)
    public static bool KiemTraSlotTonTai(int slot)
    {
        string path = AccountManager.DaDangNhap
            ? AccountManager.LayDuongDanSave(slot)
            : Application.persistentDataPath + $"/playerdata_slot{slot}.json";
        return File.Exists(path);
    }

    // Doc nhanh data cua slot bat ky (neu chua co thi tra mac dinh)
    public static PlayerData XemTruocDataSlot(int slot)
    {
        string path = AccountManager.DaDangNhap
            ? AccountManager.LayDuongDanSave(slot)
            : Application.persistentDataPath + $"/playerdata_slot{slot}.json";
        if (File.Exists(path))
            return JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
        return new PlayerData();
    }
}

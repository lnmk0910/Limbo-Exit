// SaveSystem.cs
// Hệ thống lưu/đọc dữ liệu người chơi bằng file JSON
// KHÔNG kế thừa MonoBehaviour - gọi trực tiếp qua SaveSystem.SaveGame()

using System.IO;       // Dùng để làm việc với file
using UnityEngine;     // Dùng Debug.Log và Application.persistentDataPath

public static class SaveSystem
{
    // Biến lưu slot hiện tại đang phục vụ
    public static int currentSlotIndex = 0;

    // Trả về đường dẫn lấy theo Slot
    private static string GetFilePath()
    {
        // Để tương thích ngược (nếu ai đó dùng 0)
        if (currentSlotIndex <= 0) currentSlotIndex = 1;
        return Application.persistentDataPath + $"/playerdata_slot{currentSlotIndex}.json";
    }

    // =============================================
    // HÀM LƯU: Chuyển PlayerData thành JSON rồi ghi ra file
    // =============================================
    public static void SaveGame(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log($"✅ Đã lưu game (Hồ sơ {currentSlotIndex}) tại: {GetFilePath()}");
    }

    // =============================================
    // HÀM ĐỌC: Đọc file JSON và trả về PlayerData
    // =============================================
    public static PlayerData LoadGame()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"✅ Đã đọc save Hồ sơ {currentSlotIndex} thành công!");
            return data;
        }
        else
        {
            Debug.Log($"⚠️ Chưa có file save cho Hồ sơ {currentSlotIndex}. Tạo dữ liệu mới.");
            return new PlayerData();
        }
    }

    // =============================================
    // HÀM XÓA SAVE: Dùng khi người chơi chọn "Chơi mới"
    // =============================================
    public static void DeleteSave()
    {
        string path = GetFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"🗑️ Đã xóa file save: {path}");
        }
    }

    public static bool CoFileSave()
    {
        return File.Exists(GetFilePath());
    }

    // =============================================
    // HÀM KHÁM PHÁ (Dành riêng cho MenuManager để Load Text Slot)
    // =============================================
    public static bool KiemTraSlotTonTai(int slot)
    {
        string path = Application.persistentDataPath + $"/playerdata_slot{slot}.json";
        return File.Exists(path);
    }

    public static PlayerData XemTruocDataSlot(int slot)
    {
        string path = Application.persistentDataPath + $"/playerdata_slot{slot}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData(); // Rỗng nếu không có
    }
}

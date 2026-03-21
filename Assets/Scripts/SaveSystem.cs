// SaveSystem.cs
// Hệ thống lưu/đọc dữ liệu người chơi bằng file JSON
// KHÔNG kế thừa MonoBehaviour - gọi trực tiếp qua SaveSystem.SaveGame()

using System.IO;       // Dùng để làm việc với file
using UnityEngine;     // Dùng Debug.Log và Application.persistentDataPath

public static class SaveSystem
{
    // Đường dẫn file lưu: thư mục dữ liệu của app + tên file
    // Application.persistentDataPath tự động trỏ đúng thư mục trên mọi OS
    private static string duongDanFile = Application.persistentDataPath + "/playerdata.json";

    // =============================================
    // HÀM LƯU: Chuyển PlayerData thành JSON rồi ghi ra file
    // =============================================
    public static void SaveGame(PlayerData data)
    {
        // Chuyển object PlayerData thành chuỗi JSON
        string json = JsonUtility.ToJson(data, true); // true = format đẹp, dễ đọc

        // Ghi chuỗi JSON vào file
        File.WriteAllText(duongDanFile, json);

        Debug.Log("✅ Đã lưu game tại: " + duongDanFile);
    }

    // =============================================
    // HÀM ĐỌC: Đọc file JSON và trả về PlayerData
    // =============================================
    public static PlayerData LoadGame()
    {
        // Kiểm tra file có tồn tại không
        if (File.Exists(duongDanFile))
        {
            // Đọc nội dung file thành chuỗi
            string json = File.ReadAllText(duongDanFile);

            // Chuyển chuỗi JSON thành object PlayerData
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            Debug.Log("✅ Đã đọc save thành công!");
            return data;
        }
        else
        {
            // Chưa có file save → trả về data mặc định (game mới)
            Debug.Log("⚠️ Chưa có file save. Tạo dữ liệu mới.");
            return new PlayerData();
        }
    }

    // =============================================
    // HÀM XÓA SAVE: Dùng khi người chơi chọn "Chơi mới"
    // =============================================
    public static void DeleteSave()
    {
        if (File.Exists(duongDanFile))
        {
            File.Delete(duongDanFile);
            Debug.Log("🗑️ Đã xóa file save.");
        }
    }

    // =============================================
    // KIỂM TRA: Có file save chưa? (Dùng để bật/tắt btn_Continue)
    // =============================================
    public static bool CoFileSave()
    {
        return File.Exists(duongDanFile);
    }
}

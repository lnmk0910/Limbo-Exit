// ExitGate.cs
// Cổng Thoát – khi Player chạm vào: thắng màn, lưu data, sinh map mới
// GẮN vào: Prefab_ExitGate (cần Collider IsTrigger = true)

using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGate : MonoBehaviour
{
    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🏆 Đã thoát mê cung! Đang lưu...");
            ThangMan();
        }
    }

    void ThangMan()
    {
        PlayerData data = SaveSystem.LoadGame();

        // Tăng số màn
        data.mapHienTai += 1;

        // Thưởng Mảnh Hồn
        data.soManhHon += 10;

        // ⭐ Quan trọng: reset seed = 0 để map tiếp theo sinh ngẫu nhiên
        data.seed = 0;

        SaveSystem.SaveGame(data);

        Debug.Log($"✅ Màn {data.mapHienTai} | Mảnh Hồn: {data.soManhHon} | Seed reset → map mới!");

        // Load lại Scene để sinh mê cung hoàn toàn mới
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

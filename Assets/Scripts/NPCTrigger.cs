// NPCTrigger.cs
// Khi Player đến gần NPC: hiện thông báo mua/bán (đơn giản qua Debug)
// GẮN vào: Prefab_NPC
// Giai đoạn sau có thể mở rộng thành UI cửa hàng đầy đủ

using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    [Header("=== CỬA HÀNG ===")]
    public int giaMuaKyNang = 5;   // Giá mua 1 kỹ năng (Mảnh Hồn)

    private bool dangGanPlayer = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = true;
        Debug.Log($"🧑‍💼 NPC: Chào! Nhấn E để xem hàng. Giá: {giaMuaKyNang} Mảnh Hồn");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;
        Debug.Log("🧑‍💼 NPC: Hẹn gặp lại!");
    }

    void Update()
    {
        // Nhấn E khi đứng gần NPC để "mua"
        if (dangGanPlayer && Input.GetKeyDown(KeyCode.E))
        {
            PlayerData data = SaveSystem.LoadGame();
            if (data.soManhHon >= giaMuaKyNang)
            {
                data.soManhHon -= giaMuaKyNang;
                SaveSystem.SaveGame(data);
                Debug.Log($"✅ Mua thành công! Còn lại: {data.soManhHon} Mảnh Hồn");
            }
            else
            {
                Debug.Log($"❌ Không đủ Mảnh Hồn! Cần {giaMuaKyNang}, có {data.soManhHon}");
            }
        }
    }
}

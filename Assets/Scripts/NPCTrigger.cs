// NPCTrigger.cs (cập nhật)
// Khi Player đến gần NPC Thương Nhân: hiện cửa hàng
// Nhấn E để mở/đóng shop
// GẮN vào: Prefab_NPC

using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
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
        Debug.Log("🧑‍💼 Thương Nhân Ký Ức: Nhấn E để mua vật phẩm...");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;

        // Đóng shop nếu đang mở
        if (ItemShopUI.Instance != null)
            ItemShopUI.Instance.DongShop();

        Debug.Log("🧑‍💼 Thương Nhân Ký Ức: Hẹn gặp lại...");
    }

    void Update()
    {
        if (!dangGanPlayer) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (ItemShopUI.Instance != null)
                ItemShopUI.Instance.MoShop();
            else
                Debug.LogWarning("⚠️ ItemShopUI chưa được gắn vào Scene!");
        }
    }
}

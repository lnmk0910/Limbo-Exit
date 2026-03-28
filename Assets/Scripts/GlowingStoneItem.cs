// GlowingStoneItem.cs
// Đá Phát Sáng: Nhấn 1 → ném ra xa, tạo ánh sáng trong 10 giây
// GẮN vào: Player GameObject

using UnityEngine;

public class GlowingStoneItem : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float lucNem   = 12f;   // Lực ném
    public float thoiGian = 10f;   // Thời gian phát sáng (giây)
    public GameObject prefabDa;    // Prefab viên đá (Sphere nhỏ + Point Light)
    public Transform viTriNem;     // Điểm ném (thường là PlayerCamera)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            DungDa();
    }

    void DungDa()
    {
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.DungDa())
        {
            Debug.Log("❌ Không có Đá Phát Sáng!");
            return;
        }

        if (prefabDa == null) { Debug.LogWarning("⚠️ Chưa gán Prefab Đá!"); return; }

        // Spawn viên đá tại camera
        Transform goc = viTriNem != null ? viTriNem : transform;
        GameObject da = Instantiate(prefabDa, goc.position + goc.forward, Quaternion.identity);

        // Ném theo hướng nhìn
        Rigidbody rb = da.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(goc.forward * lucNem, ForceMode.VelocityChange);

        // Tự hủy sau thoiGian giây
        Destroy(da, thoiGian);
        Debug.Log($"🪨 Ném Đá Phát Sáng! Còn lại: {PlayerInventory.Instance.daPhatSang}");
    }
}

// TimeClockItem.cs
// Đồng Hồ Thời Gian: Nhấn 2 → đóng băng thời gian quái vật trong 5 giây
// GẮN vào: Player GameObject
// Quái vật cần có tag "Enemy" để bị ảnh hưởng

using System.Collections;
using UnityEngine;

public class TimeClockItem : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianDong = 5f;     // Thời gian đóng băng (giây)
    public Color mauHieuUng = Color.cyan; // Màu hiệu ứng khi đóng băng

    // Biến tĩnh để quái vật đọc
    public static bool dangDongBang = false;

    void Update()
    {
        if (!UIManager.DangTrongGame()) return;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            DungDongHo();
    }

    void DungDongHo()
    {
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.DungDongHo())
        {
            Debug.Log("❌ Không có Đồng Hồ Thời Gian!");
            return;
        }

        StartCoroutine(HieuUngDongBang());
    }

    IEnumerator HieuUngDongBang()
    {
        dangDongBang = true;
        Debug.Log($"⏱️ Đóng băng quái vật trong {thoiGianDong}s!");

        // Tìm tất cả quái vật (tag "Enemy") và tắt NavMeshAgent
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            // Đổi màu báo hiệu đóng băng
            Renderer r = e.GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = mauHieuUng;

            // Tắt AI movement (nếu có NavMeshAgent)
            var nav = e.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;
        }

        yield return new WaitForSeconds(thoiGianDong);

        // Khôi phục quái vật
        foreach (var e in enemies)
        {
            if (e == null) continue;
            Renderer r = e.GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = Color.white;

            var nav = e.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = true;
        }

        dangDongBang = false;
        Debug.Log("⏱️ Hết hiệu lực đóng băng!");
    }
}

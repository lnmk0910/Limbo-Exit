// AncientCompassItem.cs
// La Bàn Cổ: Nhấn 3 → hiện mũi tên chỉ hướng Cổng Thoát trong 3s rồi vỡ
// GẮN vào: Player GameObject

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AncientCompassItem : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    public Transform exitGate;          // Kéo ExitGate vào đây (hoặc tự tìm)
    public GameObject panelLaBan;       // Panel UI hiển thị mũi tên
    public TMP_Text txtHuong;           // Text hiển thị hướng (N/S/E/W)
    public Image imgMuiTen;             // Image mũi tên xoay

    [Header("=== THỜI GIAN ===")]
    public float thoiGianHien = 3f;

    private bool dangDung = false;

    void Start()
    {
        if (panelLaBan != null) panelLaBan.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
            DungLaBan();

        // Cập nhật mũi tên xoay hướng về ExitGate
        if (dangDung && exitGate != null && imgMuiTen != null)
        {
            Vector3 huong = exitGate.position - transform.position;
            huong.y = 0; // Chỉ quan tâm hướng ngang
            float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;
            imgMuiTen.transform.rotation = Quaternion.Euler(0f, 0f, -goc);
        }
    }

    void DungLaBan()
    {
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.DungLaBan())
        {
            Debug.Log("❌ Không có La Bàn Cổ!");
            return;
        }

        // Tự động tìm ExitGate nếu chưa gán
        if (exitGate == null)
        {
            GameObject gate = GameObject.Find("ExitGate(Clone)");
            if (gate == null) gate = GameObject.FindWithTag("ExitGate");
            if (gate != null) exitGate = gate.transform;
        }

        StartCoroutine(HienLaBan());
    }

    IEnumerator HienLaBan()
    {
        dangDung = true;
        if (panelLaBan != null) panelLaBan.SetActive(true);
        if (txtHuong != null) txtHuong.text = "🧭 Cổng Thoát";
        Debug.Log($"🧭 La Bàn Cổ kích hoạt trong {thoiGianHien}s!");

        // Đếm ngược
        float conLai = thoiGianHien;
        while (conLai > 0)
        {
            if (txtHuong != null) txtHuong.text = $"🧭 Cổng Thoát ({conLai:F1}s)";
            conLai -= Time.deltaTime;
            yield return null;
        }

        // Vỡ vụn (ẩn UI)
        if (panelLaBan != null) panelLaBan.SetActive(false);
        dangDung = false;
        Debug.Log("🧭 La Bàn Cổ đã vỡ vụn!");
    }
}

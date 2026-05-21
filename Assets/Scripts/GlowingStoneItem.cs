// GlowingStoneItem.cs — Đá Phát Sáng: nhấn 1 để ném
using UnityEngine;

public class GlowingStoneItem : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float lucNem   = 12f;
    public float thoiGian = 10f;
    public GameObject prefabDa;
    public Transform  viTriNem;

    // Lang nghe phim tat de nem da
    void Update()
    {
        if (!UIManager.DangTrongGame()) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) DungDa();
    }

    // Tru item, spawn da va nem
    void DungDa()
    {
        if (!ItemSystem.DungDa()) return;
        if (prefabDa == null) return;

        Transform goc = viTriNem != null ? viTriNem : transform;
        GameObject da = Instantiate(prefabDa, goc.position + goc.forward, Quaternion.identity);

        Rigidbody rb = da.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(goc.forward * lucNem, ForceMode.VelocityChange);

        Destroy(da, thoiGian);
    }
}

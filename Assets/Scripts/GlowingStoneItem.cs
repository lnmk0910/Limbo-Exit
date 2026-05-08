// GlowingStoneItem.cs
// Da Phat Sang: Nhan 1 → nem ra xa, tao anh sang trong 10 giay
// GAN vao: Player GameObject

using UnityEngine;

public class GlowingStoneItem : MonoBehaviour
{
    [Header("=== CAI DAT ===")]
    public float lucNem   = 12f;
    public float thoiGian = 10f;
    public GameObject prefabDa;    // Prefab vien da (Sphere nho + Point Light)
    public Transform  viTriNem;    // Diem nem (thuong la PlayerCamera)

    void Update()
    {
        // Chi kich hoat khi dang choi, khong cho phep khi co panel mo
        if (!UIManager.DangTrongGame()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            DungDa();
    }

    void DungDa()
    {
        // Dung ItemSystem — hoat dong du PlayerInventory.Instance co null hay khong
        if (!ItemSystem.DungDa()) return;

        if (prefabDa == null)
        {
            Debug.LogWarning("[DA] Chua gan Prefab Da trong Inspector! Item da bi tru nhung khong spawn duoc.");
            return;
        }

        // Spawn vien da tai camera
        Transform goc = viTriNem != null ? viTriNem : transform;
        GameObject da = Instantiate(prefabDa, goc.position + goc.forward, Quaternion.identity);

        // Nem theo huong nhin
        Rigidbody rb = da.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(goc.forward * lucNem, ForceMode.VelocityChange);

        // Tu huy sau thoiGian giay
        Destroy(da, thoiGian);
        Debug.Log($"[DA] Nem Da Phat Sang! Con lai: {ItemSystem.SoDa()}");
    }
}

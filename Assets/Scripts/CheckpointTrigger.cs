// CheckpointTrigger.cs
// Khi Player bước vào Checkpoint:
// - Thưởng Mảnh Hồn
// - Đăng ký vị trí này là Điểm An Toàn (Respawn Point)
// GẮN vào: Prefab_Checkpoint (cần Collider IsTrigger = true)

using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private bool daKichHoat = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (daKichHoat) return;
        if (!other.CompareTag("Player")) return;

        daKichHoat = true;

        // ⭐ Đăng ký vị trí này là điểm hồi sinh an toàn
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.DangKyDiemAnToan(transform.position);

        // Thưởng Mảnh Hồn
        PlayerData data = SaveSystem.LoadGame();
        data.soManhHon += 2;
        SaveSystem.SaveGame(data);

        Debug.Log($"💚 Checkpoint kích hoạt! +2 Mảnh Hồn. Tổng: {data.soManhHon}");

        // Đổi màu xám = đã dùng
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = Color.gray;
    }
}

// CheckpointTrigger.cs
// Khi Player bước vào Checkpoint: hồi phục / lưu vị trí hiện tại
// GẮN vào: Prefab_Checkpoint
// Prefab cần có: Collider với IsTrigger = true

using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private bool daKichHoat = false; // Tránh kích hoạt nhiều lần

    void Start()
    {
        // Đảm bảo collider là trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (daKichHoat) return;
        if (!other.CompareTag("Player")) return;

        daKichHoat = true;

        // Thưởng Mảnh Hồn nhỏ khi qua Checkpoint
        PlayerData data = SaveSystem.LoadGame();
        data.soManhHon += 2;
        SaveSystem.SaveGame(data);

        Debug.Log($"💚 Checkpoint! +2 Mảnh Hồn. Tổng: {data.soManhHon}");

        // Đổi màu để biết đã kích hoạt
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = Color.gray;
    }
}

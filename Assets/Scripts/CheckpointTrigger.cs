// CheckpointTrigger.cs — Điểm an toàn: thưởng Mảnh Hồn + đăng ký respawn
using UnityEngine;

    public class CheckpointTrigger : MonoBehaviour
    {
        private bool daKichHoat = false;

        // Dat trigger collider
        void Start()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        // Khi player cham checkpoint: luu diem an toan va thuong
        void OnTriggerEnter(Collider other)
        {
            if (daKichHoat) return;
            if (!other.CompareTag("Player")) return;
            daKichHoat = true;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.DangKyDiemAnToan(transform.position);

        PlayerData data = SaveSystem.LoadGame();
        data.soManhHon += 2;
        SaveSystem.SaveGame(data);
        AudioManager.PhatCheckpoint();
        AudioManager.PhatManhHonNhan();
        GameHUD.LamMoi();

        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = Color.gray;
    }
}

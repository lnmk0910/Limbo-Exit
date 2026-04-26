// GameManager.cs
// Spawn Player/ExitGate và kết nối RespawnManager
// GẮN vào: Empty GameObject "GameManager" trong GameScene

using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("=== PREFABS ===")]
    public GameObject prefabPlayer;
    public GameObject prefabExitGate;

    [Header("=== THAM CHIẾU ===")]
    public MazeGenerator mazeGenerator;
    public RespawnManager respawnManager;   // Kéo RespawnManager vào đây

    [Header("=== KÍCH THƯỚC Ô ===")]
    public float kichThuocO = 4f;

    [Header("=== ĐỘ CAO SPAWN ===")]
    public float doCaoPlayer = 1f;
    public float doCaoGate   = 1.5f;

    void Start()
    {
        if (mazeGenerator == null) { Debug.LogError("❌ Thiếu MazeGenerator!"); return; }

        DatPlayerVaExitGate();

        // Chỉ log seed (MazeGenerator.Awake đã lưu seed rồi, không cần ghi lại)
        Debug.Log($"💾 Seed {mazeGenerator.seedHienTai} | Màn {SaveSystem.LoadGame().mapHienTai}");
    }

    void DatPlayerVaExitGate()
    {
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        Vector3 viTriPlayer = new Vector3(start.x * kichThuocO, doCaoPlayer, start.y * kichThuocO);
        Vector3 viTriGate   = new Vector3(end.x   * kichThuocO, doCaoGate,   end.y   * kichThuocO);

        GameObject playerObj = null;
        if (prefabPlayer != null)
        {
            playerObj = Instantiate(prefabPlayer, viTriPlayer, Quaternion.identity);
            Debug.Log("🧍 Player spawn tại: " + viTriPlayer);
        }

        if (prefabExitGate != null)
        {
            Instantiate(prefabExitGate, viTriGate, Quaternion.identity);
            Debug.Log("🚪 ExitGate spawn tại: " + viTriGate);
        }

        // Kết nối Player với RespawnManager
        if (respawnManager != null && playerObj != null)
        {
            respawnManager.DatPlayer(playerObj.transform);
            // Đặt viTriStart làm điểm hồi sinh mặc định (fallback)
            respawnManager.viTriStart = playerObj.transform;
        }
    }
}

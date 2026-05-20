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
    public RespawnManager respawnManager;

    [Header("=== ĐỘ CAO SPAWN ===")]
    public float doCaoPlayer = 1f;
    public float doCaoGate   = 1.5f;

    void Start()
    {
        if (mazeGenerator == null) { Debug.LogError("[LOI] Thiếu MazeGenerator!"); return; }

        DatPlayerVaExitGate();
        Debug.Log($"[SAVE] Seed {mazeGenerator.seedHienTai} | Màn {SaveSystem.LoadGame().mapHienTai}");
    }

    void DatPlayerVaExitGate()
    {
        // Luôn dùng GameSettings.kichThuocO — ĐỒNG BỘ với MazeRenderer
        float kichThuocO = GameSettings.kichThuocO;

        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        // Tính world position — trùng hệ tọa độ với MazeRenderer
        Vector3 viTriPlayer = new Vector3(start.x * kichThuocO, doCaoPlayer, start.y * kichThuocO);
        Vector3 viTriGate   = new Vector3(end.x   * kichThuocO, doCaoGate,   end.y   * kichThuocO);

        float khoangCachGrid = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
        float khoangCachWorld = Vector3.Distance(viTriPlayer, viTriGate);

        Debug.Log($"[SPAWN] Start grid:({start.x},{start.y}) → world:{viTriPlayer}");
        Debug.Log($"[SPAWN] End   grid:({end.x},{end.y}) → world:{viTriGate}");
        Debug.Log($"[SPAWN] Khoảng cách: grid={khoangCachGrid} | world={khoangCachWorld:F1}m | kichThuocO={kichThuocO}");

        // Cảnh báo nếu ExitGate quá gần Player (< 3 ô)
        if (khoangCachGrid < 3)
            Debug.LogError($"[BUG] ExitGate QUÁ GẦN Player! Grid distance={khoangCachGrid}. BFS có thể bị lỗi!");

        GameObject playerObj = null;
        if (prefabPlayer != null)
        {
            playerObj = Instantiate(prefabPlayer, viTriPlayer, Quaternion.identity);
            Debug.Log("[PLAYER] Player spawn tại: " + viTriPlayer);
        }

        if (prefabExitGate != null)
        {
            Instantiate(prefabExitGate, viTriGate, Quaternion.identity);
            Debug.Log("[CONG] ExitGate spawn tại: " + viTriGate);
        }

        // Kết nối Player với RespawnManager
        if (respawnManager != null && playerObj != null)
        {
            respawnManager.DatPlayer(playerObj.transform);
            respawnManager.viTriStart = playerObj.transform;
        }
    }
}

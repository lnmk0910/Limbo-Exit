// GameManager.cs — Spawn Player/ExitGate và kết nối RespawnManager
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

    // Kiem tra tham chieu va dat Player/ExitGate
    void Start()
    {
        if (mazeGenerator == null) { Debug.LogError("[LOI] Thiếu MazeGenerator!"); return; }
        DatPlayerVaExitGate();
    }

    // Spawn Player va ExitGate theo vi tri start/end trong maze
    void DatPlayerVaExitGate()
    {
        float kichThuocO = GameSettings.kichThuocO;

        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        Vector3 viTriPlayer = new Vector3(start.x * kichThuocO, doCaoPlayer, start.y * kichThuocO);
        Vector3 viTriGate   = new Vector3(end.x   * kichThuocO, doCaoGate,   end.y   * kichThuocO);

        GameObject playerObj = null;
        if (prefabPlayer != null)
            playerObj = Instantiate(prefabPlayer, viTriPlayer, Quaternion.identity);

        if (prefabExitGate != null)
            Instantiate(prefabExitGate, viTriGate, Quaternion.identity);

        if (respawnManager != null && playerObj != null)
        {
            respawnManager.DatPlayer(playerObj.transform);
            respawnManager.viTriStart = playerObj.transform;
        }
    }
}

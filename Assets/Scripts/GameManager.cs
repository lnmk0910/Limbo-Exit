// GameManager.cs
// Quản lý: Spawn Player/ExitGate, đọc seed từ save
// GẮN vào: Empty GameObject "GameManager" trong GameScene

using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("=== PREFABS ===")]
    public GameObject prefabPlayer;
    public GameObject prefabExitGate;

    [Header("=== THAM CHIẾU ===")]
    public MazeGenerator mazeGenerator;

    [Header("=== KÍCH THƯỚC Ô (khớp MazeRenderer) ===")]
    public float kichThuocO = 4f;

    [Header("=== ĐỘ CAO SPAWN ===")]
    public float doCaoPlayer = 1f;
    public float doCaoGate   = 1.5f;

    void Start()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("❌ Chưa gán MazeGenerator vào GameManager!");
            return;
        }

        DatPlayerVaExitGate();

        // Lưu seed của map này vào save (để load lại đúng bản đồ)
        PlayerData data = SaveSystem.LoadGame();
        data.seed = mazeGenerator.seedHienTai;
        SaveSystem.SaveGame(data);
        Debug.Log($"💾 Đã lưu seed {data.seed} | Màn {data.mapHienTai}");
    }

    void DatPlayerVaExitGate()
    {
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        Vector3 viTriPlayer = new Vector3(start.x * kichThuocO, doCaoPlayer, start.y * kichThuocO);
        Vector3 viTriGate   = new Vector3(end.x   * kichThuocO, doCaoGate,   end.y   * kichThuocO);

        if (prefabPlayer != null)
        {
            Instantiate(prefabPlayer, viTriPlayer, Quaternion.identity);
            Debug.Log("🧍 Player spawn tại: " + viTriPlayer);
        }

        if (prefabExitGate != null)
        {
            Instantiate(prefabExitGate, viTriGate, Quaternion.identity);
            Debug.Log("🚪 ExitGate spawn tại: " + viTriGate);
        }
    }
}

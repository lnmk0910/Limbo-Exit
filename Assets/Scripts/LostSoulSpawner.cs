// LostSoulSpawner.cs
// Spawn các Linh Hồn Lạc Lối ngẫu nhiên trong mê cung
// GẮN vào: cùng GameObject với MazeGenerator

using UnityEngine;

public class LostSoulSpawner : MonoBehaviour
{
    [Header("=== PREFAB ===")]
    public GameObject prefabLostSoul;

    [Header("=== SỐ LƯỢNG ===")]
    [Range(1, 8)]
    public int soLuong = 3;

    [Header("=== THAM CHIẾU ===")]
    public MazeGenerator mazeGenerator;
    public float kichThuocO = 4f;

    [Header("=== KHOẢNG CÁCH AN TOÀN ===")]
    public float khoangCachStart = 8f;   // Không spawn quá gần điểm Start

    void Start()
    {
        if (prefabLostSoul == null || mazeGenerator == null) return;
        Invoke(nameof(Spawn), 0.2f); // Chờ MazeGenerator xong
    }

    void Spawn()
    {
        int soCol = mazeGenerator.SoCol;
        int soRow = mazeGenerator.SoRow;
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;
        int[,] evGrid    = mazeGenerator.EventGrid;

        int daSinh = 0, soLanThu = 0;

        while (daSinh < soLuong && soLanThu < 100)
        {
            soLanThu++;
            int c = Random.Range(0, soCol);
            int r = Random.Range(0, soRow);

            // Bỏ Start/End
            if (c == start.x && r == start.y) continue;
            if (c == end.x   && r == end.y)   continue;

            // Bỏ ô có sự kiện khác (Checkpoint/Minigame/NPC)
            if (evGrid[c, r] != 1) continue;

            Vector3 viTri = new Vector3(c * kichThuocO, 0.5f, r * kichThuocO);
            Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 0, start.y * kichThuocO);

            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachStart) continue;

            Instantiate(prefabLostSoul, viTri, Quaternion.identity);
            daSinh++;
            Debug.Log($"👻 Spawn LostSoul #{daSinh} tại ({c},{r})");
        }

        Debug.Log($"✅ Đã spawn {daSinh}/{soLuong} Linh Hồn Lạc Lối.");
    }
}

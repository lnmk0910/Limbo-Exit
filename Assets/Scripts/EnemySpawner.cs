// EnemySpawner.cs (cập nhật)
// Spawn quái vật theo Biome:
//   0 = Đá Cổ   → EnemyAI (quái vật cơ bản)
//   1 = Thư Viện → ThuthuMuAI (Thủ Thư Mù)
//   2 = Đầm Lầy  → SinhVatBunAI (Sinh Vật Bùn)
//   3 = Tinh Thể → EnemyAI (quái vật cơ bản, tốc độ cao hơn)

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("=== PREFAB THEO BIOME ===")]
    public GameObject prefabEnemyDaCo;        // Biome 0 & 3
    public GameObject prefabThuthuMu;         // Biome 1 – Thư Viện
    public GameObject prefabSinhVatBun;       // Biome 2 – Đầm Lầy

    [Header("=== SỐ LƯỢNG ===")]
    [Range(1, 8)]
    public int soLuongEnemy = 2;

    [Header("=== THAM CHIẾU ===")]
    public MazeGenerator mazeGenerator;
    public float kichThuocO = 4f;

    [Header("=== KHOẢNG CÁCH AN TOÀN ===")]
    public float khoangCachAnToan = 12f;

    void Start()
    {
        if (mazeGenerator == null) return;
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        // Chọn prefab theo biome hiện tại
        GameObject prefabDung = LayPrefabTheoBiome();
        if (prefabDung == null)
        {
            Debug.LogWarning("⚠️ Chưa gán Prefab Enemy cho Biome này!");
            return;
        }

        int soCol = mazeGenerator.SoCol;
        int soRow = mazeGenerator.SoRow;
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        int daSinh = 0, soLanThu = 0;

        while (daSinh < soLuongEnemy && soLanThu < 100)
        {
            soLanThu++;
            int c = Random.Range(0, soCol);
            int r = Random.Range(0, soRow);
            if (c == start.x && r == start.y) continue;
            if (c == end.x   && r == end.y)   continue;

            Vector3 viTri = new Vector3(c * kichThuocO, 1f, r * kichThuocO);
            Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 1f, start.y * kichThuocO);

            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;

            Instantiate(prefabDung, viTri, Quaternion.identity);
            daSinh++;
            Debug.Log($"👾 Spawn [{LayTenQuai()}] #{daSinh} tại ({c},{r})");
        }

        Debug.Log($"✅ Spawn {daSinh}/{soLuongEnemy} [{LayTenQuai()}] | Biome {GameSettings.biomeIndex}");
    }

    GameObject LayPrefabTheoBiome()
    {
        switch (GameSettings.biomeIndex)
        {
            case 1: return prefabThuthuMu;
            case 2: return prefabSinhVatBun;
            default: return prefabEnemyDaCo; // Biome 0 và 3
        }
    }

    string LayTenQuai()
    {
        switch (GameSettings.biomeIndex)
        {
            case 1: return "Thủ Thư Mù";
            case 2: return "Sinh Vật Bùn";
            default: return "Quái Vật Đá Cổ";
        }
    }
}

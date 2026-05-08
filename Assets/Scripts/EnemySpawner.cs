// EnemySpawner.cs
// Spawn quai vat theo Biome + DDA + Heatmap
// DDA dieu chinh so luong quai
// Heatmap uu tien spawn o vung nguoi choi hay di qua (sau lan chet dau)

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("=== PREFAB THEO BIOME ===")]
    public GameObject prefabEnemyDaCo;
    public GameObject prefabThuthuMu;
    public GameObject prefabSinhVatBun;

    [Header("=== SO LUONG ===")]
    [Range(1, 8)]
    public int soLuongEnemy = 2;

    [Header("=== THAM CHIEU ===")]
    public MazeGenerator mazeGenerator;
    public float kichThuocO = 6f;

    [Header("=== KHOANG CACH AN TOAN ===")]
    public float khoangCachAnToan = 12f;

    void Start()
    {
        if (mazeGenerator == null) return;
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        PlayerData data = SaveSystem.LoadGame();
        int biomeThucTe = 0;
        if (data.biomeSequence != null && data.biomeSequence.Length > 0)
        {
            int idx = Mathf.Clamp((data.mapHienTai - 1) % data.biomeSequence.Length, 0, data.biomeSequence.Length - 1);
            biomeThucTe = data.biomeSequence[idx];
        }

        GameObject prefabDung = LayPrefabTheoBiome(biomeThucTe);
        if (prefabDung == null)
        {
            Debug.LogWarning("[QUAI] Chua gan Prefab Enemy cho Biome nay!");
            return;
        }

        // === DDA: Dieu chinh so luong quai ===
        int bonus = DDAManager.LaySoQuaiBonus();
        int soLuongThucTe = Mathf.Clamp(soLuongEnemy + bonus, 1, 10);
        float heSoTocDo = DDAManager.LayHeSoTocDoQuai();
        float heSoTamNhin = DDAManager.LayHeSoTamPhatHien();

        Debug.Log($"[DDA] Quai: {soLuongThucTe} con (base {soLuongEnemy} + DDA {bonus:+0;-0}) | " +
                  $"Toc do x{heSoTocDo:F2} | Tam nhin x{heSoTamNhin:F2}");

        int soCol = mazeGenerator.SoCol;
        int soRow = mazeGenerator.SoRow;
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;

        // === HEATMAP: Lay diem nong tu lan choi truoc (neu co) ===
        var diemNong = HeatmapTracker.Instance != null
            ? HeatmapTracker.Instance.LayDiemNong(soLuongThucTe)
            : new System.Collections.Generic.List<Vector2Int>();

        int daSinh = 0;
        int soLanThu = 0;

        // Uu tien spawn o vung nong truoc (quai "hoc" vi tri nguoi choi)
        foreach (var diemGrid in diemNong)
        {
            if (daSinh >= soLuongThucTe) break;
            if (diemGrid == start || diemGrid == end) continue;

            Vector3 viTri = new Vector3(diemGrid.x * kichThuocO, 1f, diemGrid.y * kichThuocO);
            Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 1f, start.y * kichThuocO);
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh++;
            Debug.Log($"[HEATMAP] Spawn [{LayTenQuai(biomeThucTe)}] #{daSinh} tai vung nong ({diemGrid.x},{diemGrid.y})");
        }

        // Phan con lai: spawn ngau nhien
        while (daSinh < soLuongThucTe && soLanThu < 100)
        {
            soLanThu++;
            int c = Random.Range(0, soCol);
            int r = Random.Range(0, soRow);
            if (c == start.x && r == start.y) continue;
            if (c == end.x   && r == end.y)   continue;

            Vector3 viTri = new Vector3(c * kichThuocO, 1f, r * kichThuocO);
            Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 1f, start.y * kichThuocO);
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh++;
            Debug.Log($"[QUAI] Spawn [{LayTenQuai(biomeThucTe)}] #{daSinh} tai ({c},{r})");
        }

        Debug.Log($"[OK] Spawn {daSinh}/{soLuongThucTe} [{LayTenQuai(biomeThucTe)}] | Biome {biomeThucTe}");
    }

    // === AP DUNG DDA vao tung con quai ===
    void ApDungDDA(GameObject enemy, float heSoTocDo, float heSoTamNhin)
    {
        // EnemyAI
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.tocDoTuanTra  *= heSoTocDo;
            ai.tocDoTruyDuoi *= heSoTocDo;
            ai.tamNhin       *= heSoTamNhin;
            ai.tamNghe       *= heSoTamNhin;
            return;
        }

        // ThuthuMuAI
        var thu = enemy.GetComponent<ThuthuMuAI>();
        if (thu != null)
        {
            thu.tocDoNghe      *= heSoTocDo;
            thu.tocDoTruyDuoi  *= heSoTocDo;
            thu.tamNgheKhiChay    *= heSoTamNhin;
            thu.tamNgheKhiDiThuong *= heSoTamNhin;
            return;
        }

        // SinhVatBunAI
        var bun = enemy.GetComponent<SinhVatBunAI>();
        if (bun != null)
        {
            bun.tocDoTruyDuoi *= heSoTocDo;
            bun.tamAnNap      *= heSoTamNhin;
            bun.tamTruyDuoi   *= heSoTamNhin;
        }
    }

    GameObject LayPrefabTheoBiome(int biomeIndex)
    {
        switch (biomeIndex)
        {
            case 1: return prefabThuthuMu;
            case 2: return prefabSinhVatBun;
            default: return prefabEnemyDaCo;
        }
    }

    string LayTenQuai(int biomeIndex)
    {
        switch (biomeIndex)
        {
            case 1: return "Thu Thu Mu";
            case 2: return "Sinh Vat Bun";
            default: return "Quai Da Co";
        }
    }
}

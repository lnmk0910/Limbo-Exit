// EnemySpawner.cs
// Spawn quai vat theo Biome + DDA + Heatmap
// PHIÊN BẢN MỚI:
//   - Quái phải cách Start tối thiểu khoangCachAnToan
//   - Quái phải cách nhau tối thiểu khoangCachGiuaQuai (tránh cluster)
//   - Ưu tiên Heatmap trước, random sau

using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("=== PREFAB THEO BIOME ===")]
    public GameObject prefabEnemyDaCo;
    public GameObject prefabThuthuMu;
    public GameObject prefabSinhVatBun;

    [Header("=== SỐ LƯỢNG ===")]
    [Range(1, 8)]
    public int soLuongEnemy = 2;

    [Header("=== THAM CHIẾU ===")]
    public MazeGenerator mazeGenerator;
    public float kichThuocO = 6f;

    [Header("=== KHOẢNG CÁCH ===")]
    [Tooltip("Khoảng cách tối thiểu với Player spawn (world units)")]
    public float khoangCachAnToan = 12f;

    [Tooltip("Khoảng cách tối thiểu giữa 2 con quái (world units)")]
    public float khoangCachGiuaQuai = 10f;

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
        Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 1f, start.y * kichThuocO);

        // Danh sách vị trí quái đã spawn — kiểm tra khoảng cách
        List<Vector3> daSinh = new List<Vector3>();

        // === HEATMAP: Lay diem nong tu lan choi truoc (neu co) ===
        var diemNong = HeatmapTracker.Instance != null
            ? HeatmapTracker.Instance.LayDiemNong(soLuongThucTe)
            : new List<Vector2Int>();

        // Uu tien spawn o vung nong truoc (quai "hoc" vi tri nguoi choi)
        foreach (var diemGrid in diemNong)
        {
            if (daSinh.Count >= soLuongThucTe) break;
            if (diemGrid == start || diemGrid == end) continue;

            Vector3 viTri = new Vector3(diemGrid.x * kichThuocO, 1f, diemGrid.y * kichThuocO);

            // Kiểm tra khoảng cách với Player
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;

            // Kiểm tra khoảng cách với quái đã spawn
            if (QuaGanQuaiKhac(viTri, daSinh)) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh.Add(viTri);
            Debug.Log($"[HEATMAP] Spawn [{LayTenQuai(biomeThucTe)}] #{daSinh.Count} tai vung nong ({diemGrid.x},{diemGrid.y})");
        }

        // Phan con lai: spawn ngau nhien nhưng có kiểm tra khoảng cách
        int soLanThu = 0;
        while (daSinh.Count < soLuongThucTe && soLanThu < 200)
        {
            soLanThu++;
            int c = Random.Range(0, soCol);
            int r = Random.Range(0, soRow);
            if (c == start.x && r == start.y) continue;
            if (c == end.x   && r == end.y)   continue;

            Vector3 viTri = new Vector3(c * kichThuocO, 1f, r * kichThuocO);

            // Kiểm tra khoảng cách với Player
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;

            // Kiểm tra khoảng cách với quái đã spawn
            if (QuaGanQuaiKhac(viTri, daSinh)) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh.Add(viTri);
            Debug.Log($"[QUAI] Spawn [{LayTenQuai(biomeThucTe)}] #{daSinh.Count} tai ({c},{r})");
        }

        Debug.Log($"[OK] Spawn {daSinh.Count}/{soLuongThucTe} [{LayTenQuai(biomeThucTe)}] | Biome {biomeThucTe}");
    }

    // Kiểm tra vị trí có quá gần quái đã spawn không
    bool QuaGanQuaiKhac(Vector3 viTri, List<Vector3> daSinh)
    {
        foreach (var pos in daSinh)
        {
            if (Vector3.Distance(viTri, pos) < khoangCachGiuaQuai)
                return true;
        }
        return false;
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

// EnemySpawner.cs — Spawn quái vật theo Biome + DDA + Heatmap
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

    [Header("=== KHOẢNG CÁCH ===")]
    [Tooltip("Khoảng cách tối thiểu với Player spawn (world units)")]
    public float khoangCachAnToan = 18f;
    [Tooltip("Khoảng cách tối thiểu giữa 2 con quái (world units)")]
    public float khoangCachGiuaQuai = 18f;

    // Bat dau spawn quai khi maze da san sang
    void Start()
    {
        if (mazeGenerator == null) return;
        SpawnEnemy();
    }

    // Spawn quai theo biome, DDA va heatmap
    void SpawnEnemy()
    {
        float kichThuocO = GameSettings.kichThuocO;
        PlayerData data = SaveSystem.LoadGame();
        int biomeThucTe = 0;
        if (data.biomeSequence != null && data.biomeSequence.Length > 0)
        {
            int idx = Mathf.Clamp((data.mapHienTai - 1) % data.biomeSequence.Length, 0, data.biomeSequence.Length - 1);
            biomeThucTe = data.biomeSequence[idx];
        }

        GameObject prefabDung = LayPrefabTheoBiome(biomeThucTe);
        if (prefabDung == null) return;

        // DDA điều chỉnh số lượng quái
        int bonus = DDAManager.LaySoQuaiBonus();
        int soLuongThucTe = Mathf.Clamp(soLuongEnemy + bonus, 1, 10);
        float heSoTocDo = DDAManager.LayHeSoTocDoQuai();
        float heSoTamNhin = DDAManager.LayHeSoTamPhatHien();

        int soCol = mazeGenerator.SoCol;
        int soRow = mazeGenerator.SoRow;
        Vector2Int start = mazeGenerator.viTriStart;
        Vector2Int end   = mazeGenerator.viTriEnd;
        Vector3 viTriStart3D = new Vector3(start.x * kichThuocO, 1f, start.y * kichThuocO);

        // Thu thập vị trí event để quái tránh
        List<Vector3> viTriEvent = new List<Vector3>();
        if (mazeGenerator.daDatViTriSuKien != null)
        {
            foreach (var vt in mazeGenerator.daDatViTriSuKien)
                viTriEvent.Add(new Vector3(vt.x * kichThuocO, 1f, vt.y * kichThuocO));
        }

        List<Vector3> daSinh = new List<Vector3>();

        // Ưu tiên spawn ở vùng nóng (Heatmap)
        var diemNong = HeatmapTracker.Instance != null
            ? HeatmapTracker.Instance.LayDiemNong(soLuongThucTe)
            : new List<Vector2Int>();

        foreach (var diemGrid in diemNong)
        {
            if (daSinh.Count >= soLuongThucTe) break;
            if (diemGrid == start || diemGrid == end) continue;

            Vector3 viTri = new Vector3(diemGrid.x * kichThuocO, 1f, diemGrid.y * kichThuocO);
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;
            if (QuaGanQuaiKhac(viTri, daSinh)) continue;
            if (QuaGanEvent(viTri, viTriEvent)) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh.Add(viTri);
        }

        // Random spawn phần còn lại
        int soLanThu = 0;
        while (daSinh.Count < soLuongThucTe && soLanThu < 200)
        {
            soLanThu++;
            int c = Random.Range(0, soCol);
            int r = Random.Range(0, soRow);
            if (c == start.x && r == start.y) continue;
            if (c == end.x   && r == end.y)   continue;

            Vector3 viTri = new Vector3(c * kichThuocO, 1f, r * kichThuocO);
            if (Vector3.Distance(viTri, viTriStart3D) < khoangCachAnToan) continue;
            if (QuaGanQuaiKhac(viTri, daSinh)) continue;
            if (QuaGanEvent(viTri, viTriEvent)) continue;

            GameObject enemy = Instantiate(prefabDung, viTri, Quaternion.identity);
            ApDungDDA(enemy, heSoTocDo, heSoTamNhin);
            daSinh.Add(viTri);
        }
    }

    // Kiem tra spawn co qua gan quai da co hay khong
    bool QuaGanQuaiKhac(Vector3 viTri, List<Vector3> daSinh)
    {
        foreach (var pos in daSinh)
            if (Vector3.Distance(viTri, pos) < khoangCachGiuaQuai)
                return true;
        return false;
    }

    // Kiem tra spawn co qua gan event (checkpoint/minigame/NPC)
    bool QuaGanEvent(Vector3 viTri, List<Vector3> viTriEvent)
    {
        float kcToiThieu = GameSettings.kichThuocO * 2f;
        foreach (var pos in viTriEvent)
            if (Vector3.Distance(viTri, pos) < kcToiThieu)
                return true;
        return false;
    }

    // Ap dung he so DDA vao thong so AI
    void ApDungDDA(GameObject enemy, float heSoTocDo, float heSoTamNhin)
    {
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.tocDoTuanTra  *= heSoTocDo;
            ai.tocDoTruyDuoi *= heSoTocDo;
            ai.tamNhin       *= heSoTamNhin;
            ai.tamNghe       *= heSoTamNhin;
            return;
        }

        var thu = enemy.GetComponent<ThuthuMuAI>();
        if (thu != null)
        {
            thu.tocDoNghe          *= heSoTocDo;
            thu.tocDoTruyDuoi      *= heSoTocDo;
            thu.tamNgheKhiChay     *= heSoTamNhin;
            thu.tamNgheKhiDiThuong *= heSoTamNhin;
            return;
        }

        var bun = enemy.GetComponent<SinhVatBunAI>();
        if (bun != null)
        {
            bun.tocDoTruyDuoi *= heSoTocDo;
            bun.tamAnNap      *= heSoTamNhin;
            bun.tamTruyDuoi   *= heSoTamNhin;
        }
    }

    // Chon prefab theo biome hien tai
    GameObject LayPrefabTheoBiome(int biomeIndex)
    {
        switch (biomeIndex)
        {
            case 1: return prefabThuthuMu;
            case 2: return prefabSinhVatBun;
            default: return prefabEnemyDaCo;
        }
    }
}

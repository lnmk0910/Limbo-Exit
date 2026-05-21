// HeatmapTracker.cs — Theo dõi vị trí người chơi, tạo bản đồ nhiệt cho AI tuần tra
using UnityEngine;
using System.Collections.Generic;

public class HeatmapTracker : MonoBehaviour
{
    public static HeatmapTracker Instance { get; private set; }

    [Header("=== CÀI ĐẶT ===")]
    public float khoangCachGhi = 2f;

    private float kichThuocO;
    private int[,] heatmap;
    private int soCol;
    private int soRow;
    private float demThoiGian = 0f;
    private List<Vector2Int> diemNongCache = new List<Vector2Int>();
    private bool canCapNhatCache = true;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Khoi tao heatmap theo kich thuoc maze
    void Start()
    {
        kichThuocO = GameSettings.kichThuocO;

        MazeGenerator mg = Object.FindFirstObjectByType<MazeGenerator>();
        if (mg != null) { soCol = mg.SoCol; soRow = mg.SoRow; }
        else { soCol = 15; soRow = 15; }

        heatmap = new int[soCol, soRow];
    }

    // Ghi diem nhiet theo chu ky
    void Update()
    {
        demThoiGian += Time.deltaTime;
        if (demThoiGian < khoangCachGhi) return;
        demThoiGian = 0f;

        Vector2Int grid = WorldToGrid(transform.position);
        if (grid.x < 0 || grid.x >= soCol || grid.y < 0 || grid.y >= soRow) return;

        heatmap[grid.x, grid.y]++;
        canCapNhatCache = true;
    }

    // Chuyen toa do world sang grid
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt(worldPos.x / kichThuocO);
        int row = Mathf.RoundToInt(worldPos.z / kichThuocO);
        return new Vector2Int(col, row);
    }

    // Chuyen toa do grid sang world
    public Vector3 GridToWorld(Vector2Int grid, float y = 1f)
    {
        return new Vector3(grid.x * kichThuocO, y, grid.y * kichThuocO);
    }

    // Lay muc nhiet tai o (col,row)
    public int LayNhiet(int col, int row)
    {
        if (heatmap == null) return 0;
        if (col < 0 || col >= soCol || row < 0 || row >= soRow) return 0;
        return heatmap[col, row];
    }

    // Trả về N điểm nóng nhất
    // Lay danh sach diem nong nhieu nhat
    public List<Vector2Int> LayDiemNong(int soLuong = 5)
    {
        if (!canCapNhatCache && diemNongCache.Count > 0)
            return diemNongCache;

        List<KeyValuePair<Vector2Int, int>> ds = new List<KeyValuePair<Vector2Int, int>>();
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                if (heatmap[c, r] > 0)
                    ds.Add(new KeyValuePair<Vector2Int, int>(new Vector2Int(c, r), heatmap[c, r]));

        ds.Sort((a, b) => b.Value.CompareTo(a.Value));

        diemNongCache.Clear();
        for (int i = 0; i < Mathf.Min(soLuong, ds.Count); i++)
            diemNongCache.Add(ds[i].Key);

        canCapNhatCache = false;
        return diemNongCache;
    }

    // Trả về 1 điểm nóng ngẫu nhiên (có trọng số)
    // Chon ngau nhien 1 diem nong theo trong so
    public Vector3? LayDiemNongNgauNhien()
    {
        List<Vector2Int> diemNong = LayDiemNong(10);
        if (diemNong.Count == 0) return null;

        int tongNhiet = 0;
        foreach (var d in diemNong) tongNhiet += heatmap[d.x, d.y];
        if (tongNhiet <= 0) return null;

        int random = Random.Range(0, tongNhiet);
        int tich = 0;
        foreach (var d in diemNong)
        {
            tich += heatmap[d.x, d.y];
            if (tich >= random) return GridToWorld(d);
        }
        return GridToWorld(diemNong[0]);
    }

    // Reset heatmap va cache
    public void ResetHeatmap()
    {
        if (heatmap != null)
            for (int c = 0; c < soCol; c++)
                for (int r = 0; r < soRow; r++)
                    heatmap[c, r] = 0;
        diemNongCache.Clear();
        canCapNhatCache = true;
    }

    // Ve gizmo heatmap de debug
    void OnDrawGizmos()
    {
        if (heatmap == null) return;
        int maxNhiet = 1;
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                if (heatmap[c, r] > maxNhiet) maxNhiet = heatmap[c, r];

        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
            {
                if (heatmap[c, r] <= 0) continue;
                float cuongDo = (float)heatmap[c, r] / maxNhiet;
                Gizmos.color = new Color(1f, 0f, 0f, cuongDo * 0.5f);
                Vector3 pos = new Vector3(c * kichThuocO, 0.5f, r * kichThuocO);
                Gizmos.DrawCube(pos, new Vector3(kichThuocO * 0.8f, 0.1f, kichThuocO * 0.8f));
            }
    }
}

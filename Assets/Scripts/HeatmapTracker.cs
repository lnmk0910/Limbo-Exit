// HeatmapTracker.cs
// AI Adaptive Patrol — Theo doi vi tri nguoi choi va tao ban do nhiet
// Quai vat doc heatmap de uu tien tuan tra o vung nguoi choi hay di qua
// GAN vao: Player GameObject (tu dong boi GameManager hoac gan san)

using UnityEngine;
using System.Collections.Generic;

public class HeatmapTracker : MonoBehaviour
{
    public static HeatmapTracker Instance { get; private set; }

    [Header("=== CAI DAT ===")]
    public float khoangCachGhi = 2f;     // Ghi 1 diem moi 2 giay
    public float kichThuocO    = 6f;     // Phai khop voi GameSettings

    // Ban do nhiet: heatmap[col, row] = so lan nguoi choi di qua
    private int[,] heatmap;
    private int soCol;
    private int soRow;
    private float demThoiGian = 0f;

    // Danh sach cac diem nong nhat (cache)
    private List<Vector2Int> diemNongCache = new List<Vector2Int>();
    private bool canCapNhatCache = true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        // Doc kich thuoc tu MazeGenerator
        MazeGenerator mg = Object.FindFirstObjectByType<MazeGenerator>();
        if (mg != null)
        {
            soCol = mg.SoCol;
            soRow = mg.SoRow;
            kichThuocO = GameSettings.kichThuocO;
        }
        else
        {
            soCol = 15;
            soRow = 15;
        }

        heatmap = new int[soCol, soRow];
        Debug.Log($"[HEATMAP] Khoi tao ban do nhiet {soCol}x{soRow}");
    }

    void Update()
    {
        demThoiGian += Time.deltaTime;
        if (demThoiGian < khoangCachGhi) return;
        demThoiGian = 0f;

        // Chuyen vi tri Player thanh toa do grid
        Vector2Int grid = WorldToGrid(transform.position);
        if (grid.x < 0 || grid.x >= soCol || grid.y < 0 || grid.y >= soRow) return;

        heatmap[grid.x, grid.y]++;
        canCapNhatCache = true;
    }

    // -----------------------------------------------
    // CHUYEN DOI TOA DO
    // -----------------------------------------------
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt(worldPos.x / kichThuocO);
        int row = Mathf.RoundToInt(worldPos.z / kichThuocO);
        return new Vector2Int(col, row);
    }

    public Vector3 GridToWorld(Vector2Int grid, float y = 1f)
    {
        return new Vector3(grid.x * kichThuocO, y, grid.y * kichThuocO);
    }

    // -----------------------------------------------
    // DOC HEATMAP
    // -----------------------------------------------

    // Tra ve gia tri nhiet tai 1 o
    public int LayNhiet(int col, int row)
    {
        if (heatmap == null) return 0;
        if (col < 0 || col >= soCol || row < 0 || row >= soRow) return 0;
        return heatmap[col, row];
    }

    // Tra ve N diem nong nhat (noi nguoi choi hay di qua)
    public List<Vector2Int> LayDiemNong(int soLuong = 5)
    {
        if (!canCapNhatCache && diemNongCache.Count > 0)
            return diemNongCache;

        // Thu thap tat ca diem co nhiet > 0
        List<KeyValuePair<Vector2Int, int>> ds = new List<KeyValuePair<Vector2Int, int>>();
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                if (heatmap[c, r] > 0)
                    ds.Add(new KeyValuePair<Vector2Int, int>(new Vector2Int(c, r), heatmap[c, r]));

        // Sap xep giam dan theo nhiet
        ds.Sort((a, b) => b.Value.CompareTo(a.Value));

        diemNongCache.Clear();
        for (int i = 0; i < Mathf.Min(soLuong, ds.Count); i++)
            diemNongCache.Add(ds[i].Key);

        canCapNhatCache = false;
        return diemNongCache;
    }

    // Tra ve 1 diem nong ngau nhien (de quai tuan tra den)
    public Vector3? LayDiemNongNgauNhien()
    {
        List<Vector2Int> diemNong = LayDiemNong(10);
        if (diemNong.Count == 0) return null;

        // Random co trong so — diem nong hon co xac suat cao hon
        int tongNhiet = 0;
        foreach (var d in diemNong)
            tongNhiet += heatmap[d.x, d.y];

        if (tongNhiet <= 0) return null;

        int random = Random.Range(0, tongNhiet);
        int tich = 0;
        foreach (var d in diemNong)
        {
            tich += heatmap[d.x, d.y];
            if (tich >= random)
                return GridToWorld(d);
        }

        return GridToWorld(diemNong[0]);
    }

    // -----------------------------------------------
    // RESET — goi khi sang tang moi
    // -----------------------------------------------
    public void ResetHeatmap()
    {
        if (heatmap != null)
        {
            for (int c = 0; c < soCol; c++)
                for (int r = 0; r < soRow; r++)
                    heatmap[c, r] = 0;
        }
        diemNongCache.Clear();
        canCapNhatCache = true;
        Debug.Log("[HEATMAP] Da reset ban do nhiet");
    }

    // -----------------------------------------------
    // DEBUG: Ve heatmap trong Scene View
    // -----------------------------------------------
    void OnDrawGizmos()
    {
        if (heatmap == null) return;

        int maxNhiet = 1;
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                if (heatmap[c, r] > maxNhiet)
                    maxNhiet = heatmap[c, r];

        for (int c = 0; c < soCol; c++)
        {
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
}

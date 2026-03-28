// MazeRenderer.cs
// Render mê cung 3D theo kiểu kết cấu của từng Biome:
//   WallStyle.ChuNhatDac  → 1 khối hộp chữ nhật / tường
//   WallStyle.TruTron     → N cột trụ tròn xếp liên tiếp
//   WallStyle.TruLucGiac  → N cột lục giác (Cylinder xoay 30°)
//   GroundStyle.HinhVuong → Cube mỏng
//   GroundStyle.HinhTron  → Cylinder mỏng (hình tròn)
// GẮN vào: cùng GameObject "MazeGenerator"

using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("=== PREFABS CƠ BẢN ===")]
    public GameObject prefabNen;
    public GameObject prefabTuong;

    [Header("=== PREFABS SỰ KIỆN ===")]
    public GameObject prefabCheckpoint;
    public GameObject prefabMinigame;
    public GameObject prefabNPC;

    [Header("=== KÍCH THƯỚC Ô ===")]
    public float kichThuocO = 4f;

    // Cache biome hiện tại
    private BiomeData biome;
    private MazeGenerator mazeGen;

    void Start()
    {
        mazeGen = GetComponent<MazeGenerator>();
        if (mazeGen == null) { Debug.LogError("❌ Không tìm thấy MazeGenerator!"); return; }

        // Lấy biome từ BiomeManager (nếu có)
        BiomeManager bm = GetComponent<BiomeManager>();
        biome = (bm != null) ? bm.BiomeHienTai : null;

        float chieuCao = GameSettings.chieuCaoTuong;
        float doDay    = GameSettings.doDayTuong;
        RenderMeCung(chieuCao, doDay);
    }

    void RenderMeCung(float chieuCao, float doDay)
    {
        int soCol        = mazeGen.SoCol;
        int soRow        = mazeGen.SoRow;
        MazeCell[,] luoi = mazeGen.Luoi;
        int[,] evGrid    = mazeGen.EventGrid;

        for (int c = 0; c < soCol; c++)
        {
            for (int r = 0; r < soRow; r++)
            {
                Vector3 viTriO = new Vector3(c * kichThuocO, 0, r * kichThuocO);

                SpawnNen(viTriO);
                SpawnSuKien(evGrid[c, r], viTriO);

                MazeCell o = luoi[c, r];

                if (o.tuongTren)
                    SpawnTuong(viTriO + new Vector3(0, 0, kichThuocO / 2f),
                               0f, chieuCao, doDay);

                if (o.tuongTrai)
                    SpawnTuong(viTriO + new Vector3(-kichThuocO / 2f, 0, 0),
                               90f, chieuCao, doDay);

                if (r == 0 && o.tuongDuoi)
                    SpawnTuong(viTriO + new Vector3(0, 0, -kichThuocO / 2f),
                               0f, chieuCao, doDay);

                if (c == soCol - 1 && o.tuongPhai)
                    SpawnTuong(viTriO + new Vector3(kichThuocO / 2f, 0, 0),
                               90f, chieuCao, doDay);
            }
        }
        Debug.Log("✅ Render xong mê cung 3D!");
    }

    // -----------------------------------------------
    // SPAWN SÀN (hình dạng theo biome)
    // -----------------------------------------------
    void SpawnNen(Vector3 viTri)
    {
        GameObject go = prefabNen;
        if (biome != null && biome.prefabNen != null) go = biome.prefabNen;
        if (go == null) return;

        GameObject nen = Instantiate(go, viTri, Quaternion.identity);
        nen.name = "Nen";

        GroundStyle style = (biome != null) ? biome.kieuSan : GroundStyle.HinhVuong;
        switch (style)
        {
            case GroundStyle.HinhVuong:
                nen.transform.localScale = new Vector3(kichThuocO, 0.1f, kichThuocO);
                break;

            case GroundStyle.HinhTron:
                // Cylinder Unity có radius=0.5 → scale X,Z = kichThuocO để radius = kichThuocO/2
                nen.transform.localScale = new Vector3(kichThuocO, 0.05f, kichThuocO);
                break;

            case GroundStyle.HinhLucGiac:
                // Xấp xỉ lục giác bằng Cylinder xoay 30° trên trục Y
                nen.transform.localScale   = new Vector3(kichThuocO, 0.05f, kichThuocO);
                nen.transform.eulerAngles  = new Vector3(0, 30f, 0);
                break;
        }
    }

    // -----------------------------------------------
    // SPAWN TƯỜNG (hình dạng theo biome)
    // gocNgang: 0° = tường song song X, 90° = song song Z
    // -----------------------------------------------
    void SpawnTuong(Vector3 viTri, float gocNgang, float chieuCao, float doDay)
    {
        WallStyle style  = (biome != null) ? biome.kieuTuong : WallStyle.ChuNhatDac;
        GameObject go    = prefabTuong;
        if (biome != null && biome.prefabTuong != null) go = biome.prefabTuong;
        if (go == null) return;

        switch (style)
        {
            case WallStyle.ChuNhatDac:
                SpawnTuongChuNhat(go, viTri, gocNgang, chieuCao, doDay);
                break;

            case WallStyle.TruTron:
                SpawnDayCotTru(go, viTri, gocNgang, chieuCao, false);
                break;

            case WallStyle.TruLucGiac:
                SpawnDayCotTru(go, viTri, gocNgang, chieuCao, true);
                break;
        }
    }

    // --- Tường chữ nhật đặc (cũ) ---
    void SpawnTuongChuNhat(GameObject go, Vector3 viTri, float gocNgang,
                            float chieuCao, float doDay)
    {
        viTri.y = chieuCao / 2f;
        GameObject t = Instantiate(go, viTri, Quaternion.Euler(0, gocNgang, 0));
        t.name = "Tuong";
        t.transform.localScale = new Vector3(kichThuocO, chieuCao, doDay);
    }

    // --- Dãy cột trụ xếp liên tiếp (Cylinder) ---
    void SpawnDayCotTru(GameObject go, Vector3 viTri, float gocNgang,
                         float chieuCao, bool xoayLucGiac)
    {
        int soTru    = (biome != null) ? biome.soTruPerWall : 6;
        float duongKinh = (biome != null) ? biome.duongKinhTru : 0.5f;

        // Hướng dọc theo tường (vuông góc với gocNgang)
        Vector3 huong = (gocNgang == 0f)
            ? Vector3.right   // tường song song X → cột xếp theo X
            : Vector3.forward; // tường song song Z → cột xếp theo Z

        // Khoảng cách giữa các cột = kichThuocO / soTru
        float buoc = kichThuocO / soTru;

        // Điểm bắt đầu: dịch từ vị trí tường về -kichThuocO/2
        Vector3 batDau = viTri - huong * (kichThuocO / 2f - buoc / 2f);
        batDau.y = chieuCao / 2f;

        for (int i = 0; i < soTru; i++)
        {
            Vector3 viTriCot = batDau + huong * (i * buoc);
            float gocY = xoayLucGiac ? 30f : 0f; // Lục giác xoay thêm 30°

            GameObject cot = Instantiate(go, viTriCot, Quaternion.Euler(0, gocY, 0));
            cot.name = "Cot";
            // Cylinder Unity: trục Y = chiều cao, X/Z = đường kính
            cot.transform.localScale = new Vector3(duongKinh, chieuCao / 2f, duongKinh);
        }
    }

    // -----------------------------------------------
    // SPAWN SỰ KIỆN
    // -----------------------------------------------
    void SpawnSuKien(int ma, Vector3 viTriO)
    {
        GameObject prefab = null;
        string ten = "";
        switch (ma)
        {
            case 2: prefab = prefabCheckpoint; ten = "Checkpoint"; break;
            case 3: prefab = prefabMinigame;   ten = "Minigame";   break;
            case 4: prefab = prefabNPC;        ten = "NPC";        break;
            default: return;
        }
        if (prefab == null) { Debug.LogWarning($"⚠️ Prefab {ten} chưa gán!"); return; }
        GameObject obj = Instantiate(prefab, viTriO + new Vector3(0, 0.5f, 0), Quaternion.identity);
        obj.name = ten;
    }
}

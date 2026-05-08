// MazeRenderer.cs
// Render me cung 3D theo kieu ket cau cua tung Biome
// Doc kich thuoc tu GameSettings (kichThuocO, chieuCaoTuong, doDayTuong)
// GAN vao: cung GameObject "MazeGenerator"

using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("=== PREFABS CO BAN ===")]
    public GameObject prefabNen;
    public GameObject prefabTuong;

    [Header("=== PREFABS SU KIEN ===")]
    public GameObject prefabCheckpoint;
    public GameObject prefabMinigame;
    public GameObject prefabNPC;

    // Doc tu GameSettings — khong can set trong Inspector
    private float kichThuocO;

    // Cache biome hien tai
    private BiomeData biome;
    private MazeGenerator mazeGen;

    void Start()
    {
        mazeGen = GetComponent<MazeGenerator>();
        if (mazeGen == null) { Debug.LogError("[LOI] Khong tim thay MazeGenerator!"); return; }

        // Doc kich thuoc tu GameSettings (centralized)
        kichThuocO = GameSettings.kichThuocO;

        // Lay biome tu BiomeManager (neu co)
        BiomeManager bm = GetComponent<BiomeManager>();
        biome = (bm != null) ? bm.BiomeHienTai : null;

        float chieuCao = GameSettings.chieuCaoTuong;
        float doDay    = GameSettings.doDayTuong;

        Debug.Log($"[RENDER] O={kichThuocO}m | Tuong cao={chieuCao}m day={doDay}m");
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
        Debug.Log("[OK] Render xong me cung 3D!");
    }

    // -----------------------------------------------
    // SPAWN SAN
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
                // San vuong rong bang kich thuoc o, day 0.2 (du day de nhin ro)
                nen.transform.localScale = new Vector3(kichThuocO, 0.2f, kichThuocO);
                break;

            case GroundStyle.HinhTron:
                nen.transform.localScale = new Vector3(kichThuocO, 0.1f, kichThuocO);
                break;

            case GroundStyle.HinhLucGiac:
                nen.transform.localScale   = new Vector3(kichThuocO, 0.1f, kichThuocO);
                nen.transform.eulerAngles  = new Vector3(0, 30f, 0);
                break;
        }
    }

    // -----------------------------------------------
    // SPAWN TUONG
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

    // --- Tuong chu nhat dac ---
    void SpawnTuongChuNhat(GameObject go, Vector3 viTri, float gocNgang,
                            float chieuCao, float doDay)
    {
        viTri.y = chieuCao / 2f;
        GameObject t = Instantiate(go, viTri, Quaternion.Euler(0, gocNgang, 0));
        t.name = "Tuong";
        t.transform.localScale = new Vector3(kichThuocO, chieuCao, doDay);
    }

    // --- Day cot tru xep lien tiep (Cylinder) ---
    void SpawnDayCotTru(GameObject go, Vector3 viTri, float gocNgang,
                         float chieuCao, bool xoayLucGiac)
    {
        int soTru       = (biome != null) ? biome.soTruPerWall : 8;
        float duongKinh = (biome != null) ? biome.duongKinhTru : 0.7f;

        Vector3 huong = (gocNgang == 0f) ? Vector3.right : Vector3.forward;
        float buoc = kichThuocO / soTru;
        Vector3 batDau = viTri - huong * (kichThuocO / 2f - buoc / 2f);
        batDau.y = chieuCao / 2f;

        for (int i = 0; i < soTru; i++)
        {
            Vector3 viTriCot = batDau + huong * (i * buoc);
            float gocY = xoayLucGiac ? 30f : 0f;

            GameObject cot = Instantiate(go, viTriCot, Quaternion.Euler(0, gocY, 0));
            cot.name = "Cot";
            // Cot day hon: duong kinh tang, chieu cao day du
            cot.transform.localScale = new Vector3(duongKinh, chieuCao / 2f, duongKinh);
        }
    }

    // -----------------------------------------------
    // SPAWN SU KIEN
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
        if (prefab == null) { Debug.LogWarning($"[!] Prefab {ten} chua gan!"); return; }
        GameObject obj = Instantiate(prefab, viTriO + new Vector3(0, 0.5f, 0), Quaternion.identity);
        obj.name = ten;
    }
}

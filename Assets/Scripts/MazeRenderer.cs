// MazeRenderer.cs — Render mê cung 3D theo kiểu kết cấu Biome
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

    private float kichThuocO;
    private BiomeData biome;
    private MazeGenerator mazeGen;

    // Doc maze va biome de render me cung
    void Start()
    {
        mazeGen = GetComponent<MazeGenerator>();
        if (mazeGen == null) return;

        kichThuocO = GameSettings.kichThuocO;

        BiomeManager bm = GetComponent<BiomeManager>();
        biome = (bm != null) ? bm.BiomeHienTai : null;

        RenderMeCung(GameSettings.chieuCaoTuong, GameSettings.doDayTuong);
    }

    // Render nen, tuong va su kien cho toan bo luoi
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
                    SpawnTuong(viTriO + new Vector3(0, 0, kichThuocO / 2f), 0f, chieuCao, doDay);
                if (o.tuongTrai)
                    SpawnTuong(viTriO + new Vector3(-kichThuocO / 2f, 0, 0), 90f, chieuCao, doDay);
                if (r == 0 && o.tuongDuoi)
                    SpawnTuong(viTriO + new Vector3(0, 0, -kichThuocO / 2f), 0f, chieuCao, doDay);
                if (c == soCol - 1 && o.tuongPhai)
                    SpawnTuong(viTriO + new Vector3(kichThuocO / 2f, 0, 0), 90f, chieuCao, doDay);
            }
        }
    }

    // Spawn nen theo biome va style san
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
                nen.transform.localScale = new Vector3(kichThuocO, 0.2f, kichThuocO); break;
            case GroundStyle.HinhTron:
                nen.transform.localScale = new Vector3(kichThuocO, 0.1f, kichThuocO); break;
            case GroundStyle.HinhLucGiac:
                nen.transform.localScale  = new Vector3(kichThuocO, 0.1f, kichThuocO);
                nen.transform.eulerAngles = new Vector3(0, 30f, 0); break;
        }
    }

    // Spawn tuong theo kieu cua biome
    void SpawnTuong(Vector3 viTri, float gocNgang, float chieuCao, float doDay)
    {
        WallStyle style = (biome != null) ? biome.kieuTuong : WallStyle.ChuNhatDac;
        GameObject go   = prefabTuong;
        if (biome != null && biome.prefabTuong != null) go = biome.prefabTuong;
        if (go == null) return;

        switch (style)
        {
            case WallStyle.ChuNhatDac:  SpawnTuongChuNhat(go, viTri, gocNgang, chieuCao, doDay); break;
            case WallStyle.TruTron:     SpawnDayCotTru(go, viTri, gocNgang, chieuCao, false);    break;
            case WallStyle.TruLucGiac:  SpawnDayCotTru(go, viTri, gocNgang, chieuCao, true);     break;
        }
    }

    // Tuong chu nhat dac co do day co dinh
    void SpawnTuongChuNhat(GameObject go, Vector3 viTri, float gocNgang, float chieuCao, float doDay)
    {
        viTri.y = chieuCao / 2f;
        GameObject t = Instantiate(go, viTri, Quaternion.Euler(0, gocNgang, 0));
        t.name = "Tuong";
        t.transform.localScale = new Vector3(kichThuocO, chieuCao, doDay);
    }

    // Tuong dang hang cot tru (tron hoac luc giac)
    void SpawnDayCotTru(GameObject go, Vector3 viTri, float gocNgang, float chieuCao, bool xoayLucGiac)
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
            cot.transform.localScale = new Vector3(duongKinh, chieuCao / 2f, duongKinh);
        }
    }

    // Spawn doi tuong su kien theo ma tren eventGrid
    void SpawnSuKien(int ma, Vector3 viTriO)
    {
        GameObject prefab = null;
        switch (ma)
        {
            case 2: prefab = prefabCheckpoint; break;
            case 3: prefab = prefabMinigame;   break;
            case 4: prefab = prefabNPC;        break;
            default: return;
        }
        if (prefab == null) return;
        Instantiate(prefab, viTriO + new Vector3(0, 0.5f, 0), Quaternion.identity);
    }
}

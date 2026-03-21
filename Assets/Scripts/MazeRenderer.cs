// MazeRenderer.cs
// Đọc dữ liệu từ MazeGenerator và spawn Prefab 3D theo mã eventGrid:
//   1 = Đường đi bình thường (chỉ spawn sàn)
//   2 = Checkpoint → spawn prefabCheckpoint
//   3 = Minigame   → spawn prefabMinigame
//   4 = NPC        → spawn prefabNPC
// GẮN vào: cùng GameObject "MazeGenerator"

using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [Header("=== PREFABS CƠ BẢN ===")]
    public GameObject prefabNen;       // Sàn nền
    public GameObject prefabTuong;     // Tường

    [Header("=== PREFABS SỰ KIỆN ===")]
    public GameObject prefabCheckpoint; // Mã 2 – màu xanh lá
    public GameObject prefabMinigame;   // Mã 3 – màu tím
    public GameObject prefabNPC;        // Mã 4 – màu vàng

    [Header("=== KÍCH THƯỚC Ô ===")]
    public float kichThuocO = 4f;

    private MazeGenerator mazeGen;

    void Start()
    {
        mazeGen = GetComponent<MazeGenerator>();
        if (mazeGen == null)
        {
            Debug.LogError("❌ Không tìm thấy MazeGenerator!");
            return;
        }

        float chieuCao = GameSettings.chieuCaoTuong;
        float doDay    = GameSettings.doDayTuong;

        RenderMeCung(chieuCao, doDay);
    }

    void RenderMeCung(float chieuCao, float doDay)
    {
        int soCol       = mazeGen.SoCol;
        int soRow       = mazeGen.SoRow;
        MazeCell[,] luoi = mazeGen.Luoi;
        int[,] evGrid   = mazeGen.EventGrid;

        for (int c = 0; c < soCol; c++)
        {
            for (int r = 0; r < soRow; r++)
            {
                Vector3 viTriO = new Vector3(c * kichThuocO, 0, r * kichThuocO);

                // --- SPAWN SÀN cho mọi ô ---
                SpawnNen(viTriO);

                // --- SPAWN PREFAB SỰ KIỆN (nếu có) ---
                int maCode = evGrid[c, r];
                SpawnSuKien(maCode, viTriO);

                // --- SPAWN TƯỜNG ---
                MazeCell o = luoi[c, r];

                if (o.tuongTren)
                    SpawnTuong(viTriO + new Vector3(0, 0, kichThuocO / 2f),
                               Quaternion.identity, chieuCao, doDay, kichThuocO);

                if (o.tuongTrai)
                    SpawnTuong(viTriO + new Vector3(-kichThuocO / 2f, 0, 0),
                               Quaternion.Euler(0, 90, 0), chieuCao, doDay, kichThuocO);

                if (r == 0 && o.tuongDuoi)
                    SpawnTuong(viTriO + new Vector3(0, 0, -kichThuocO / 2f),
                               Quaternion.identity, chieuCao, doDay, kichThuocO);

                if (c == soCol - 1 && o.tuongPhai)
                    SpawnTuong(viTriO + new Vector3(kichThuocO / 2f, 0, 0),
                               Quaternion.Euler(0, 90, 0), chieuCao, doDay, kichThuocO);
            }
        }

        Debug.Log("✅ Đã render xong mê cung 3D!");
    }

    // -----------------------------------------------
    // Spawn Prefab sự kiện theo mã số
    // -----------------------------------------------
    void SpawnSuKien(int ma, Vector3 viTriO)
    {
        GameObject prefab = null;
        string tenLoai = "";

        switch (ma)
        {
            case 2: prefab = prefabCheckpoint; tenLoai = "Checkpoint"; break;
            case 3: prefab = prefabMinigame;   tenLoai = "Minigame";   break;
            case 4: prefab = prefabNPC;        tenLoai = "NPC";        break;
            default: return; // Mã 1 = đường thường, không spawn gì
        }

        if (prefab == null)
        {
            Debug.LogWarning($"⚠️ Prefab {tenLoai} chưa được gán trong Inspector!");
            return;
        }

        // Nâng lên một chút so với mặt sàn
        Vector3 viTriSpawn = viTriO + new Vector3(0, 0.5f, 0);
        GameObject obj = Instantiate(prefab, viTriSpawn, Quaternion.identity);
        obj.name = tenLoai;
    }

    void SpawnNen(Vector3 viTri)
    {
        if (prefabNen == null) return;
        GameObject nen = Instantiate(prefabNen, viTri, Quaternion.identity);
        nen.name = "Nen";
        nen.transform.localScale = new Vector3(kichThuocO, 0.1f, kichThuocO);
    }

    void SpawnTuong(Vector3 viTri, Quaternion gocXoay, float chieuCao, float doDay, float chieuDai)
    {
        if (prefabTuong == null) return;
        viTri.y = chieuCao / 2f;
        GameObject tuong = Instantiate(prefabTuong, viTri, gocXoay);
        tuong.name = "Tuong";
        tuong.transform.localScale = new Vector3(chieuDai, chieuCao, doDay);
    }
}

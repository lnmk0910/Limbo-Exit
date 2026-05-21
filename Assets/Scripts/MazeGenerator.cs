// MazeGenerator.cs — Tạo mê cung: Recursive Backtracker (DFS) + Seed + Event Grid + BFS EndPoint
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    private MazeCell[,] luoi;
    private int[,] eventGrid;
    private int soCol;
    private int soRow;

    public int seedHienTai { get; private set; }
    public Vector2Int viTriStart { get; private set; }
    public Vector2Int viTriEnd   { get; private set; }

    public MazeCell[,] Luoi  => luoi;
    public int[,] EventGrid  => eventGrid;
    public int SoCol         => soCol;
    public int SoRow         => soRow;

    [Header("=== CÀI ĐẶT SỰ KIỆN ===")]
    [Range(0f, 1f)] public float tiLeCheckpoint = 0.05f;
    [Range(0f, 1f)] public float tiLeMinigame   = 0.04f;
    [Range(0f, 1f)] public float tiLeNPC        = 0.03f;

    [Header("=== KHOẢNG CÁCH TỐI THIỂU ===")]
    [Range(2, 6)] public int kcCungLoai = 3;
    [Range(1, 4)] public int kcKhacLoai = 2;

    // Danh sách vị trí sự kiện — EnemySpawner đọc để tránh spawn gần
    public List<Vector2Int> daDatViTriSuKien { get; private set; } = new List<Vector2Int>();

    // Tinh kich thuoc map, seed, biome va sinh me cung
    void Awake()
    {
        PlayerData data = SaveSystem.LoadGame();

        // DDA: tính độ khó + kích thước map
        DDAManager.ResetChoTangMoi();
        DDAManager.TinhDiemDoKho();
        int mapBonus = DDAManager.LayMapSizeBonus();

        int scale = 6 + (data.mapHienTai - 1) * 2 + mapBonus;
        int sizeMax = Mathf.Clamp(scale, 4, 15);
        soCol = sizeMax;
        soRow = sizeMax;
        GameSettings.rong = sizeMax;
        GameSettings.dai  = sizeMax;

        // Seed
        if (data.seed == 0)
        {
            seedHienTai = Random.Range(1, int.MaxValue);
            data.seed = seedHienTai;
            SaveSystem.SaveGame(data);
        }
        else seedHienTai = data.seed;

        Random.InitState(seedHienTai);

        // Tráo biome sequence cho lượt chơi mới
        if (data.biomeSequence == null || data.biomeSequence.Length == 0)
        {
            data.biomeSequence = new int[] { 0, 1, 2, 3 };
            System.Random sysRnd = new System.Random();
            for (int i = data.biomeSequence.Length - 1; i > 0; i--)
            {
                int r = sysRnd.Next(0, i + 1);
                (data.biomeSequence[i], data.biomeSequence[r]) = (data.biomeSequence[r], data.biomeSequence[i]);
            }
            SaveSystem.SaveGame(data);
        }

        SinhMeCung();
        DanhDauSuKien();
        DDAManager.GhiNhanBatDauTang();

        // Áp dụng biome trước khi MazeRenderer.Start() chạy
        int indexTrongMang = (data.mapHienTai - 1) % data.biomeSequence.Length;
        int targetBiome = data.biomeSequence[indexTrongMang];
        BiomeManager bm = FindFirstObjectByType<BiomeManager>();
        if (bm != null) bm.ApDungBiome(targetBiome);
    }

    // Sinh luoi me cung bang DFS (Recursive Backtracker)
    public void SinhMeCung()
    {
        luoi = new MazeCell[soCol, soRow];
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                luoi[c, r] = new MazeCell(c, r);

        Stack<MazeCell> stack = new Stack<MazeCell>();
        MazeCell oHienTai = luoi[0, 0];
        oHienTai.daThăm = true;
        stack.Push(oHienTai);

        while (stack.Count > 0)
        {
            oHienTai = stack.Peek();
            List<MazeCell> hangXom = LayHangXomChuaTham(oHienTai);
            if (hangXom.Count > 0)
            {
                MazeCell oKe = hangXom[Random.Range(0, hangXom.Count)];
                PhaVach(oHienTai, oKe);
                oKe.daThăm = true;
                stack.Push(oKe);
            }
            else stack.Pop();
        }

        viTriStart = new Vector2Int(0, 0);
        viTriEnd = TimDiemXaNhat(viTriStart);
    }

    // BFS tìm điểm xa nhất từ Start (theo đường đi thực tế)
    // Tim diem xa nhat theo duong di thuc te
    Vector2Int TimDiemXaNhat(Vector2Int batDau)
    {
        int[,] khoangCach = new int[soCol, soRow];
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                khoangCach[c, r] = -1;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        khoangCach[batDau.x, batDau.y] = 0;
        queue.Enqueue(batDau);

        Vector2Int xaNhat = batDau;
        int maxDist = 0;

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            MazeCell cell = luoi[cur.x, cur.y];
            int dist = khoangCach[cur.x, cur.y];

            if (dist >= maxDist) { maxDist = dist; xaNhat = cur; }

            if (!cell.tuongTren && cur.y + 1 < soRow && khoangCach[cur.x, cur.y + 1] == -1)
            { khoangCach[cur.x, cur.y + 1] = dist + 1; queue.Enqueue(new Vector2Int(cur.x, cur.y + 1)); }
            if (!cell.tuongDuoi && cur.y - 1 >= 0 && khoangCach[cur.x, cur.y - 1] == -1)
            { khoangCach[cur.x, cur.y - 1] = dist + 1; queue.Enqueue(new Vector2Int(cur.x, cur.y - 1)); }
            if (!cell.tuongTrai && cur.x - 1 >= 0 && khoangCach[cur.x - 1, cur.y] == -1)
            { khoangCach[cur.x - 1, cur.y] = dist + 1; queue.Enqueue(new Vector2Int(cur.x - 1, cur.y)); }
            if (!cell.tuongPhai && cur.x + 1 < soCol && khoangCach[cur.x + 1, cur.y] == -1)
            { khoangCach[cur.x + 1, cur.y] = dist + 1; queue.Enqueue(new Vector2Int(cur.x + 1, cur.y)); }
        }
        return xaNhat;
    }

    // Đánh dấu sự kiện: Round-robin Checkpoint → Minigame → NPC
    // Dat vi tri su kien vao eventGrid theo ty le va khoang cach
    void DanhDauSuKien()
    {
        eventGrid = new int[soCol, soRow];
        daDatViTriSuKien = new List<Vector2Int>();

        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                eventGrid[c, r] = 1;

        List<Vector2Int> oTrong = new List<Vector2Int>();
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
            {
                Vector2Int viTri = new Vector2Int(c, r);
                if (viTri == viTriStart || viTri == viTriEnd) continue;
                oTrong.Add(viTri);
            }

        for (int i = oTrong.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (oTrong[i], oTrong[j]) = (oTrong[j], oTrong[i]);
        }

        int tong = oTrong.Count;
        int soCheckpoint = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeCheckpoint()));
        int soMinigame   = Mathf.Max(1, Mathf.FloorToInt(tong * tiLeMinigame));
        int soNPC        = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeNPC()));

        int kcCL = (soCol <= 6) ? Mathf.Max(2, kcCungLoai - 1) : kcCungLoai;
        int kcKL = (soCol <= 6) ? Mathf.Max(1, kcKhacLoai - 1) : kcKhacLoai;

        List<int> daDatMa = new List<int>();
        int[] conLai = { soCheckpoint, soMinigame, soNPC };
        int[] maSK   = { 2, 3, 4 };

        int viTriDuyet = 0, loaiHienTai = 0, soLanThatBai = 0;
        int tongCanDat = soCheckpoint + soMinigame + soNPC;
        int tongDaDat = 0;

        while (tongDaDat < tongCanDat && soLanThatBai < oTrong.Count * 3)
        {
            if (conLai[loaiHienTai] <= 0) { loaiHienTai = (loaiHienTai + 1) % 3; continue; }

            bool daDatDuoc = false;
            int batDauIdx = viTriDuyet;
            for (int attempt = 0; attempt < oTrong.Count; attempt++)
            {
                int idx = (batDauIdx + attempt) % oTrong.Count;
                Vector2Int viTri = oTrong[idx];
                if (eventGrid[viTri.x, viTri.y] != 1) continue;
                if (QuaGan(viTri, daDatViTriSuKien, daDatMa, maSK[loaiHienTai], kcCL, kcKL)) continue;

                eventGrid[viTri.x, viTri.y] = maSK[loaiHienTai];
                daDatViTriSuKien.Add(viTri);
                daDatMa.Add(maSK[loaiHienTai]);
                conLai[loaiHienTai]--;
                tongDaDat++;
                viTriDuyet = (idx + 1) % oTrong.Count;
                daDatDuoc = true;
                break;
            }

            soLanThatBai = daDatDuoc ? 0 : soLanThatBai + 1;
            loaiHienTai = (loaiHienTai + 1) % 3;
        }
    }

    // Kiem tra vi tri moi co qua gan su kien da dat hay khong
    bool QuaGan(Vector2Int viTri, List<Vector2Int> daDatViTri, List<int> daDatMa,
                int maSuKienMoi, int kcCL, int kcKL)
    {
        for (int i = 0; i < daDatViTri.Count; i++)
        {
            int manhattan = Mathf.Abs(viTri.x - daDatViTri[i].x) + Mathf.Abs(viTri.y - daDatViTri[i].y);
            int kcYeuCau = (daDatMa[i] == maSuKienMoi) ? kcCL : kcKL;
            if (manhattan < kcYeuCau) return true;
        }
        return false;
    }

    // Lay danh sach o chua tham ke ben o hien tai
    private List<MazeCell> LayHangXomChuaTham(MazeCell o)
    {
        List<MazeCell> ds = new List<MazeCell>();
        int c = o.col, r = o.row;
        if (r + 1 < soRow && !luoi[c, r+1].daThăm) ds.Add(luoi[c, r+1]);
        if (r - 1 >= 0    && !luoi[c, r-1].daThăm) ds.Add(luoi[c, r-1]);
        if (c - 1 >= 0    && !luoi[c-1, r].daThăm) ds.Add(luoi[c-1, r]);
        if (c + 1 < soCol && !luoi[c+1, r].daThăm) ds.Add(luoi[c+1, r]);
        return ds;
    }

    // Pha vach giua 2 o ke nhau
    private void PhaVach(MazeCell a, MazeCell b)
    {
        int dc = b.col - a.col, dr = b.row - a.row;
        if (dr ==  1) { a.tuongTren = false; b.tuongDuoi = false; }
        if (dr == -1) { a.tuongDuoi = false; b.tuongTren = false; }
        if (dc == -1) { a.tuongTrai = false; b.tuongPhai = false; }
        if (dc ==  1) { a.tuongPhai = false; b.tuongTrai = false; }
    }
}

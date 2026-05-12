// MazeGenerator.cs
// Thuật toán tạo mê cung: RECURSIVE BACKTRACKER (DFS) + SEED + EVENT GRID
//
// Mảng eventGrid[col, row] chứa mã số từng ô:
//   0 = Tường (Wall)
//   1 = Đường đi bình thường
//   2 = Checkpoint (hồi máu / điểm lưu)
//   3 = Minigame (thử thách nhỏ)
//   4 = NPC Thương nhân (mua/bán)
//
// PHIÊN BẢN MỚI:
//   - ExitGate đặt tại điểm XA NHẤT theo đường đi (BFS) thay vì góc cố định
//   - Sự kiện (Checkpoint/NPC/Minigame) phải cách nhau tối thiểu 2 ô
//   - Sự kiện phân bố đều trên bản đồ (chia vùng)
//
// GẮN vào: Empty GameObject tên "MazeGenerator" trong GameScene

using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // -----------------------------------------------
    // KHAI BÁO
    // -----------------------------------------------
    private MazeCell[,] luoi;       // Lưới 2D tường/lối đi
    private int[,] eventGrid;       // Lưới mã số sự kiện
    private int soCol;
    private int soRow;

    // Seed hiện tại (lưu lại để truyền vào SaveSystem)
    public int seedHienTai { get; private set; }

    // Vị trí Start và End
    public Vector2Int viTriStart { get; private set; }
    public Vector2Int viTriEnd   { get; private set; }

    // Properties cho MazeRenderer
    public MazeCell[,] Luoi      => luoi;
    public int[,] EventGrid      => eventGrid;
    public int SoCol             => soCol;
    public int SoRow             => soRow;

    [Header("=== CÀI ĐẶT SỰ KIỆN ===")]
    [Range(0f, 1f)]
    public float tiLeCheckpoint = 0.05f;  // 5% ô đường đi là Checkpoint
    [Range(0f, 1f)]
    public float tiLeMinigame   = 0.04f;  // 4% là Minigame
    [Range(0f, 1f)]
    public float tiLeNPC        = 0.03f;  // 3% là NPC

    [Header("=== KHOẢNG CÁCH TỐI THIỂU GIỮA CÁC SỰ KIỆN ===")]
    [Tooltip("Số ô tối thiểu giữa 2 sự kiện cùng loại (Manhattan distance)")]
    [Range(1, 5)]
    public int khoangCachToiThieu = 2;

    // -----------------------------------------------
    // AWAKE: chạy TRƯỚC tất cả Start() — sinh mê cung ngay
    // -----------------------------------------------
    void Awake()
    {
        PlayerData data = SaveSystem.LoadGame();

        // === DDA: Tinh diem do kho truoc ===
        DDAManager.ResetChoTangMoi();
        float diemDoKho = DDAManager.TinhDiemDoKho();
        int mapBonus = DDAManager.LayMapSizeBonus();

        // Map: base 6x6, moi tang +2, DDA dieu chinh them. Max 15, Min 4.
        int scale = 6 + (data.mapHienTai - 1) * 2 + mapBonus;
        int sizeMax = Mathf.Clamp(scale, 4, 15);
        soCol = sizeMax;
        soRow = sizeMax;

        GameSettings.rong = sizeMax;
        GameSettings.dai = sizeMax;

        Debug.Log($"[DDA] Map size: {soCol}x{soRow} (base + DDA bonus {mapBonus:+0;-0})");

        // Lấy seed từ save (nếu seed = 0 thì sinh ngẫu nhiên)
        if (data.seed == 0)
        {
            seedHienTai = Random.Range(1, int.MaxValue);
            data.seed = seedHienTai;
            SaveSystem.SaveGame(data); // Lưu seed mới vào file
        }
        else
        {
            seedHienTai = data.seed;
        }

        Debug.Log($"[SEED] Seed: {seedHienTai} - Size: {soCol}x{soRow}");

        // Khởi tạo Random với seed → đảm bảo map sinh ra giống nhau
        Random.InitState(seedHienTai);

        // [!]️ NARRATIVE AI: TRÁO BÀI LỘ TRÌNH 4 BIOME
        // Chỉ tráo khi biomeSequence chưa có → tức là lượt chơi mới hoàn toàn
        if (data.biomeSequence == null || data.biomeSequence.Length == 0)
        {
            data.biomeSequence = new int[] { 0, 1, 2, 3 };

            // Dùng System.Random riêng, không bị InitState seed ảnh hưởng
            System.Random sysRnd = new System.Random();
            for (int i = data.biomeSequence.Length - 1; i > 0; i--)
            {
                int r = sysRnd.Next(0, i + 1);
                int tmp = data.biomeSequence[i];
                data.biomeSequence[i] = data.biomeSequence[r];
                data.biomeSequence[r] = tmp;
            }
            SaveSystem.SaveGame(data);
            Debug.Log($"[RANDOM] Lộ trình Biome: {data.biomeSequence[0]}→{data.biomeSequence[1]}→{data.biomeSequence[2]}→{data.biomeSequence[3]}");
        }

        SinhMeCung();

        // TÌM ĐIỂM XA NHẤT TỪ START bằng BFS → đặt ExitGate
        viTriEnd = TimDiemXaNhat(viTriStart);
        Debug.Log($"[EXIT] ExitGate đặt tại {viTriEnd} (xa nhất từ Start {viTriStart})");

        DanhDauSuKien();

        // === DDA: Ghi nhan bat dau tang moi ===
        DDAManager.GhiNhanBatDauTang();

        // Lấy đúng Biome dựa theo Tầng (Logic: mapHienTai - 1 % 4 để xoay vòng)
        int indexTrongMang = (data.mapHienTai - 1) % data.biomeSequence.Length;
        int targetBiome = data.biomeSequence[indexTrongMang];

        // [!]️ GIẢI QUYẾT LỖ HỔNG RACE CONDITION BIOME:
        // Gọi thẳng BiomeManager ngay lập tức để nó cập nhật Prefab Tường/Sàn 
        // TRƯỚC KHI MazeRenderer.Start() kịp vẽ ra!
        BiomeManager bm = FindFirstObjectByType<BiomeManager>();
        if (bm != null)
        {
            bm.ApDungBiome(targetBiome);
        }
    }

    // -----------------------------------------------
    // SINH MÊ CUNG (Recursive Backtracker DFS)
    // -----------------------------------------------
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
            else
            {
                stack.Pop();
            }
        }

        viTriStart = new Vector2Int(0, 0);
        // viTriEnd sẽ được tính bằng BFS sau → điểm xa nhất theo đường đi
        viTriEnd = new Vector2Int(soCol - 1, soRow - 1); // Tạm, sẽ bị ghi đè

        Debug.Log($"[OK] Mê cung {soCol}x{soRow} đã sinh xong! Start:{viTriStart}");
    }

    // -----------------------------------------------
    // TÌM ĐIỂM XA NHẤT TỪ START (BFS trên mê cung)
    // Duyệt qua đường đi thực tế, KHÔNG tính đường thẳng Euclid
    // -----------------------------------------------
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

            // Kiểm tra 4 hướng — chỉ đi qua nếu KHÔNG CÓ TƯỜNG
            // Trên (row+1)
            if (!cell.tuongTren && cur.y + 1 < soRow && khoangCach[cur.x, cur.y + 1] == -1)
            {
                khoangCach[cur.x, cur.y + 1] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x, cur.y + 1));
            }
            // Dưới (row-1)
            if (!cell.tuongDuoi && cur.y - 1 >= 0 && khoangCach[cur.x, cur.y - 1] == -1)
            {
                khoangCach[cur.x, cur.y - 1] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x, cur.y - 1));
            }
            // Trái (col-1)
            if (!cell.tuongTrai && cur.x - 1 >= 0 && khoangCach[cur.x - 1, cur.y] == -1)
            {
                khoangCach[cur.x - 1, cur.y] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x - 1, cur.y));
            }
            // Phải (col+1)
            if (!cell.tuongPhai && cur.x + 1 < soCol && khoangCach[cur.x + 1, cur.y] == -1)
            {
                khoangCach[cur.x + 1, cur.y] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x + 1, cur.y));
            }

            // Cập nhật ô xa nhất
            if (dist > maxDist)
            {
                maxDist = dist;
                xaNhat = cur;
            }
        }

        Debug.Log($"[BFS] Điểm xa nhất từ {batDau}: {xaNhat} (khoảng cách = {maxDist} bước)");
        return xaNhat;
    }

    // -----------------------------------------------
    // ĐÁNH DẤU SỰ KIỆN vào eventGrid
    // Phiên bản mới: kiểm tra khoảng cách tối thiểu giữa các sự kiện
    // -----------------------------------------------
    void DanhDauSuKien()
    {
        eventGrid = new int[soCol, soRow];

        // Bước 1: Mặc định tất cả = 1 (đường đi)
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                eventGrid[c, r] = 1;

        // Bước 2: Thu thập danh sách ô hợp lệ (không phải Start/End)
        List<Vector2Int> oTrong = new List<Vector2Int>();
        for (int c = 0; c < soCol; c++)
        {
            for (int r = 0; r < soRow; r++)
            {
                Vector2Int viTri = new Vector2Int(c, r);
                if (viTri == viTriStart || viTri == viTriEnd) continue;
                oTrong.Add(viTri);
            }
        }

        // Bước 3: Xáo trộn danh sách (dùng seed đã set)
        for (int i = oTrong.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (oTrong[i], oTrong[j]) = (oTrong[j], oTrong[i]);
        }

        // Bước 4: Tính số lượng mỗi loại sự kiện
        int tong = oTrong.Count;
        int soCheckpoint = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeCheckpoint()));
        int soMinigame   = Mathf.Max(1, Mathf.FloorToInt(tong * tiLeMinigame));
        int soNPC        = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeNPC()));

        // Danh sách các ô đã đặt sự kiện — dùng để kiểm tra khoảng cách
        List<Vector2Int> daDat = new List<Vector2Int>();

        // Bước 5: Đặt từng loại sự kiện với kiểm tra khoảng cách
        DatSuKienVoiKhoangCach(oTrong, daDat, 2, soCheckpoint, "Checkpoint");
        DatSuKienVoiKhoangCach(oTrong, daDat, 3, soMinigame,   "Minigame");
        DatSuKienVoiKhoangCach(oTrong, daDat, 4, soNPC,        "NPC");
    }

    // -----------------------------------------------
    // ĐẶT SỰ KIỆN VỚI KIỂM TRA KHOẢNG CÁCH TỐI THIỂU
    // Tránh 2 sự kiện nằm quá gần nhau (Manhattan distance)
    // -----------------------------------------------
    void DatSuKienVoiKhoangCach(List<Vector2Int> danhSachO, List<Vector2Int> daDat,
                                 int maSuKien, int soLuong, string tenSuKien)
    {
        int daDatSuKien = 0;

        for (int i = 0; i < danhSachO.Count && daDatSuKien < soLuong; i++)
        {
            Vector2Int viTri = danhSachO[i];

            // Bỏ qua ô đã được đặt sự kiện
            if (eventGrid[viTri.x, viTri.y] != 1) continue;

            // Kiểm tra khoảng cách với TẤT CẢ sự kiện đã đặt
            if (QuaGan(viTri, daDat))
                continue;

            // Đặt sự kiện
            eventGrid[viTri.x, viTri.y] = maSuKien;
            daDat.Add(viTri);
            daDatSuKien++;
            Debug.Log($"[EVENT] {tenSuKien} #{daDatSuKien} tại ({viTri.x},{viTri.y})");
        }

        if (daDatSuKien < soLuong)
            Debug.LogWarning($"[!] Chỉ đặt được {daDatSuKien}/{soLuong} {tenSuKien} (map quá nhỏ hoặc khoảng cách tối thiểu quá lớn)");
    }

    // Kiểm tra ô có quá gần sự kiện đã đặt không (Manhattan distance)
    bool QuaGan(Vector2Int viTri, List<Vector2Int> daDat)
    {
        foreach (var d in daDat)
        {
            int manhattan = Mathf.Abs(viTri.x - d.x) + Mathf.Abs(viTri.y - d.y);
            if (manhattan < khoangCachToiThieu)
                return true;
        }
        return false;
    }

    // -----------------------------------------------
    // HÀM PHỤ: Lấy hàng xóm chưa thăm
    // -----------------------------------------------
    private List<MazeCell> LayHangXomChuaTham(MazeCell o)
    {
        List<MazeCell> ds = new List<MazeCell>();
        int c = o.col, r = o.row;
        if (r + 1 < soRow && !luoi[c, r+1].daThăm) ds.Add(luoi[c, r+1]);
        if (r - 1 >= 0   && !luoi[c, r-1].daThăm) ds.Add(luoi[c, r-1]);
        if (c - 1 >= 0   && !luoi[c-1, r].daThăm) ds.Add(luoi[c-1, r]);
        if (c + 1 < soCol && !luoi[c+1, r].daThăm) ds.Add(luoi[c+1, r]);
        return ds;
    }

    // -----------------------------------------------
    // HÀM PHỤ: Phá tường giữa 2 ô
    // -----------------------------------------------
    private void PhaVach(MazeCell a, MazeCell b)
    {
        int dc = b.col - a.col, dr = b.row - a.row;
        if (dr ==  1) { a.tuongTren = false; b.tuongDuoi = false; }
        if (dr == -1) { a.tuongDuoi = false; b.tuongTren = false; }
        if (dc == -1) { a.tuongTrai = false; b.tuongPhai = false; }
        if (dc ==  1) { a.tuongPhai = false; b.tuongTrai = false; }
    }
}

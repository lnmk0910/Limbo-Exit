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
    [Tooltip("Số ô tối thiểu giữa 2 sự kiện CÙNG LOẠI (VD: 2 NPC, 2 Checkpoint)")]
    [Range(2, 6)]
    public int kcCungLoai = 3;

    [Tooltip("Số ô tối thiểu giữa 2 sự kiện KHÁC LOẠI (VD: NPC gần Checkpoint)")]
    [Range(1, 4)]
    public int kcKhacLoai = 2;

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
        // viTriEnd đã được BFS tính trong SinhMeCung()

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

        // BFS tìm điểm xa nhất NGAY SAU KHI sinh mê cung (không để tạm)
        viTriEnd = TimDiemXaNhat(viTriStart);

        float kichThuocO = GameSettings.kichThuocO;
        Debug.Log($"[OK] Mê cung {soCol}x{soRow} sinh xong! Start:{viTriStart} End:{viTriEnd}");
        Debug.Log($"[EXIT] ExitGate world pos: ({viTriEnd.x * kichThuocO}, {viTriEnd.y * kichThuocO}) — xa nhất từ Start");
    }

    // -----------------------------------------------
    // TÌM ĐIỂM XA NHẤT TỪ START (BFS trên mê cung)
    // Duyệt qua đường đi thực tế, KHÔNG tính đường thẳng Euclid
    // QUAN TRỌNG: Dùng mảng khoangCach riêng, KHÔNG dùng daThăm
    //             (vì daThăm đã bị DFS set true toàn bộ)
    // -----------------------------------------------
    Vector2Int TimDiemXaNhat(Vector2Int batDau)
    {
        // Mảng khoảng cách: -1 = chưa thăm, 0+ = đã thăm
        int[,] khoangCach = new int[soCol, soRow];
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                khoangCach[c, r] = -1;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        khoangCach[batDau.x, batDau.y] = 0;
        queue.Enqueue(batDau);

        Vector2Int xaNhat = batDau;
        int maxDist = 0;
        int tongODaTham = 0;

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            MazeCell cell = luoi[cur.x, cur.y];
            int dist = khoangCach[cur.x, cur.y];
            tongODaTham++;

            // Cập nhật ô xa nhất (>= để bắt ô cuối cùng nếu cùng khoảng cách)
            if (dist >= maxDist)
            {
                maxDist = dist;
                xaNhat = cur;
            }

            // Kiểm tra 4 hướng — chỉ đi qua nếu KHÔNG CÓ TƯỜNG
            // Trên (row+1): cell.tuongTren = false → có lối đi lên
            if (!cell.tuongTren && cur.y + 1 < soRow && khoangCach[cur.x, cur.y + 1] == -1)
            {
                khoangCach[cur.x, cur.y + 1] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x, cur.y + 1));
            }
            // Dưới (row-1): cell.tuongDuoi = false → có lối đi xuống
            if (!cell.tuongDuoi && cur.y - 1 >= 0 && khoangCach[cur.x, cur.y - 1] == -1)
            {
                khoangCach[cur.x, cur.y - 1] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x, cur.y - 1));
            }
            // Trái (col-1): cell.tuongTrai = false → có lối đi sang trái
            if (!cell.tuongTrai && cur.x - 1 >= 0 && khoangCach[cur.x - 1, cur.y] == -1)
            {
                khoangCach[cur.x - 1, cur.y] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x - 1, cur.y));
            }
            // Phải (col+1): cell.tuongPhai = false → có lối đi sang phải
            if (!cell.tuongPhai && cur.x + 1 < soCol && khoangCach[cur.x + 1, cur.y] == -1)
            {
                khoangCach[cur.x + 1, cur.y] = dist + 1;
                queue.Enqueue(new Vector2Int(cur.x + 1, cur.y));
            }
        }

        Debug.Log($"[BFS] Duyệt {tongODaTham}/{soCol * soRow} ô | Xa nhất: {xaNhat} | Khoảng cách: {maxDist} bước | Start: {batDau}");
        return xaNhat;
    }

    // Danh sach vi tri su kien da dat — EnemySpawner doc de tranh spawn gan
    public List<Vector2Int> daDatViTriSuKien { get; private set; } = new List<Vector2Int>();

    // -----------------------------------------------
    // ĐÁNH DẤU SỰ KIỆN vào eventGrid
    // ROUND-ROBIN: Checkpoint → Minigame → NPC → Checkpoint → ...
    // Đảm bảo các loại xen kẽ, phân bố đều trên map
    // -----------------------------------------------
    void DanhDauSuKien()
    {
        eventGrid = new int[soCol, soRow];
        daDatViTriSuKien = new List<Vector2Int>();

        // Bước 1: Mặc định tất cả = 1 (đường đi)
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                eventGrid[c, r] = 1;

        // Bước 2: Thu thập ô hợp lệ
        List<Vector2Int> oTrong = new List<Vector2Int>();
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
            {
                Vector2Int viTri = new Vector2Int(c, r);
                if (viTri == viTriStart || viTri == viTriEnd) continue;
                oTrong.Add(viTri);
            }

        // Bước 3: Xáo trộn (dùng seed)
        for (int i = oTrong.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (oTrong[i], oTrong[j]) = (oTrong[j], oTrong[i]);
        }

        // Bước 4: Tính số lượng
        int tong = oTrong.Count;
        int soCheckpoint = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeCheckpoint()));
        int soMinigame   = Mathf.Max(1, Mathf.FloorToInt(tong * tiLeMinigame));
        int soNPC        = Mathf.Max(1, Mathf.FloorToInt(tong * DDAManager.LayTiLeNPC()));

        // Tự động giảm khoảng cách nếu map nhỏ (≤6 ô)
        int kcCL = (soCol <= 6) ? Mathf.Max(2, kcCungLoai - 1) : kcCungLoai;
        int kcKL = (soCol <= 6) ? Mathf.Max(1, kcKhacLoai - 1) : kcKhacLoai;

        Debug.Log($"[SPACING] Map {soCol}x{soRow} | CK:{soCheckpoint} MG:{soMinigame} NPC:{soNPC} | " +
                  $"cùng≥{kcCL} khác≥{kcKL}");

        // Bước 5: ROUND-ROBIN — xen kẽ từng loại
        // Tạo hàng đợi: (mã sự kiện, số còn lại, tên)
        List<int> daDatMa = new List<int>();
        int[] conLai = { soCheckpoint, soMinigame, soNPC };
        int[] maSK   = { 2, 3, 4 };
        string[] tenSK = { "Checkpoint", "Minigame", "NPC" };

        int viTriDuyet = 0;     // Con trỏ duyệt oTrong
        int loaiHienTai = 0;    // 0=CK, 1=MG, 2=NPC
        int soLanThatBai = 0;   // Đếm thất bại liên tiếp để tránh vòng lặp vô hạn
        int tongCanDat = soCheckpoint + soMinigame + soNPC;
        int tongDaDat = 0;

        while (tongDaDat < tongCanDat && soLanThatBai < oTrong.Count * 3)
        {
            // Bỏ qua loại đã đặt đủ
            if (conLai[loaiHienTai] <= 0)
            {
                loaiHienTai = (loaiHienTai + 1) % 3;
                continue;
            }

            // Tìm ô phù hợp cho loại hiện tại
            bool daDatDuoc = false;
            int batDau = viTriDuyet;

            for (int attempt = 0; attempt < oTrong.Count; attempt++)
            {
                int idx = (batDau + attempt) % oTrong.Count;
                Vector2Int viTri = oTrong[idx];

                // Ô đã dùng
                if (eventGrid[viTri.x, viTri.y] != 1) continue;

                // Kiểm tra khoảng cách
                if (QuaGan(viTri, daDatViTriSuKien, daDatMa, maSK[loaiHienTai], kcCL, kcKL))
                    continue;

                // Đặt sự kiện
                eventGrid[viTri.x, viTri.y] = maSK[loaiHienTai];
                daDatViTriSuKien.Add(viTri);
                daDatMa.Add(maSK[loaiHienTai]);
                conLai[loaiHienTai]--;
                tongDaDat++;
                viTriDuyet = (idx + 1) % oTrong.Count;
                daDatDuoc = true;

                Debug.Log($"[EVENT] {tenSK[loaiHienTai]} #{soCheckpoint + soMinigame + soNPC - tongCanDat + tongDaDat} tại ({viTri.x},{viTri.y})");
                break;
            }

            if (!daDatDuoc)
                soLanThatBai++;
            else
                soLanThatBai = 0;

            // Chuyển sang loại tiếp theo (round-robin)
            loaiHienTai = (loaiHienTai + 1) % 3;
        }

        // Log kết quả
        Debug.Log($"[EVENT] Tổng đặt: {tongDaDat}/{tongCanDat} " +
                  $"(CK:{soCheckpoint - conLai[0]} MG:{soMinigame - conLai[1]} NPC:{soNPC - conLai[2]})");

        if (conLai[0] > 0 || conLai[1] > 0 || conLai[2] > 0)
            Debug.LogWarning($"[!] Thiếu: CK:{conLai[0]} MG:{conLai[1]} NPC:{conLai[2]} (map nhỏ hoặc khoảng cách lớn)");
    }

    // Kiểm tra khoảng cách — cùng loại nghiêm ngặt hơn khác loại
    bool QuaGan(Vector2Int viTri, List<Vector2Int> daDatViTri, List<int> daDatMa,
                int maSuKienMoi, int kcCL, int kcKL)
    {
        for (int i = 0; i < daDatViTri.Count; i++)
        {
            int manhattan = Mathf.Abs(viTri.x - daDatViTri[i].x) + Mathf.Abs(viTri.y - daDatViTri[i].y);
            int kcYeuCau = (daDatMa[i] == maSuKienMoi) ? kcCL : kcKL;

            if (manhattan < kcYeuCau)
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

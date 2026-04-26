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

    // -----------------------------------------------
    // AWAKE: chạy TRƯỚC tất cả Start() — sinh mê cung ngay
    // -----------------------------------------------
    void Awake()
    {
        PlayerData data = SaveSystem.LoadGame();

        // CHỐNG NGỘP: Map bắt đầu từ 6x6, mỗi tầng tăng 2 ô. Max 15x15.
        int scale = 6 + (data.mapHienTai - 1) * 2;
        int sizeMax = Mathf.Min(scale, 15);
        soCol = sizeMax;
        soRow = sizeMax;

        // Cập nhật ngầm để các script khác đọc nếu cần
        GameSettings.rong = sizeMax;
        GameSettings.dai = sizeMax;

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

        Debug.Log($"🌱 Seed: {seedHienTai} - Size: {soCol}x{soRow}");

        // Khởi tạo Random với seed → đảm bảo map sinh ra giống nhau
        Random.InitState(seedHienTai);

        // ⚠️ NARRATIVE AI: TRÁO BÀI LỘ TRÌNH 4 BIOME
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
            Debug.Log($"🎲 Lộ trình Biome: {data.biomeSequence[0]}→{data.biomeSequence[1]}→{data.biomeSequence[2]}→{data.biomeSequence[3]}");
        }

        SinhMeCung();
        DanhDauSuKien();

        // Lấy đúng Biome dựa theo Tầng (Logic: mapHienTai - 1 % 4 để xoay vòng)
        int indexTrongMang = (data.mapHienTai - 1) % data.biomeSequence.Length;
        int targetBiome = data.biomeSequence[indexTrongMang];

        // ⚠️ GIẢI QUYẾT LỖ HỔNG RACE CONDITION BIOME:
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
        viTriEnd   = new Vector2Int(soCol - 1, soRow - 1);
        luoi[0, 0].tuongTrai = false;
        luoi[soCol - 1, soRow - 1].tuongPhai = false;

        Debug.Log($"✅ Mê cung {soCol}x{soRow} đã sinh xong! Start:{viTriStart} → End:{viTriEnd}");
    }

    // -----------------------------------------------
    // ĐÁNH DẤU SỰ KIỆN vào eventGrid
    // -----------------------------------------------
    void DanhDauSuKien()
    {
        eventGrid = new int[soCol, soRow];

        // Bước 1: Mặc định tất cả = 1 (đường đi)
        // Ô không có lối đi nào thực ra không cần đặt sự kiện
        // nhưng ta dùng eventGrid chủ yếu để MazeRenderer đọc
        for (int c = 0; c < soCol; c++)
            for (int r = 0; r < soRow; r++)
                eventGrid[c, r] = 1; // đường đi bình thường

        // Bước 2: Thu thập danh sách ô đường đi (không phải Start/End)
        List<Vector2Int> oTrong = new List<Vector2Int>();
        for (int c = 0; c < soCol; c++)
        {
            for (int r = 0; r < soRow; r++)
            {
                Vector2Int viTri = new Vector2Int(c, r);
                // Bỏ qua Start và End
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

        // Bước 4: Đánh dấu sự kiện theo tỉ lệ
        int tong = oTrong.Count;
        int soCheckpoint = Mathf.FloorToInt(tong * tiLeCheckpoint);
        int soMinigame   = Mathf.FloorToInt(tong * tiLeMinigame);
        int soNPC        = Mathf.FloorToInt(tong * tiLeNPC);

        int idx = 0;

        // Checkpoint (mã 2)
        for (int i = 0; i < soCheckpoint && idx < tong; i++, idx++)
        {
            eventGrid[oTrong[idx].x, oTrong[idx].y] = 2;
            Debug.Log($"📍 Checkpoint tại {oTrong[idx]}");
        }

        // Minigame (mã 3)
        for (int i = 0; i < soMinigame && idx < tong; i++, idx++)
        {
            eventGrid[oTrong[idx].x, oTrong[idx].y] = 3;
            Debug.Log($"🎮 Minigame tại {oTrong[idx]}");
        }

        // NPC Thương nhân (mã 4)
        for (int i = 0; i < soNPC && idx < tong; i++, idx++)
        {
            eventGrid[oTrong[idx].x, oTrong[idx].y] = 4;
            Debug.Log($"🧑‍💼 NPC tại {oTrong[idx]}");
        }
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

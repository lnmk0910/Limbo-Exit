// BiomeData.cs
// ScriptableObject cấu hình visual + gameplay + hình dạng kết cấu của 1 Biome
// Tạo: Project → click phải → Create → Limbo Exit → Biome Data

using UnityEngine;

// Kiểu kết cấu tường & sàn
public enum WallStyle
{
    ChuNhatDac,     // 0: Khối hộp chữ nhật đặc (mặc định - Đá Cổ, Tinh Thể)
    TruTron,        // 1: Cột trụ tròn xếp liên tiếp (Thư Viện, Đầm Lầy)
    TruLucGiac,     // 2: Cột lục giác xếp liên tiếp (biome bonus)
}

public enum GroundStyle
{
    HinhVuong,      // 0: Sàn hình vuông (dùng Cube - mặc định)
    HinhTron,       // 1: Sàn hình tròn (dùng Cylinder - Thư Viện, Đầm Lầy)
    HinhLucGiac,    // 2: Sàn lục giác (xấp xỉ Cylinder + xoay 90°)
}

[CreateAssetMenu(fileName = "NewBiome", menuName = "Limbo Exit/Biome Data")]
public class BiomeData : ScriptableObject
{
    [Header("=== THÔNG TIN ===")]
    public string tenBiome = "Mê Cung Đá Cổ";
    [TextArea(2, 4)]
    public string moTa = "Tường đá cổ xưa rêu phong, bẫy chông ẩn trong bóng tối.";
    public Sprite anhDaiDien;

    [Header("=== HÌNH DẠNG KẾT CẤU ===")]
    public WallStyle kieuTuong  = WallStyle.ChuNhatDac;
    public GroundStyle kieuSan  = GroundStyle.HinhVuong;

    [Tooltip("Số cột trụ xếp trong 1 đoạn tường (chỉ dùng khi kieuTuong = TruTron/TruLucGiac)")]
    [Range(2, 12)]
    public int soTruPerWall = 6;

    [Tooltip("Đường kính mỗi cột trụ")]
    [Range(0.2f, 1.5f)]
    public float duongKinhTru = 0.5f;

    [Header("=== PREFABS ===")]
    public GameObject prefabTuong;  // Prefab 1 đơn vị tường/cột
    public GameObject prefabNen;    // Prefab sàn
    public GameObject prefabBay;    // Bẫy đặc trưng

    [Header("=== ÁNH SÁNG & MÀU ===")]
    public Color mauAmbientLight = new Color(0.1f, 0.1f, 0.15f);
    public Color mauFog          = new Color(0.05f, 0.05f, 0.1f);
    public bool  batFog          = false;
    [Range(0f, 50f)]
    public float matDoFog        = 0.05f;

    [Header("=== GAMEPLAY ===")]
    [Range(0.3f, 1f)]
    public float heSoTocDo   = 1f;
    public bool  coNuoc      = false;
    public bool  coSuong     = false;
    [Range(2f, 50f)]
    public float tamNhinToiDa = 30f;

    [Header("=== ÂM NHẠC ===")]
    public AudioClip nhacNen;
}

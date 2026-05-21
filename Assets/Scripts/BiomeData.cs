// BiomeData.cs — ScriptableObject cấu hình visual + gameplay của 1 Biome
using UnityEngine;

public enum WallStyle
{
    ChuNhatDac,
    TruTron,
    TruLucGiac,
}

public enum GroundStyle
{
    HinhVuong,
    HinhTron,
    HinhLucGiac,
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

    [Range(2, 12)]
    public int soTruPerWall = 6;
    [Range(0.2f, 1.5f)]
    public float duongKinhTru = 0.5f;

    [Header("=== PREFABS ===")]
    public GameObject prefabTuong;
    public GameObject prefabNen;
    public GameObject prefabBay;

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

// BiomeManager.cs
// Áp dụng BiomeData vào Scene: đổi prefab tường/sàn, fog, ánh sáng, tốc độ Player
// GẮN vào: GameObject "MazeGenerator" (cùng với MazeRenderer)
// Tự động đọc biomeIndex từ GameSettings để chọn đúng biome

using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    [Header("=== DANH SÁCH BIOME (theo thứ tự index) ===")]
    public BiomeData[] danhSachBiome;
    // 0 = Mê Cung Đá Cổ
    // 1 = Thư Viện Vô Tận
    // 2 = Đầm Lầy Sương Mù
    // 3 = Mê Cung Tinh Thể (bonus sáng tạo)

    [Header("=== THAM CHIẾU ===")]
    public MazeRenderer mazeRenderer;       // Để cập nhật prefab
    public Light anhSangMoiTruong;          // Directional Light chính
    public Camera mainCamera;               // Để điều chỉnh far clip (tầm nhìn)

    // Biome đang dùng
    public BiomeData BiomeHienTai { get; private set; }

    void Awake()
    {
        ApDungBiome(GameSettings.biomeIndex);
    }

    // -----------------------------------------------
    // ÁP DỤNG BIOME VÀO SCENE
    // -----------------------------------------------
    public void ApDungBiome(int index)
    {
        if (danhSachBiome == null || danhSachBiome.Length == 0)
        {
            Debug.LogWarning("⚠️ Chưa gán Biome nào vào BiomeManager!");
            return;
        }

        index = Mathf.Clamp(index, 0, danhSachBiome.Length - 1);
        BiomeHienTai = danhSachBiome[index];

        if (BiomeHienTai == null) return;

        // ---- Cập nhật Prefab cho MazeRenderer ----
        if (mazeRenderer != null)
        {
            if (BiomeHienTai.prefabTuong != null)
                mazeRenderer.prefabTuong = BiomeHienTai.prefabTuong;
            if (BiomeHienTai.prefabNen != null)
                mazeRenderer.prefabNen = BiomeHienTai.prefabNen;
        }

        // ---- Ánh sáng môi trường ----
        RenderSettings.ambientLight = BiomeHienTai.mauAmbientLight;

        // ---- Directional Light ----
        if (anhSangMoiTruong != null)
            anhSangMoiTruong.color = BiomeHienTai.mauAmbientLight * 3f;

        // ---- Fog ----
        RenderSettings.fog = BiomeHienTai.batFog;
        RenderSettings.fogColor = BiomeHienTai.mauFog;
        RenderSettings.fogDensity = BiomeHienTai.matDoFog;
        RenderSettings.fogMode = FogMode.Exponential;

        // ---- Tầm nhìn Camera ----
        if (mainCamera != null)
            mainCamera.farClipPlane = BiomeHienTai.tamNhinToiDa;

        Debug.Log($"🌍 Biome: {BiomeHienTai.tenBiome} | Fog: {BiomeHienTai.batFog} | TốcĐộ: {BiomeHienTai.heSoTocDo}x");
    }

    // ---- Getter để PlayerController đọc hệ số tốc độ ----
    public static float LayHeSoTocDo()
    {
        // Tìm BiomeManager trong Scene
        BiomeManager bm = FindFirstObjectByType<BiomeManager>();
        if (bm != null && bm.BiomeHienTai != null)
            return bm.BiomeHienTai.heSoTocDo;
        return 1f;
    }
}

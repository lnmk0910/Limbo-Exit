// BiomeManager.cs — Áp dụng BiomeData vào Scene: prefab, fog, ánh sáng, tốc độ
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    [Header("=== DANH SÁCH BIOME ===")]
    public BiomeData[] danhSachBiome;

    [Header("=== THAM CHIẾU ===")]
    public MazeRenderer mazeRenderer;
    public Light anhSangMoiTruong;
    public Camera mainCamera;

    public BiomeData BiomeHienTai { get; private set; }

    // Ap dung biome vao maze, fog, anh sang va camera
    public void ApDungBiome(int index)
    {
        if (danhSachBiome == null || danhSachBiome.Length == 0) return;

        index = Mathf.Clamp(index, 0, danhSachBiome.Length - 1);
        BiomeHienTai = danhSachBiome[index];
        if (BiomeHienTai == null) return;

        // Prefab cho MazeRenderer
        if (mazeRenderer != null)
        {
            if (BiomeHienTai.prefabTuong != null) mazeRenderer.prefabTuong = BiomeHienTai.prefabTuong;
            if (BiomeHienTai.prefabNen != null)   mazeRenderer.prefabNen   = BiomeHienTai.prefabNen;
        }

        // Ánh sáng
        RenderSettings.ambientLight = BiomeHienTai.mauAmbientLight;
        if (anhSangMoiTruong != null)
            anhSangMoiTruong.color = BiomeHienTai.mauAmbientLight * 3f;

        // Fog
        RenderSettings.fog        = BiomeHienTai.batFog;
        RenderSettings.fogColor   = BiomeHienTai.mauFog;
        RenderSettings.fogDensity = BiomeHienTai.matDoFog;
        RenderSettings.fogMode    = FogMode.Exponential;

        // Camera
        if (mainCamera != null) mainCamera.farClipPlane = BiomeHienTai.tamNhinToiDa;

        AudioManager.PhatBGMTheoBiome(index);
    }

    // Lay he so toc do theo biome hien tai
    public static float LayHeSoTocDo()
    {
        BiomeManager bm = FindFirstObjectByType<BiomeManager>();
        if (bm != null && bm.BiomeHienTai != null) return bm.BiomeHienTai.heSoTocDo;
        return 1f;
    }
}

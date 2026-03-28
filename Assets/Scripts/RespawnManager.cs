// RespawnManager.cs
// Quản lý hệ thống hồi sinh:
// - Theo dõi các "Điểm An Toàn" người chơi đã kích hoạt
// - Khi chết: chọn ngẫu nhiên 1 điểm → spawn Player lại đó
// - Nếu chưa có điểm nào: về vị trí Start
// GẮN vào: Empty GameObject "RespawnManager" trong GameScene

using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("=== VỊ TRÍ START (fallback) ===")]
    public Transform viTriStart;    // Kéo Player vào để biết vị trí ban đầu

    [Header("=== PHẦN TRĂM MẢnh HỒN GIỮ LẠI KHI CHẾT ===")]
    [Range(0f, 1f)]
    public float phanTramGiuManhHon = 0.5f;  // Mặc định giữ 50%

    // Danh sách các điểm an toàn đã mở (tọa độ thế giới)
    private List<Vector3> danhSachDiemAnToan = new List<Vector3>();

    // Vị trí Player hiện tại (cập nhật liên tục)
    private Transform playerTransform;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Gọi bởi GameManager sau khi spawn Player
    public void DatPlayer(Transform player)
    {
        playerTransform = player;
    }

    // -----------------------------------------------
    // Đăng ký điểm an toàn (gọi từ CheckpointTrigger)
    // -----------------------------------------------
    public void DangKyDiemAnToan(Vector3 viTri)
    {
        if (!danhSachDiemAnToan.Contains(viTri))
        {
            danhSachDiemAnToan.Add(viTri);
            Debug.Log($"💚 Đã lưu điểm an toàn: {viTri} | Tổng: {danhSachDiemAnToan.Count}");
        }
    }

    // -----------------------------------------------
    // Lấy điểm hồi sinh ngẫu nhiên
    // -----------------------------------------------
    public Vector3 LayDiemHoiSinhNgauNhien()
    {
        if (danhSachDiemAnToan.Count > 0)
        {
            // Chọn ngẫu nhiên 1 trong các điểm đã lưu
            int idx = Random.Range(0, danhSachDiemAnToan.Count);
            return danhSachDiemAnToan[idx];
        }

        // Chưa có checkpoint → về Start
        return viTriStart != null ? viTriStart.position : Vector3.zero;
    }

    // -----------------------------------------------
    // THỰC HIỆN HỒI SINH (gọi từ DeathScreen khi nhấn "Tiếp tục")
    // -----------------------------------------------
    public void HoiSinhPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("❌ Không tìm thấy Player để hồi sinh!");
            return;
        }

        Vector3 diemHoiSinh = LayDiemHoiSinhNgauNhien();

        // Dịch chuyển Player về điểm hồi sinh
        playerTransform.position = diemHoiSinh + Vector3.up * 1f;

        // Mất vật phẩm, giữ % Mảnh Hồn
        MatVatPhamKhiChet();

        Debug.Log($"🔄 Hồi sinh tại: {diemHoiSinh}");
    }

    // -----------------------------------------------
    // Xử lý mất vật phẩm + giữ % Mảnh Hồn
    // -----------------------------------------------
    void MatVatPhamKhiChet()
    {
        // Xóa vật phẩm trong túi
        PlayerData data = SaveSystem.LoadGame();

        int manhHonGiuLai = Mathf.FloorToInt(data.soManhHon * phanTramGiuManhHon);
        data.soManhHon   = manhHonGiuLai;
        data.soDaPhatSang = 0;
        data.soDongHo     = 0;
        data.soLaBan      = 0;

        SaveSystem.SaveGame(data);

        // Đồng bộ PlayerInventory runtime
        if (PlayerInventory.Instance != null)
        {
            // Reset bằng cách reload
        }

        Debug.Log($"💀 Chết! Giữ lại {manhHonGiuLai} Mảnh Hồn (50%). Mất hết vật phẩm.");
    }
}

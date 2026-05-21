// RespawnManager.cs — Quản lý hồi sinh: điểm an toàn + respawn
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("=== VỊ TRÍ START (fallback) ===")]
    public Transform viTriStart;

    [Header("=== PHẦN TRĂM MẢNH HỒN GIỮ LẠI KHI CHẾT ===")]
    [Range(0f, 1f)]
    public float phanTramGiuManhHon = 0.5f;

    private List<Vector3> danhSachDiemAnToan = new List<Vector3>();
    private Transform playerTransform;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Gan tham chieu player de respawn
    public void DatPlayer(Transform player)
    {
        playerTransform = player;
    }

    // Đăng ký điểm an toàn (gọi từ CheckpointTrigger)
    // Luu lai diem an toan de respawn
    public void DangKyDiemAnToan(Vector3 viTri)
    {
        if (!danhSachDiemAnToan.Contains(viTri))
            danhSachDiemAnToan.Add(viTri);
    }

    // Lay ngau nhien 1 diem an toan (fallback ve start)
    public Vector3 LayDiemHoiSinhNgauNhien()
    {
        if (danhSachDiemAnToan.Count > 0)
            return danhSachDiemAnToan[Random.Range(0, danhSachDiemAnToan.Count)];
        return viTriStart != null ? viTriStart.position : Vector3.zero;
    }

    // Gọi từ DeathScreen khi nhấn "Tiếp tục"
    // Dua player ve diem hoi sinh
    public void HoiSinhPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[LOI] Không tìm thấy Player để hồi sinh!");
            return;
        }

        playerTransform.position = LayDiemHoiSinhNgauNhien() + Vector3.up * 1f;
        AudioManager.TatAmThanhQuai();
    }
}

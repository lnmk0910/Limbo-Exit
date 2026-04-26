// EnemyAI.cs - Thêm cơ chế biến mất & spawn lại sau khi bắt Player
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum TrangThai { TuanTra, PhatHien, TruyDuoi, BienMat }
    public TrangThai trangThaiHienTai = TrangThai.TuanTra;

    [Header("=== TỐC ĐỘ ===")]
    public float tocDoTuanTra  = 2.5f;
    public float tocDoTruyDuoi = 5f;

    [Header("=== TẦM PHÁT HIỆN ===")]
    public float tamNhin = 8f;
    public float tamNghe = 4f;

    [Header("=== BẮT & HỒI SINH ===")]
    public float khoangCachBat     = 1.3f;
    public float thoiGianBienMat   = 10f;   // Thời gian ẩn trước khi spawn lại
    public float khoangCachSpawnMin = 15f;  // Spawn lại cách Player ít nhất bao nhiêu

    [Header("=== TUẦN TRA ===")]
    public float khoangCachTuanTra  = 6f;
    public float thoiGianChoTaiDiem = 2f;

    [Header("=== DEBUG ===")]
    public bool hienThiGizmos = true;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private Collider[] colliders;
    private float thoiGianChoDem = 0f;
    private float demPhatHien    = 0f;
    private bool daBat          = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
        else Debug.LogWarning("⚠️ Enemy không tìm thấy Player!");

        TimDiemTuanTraMoi();
    }

    void Update()
    {
        if (daBat || trangThaiHienTai == TrangThai.BienMat) return;
        if (TimeClockItem.dangDongBang) return;

        if (playerTransform != null)
        {
            float kc = Vector3.Distance(transform.position, playerTransform.position);
            if (kc <= khoangCachBat) { BatDuocPlayer(); return; }
        }

        switch (trangThaiHienTai)
        {
            case TrangThai.TuanTra:  XuLyTuanTra();  break;
            case TrangThai.PhatHien: XuLyPhatHien(); break;
            case TrangThai.TruyDuoi: XuLyTruyDuoi(); break;
        }
        KiemTraPhatHien();
    }

    // -----------------------------------------------
    // BẮT PLAYER → Biến mất → Spawn lại
    // -----------------------------------------------
    void BatDuocPlayer()
    {
        daBat = true;
        Debug.Log($"💀 Enemy bắt được Player! Biến mất {thoiGianBienMat}s...");
        DeathScreen.Instance?.HienManHinhChet();
        StartCoroutine(BienMatVaSpawnLai());
    }

    IEnumerator BienMatVaSpawnLai()
    {
        trangThaiHienTai = TrangThai.BienMat;

        // Ẩn toàn bộ
        agent.enabled = false;
        SetHienThi(false);

        // Chờ X giây (dùng realtime vì game có thể bị pause)
        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        // Tìm vị trí spawn mới (xa Player)
        Vector3 viTriMoi = TimViTriSpawnXaPlayer();

        // Dịch chuyển đến vị trí mới
        transform.position = viTriMoi;

        // Hiện lại + bật NavMesh
        agent.enabled = true;
        agent.Warp(viTriMoi); // Warp để NavMeshAgent đặt đúng vị trí
        SetHienThi(true);

        // Reset trạng thái
        daBat = false;
        trangThaiHienTai = TrangThai.TuanTra;
        TimDiemTuanTraMoi();

        Debug.Log($"👾 Enemy spawn lại tại {viTriMoi}");
    }

    Vector3 TimViTriSpawnXaPlayer()
    {
        Vector3 viTriPlayer = playerTransform != null
            ? playerTransform.position : Vector3.zero;

        for (int i = 0; i < 30; i++)
        {
            // Chọn hướng ngẫu nhiên
            Vector3 huong = Random.insideUnitSphere * 30f;
            huong.y = 0;
            Vector3 ungVien = transform.position + huong;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(ungVien, out hit, 10f, NavMesh.AllAreas))
            {
                // Kiểm tra đủ xa Player
                if (Vector3.Distance(hit.position, viTriPlayer) >= khoangCachSpawnMin)
                    return hit.position;
            }
        }

        // Fallback: dịch sang xa Player theo trục X
        return viTriPlayer + Vector3.right * khoangCachSpawnMin;
    }

    void SetHienThi(bool hien)
    {
        foreach (var r in renderers) if (r != null) r.enabled = hien;
    }

    // -----------------------------------------------
    // TUẦN TRA
    // -----------------------------------------------
    void XuLyTuanTra()
    {
        agent.speed = tocDoTuanTra;
        if (!agent.pathPending && agent.remainingDistance < 0.8f)
        {
            thoiGianChoDem += Time.deltaTime;
            if (thoiGianChoDem >= thoiGianChoTaiDiem)
            {
                thoiGianChoDem = 0f;
                TimDiemTuanTraMoi();
            }
        }
    }

    void TimDiemTuanTraMoi()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 huong = Random.insideUnitSphere * khoangCachTuanTra;
            huong.y = 0;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + huong, out hit, khoangCachTuanTra, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    // -----------------------------------------------
    // PHÁT HIỆN
    // -----------------------------------------------
    void XuLyPhatHien()
    {
        agent.SetDestination(transform.position);
        demPhatHien += Time.deltaTime;
        if (demPhatHien >= 0.5f)
        {
            demPhatHien = 0f;
            trangThaiHienTai = TrangThai.TruyDuoi;
            Debug.Log("👁️ Phát hiện! Bắt đầu truy đuổi...");
        }
    }

    // -----------------------------------------------
    // TRUY ĐUỔI
    // -----------------------------------------------
    void XuLyTruyDuoi()
    {
        agent.speed = tocDoTruyDuoi;
        if (playerTransform != null)
            agent.SetDestination(playerTransform.position);

        float kc = playerTransform != null
            ? Vector3.Distance(transform.position, playerTransform.position) : 999f;
        if (kc > tamNhin * 2f)
        {
            trangThaiHienTai = TrangThai.TuanTra;
            TimDiemTuanTraMoi();
        }
    }

    void KiemTraPhatHien()
    {
        if (playerTransform == null) return;
        float kc = Vector3.Distance(transform.position, playerTransform.position);
        bool ngheThay = kc <= tamNghe;
        bool nhinThay = false;
        if (kc <= tamNhin)
        {
            Vector3 huong = (playerTransform.position - transform.position).normalized;
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, huong, kc,
                                  ~LayerMask.GetMask("Player", "Enemy")))
                nhinThay = true;
        }
        if ((ngheThay || nhinThay) && trangThaiHienTai == TrangThai.TuanTra)
            trangThaiHienTai = TrangThai.PhatHien;
    }

    void OnDrawGizmosSelected()
    {
        if (!hienThiGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tamNhin);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tamNghe);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, khoangCachBat);
    }
}

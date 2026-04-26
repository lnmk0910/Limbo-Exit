// ThuthuMuAI.cs - Thêm cơ chế biến mất & spawn lại
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ThuthuMuAI : MonoBehaviour
{
    public enum TrangThai { NgheNgong, PhatHien, TruyDuoi, BienMat }
    public TrangThai trangThai = TrangThai.NgheNgong;

    [Header("=== TẦM NGHE ===")]
    public float tamNgheKhiDiThuong = 3f;
    public float tamNgheKhiChay    = 12f;
    public float tamNgheDungYen    = 1.5f;

    [Header("=== TỐC ĐỘ ===")]
    public float tocDoNghe     = 1f;
    public float tocDoTruyDuoi = 7f;

    [Header("=== BẮT & SPAWN LẠI ===")]
    public float khoangCachBat      = 1.2f;
    public float thoiGianBienMat    = 12f;   // Ẩn lâu hơn Enemy thường
    public float khoangCachSpawnMin = 15f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private bool  daBat           = false;
    private float thoiGianChoDem  = 0f;
    private float demPhatHien     = 0f;  // Đếm thời gian dừng khi phát hiện

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        renderers = GetComponentsInChildren<Renderer>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        TimDiemNgheNgong();
    }

    void Update()
    {
        if (daBat || trangThai == TrangThai.BienMat) return;
        if (TimeClockItem.dangDongBang) return;

        if (playerTransform != null)
        {
            float kc = Vector3.Distance(transform.position, playerTransform.position);
            if (kc <= khoangCachBat) { BatDuocPlayer(); return; }
        }

        switch (trangThai)
        {
            case TrangThai.NgheNgong: XuLyNgheNgong();  break;
            case TrangThai.PhatHien:  XuLyPhatHien();   break;
            case TrangThai.TruyDuoi: XuLyTruyDuoi();   break;
        }
    }

    void XuLyNgheNgong()
    {
        agent.speed = tocDoNghe;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            thoiGianChoDem += Time.deltaTime;
            if (thoiGianChoDem >= 3f) { thoiGianChoDem = 0f; TimDiemNgheNgong(); }
        }

        if (playerTransform == null) return;
        float khoangCach = Vector3.Distance(transform.position, playerTransform.position);
        if (khoangCach <= LayTamNghe())
        {
            trangThai = TrangThai.PhatHien; // Dừng lại nghe ngóng trước
            Debug.Log("👂 Thủ Thư Mù nghe thấy! Dừng lại...");
        }
    }

    void TimDiemNgheNgong()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 h = Random.insideUnitSphere * 5f; h.y = 0; h += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(h, out hit, 5f, NavMesh.AllAreas))
            { agent.SetDestination(hit.position); return; }
        }
    }

    // -----------------------------------------------
    // PHÁT HIỆN — dừng lại 0.5s trước khi đuổi
    // -----------------------------------------------
    void XuLyPhatHien()
    {
        agent.SetDestination(transform.position); // Dừng lại
        demPhatHien += Time.deltaTime;
        if (demPhatHien >= 0.5f)
        {
            demPhatHien  = 0f;
            trangThai    = TrangThai.TruyDuoi;
            AudioManager.PhatPhatHien();
            Debug.Log("👂 Thủ Thư Mù bắt đầu truy đuổi!");
        }
    }

    float LayTamNghe()
    {
        Rigidbody rb = playerTransform?.GetComponent<Rigidbody>();
        if (rb == null) return tamNgheKhiDiThuong;
        float v = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        if (v < 0.5f) return tamNgheDungYen;
        if (v > 7f)   return tamNgheKhiChay;
        return tamNgheKhiDiThuong;
    }

    void XuLyTruyDuoi()
    {
        agent.speed = tocDoTruyDuoi;
        if (playerTransform != null) agent.SetDestination(playerTransform.position);

        float kc = playerTransform != null
            ? Vector3.Distance(transform.position, playerTransform.position) : 999f;
        if (kc > LayTamNghe() * 2f)
        {
            trangThai = TrangThai.NgheNgong;
            TimDiemNgheNgong();
        }
    }

    void BatDuocPlayer()
    {
        daBat = true;
        Debug.Log($"💀 Thủ Thư Mù bắt được! Ẩn {thoiGianBienMat}s rồi spawn lại...");
        DeathScreen.Instance?.HienManHinhChet();
        StartCoroutine(BienMatVaSpawnLai());
    }

    IEnumerator BienMatVaSpawnLai()
    {
        trangThai = TrangThai.BienMat;
        agent.enabled = false;
        foreach (var r in renderers) if (r != null) r.enabled = false;

        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        // Tìm vị trí mới xa Player
        Vector3 viTriMoi = TimViTriXaPlayer();
        transform.position = viTriMoi;
        agent.enabled = true;
        agent.Warp(viTriMoi);
        foreach (var r in renderers) if (r != null) r.enabled = true;

        daBat = false;
        trangThai = TrangThai.NgheNgong;
        TimDiemNgheNgong();
        Debug.Log($"👂 Thủ Thư Mù xuất hiện lại tại {viTriMoi}");
    }

    Vector3 TimViTriXaPlayer()
    {
        Vector3 vp = playerTransform != null ? playerTransform.position : Vector3.zero;
        for (int i = 0; i < 30; i++)
        {
            Vector3 h = Random.insideUnitSphere * 35f; h.y = 0; h += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(h, out hit, 10f, NavMesh.AllAreas))
                if (Vector3.Distance(hit.position, vp) >= khoangCachSpawnMin)
                    return hit.position;
        }
        return vp + Vector3.right * khoangCachSpawnMin;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tamNgheKhiChay);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tamNgheKhiDiThuong);
    }
}

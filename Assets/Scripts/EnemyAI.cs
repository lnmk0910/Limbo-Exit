// EnemyAI.cs — AI quái vật cơ bản: tuần tra, phát hiện, truy đuổi, bắt player
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
    public float khoangCachBat     = 1.5f;
    public float thoiGianBienMat   = 10f;
    public float khoangCachSpawnMin = 15f;

    [Header("=== TUẦN TRA ===")]
    public float khoangCachTuanTra  = 6f;
    public float thoiGianChoTaiDiem = 2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private Collider[] colliders;
    private float thoiGianChoDem = 0f;
    private float demPhatHien    = 0f;
    private bool daBat           = false;

    // Khoi tao agent, renderer/collider va diem tuan tra
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        TimPlayer();
        TimDiemTuanTraMoi();
    }

    // Tim tham chieu Player theo tag
    void TimPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // Cap nhat trang thai AI va xu ly phat hien/bat player
    void Update()
    {
        if (daBat || trangThaiHienTai == TrangThai.BienMat) return;
        if (TimeClockItem.dangDongBang) { if (agent.enabled) agent.isStopped = true; return; }
        if (agent.enabled && agent.isStopped) agent.isStopped = false;

        if (playerTransform == null) { TimPlayer(); return; }

        float kc = Vector3.Distance(transform.position, playerTransform.position);
        if (kc <= khoangCachBat) { BatDuocPlayer(); return; }

        switch (trangThaiHienTai)
        {
            case TrangThai.TuanTra:  XuLyTuanTra();  break;
            case TrangThai.PhatHien: XuLyPhatHien(); break;
            case TrangThai.TruyDuoi: XuLyTruyDuoi(); break;
        }
        KiemTraPhatHien();
    }

    // Bat player, hien man hinh chet va bat dau bien mat
    void BatDuocPlayer()
    {
        if (daBat) return;
        daBat = true;

        DeathScreen ds = DeathScreen.Instance ?? Object.FindFirstObjectByType<DeathScreen>();
        if (ds != null) ds.HienManHinhChet();

        StartCoroutine(BienMatVaSpawnLai());
    }

    // An quai mot thoi gian roi spawn xa player
    IEnumerator BienMatVaSpawnLai()
    {
        trangThaiHienTai = TrangThai.BienMat;
        agent.enabled = false;
        SetHienThi(false);
        SetColliders(false);
        TatAmThanh();

        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        Vector3 viTriMoi = TimViTriSpawnXaPlayer();
        transform.position = viTriMoi;

        agent.enabled = true;
        agent.Warp(viTriMoi);
        SetHienThi(true);
        SetColliders(true);

        daBat = false;
        trangThaiHienTai = TrangThai.TuanTra;
        TimDiemTuanTraMoi();
    }

    // Tim vi tri spawn cach xa player de tranh bat lai ngay
    Vector3 TimViTriSpawnXaPlayer()
    {
        Vector3 vp = playerTransform != null ? playerTransform.position : Vector3.zero;
        for (int i = 0; i < 30; i++)
        {
            Vector3 h = Random.insideUnitSphere * 30f; h.y = 0;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + h, out hit, 10f, NavMesh.AllAreas))
                if (Vector3.Distance(hit.position, vp) >= khoangCachSpawnMin)
                    return hit.position;
        }
        return vp + Vector3.right * khoangCachSpawnMin;
    }

    void SetHienThi(bool hien)
    {
        foreach (var r in renderers) if (r != null) r.enabled = hien;
    }

    void SetColliders(bool bat)
    {
        foreach (var c in colliders) if (c != null) c.enabled = bat;
    }

    void TatAmThanh()
    {
        AudioSource src = GetComponent<AudioSource>();
        if (src != null) src.Stop();
    }

    // Tuần tra
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
        // Heatmap AI: 40% xác suất đi đến vùng nóng
        if (HeatmapTracker.Instance != null && Random.value < 0.4f)
        {
            Vector3? diemNong = HeatmapTracker.Instance.LayDiemNongNgauNhien();
            if (diemNong.HasValue)
            {
                NavMeshHit hitNong;
                if (NavMesh.SamplePosition(diemNong.Value, out hitNong, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hitNong.position);
                    return;
                }
            }
        }

        for (int i = 0; i < 15; i++)
        {
            Vector3 h = Random.insideUnitSphere * khoangCachTuanTra; h.y = 0;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + h, out hit, khoangCachTuanTra, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    // Phát hiện — dừng lại 0.5s trước khi đuổi
    void XuLyPhatHien()
    {
        agent.SetDestination(transform.position);
        demPhatHien += Time.deltaTime;
        if (demPhatHien >= 0.5f)
        {
            demPhatHien = 0f;
            trangThaiHienTai = TrangThai.TruyDuoi;
        }
    }

    // Truy đuổi
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

    // Kiem tra nghe/nhin player de chuyen trang thai
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tamNhin);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tamNghe);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, khoangCachBat);
    }
}

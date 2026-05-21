// ThuthuMuAI.cs — AI Thủ Thư Mù: nghe ngóng, phát hiện bằng âm thanh, truy đuổi
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
    public float khoangCachBat      = 1.5f;
    public float thoiGianBienMat    = 12f;
    public float khoangCachSpawnMin = 15f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool  daBat           = false;
    private float thoiGianChoDem  = 0f;
    private float demPhatHien     = 0f;

    // Khoi tao agent, renderer/collider va diem nghe
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        TimPlayer();
        TimDiemNgheNgong();
    }

    // Tim tham chieu player
    void TimPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // Cap nhat trang thai AI theo am thanh
    void Update()
    {
        if (daBat || trangThai == TrangThai.BienMat) return;
        if (TimeClockItem.dangDongBang) { if (agent.enabled) agent.isStopped = true; return; }
        if (agent.enabled && agent.isStopped) agent.isStopped = false;

        if (playerTransform == null) { TimPlayer(); return; }

        float kc = Vector3.Distance(transform.position, playerTransform.position);
        if (kc <= khoangCachBat) { BatDuocPlayer(); return; }

        switch (trangThai)
        {
            case TrangThai.NgheNgong: XuLyNgheNgong();  break;
            case TrangThai.PhatHien:  XuLyPhatHien();   break;
            case TrangThai.TruyDuoi:  XuLyTruyDuoi();   break;
        }
    }

    // Di nghe ngong va chuyen sang phat hien neu nghe thay
    void XuLyNgheNgong()
    {
        agent.speed = tocDoNghe;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            thoiGianChoDem += Time.deltaTime;
            if (thoiGianChoDem >= 3f) { thoiGianChoDem = 0f; TimDiemNgheNgong(); }
        }

        if (playerTransform == null) return;
        float kc = Vector3.Distance(transform.position, playerTransform.position);
        if (kc <= LayTamNghe())
        {
            trangThai = TrangThai.PhatHien;
        }
    }

    // Tim diem nghe ngong moi gan vi tri hien tai
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

    // Dừng lại 0.5s trước khi đuổi
    // Dung lai de tao tre phan hoi truoc khi truy duoi
    void XuLyPhatHien()
    {
        agent.SetDestination(transform.position);
        demPhatHien += Time.deltaTime;
        if (demPhatHien >= 0.5f)
        {
            demPhatHien = 0f;
            trangThai   = TrangThai.TruyDuoi;
            AudioManager.PhatPhatHien();
        }
    }

    // Tinh tam nghe dua tren toc do player
    float LayTamNghe()
    {
        Rigidbody rb = playerTransform?.GetComponent<Rigidbody>();
        if (rb == null) return tamNgheKhiDiThuong;
        float v = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        if (v < 0.5f) return tamNgheDungYen;
        if (v > 7f)   return tamNgheKhiChay;
        return tamNgheKhiDiThuong;
    }

    // Truy duoi player va quay lai nghe ngong neu mat dau vet
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

    // Bat player va khoi dong bien mat
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
        trangThai = TrangThai.BienMat;
        agent.enabled = false;
        SetHienThi(false);
        SetColliders(false);

        AudioSource src = GetComponent<AudioSource>();
        if (src != null) src.Stop();

        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        Vector3 viTriMoi = TimViTriXaPlayer();
        transform.position = viTriMoi;

        agent.enabled = true;
        agent.Warp(viTriMoi);
        SetHienThi(true);
        SetColliders(true);

        daBat = false;
        trangThai = TrangThai.NgheNgong;
        TimDiemNgheNgong();
    }

    // Tim vi tri spawn xa player
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

    // Bat/tat renderer cua quai
    void SetHienThi(bool hien)
    {
        foreach (var r in renderers) if (r != null) r.enabled = hien;
    }

    // Bat/tat collider cua quai
    void SetColliders(bool bat)
    {
        foreach (var c in colliders) if (c != null) c.enabled = bat;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tamNgheKhiChay);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tamNgheKhiDiThuong);
    }
}

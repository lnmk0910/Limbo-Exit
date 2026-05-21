using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SinhVatBunAI : MonoBehaviour
{
    public enum TrangThai { AnNap, NhayLen, TruyDuoi, BienMat }
    public TrangThai trangThai = TrangThai.AnNap;

    [Header("=== TẦM PHÁT HIỆN ===")]
    public float tamAnNap    = 5f;
    public float tamTruyDuoi = 12f;

    [Header("=== TỐC ĐỘ ===")]
    public float tocDoTruyDuoi = 4.5f;

    [Header("=== BẮT & SPAWN LẠI ===")]
    public float khoangCachBat      = 1.8f;
    public float thoiGianBienMat    = 15f;
    public float khoangCachSpawnMin = 12f;

    [Header("=== HIỆU ỨNG ===")]
    public float thoiGianNhayLen = 0.8f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool daBat = false;

    // Khoi tao agent, renderer/collider va an quai
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.enabled = false;
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        SetHienThi(false);

        TimPlayer();
    }

    // Tim tham chieu player
    void TimPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // Cap nhat trang thai an nap/nhay len/truy duoi
    void Update()
    {
        if (daBat || trangThai == TrangThai.BienMat || trangThai == TrangThai.NhayLen) return;
        if (TimeClockItem.dangDongBang) { if (agent.enabled) agent.isStopped = true; return; }
        if (agent.enabled && agent.isStopped) agent.isStopped = false;

        // Retry tìm player nếu chưa có
        if (playerTransform == null) { TimPlayer(); return; }

        float kc = Vector3.Distance(transform.position, playerTransform.position);

        // Bắt player khi đang TruyDuoi
        if (kc <= khoangCachBat && trangThai == TrangThai.TruyDuoi)
        {
            BatDuocPlayer();
            return;
        }

        switch (trangThai)
        {
            case TrangThai.AnNap:
                if (kc <= tamAnNap)
                    StartCoroutine(NhayLen());
                break;
            case TrangThai.TruyDuoi:
                agent.speed = tocDoTruyDuoi;
                agent.SetDestination(playerTransform.position);
                if (kc > tamTruyDuoi) StartCoroutine(ChimXuongVaSpawnLai());
                break;
        }
    }

    // Backup: dùng collider trigger phòng distance check miss
    // Trigger bat player neu cham gan
    void OnTriggerEnter(Collider other)
    {
        if (daBat || trangThai == TrangThai.AnNap || trangThai == TrangThai.BienMat) return;
        if (!other.CompareTag("Player")) return;
        BatDuocPlayer();
    }

    // Nhay len tu an nap va bat dau truy duoi
    IEnumerator NhayLen()
    {
        if (trangThai != TrangThai.AnNap) yield break;

        trangThai = TrangThai.NhayLen;
        SetHienThi(true);
        SetColliders(true);
        transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < thoiGianNhayLen)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / thoiGianNhayLen);

            // Bắt player ngay trong lúc nhảy lên nếu quá gần
            if (playerTransform != null)
            {
                float kc = Vector3.Distance(transform.position, playerTransform.position);
                if (kc <= khoangCachBat)
                {
                    transform.localScale = Vector3.one;
                    BatDuocPlayer();
                    yield break;
                }
            }

            yield return null;
        }

        transform.localScale = Vector3.one;

        // Warp agent lên NavMesh trước khi bật
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
            transform.position = hit.position;

        agent.enabled = true;
        agent.Warp(transform.position);
        trangThai = TrangThai.TruyDuoi;
    }

    // Chim xuong va spawn lai sau mot khoang thoi gian
    IEnumerator ChimXuongVaSpawnLai()
    {
        trangThai = TrangThai.BienMat;
        agent.enabled = false;

        AudioSource src = GetComponent<AudioSource>();
        if (src != null) src.Stop();

        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 0.5f);
            yield return null;
        }
        SetHienThi(false);
        SetColliders(false);
        transform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        Vector3 viTriMoi = TimViTriMoi();
        transform.position = viTriMoi;

        daBat = false;
        trangThai = TrangThai.AnNap;
    }

    // Bat player va hien man hinh chet
    void BatDuocPlayer()
    {
        if (daBat) return;
        daBat = true;
        StopAllCoroutines();

        // Ẩn quái
        if (agent.enabled) agent.isStopped = true;

        DeathScreen ds = DeathScreen.Instance;
        if (ds == null) ds = Object.FindFirstObjectByType<DeathScreen>();

        if (ds != null)
            ds.HienManHinhChet();
        else
            Debug.LogError("[BUN] Không tìm thấy DeathScreen!");

        StartCoroutine(ChimXuongVaSpawnLai());
    }

    // Tim vi tri moi xa player
    Vector3 TimViTriMoi()
    {
        Vector3 vp = playerTransform != null ? playerTransform.position : Vector3.zero;
        for (int i = 0; i < 30; i++)
        {
            Vector3 h = Random.insideUnitSphere * 30f; h.y = 0; h += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(h, out hit, 10f, NavMesh.AllAreas))
                if (Vector3.Distance(hit.position, vp) >= khoangCachSpawnMin)
                    return hit.position;
        }
        return vp + Vector3.forward * khoangCachSpawnMin;
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
        Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, tamAnNap);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, tamTruyDuoi);
    }
}

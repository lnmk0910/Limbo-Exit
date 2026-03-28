// SinhVatBunAI.cs - Thêm cơ chế biến mất & spawn lại
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
    public float khoangCachBat      = 1.3f;
    public float thoiGianBienMat    = 15f;  // Bùn ẩn lâu nhất (bí ẩn nhất)
    public float khoangCachSpawnMin = 12f;

    [Header("=== HIỆU ỨNG ===")]
    public float thoiGianNhayLen = 0.8f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Renderer[] renderers;
    private bool daBat = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.enabled = false;
        renderers = GetComponentsInChildren<Renderer>();
        SetHienThi(false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (daBat || trangThai == TrangThai.BienMat) return;
        if (TimeClockItem.dangDongBang) { agent.isStopped = true; return; }
        agent.isStopped = false;

        float kc = playerTransform != null
            ? Vector3.Distance(transform.position, playerTransform.position) : 999f;

        if (kc <= khoangCachBat && trangThai == TrangThai.TruyDuoi)
        { BatDuocPlayer(); return; }

        switch (trangThai)
        {
            case TrangThai.AnNap:
                if (kc <= tamAnNap) StartCoroutine(NhayLen());
                break;
            case TrangThai.TruyDuoi:
                agent.speed = tocDoTruyDuoi;
                if (playerTransform != null) agent.SetDestination(playerTransform.position);
                if (kc > tamTruyDuoi) StartCoroutine(ChimXuongVaSpawnLai(false));
                break;
        }
    }

    IEnumerator NhayLen()
    {
        trangThai = TrangThai.NhayLen;
        SetHienThi(true);
        transform.localScale = Vector3.zero;
        float t = 0f;
        while (t < thoiGianNhayLen)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / thoiGianNhayLen);
            yield return null;
        }
        transform.localScale = Vector3.one;
        agent.enabled = true;
        trangThai = TrangThai.TruyDuoi;
        Debug.Log("💧 Sinh Vật Bùn nổi lên!");
    }

    // Chìm xuống rồi spawn lại (sau khi bắt hoặc Player chạy thoát)
    IEnumerator ChimXuongVaSpawnLai(bool daBatPlayer)
    {
        trangThai = TrangThai.BienMat;
        agent.enabled = false;

        // Thu nhỏ dần
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 0.5f);
            yield return null;
        }
        SetHienThi(false);
        transform.localScale = Vector3.one;

        // Chờ rồi chọn vị trí mới
        yield return new WaitForSecondsRealtime(thoiGianBienMat);

        Vector3 viTriMoi = TimViTriMoi();
        transform.position = viTriMoi;

        daBat = false;
        trangThai = TrangThai.AnNap;
        Debug.Log($"💧 Sinh Vật Bùn ẩn lại tại {viTriMoi}");
    }

    void BatDuocPlayer()
    {
        daBat = true;
        Debug.Log($"💀 Sinh Vật Bùn bắt được! Ẩn {thoiGianBienMat}s...");
        DeathScreen.Instance?.HienManHinhChet();
        StartCoroutine(ChimXuongVaSpawnLai(true));
    }

    Vector3 TimViTriMoi()
    {
        Vector3 vp = playerTransform != null ? playerTransform.position : Vector3.zero;
        for (int i = 0; i < 30; i++)
        {
            Vector3 h = Random.insideUnitSphere * 30f; h.y = 0; h += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(h, out hit, 10f, NavMesh.AllAreas))
                if (Vector3.Distance(hit.position, vp) >= khoangCachSpawnMin)
                    return hit.position + Vector3.down * 0.5f; // Hơi thấp hơn mặt đất
        }
        return vp + Vector3.forward * khoangCachSpawnMin;
    }

    void SetHienThi(bool hien)
    {
        foreach (var r in renderers) if (r != null) r.enabled = hien;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, tamAnNap);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, tamTruyDuoi);
    }
}

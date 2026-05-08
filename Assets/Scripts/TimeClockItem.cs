// TimeClockItem.cs
// Dong Ho Thoi Gian: Nhan 2 → dong bang TAT CA quai vat trong 5 giay
// Tim quai bang Component (EnemyAI, ThuthuMuAI, SinhVatBunAI) — khong phu thuoc tag
// GAN vao: Player GameObject

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TimeClockItem : MonoBehaviour
{
    [Header("=== CAI DAT ===")]
    public float thoiGianDong    = 5f;    // Thoi gian dong bang (giay)
    public Color mauDongBang     = Color.cyan;

    // Bien tinh de TAT CA AI doc
    public static bool dangDongBang = false;

    void Update()
    {
        if (!UIManager.DangTrongGame()) return;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            DungDongHo();
    }

    void DungDongHo()
    {
        if (!ItemSystem.DungDongHo()) return;
        StartCoroutine(HieuUngDongBang());
    }

    IEnumerator HieuUngDongBang()
    {
        dangDongBang = true;

        // ===== Tim TAT CA quai vat bang Component (khong can tag) =====
        List<NavMeshAgent> danhSachAgent = new List<NavMeshAgent>();
        List<KeyValuePair<Renderer, Color>> danhSachMau = new List<KeyValuePair<Renderer, Color>>();

        // Tim EnemyAI
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            if (e == null) continue;
            var nav = e.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(e.gameObject, danhSachMau);
        }

        // Tim ThuthuMuAI
        foreach (var t in Object.FindObjectsByType<ThuthuMuAI>(FindObjectsSortMode.None))
        {
            if (t == null) continue;
            var nav = t.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(t.gameObject, danhSachMau);
        }

        // Tim SinhVatBunAI
        foreach (var s in Object.FindObjectsByType<SinhVatBunAI>(FindObjectsSortMode.None))
        {
            if (s == null) continue;
            var nav = s.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(s.gameObject, danhSachMau);
        }

        int soQuai = danhSachAgent.Count;
        Debug.Log($"[DH] Dong bang {soQuai} quai vat trong {thoiGianDong}s!");

        // Hien HUD thong bao
        GameHUD.LamMoi();

        // Cho het thoi gian
        yield return new WaitForSeconds(thoiGianDong);

        // Khoi phuc tat ca
        foreach (var nav in danhSachAgent)
        {
            if (nav != null && nav.enabled) nav.isStopped = false;
        }

        // Khoi phuc mau goc
        foreach (var kv in danhSachMau)
        {
            if (kv.Key != null) kv.Key.material.color = kv.Value;
        }

        dangDongBang = false;
        Debug.Log("[DH] Het hieu luc dong bang! Quai vat hoat dong lai.");
        GameHUD.LamMoi();
    }

    // Luu mau goc roi doi sang mau dong bang
    void LuuVaDoiMau(GameObject obj, List<KeyValuePair<Renderer, Color>> ds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            ds.Add(new KeyValuePair<Renderer, Color>(r, r.material.color));
            r.material.color = mauDongBang;
        }
    }
}

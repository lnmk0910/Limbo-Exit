// TimeClockItem.cs — Đồng Hồ Thời Gian: nhấn 2 để đóng băng quái vật
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TimeClockItem : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianDong = 5f;
    public Color mauDongBang  = Color.cyan;

    public static bool dangDongBang = false;

    // Lang nghe phim tat de dung dong ho
    void Update()
    {
        if (!UIManager.DangTrongGame()) return;
        if (Input.GetKeyDown(KeyCode.Alpha2)) DungDongHo();
    }

    // Dung item va bat hieu ung dong bang
    void DungDongHo()
    {
        if (!ItemSystem.DungDongHo()) return;
        StartCoroutine(HieuUngDongBang());
    }

    // Dong bang tat ca quai trong mot khoang thoi gian
    IEnumerator HieuUngDongBang()
    {
        dangDongBang = true;

        List<NavMeshAgent> danhSachAgent = new List<NavMeshAgent>();
        List<KeyValuePair<Renderer, Color>> danhSachMau = new List<KeyValuePair<Renderer, Color>>();

        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            if (e == null) continue;
            var nav = e.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(e.gameObject, danhSachMau);
        }

        foreach (var t in Object.FindObjectsByType<ThuthuMuAI>(FindObjectsSortMode.None))
        {
            if (t == null) continue;
            var nav = t.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(t.gameObject, danhSachMau);
        }

        foreach (var s in Object.FindObjectsByType<SinhVatBunAI>(FindObjectsSortMode.None))
        {
            if (s == null) continue;
            var nav = s.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled) { nav.isStopped = true; danhSachAgent.Add(nav); }
            LuuVaDoiMau(s.gameObject, danhSachMau);
        }

        GameHUD.LamMoi();
        yield return new WaitForSeconds(thoiGianDong);

        foreach (var nav in danhSachAgent)
            if (nav != null && nav.enabled) nav.isStopped = false;

        foreach (var kv in danhSachMau)
            if (kv.Key != null) kv.Key.material.color = kv.Value;

        dangDongBang = false;
        GameHUD.LamMoi();
    }

    // Luu mau cu va doi mau renderers sang mau dong bang
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

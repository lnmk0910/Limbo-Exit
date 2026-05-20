// NPCTrigger.cs
// NPC ke chuyen + ban goi y (gop tu LostSoul)
// - Nhan E → mo hoi thoai lore
// - Nhan F → doi Da Phat Sang → nhan goi y huong Cong Thoat hoac vi tri quai
// GAN vao: Prefab_NPC

using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    [Header("=== LOI KE CUA NPC ===")]
    [TextArea(2, 4)]
    public string[] cauThoai =
    {
        "...Toi da o day... bao lau roi nhi...",
        "...Bong toi nay khong bao gio ngu...",
        "...Hay can than khi di... co ke dang rinh rap...",
        "...Cong thoat... no an sau lop suong...",
        "...Dung chay... tieng buoc chan se keo no den...",
        "...Toi da tung nhu ban... roi toi quen mat loi ra...",
    };

    [Header("=== THONG TIN NPC ===")]
    public string tenNPC = "Linh Hon Lac Loi";
    public Sprite avatarNPC;

    [Header("=== GOI Y (doi bang Da Phat Sang) ===")]
    public bool coChucNangGoiY = true;   // Bat/tat tinh nang goi y
    public int  giaDaGoiY      = 1;      // So Da can de doi goi y

    [Header("=== THAM CHIEU ===")]
    public Transform exitGate;           // Tu tim neu chua gan

    private bool dangGanPlayer = false;
    private int  chiSoThoai    = 0;

    void Start()
    {
        // Dam bao collider cua NPC la trigger de Player di xuyen qua
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        chiSoThoai = Random.Range(0, cauThoai.Length);

        // Tu tim ExitGate
        if (exitGate == null)
        {
            GameObject gate = GameObject.FindWithTag("ExitGate");
            if (gate == null) gate = GameObject.Find("ExitGate(Clone)");
            if (gate != null) exitGate = gate.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = true;
        Debug.Log($"[NPC] [{tenNPC}]: Nhan E de noi chuyen.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;

        if (DialogueUI.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.HoiThoai))
            DialogueUI.Instance.DongHoiThoai();
    }

    void Update()
    {
        if (!dangGanPlayer) return;
        if (!UIManager.DangTrongGame()) return;

        // E -> mo hoi thoai lore
        if (Input.GetKeyDown(KeyCode.E))
        {
            MoHoiThoai();
            return;
        }

        // F -> doi Da lay goi y (neu NPC co tinh nang nay)
        if (coChucNangGoiY && Input.GetKeyDown(KeyCode.F))
        {
            DoiDaLayGoiY();
        }
    }

    // -----------------------------------------------
    // MO HOI THOAI LORE
    // -----------------------------------------------
    void MoHoiThoai()
    {
        if (DialogueUI.Instance == null)
        {
            Debug.LogWarning("[NPC] Chua co DialogueUI trong Scene!");
            return;
        }

        // Ghep cau thoai + goi y dung phim F (neu co tinh nang goi y)
        string cauHienTai = cauThoai[chiSoThoai % cauThoai.Length];
        string[] danhSachCau;

        if (coChucNangGoiY)
        {
            danhSachCau = new string[]
            {
                cauHienTai,
                $"[F] Tốn {giaDaGoiY} Đá Phát Sáng → nhận gợi ý về Cổng Thoát / Kẻ Địch"
            };
        }
        else
        {
            danhSachCau = new string[] { cauHienTai };
        }

        DialogueUI.Instance.MoHoiThoai(tenNPC, danhSachCau, avatarNPC);
        chiSoThoai = (chiSoThoai + 1) % cauThoai.Length;
    }

    // -----------------------------------------------
    // DOI DA PHAT SANG → NHAN GOI Y
    // -----------------------------------------------
    void DoiDaLayGoiY()
    {
        if (PlayerInventory.Instance == null) return;

        if (PlayerInventory.Instance.daPhatSang < giaDaGoiY)
        {
            Debug.Log($"[NPC] Can {giaDaGoiY} Da Phat Sang de doi goi y.");

            // Thong bao qua DialogueUI neu dang mo
            if (DialogueUI.Instance != null)
            {
                string[] thongBao = { $"Không đủ Đá Phát Sáng! (Cần {giaDaGoiY})" };
                DialogueUI.Instance.MoHoiThoai(tenNPC, thongBao, avatarNPC);
            }
            return;
        }

        // Tru Da
        for (int i = 0; i < giaDaGoiY; i++)
            PlayerInventory.Instance.DungDa();

        // Random: goi y huong thoat HOAC vi tri quai
        if (Random.value > 0.5f)
            GoiYHuongCongThoat();
        else
            GoiYViTriQuaiVat();
    }

    // -----------------------------------------------
    // GOI Y HUONG CONG THOAT
    // -----------------------------------------------
    void GoiYHuongCongThoat()
    {
        if (exitGate == null)
        {
            Debug.LogWarning("[NPC] Chua tim duoc ExitGate!");
            return;
        }

        Vector3 huong = exitGate.position - transform.position;
        huong.y = 0;
        float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;

        string tenHuong;
        if      (goc >= -45 && goc < 45)   tenHuong = "Bắc (North)";
        else if (goc >= 45  && goc < 135)  tenHuong = "Đông (East)";
        else if (goc >= 135 || goc < -135) tenHuong = "Nam (South)";
        else                               tenHuong = "Tây (West)";

        string goiY = $"Cổng Thoát hướng {tenHuong} - khoảng {huong.magnitude:F0} đơn vị";
        Debug.Log($"[NPC] Goi y: {goiY}");

        string[] cauGoiY = { goiY };
        DialogueUI.Instance?.MoHoiThoai(tenNPC, cauGoiY, avatarNPC);
    }

    // -----------------------------------------------
    // GOI Y VI TRI QUAI VAT GAN NHAT
    // -----------------------------------------------
    void GoiYViTriQuaiVat()
    {
        // Tim TAT CA loai quai (khong chi EnemyAI)
        Transform quaiGanNhat = null;
        float kcNho = float.MaxValue;

        foreach (var q in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            float kc = Vector3.Distance(transform.position, q.transform.position);
            if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; }
        }
        foreach (var q in FindObjectsByType<ThuthuMuAI>(FindObjectsSortMode.None))
        {
            float kc = Vector3.Distance(transform.position, q.transform.position);
            if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; }
        }
        foreach (var q in FindObjectsByType<SinhVatBunAI>(FindObjectsSortMode.None))
        {
            float kc = Vector3.Distance(transform.position, q.transform.position);
            if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; }
        }

        string goiY;
        if (quaiGanNhat == null)
        {
            goiY = "Không cảm nhận được mối nguy hiểm nào gần đây.";
        }
        else
        {
            Vector3 hq = quaiGanNhat.position - transform.position;
            hq.y = 0;
            float g = Mathf.Atan2(hq.x, hq.z) * Mathf.Rad2Deg;

            string tenH;
            if      (g >= -45 && g < 45)   tenH = "Bắc";
            else if (g >= 45  && g < 135)  tenH = "Đông";
            else if (g >= 135 || g < -135) tenH = "Nam";
            else                           tenH = "Tây";

            string mucDo = kcNho < 10f ? "RẤT GẦN! Hãy chạy ngay!" : "Còn khá xa.";
            goiY = $"Cảm nhận mối nguy hướng {tenH} - {mucDo}";
        }

        Debug.Log($"[NPC] Canh bao: {goiY}");
        string[] cauGoiY = { goiY };
        DialogueUI.Instance?.MoHoiThoai(tenNPC, cauGoiY, avatarNPC);
    }
}

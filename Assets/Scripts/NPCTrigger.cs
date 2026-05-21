// NPCTrigger.cs — NPC kể chuyện + bán gợi ý (đổi Đá Phát Sáng)
using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    [Header("=== LỜI KỂ CỦA NPC ===")]
    [TextArea(2, 4)]
    public string[] cauThoai =
    {
        "...Tôi đã ở đây... bao lâu rồi nhỉ...",
        "...Bóng tối này không bao giờ ngủ...",
        "...Hãy cẩn thận khi đi... có kẻ đang rình rập...",
        "...Cổng thoát... nó ẩn sau lớp sương...",
        "...Đừng chạy... tiếng bước chân sẽ kéo nó đến...",
        "...Tôi đã từng như bạn... rồi tôi quên mất lối ra...",
    };

    [Header("=== THÔNG TIN NPC ===")]
    public string tenNPC = "Linh Hồn Lạc Lối";
    public Sprite avatarNPC;

    [Header("=== GỢI Ý ===")]
    public bool coChucNangGoiY = true;
    public int  giaDaGoiY      = 1;

    [Header("=== THAM CHIẾU ===")]
    public Transform exitGate;

    private bool dangGanPlayer = false;
    private int  chiSoThoai    = 0;

    // Dat trigger va tim ExitGate
    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        chiSoThoai = Random.Range(0, cauThoai.Length);

        if (exitGate == null)
        {
            GameObject gate = GameObject.FindWithTag("ExitGate");
            if (gate == null) gate = GameObject.Find("ExitGate(Clone)");
            if (gate != null) exitGate = gate.transform;
        }
    }

    // Danh dau player vao vung NPC
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = true;
    }

    // Thoat vung NPC va dong hoi thoai neu dang mo
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;
        if (DialogueUI.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.HoiThoai))
            DialogueUI.Instance.DongHoiThoai();
    }

    // Lang nghe phim tat de mo hoi thoai/doi goi y
    void Update()
    {
        if (!dangGanPlayer || !UIManager.DangTrongGame()) return;
        if (Input.GetKeyDown(KeyCode.E)) { MoHoiThoai(); return; }
        if (coChucNangGoiY && Input.GetKeyDown(KeyCode.F)) DoiDaLayGoiY();
    }

    // Mo hoi thoai voi NPC va tang chi so cau
    void MoHoiThoai()
    {
        if (DialogueUI.Instance == null) return;

        string cauHienTai = cauThoai[chiSoThoai % cauThoai.Length];
        string[] danhSachCau;

        if (coChucNangGoiY)
            danhSachCau = new string[] { cauHienTai, $"[F] Tốn {giaDaGoiY} Đá Phát Sáng → nhận gợi ý về Cổng Thoát / Kẻ Địch" };
        else
            danhSachCau = new string[] { cauHienTai };

        DialogueUI.Instance.MoHoiThoai(tenNPC, danhSachCau, avatarNPC);
        chiSoThoai = (chiSoThoai + 1) % cauThoai.Length;
    }

    // Doi da lay goi y (cong thoat hoac quai)
    void DoiDaLayGoiY()
    {
        if (PlayerInventory.Instance == null) return;
        if (PlayerInventory.Instance.daPhatSang < giaDaGoiY)
        {
            if (DialogueUI.Instance != null)
                DialogueUI.Instance.MoHoiThoai(tenNPC, new[] { $"Không đủ Đá Phát Sáng! (Cần {giaDaGoiY})" }, avatarNPC);
            return;
        }

        for (int i = 0; i < giaDaGoiY; i++) PlayerInventory.Instance.DungDa();

        if (Random.value > 0.5f) GoiYHuongCongThoat();
        else GoiYViTriQuaiVat();
    }

    // Goi y huong cong thoat theo vi tri ExitGate
    void GoiYHuongCongThoat()
    {
        if (exitGate == null) return;
        Vector3 huong = exitGate.position - transform.position;
        huong.y = 0;
        float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;

        string tenHuong;
        if      (goc >= -45 && goc < 45)   tenHuong = "Bắc";
        else if (goc >= 45  && goc < 135)  tenHuong = "Đông";
        else if (goc >= 135 || goc < -135) tenHuong = "Nam";
        else                               tenHuong = "Tây";

        string goiY = $"Cổng Thoát hướng {tenHuong} - khoảng {huong.magnitude:F0} đơn vị";
        DialogueUI.Instance?.MoHoiThoai(tenNPC, new[] { goiY }, avatarNPC);
    }

    // Goi y vi tri quai vat gan nhat
    void GoiYViTriQuaiVat()
    {
        Transform quaiGanNhat = null;
        float kcNho = float.MaxValue;

        foreach (var q in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        { float kc = Vector3.Distance(transform.position, q.transform.position); if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; } }
        foreach (var q in FindObjectsByType<ThuthuMuAI>(FindObjectsSortMode.None))
        { float kc = Vector3.Distance(transform.position, q.transform.position); if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; } }
        foreach (var q in FindObjectsByType<SinhVatBunAI>(FindObjectsSortMode.None))
        { float kc = Vector3.Distance(transform.position, q.transform.position); if (kc < kcNho) { kcNho = kc; quaiGanNhat = q.transform; } }

        string goiY;
        if (quaiGanNhat == null) goiY = "Không cảm nhận được mối nguy hiểm nào gần đây.";
        else
        {
            Vector3 hq = quaiGanNhat.position - transform.position; hq.y = 0;
            float g = Mathf.Atan2(hq.x, hq.z) * Mathf.Rad2Deg;
            string tenH;
            if      (g >= -45 && g < 45)   tenH = "Bắc";
            else if (g >= 45  && g < 135)  tenH = "Đông";
            else if (g >= 135 || g < -135) tenH = "Nam";
            else                           tenH = "Tây";
            string mucDo = kcNho < 10f ? "RẤT GẦN! Hãy chạy ngay!" : "Còn khá xa.";
            goiY = $"Cảm nhận mối nguy hướng {tenH} - {mucDo}";
        }
        DialogueUI.Instance?.MoHoiThoai(tenNPC, new[] { goiY }, avatarNPC);
    }
}

// LostSoul.cs
// Linh Hồn Lạc Lối – NPC thụ động ngồi trong mê cung
// Khi Player lại gần: hiện lời thoại lore
// Nhấn F để tặng "Ánh Sáng" (Đá Phát Sáng) → nhận gợi ý:
//   - Hướng Cổng Thoát
//   - Cảnh báo vị trí quái vật gần nhất
// GẮN vào: Prefab_LostSoul

using UnityEngine;
using TMPro;

public class LostSoul : MonoBehaviour
{
    [Header("=== LỜI THOẠI LORE ===")]
    [TextArea(2, 4)]
    public string[] cacLoreThoai = {
        "...Tôi đã ở đây... bao lâu rồi nhỉ...",
        "...Bóng tối này... nó không bao giờ ngủ...",
        "...Có kẻ đang săn đuổi bạn... hãy cẩn thận...",
        "...Cổng thoát... nó ẩn sau lớp sương...",
        "...Đừng chạy... tiếng bước chân sẽ kéo nó đến...",
        "...Tôi đã từng như bạn... rồi tôi quên mất lối ra...",
    };

    [Header("=== GỢI Ý ===")]
    public int giaTinhSang = 1;          // Số Đá Phát Sáng cần tặng

    [Header("=== UI ===")]
    public GameObject panelThoai;        // Panel hiện lời thoại
    public TMP_Text txtThoai;            // Text lời thoại
    public TMP_Text txtGoiY;             // Text gợi ý (hướng/vị trí)

    [Header("=== THAM CHIẾU ===")]
    public Transform exitGate;           // Cổng thoát (để tính hướng)

    private bool dangGanPlayer = false;
    private bool daChoThoai = false;
    private int chiSoLore = 0;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (panelThoai != null) panelThoai.SetActive(false);

        // Chọn ngẫu nhiên lore ban đầu
        chiSoLore = Random.Range(0, cacLoreThoai.Length);

        // Tự tìm ExitGate nếu chưa gán
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
        HienLoreThoai();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;
        daChoThoai = false;
        if (panelThoai != null) panelThoai.SetActive(false);
    }

    void Update()
    {
        if (!dangGanPlayer) return;

        // Nhấn F để tặng Đá Phát Sáng → nhận gợi ý
        if (Input.GetKeyDown(KeyCode.F))
            TangAnhSang();
    }

    // -----------------------------------------------
    // HIỆN LỜI THOẠI LORE
    // -----------------------------------------------
    void HienLoreThoai()
    {
        if (panelThoai != null) panelThoai.SetActive(true);

        string lore = cacLoreThoai[chiSoLore % cacLoreThoai.Length];
        if (txtThoai != null)
            txtThoai.text = $"👻 \"{lore}\"\n\n[F] Tặng {giaTinhSang} Đá Sáng → nhận gợi ý";

        if (txtGoiY != null) txtGoiY.text = "";
    }

    // -----------------------------------------------
    // TẶNG ĐÁ PHÁT SÁNG → NHẬN GỢI Ý
    // -----------------------------------------------
    void TangAnhSang()
    {
        if (PlayerInventory.Instance == null) return;

        // Kiểm tra có đủ Đá không
        if (PlayerInventory.Instance.daPhatSang < giaTinhSang)
        {
            if (txtGoiY != null)
                txtGoiY.text = $"❌ Không đủ Đá Phát Sáng! (Cần {giaTinhSang})";
            Debug.Log($"❌ Cần {giaTinhSang} Đá Phát Sáng để đổi gợi ý.");
            return;
        }

        // Trừ Đá
        for (int i = 0; i < giaTinhSang; i++)
            PlayerInventory.Instance.DungDa();

        // Chọn ngẫu nhiên: gợi ý hướng thoát HOẶC vị trí quái
        bool goiYHuongThoat = Random.value > 0.5f;

        if (goiYHuongThoat)
            HienHuongCongThoat();
        else
            HienViTriQuaiVat();

        daChoThoai = true;
        chiSoLore = (chiSoLore + 1) % cacLoreThoai.Length; // lần sau hiện lore khác
    }

    // -----------------------------------------------
    // Gợi ý hướng cổng thoát (N/S/E/W)
    // -----------------------------------------------
    void HienHuongCongThoat()
    {
        if (exitGate == null) { Debug.LogWarning("⚠️ Chưa tìm được ExitGate!"); return; }

        Vector3 huong = exitGate.position - transform.position;
        huong.y = 0;
        float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;

        string tenHuong;
        if      (goc >= -45 && goc < 45)    tenHuong = "⬆️ Bắc (North)";
        else if (goc >= 45  && goc < 135)   tenHuong = "➡️ Đông (East)";
        else if (goc >= 135 || goc < -135)  tenHuong = "⬇️ Nam (South)";
        else                                tenHuong = "⬅️ Tây (West)";

        string goiY = $"🚪 Cổng Thoát ở hướng {tenHuong}\n(khoảng {huong.magnitude:F0} đơn vị)";
        if (txtGoiY != null) txtGoiY.text = goiY;
        Debug.Log($"🧭 Linh Hồn gợi ý: {goiY}");
    }

    // -----------------------------------------------
    // Gợi ý vị trí quái vật gần nhất
    // -----------------------------------------------
    void HienViTriQuaiVat()
    {
        EnemyAI[] danhSachQuai = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        if (danhSachQuai.Length == 0)
        {
            if (txtGoiY != null) txtGoiY.text = "😌 Không cảm nhận được mối nguy hiểm nào gần đây.";
            return;
        }

        // Tìm quái gần nhất
        EnemyAI quaiGanNhat = null;
        float khoangCachNho = float.MaxValue;
        foreach (var q in danhSachQuai)
        {
            float kc = Vector3.Distance(transform.position, q.transform.position);
            if (kc < khoangCachNho) { khoangCachNho = kc; quaiGanNhat = q; }
        }

        Vector3 huongQuai = quaiGanNhat.transform.position - transform.position;
        huongQuai.y = 0;
        float gocQuai = Mathf.Atan2(huongQuai.x, huongQuai.z) * Mathf.Rad2Deg;

        string tenHuong;
        if      (gocQuai >= -45 && gocQuai < 45)    tenHuong = "⬆️ Bắc";
        else if (gocQuai >= 45  && gocQuai < 135)   tenHuong = "➡️ Đông";
        else if (gocQuai >= 135 || gocQuai < -135)  tenHuong = "⬇️ Nam";
        else                                         tenHuong = "⬅️ Tây";

        string caoBao = khoangCachNho < 10f ? "⚠️ RẤT GẦN!" : "Còn khá xa.";
        string goiY = $"👁️ Cảm nhận mối nguy {tenHuong}\n{caoBao}";
        if (txtGoiY != null) txtGoiY.text = goiY;
        Debug.Log($"⚠️ Linh Hồn cảnh báo: {goiY}");
    }
}

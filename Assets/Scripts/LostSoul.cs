// LostSoul.cs
// Linh Hồn Lạc Lối – NPC thụ động ngồi trong mê cung
// Khi Player lại gần: hiện lời thoại qua DialogueUI (tích hợp UIManager)
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

    [Header("=== GỢI Ý (đổi bằng Đá Phát Sáng) ===")]
    public int giaTinhSang = 1;

    [Header("=== THÔNG TIN NPC ===")]
    public string tenNPC = "Linh Hồn Lạc Lối";
    public Sprite avatarNPC;

    [Header("=== THAM CHIẾU ===")]
    public Transform exitGate; // Cổng thoát (tự tìm nếu chưa gán)

    private bool dangGanPlayer = false;
    private int  chiSoLore     = 0;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        chiSoLore = Random.Range(0, cacLoreThoai.Length);

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
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;

        // Đóng hội thoại nếu đang mở
        if (DialogueUI.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.HoiThoai))
            DialogueUI.Instance.DongHoiThoai();
    }

    void Update()
    {
        if (!dangGanPlayer) return;

        // Nhấn E → mở hội thoại lore (qua DialogueUI như NPCTrigger)
        if (UIManager.DangTrongGame() && Input.GetKeyDown(KeyCode.E))
        {
            MoHoiThoaiLore();
            return;
        }

        // Nhấn F → đổi Đá lấy gợi ý (chỉ khi đang chơi bình thường)
        if (UIManager.DangTrongGame() && Input.GetKeyDown(KeyCode.F))
        {
            TangAnhSang();
        }
    }

    // -----------------------------------------------
    // MỞ HỘI THOẠI QUA DialogueUI
    // -----------------------------------------------
    void MoHoiThoaiLore()
    {
        if (DialogueUI.Instance == null) return;

        string cauHienTai = cacLoreThoai[chiSoLore % cacLoreThoai.Length];
        string[] cauVaGoiY = new string[]
        {
            cauHienTai,
            $"[F] Tặng {giaTinhSang} Đá Phát Sáng → nhận gợi ý về Cổng Thoát / Kẻ Địch"
        };

        DialogueUI.Instance.MoHoiThoai(tenNPC, cauVaGoiY, avatarNPC);
        chiSoLore = (chiSoLore + 1) % cacLoreThoai.Length;
    }

    // -----------------------------------------------
    // TẶNG ĐÁ → NHẬN GỢI Ý
    // -----------------------------------------------
    void TangAnhSang()
    {
        if (PlayerInventory.Instance == null) return;

        if (PlayerInventory.Instance.daPhatSang < giaTinhSang)
        {
            Debug.Log($"❌ Cần {giaTinhSang} Đá Phát Sáng để đổi gợi ý.");
            return;
        }

        for (int i = 0; i < giaTinhSang; i++)
            PlayerInventory.Instance.DungDa();

        // Random: gợi ý hướng thoát HOẶC vị trí quái
        if (Random.value > 0.5f)
            HienHuongCongThoat();
        else
            HienViTriQuaiVat();
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
        if      (goc >= -45 && goc < 45)   tenHuong = "⬆️ Bắc";
        else if (goc >= 45  && goc < 135)  tenHuong = "➡️ Đông";
        else if (goc >= 135 || goc < -135) tenHuong = "⬇️ Nam";
        else                               tenHuong = "⬅️ Tây";

        string goiY = $"🚪 Cổng Thoát ở hướng {tenHuong} (~{huong.magnitude:F0} đơn vị)";
        Debug.Log($"🧭 Linh Hồn gợi ý: {goiY}");

        // Hiện qua DialogueUI nếu đang mở, hoặc mở mới
        string[] cauGoiY = { goiY };
        DialogueUI.Instance?.MoHoiThoai(tenNPC, cauGoiY, avatarNPC);
    }

    // -----------------------------------------------
    // Gợi ý vị trí quái vật gần nhất
    // -----------------------------------------------
    void HienViTriQuaiVat()
    {
        EnemyAI[] danhSachQuai = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        string goiY;

        if (danhSachQuai.Length == 0)
        {
            goiY = "😌 Không cảm nhận được mối nguy hiểm nào gần đây.";
        }
        else
        {
            EnemyAI quaiGanNhat = null;
            float   kcNho = float.MaxValue;
            foreach (var q in danhSachQuai)
            {
                float kc = Vector3.Distance(transform.position, q.transform.position);
                if (kc < kcNho) { kcNho = kc; quaiGanNhat = q; }
            }

            Vector3 hq = quaiGanNhat.transform.position - transform.position;
            hq.y = 0;
            float g = Mathf.Atan2(hq.x, hq.z) * Mathf.Rad2Deg;

            string tenH;
            if      (g >= -45 && g < 45)  tenH = "⬆️ Bắc";
            else if (g >= 45  && g < 135) tenH = "➡️ Đông";
            else if (g >= 135 || g < -135)tenH = "⬇️ Nam";
            else                          tenH = "⬅️ Tây";

            string nguyHiem = kcNho < 10f ? "⚠️ RẤT GẦN!" : "Còn khá xa.";
            goiY = $"👁️ Cảm nhận mối nguy {tenH}\n{nguyHiem}";
        }

        Debug.Log($"⚠️ Linh Hồn cảnh báo: {goiY}");
        string[] cauGoiY = { goiY };
        DialogueUI.Instance?.MoHoiThoai(tenNPC, cauGoiY, avatarNPC);
    }
}

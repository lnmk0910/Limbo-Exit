// NPCTrigger.cs
// NPC kể chuyện thuần túy — không có chức năng mua bán
// Khi Player đến gần → nhấn E → mở khung hội thoại
// Hết câu / Esc → đóng hội thoại, chơi tiếp
// GẮN vào: Prefab_NPC

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
    };

    [Header("=== THÔNG TIN NPC ===")]
    public string tenNPC = "Linh Hồn Lạc Lối";
    public Sprite avatarNPC; // Ảnh đại diện (tùy chọn, để trống nếu không có)

    private bool dangGanPlayer = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = true;
        Debug.Log($"💬 [{tenNPC}]: Nhấn E để nói chuyện...");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangGanPlayer = false;

        // Đóng hội thoại nếu player rời đi khi đang đọc
        if (DialogueUI.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.HoiThoai))
            DialogueUI.Instance.DongHoiThoai();
    }

    void Update()
    {
        if (!dangGanPlayer) return;
        if (!UIManager.DangTrongGame()) return; // Chỉ mở khi không có panel nào khác

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueUI.Instance != null)
                DialogueUI.Instance.MoHoiThoai(tenNPC, cauThoai, avatarNPC);
            else
                Debug.LogWarning("⚠️ Chưa có DialogueUI trong Scene!");
        }
    }
}

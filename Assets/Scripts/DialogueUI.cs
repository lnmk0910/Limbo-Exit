// DialogueUI.cs
// Khung hội thoại hiện khi Player đến gần NPC
// Hiển thị: Ảnh avatar NPC + Tên + Lời thoại (từng câu)
// Phím: E / Space / Enter → Sang câu tiếp | Esc → Đóng hội thoại
// GẮN vào: Canvas_GameUI (1 instance duy nhất)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("=== PANEL CHÍNH ===")]
    public GameObject panelHoiThoai;

    [Header("=== THÀNH PHẦN UI ===")]
    public Image   imgAvatar;        // Ảnh đại diện NPC
    public TMP_Text txtTenNPC;       // Tên NPC
    public TMP_Text txtNoiDung;      // Nội dung lời thoại
    public TMP_Text txtGoiY;         // Gợi ý phím: [E/Space] Tiếp   [Esc] Đóng

    [Header("=== HIỆU ỨNG GÕ CHỮ ===")]
    public float tocDoGoiChu = 0.04f; // Giây/ký tự (0 = hiện ngay)

    // Dữ liệu runtime
    private string[] cacCauThoai;
    private int      chiSoCauHienTai = 0;
    private bool     dangHienThi     = false;
    private bool     dangGoiChu      = false;
    private Coroutine coroutineGoiChu;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelHoiThoai != null) panelHoiThoai.SetActive(false);
    }

    void Update()
    {
        if (!dangHienThi || !UIManager.DangO(UIManager.TrangThaiUI.HoiThoai)) return;

        bool nhimTiep = Input.GetKeyDown(KeyCode.E)     ||
                        Input.GetKeyDown(KeyCode.Space)  ||
                        Input.GetKeyDown(KeyCode.Return);

        if (nhimTiep)
        {
            if (dangGoiChu)
                HienNgay(); // Đang gõ → hiện hết ngay lập tức
            else
                BuocTiep(); // Đã xong → chuyển câu tiếp
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            DongHoiThoai();
    }

    // -----------------------------------------------
    // MỞ HỘI THOẠI — gọi từ NPCTrigger
    // -----------------------------------------------
    public void MoHoiThoai(string tenNPC, string[] cauThoai, Sprite avatar = null)
    {
        if (dangHienThi) return;

        cacCauThoai     = cauThoai;
        chiSoCauHienTai = 0;
        dangHienThi     = true;

        UIManager.Mo(UIManager.TrangThaiUI.HoiThoai);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (txtTenNPC  != null) txtTenNPC.text  = tenNPC;
        if (imgAvatar  != null)
        {
            imgAvatar.gameObject.SetActive(avatar != null);
            if (avatar != null) imgAvatar.sprite = avatar;
        }
        if (txtGoiY != null)
            txtGoiY.text = "[E/Space] Tiếp theo   [Esc] Đóng";

        if (panelHoiThoai != null) panelHoiThoai.SetActive(true);
        AudioManager.PhatMoMenu();

        HienCau(chiSoCauHienTai);
    }

    // -----------------------------------------------
    // HIỆN CÂU THEO CHỈ SỐ
    // -----------------------------------------------
    void HienCau(int idx)
    {
        if (cacCauThoai == null || idx >= cacCauThoai.Length) { DongHoiThoai(); return; }

        if (coroutineGoiChu != null) StopCoroutine(coroutineGoiChu);

        if (tocDoGoiChu <= 0f)
        {
            if (txtNoiDung != null) txtNoiDung.text = cacCauThoai[idx];
            CapNhatGoiY(idx);
        }
        else
        {
            coroutineGoiChu = StartCoroutine(GoiChu(cacCauThoai[idx], idx));
        }
    }

    IEnumerator GoiChu(string cau, int idx)
    {
        dangGoiChu = true;
        if (txtNoiDung != null) txtNoiDung.text = "";

        foreach (char c in cau)
        {
            if (txtNoiDung != null) txtNoiDung.text += c;
            AudioManager.PhatHoiThoai(); // Sfx gõ chữ
            yield return new WaitForSecondsRealtime(tocDoGoiChu);
        }

        dangGoiChu = false;
        CapNhatGoiY(idx);
    }

    // Bấm khi đang gõ → hiện hết ngay
    void HienNgay()
    {
        if (coroutineGoiChu != null) StopCoroutine(coroutineGoiChu);
        dangGoiChu = false;
        if (txtNoiDung != null && cacCauThoai != null)
            txtNoiDung.text = cacCauThoai[chiSoCauHienTai];
        CapNhatGoiY(chiSoCauHienTai);
    }

    // Sang câu tiếp / đóng nếu hết
    void BuocTiep()
    {
        chiSoCauHienTai++;
        if (chiSoCauHienTai >= cacCauThoai.Length)
            DongHoiThoai(); // Hết câu → đóng, không làm gì thêm
        else
            HienCau(chiSoCauHienTai);
    }

    void CapNhatGoiY(int idx)
    {
        if (txtGoiY == null) return;
        bool laCauCuoi = (idx == cacCauThoai.Length - 1);
        txtGoiY.text = laCauCuoi
            ? "[E/Space] Đóng"
            : "[E/Space] Tiếp theo   [Esc] Đóng";
    }

    // ĐÓNG (không mở shop)
    // -----------------------------------------------
    public void DongHoiThoai()
    {
        DongPanel();
        UIManager.DongVeGame();
        AudioManager.PhatDongMenu();
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void DongPanel()
    {
        if (coroutineGoiChu != null) StopCoroutine(coroutineGoiChu);
        dangHienThi = false;
        dangGoiChu  = false;
        if (panelHoiThoai != null) panelHoiThoai.SetActive(false);
    }
}

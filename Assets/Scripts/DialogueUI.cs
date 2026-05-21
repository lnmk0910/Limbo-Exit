// DialogueUI.cs — Khung hội thoại NPC: gõ chữ, phím E/Space tiếp, Esc đóng
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
    public Image    imgAvatar;
    public TMP_Text txtTenNPC;
    public TMP_Text txtNoiDung;
    public TMP_Text txtGoiY;

    [Header("=== HIỆU ỨNG GÕ CHỮ ===")]
    public float tocDoGoiChu = 0.04f;

    private string[] cacCauThoai;
    private int      chiSoCauHienTai = 0;
    private bool     dangHienThi     = false;
    private bool     dangGoiChu      = false;
    private Coroutine coroutineGoiChu;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // An panel hoi thoai khi bat dau
    void Start()
    {
        if (panelHoiThoai != null) panelHoiThoai.SetActive(false);
    }

    // Xu ly input khi dang hoi thoai
    void Update()
    {
        if (!dangHienThi || !UIManager.DangO(UIManager.TrangThaiUI.HoiThoai)) return;

        bool nhimTiep = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
        if (nhimTiep)
        {
            if (dangGoiChu) HienNgay();
            else BuocTiep();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) DongHoiThoai();
    }

    // Mo hoi thoai va khoa thoi gian
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

        if (txtTenNPC != null) txtTenNPC.text = tenNPC;
        if (imgAvatar != null)
        {
            imgAvatar.gameObject.SetActive(avatar != null);
            if (avatar != null) imgAvatar.sprite = avatar;
        }
        if (txtGoiY != null) txtGoiY.text = "[E/Space] Tiếp theo   [Esc] Đóng";
        if (panelHoiThoai != null) panelHoiThoai.SetActive(true);
        AudioManager.PhatMoMenu();
        HienCau(chiSoCauHienTai);
    }

    // Hien 1 cau theo index (co go chu neu bat)
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
            coroutineGoiChu = StartCoroutine(GoiChu(cacCauThoai[idx], idx));
    }

    // Hieu ung go chu tung ky tu
    IEnumerator GoiChu(string cau, int idx)
    {
        dangGoiChu = true;
        if (txtNoiDung != null) txtNoiDung.text = "";
        foreach (char c in cau)
        {
            if (txtNoiDung != null) txtNoiDung.text += c;
            AudioManager.PhatHoiThoai();
            yield return new WaitForSecondsRealtime(tocDoGoiChu);
        }
        dangGoiChu = false;
        CapNhatGoiY(idx);
    }

    // Hien ngay toan bo cau hien tai
    void HienNgay()
    {
        if (coroutineGoiChu != null) StopCoroutine(coroutineGoiChu);
        dangGoiChu = false;
        if (txtNoiDung != null && cacCauThoai != null)
            txtNoiDung.text = cacCauThoai[chiSoCauHienTai];
        CapNhatGoiY(chiSoCauHienTai);
    }

    // Chuyen sang cau tiep theo
    void BuocTiep()
    {
        chiSoCauHienTai++;
        if (chiSoCauHienTai >= cacCauThoai.Length) DongHoiThoai();
        else HienCau(chiSoCauHienTai);
    }

    // Cap nhat text goi y theo vi tri cau
    void CapNhatGoiY(int idx)
    {
        if (txtGoiY == null) return;
        bool laCauCuoi = (idx == cacCauThoai.Length - 1);
        txtGoiY.text = laCauCuoi ? "[E/Space] Đóng" : "[E/Space] Tiếp theo   [Esc] Đóng";
    }

    // Dong hoi thoai va khoi phuc game
    public void DongHoiThoai()
    {
        DongPanel();
        UIManager.DongVeGame();
        AudioManager.PhatDongMenu();
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // Don dep trang thai noi bo va an panel
    void DongPanel()
    {
        if (coroutineGoiChu != null) StopCoroutine(coroutineGoiChu);
        dangHienThi = false;
        dangGoiChu  = false;
        if (panelHoiThoai != null) panelHoiThoai.SetActive(false);
    }
}

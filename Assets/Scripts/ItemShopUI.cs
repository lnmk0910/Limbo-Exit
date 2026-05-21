// ItemShopUI.cs — Cửa hàng vật phẩm: mua Đá, Đồng Hồ, La Bàn
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemShopUI : MonoBehaviour
{
    public static ItemShopUI Instance { get; private set; }

    [Header("=== PANEL ===")]
    public GameObject panelShop;

    [Header("=== TEXT THÔNG TIN ===")]
    public TMP_Text txtManhHon;
    public TMP_Text txtSoDa;
    public TMP_Text txtSoDongHo;
    public TMP_Text txtSoLaBan;

    [Header("=== GIÁ VẬT PHẨM ===")]
    public int giaDa     = 3;
    public int giaDongHo = 5;
    public int giaLaBan  = 4;

    private bool dangMo = false;
    private bool dangNghiNgoiChuyenMap = false;

    // Khoi tao singleton
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // An panel khi bat dau
    void Start()
    {
        if (panelShop != null) panelShop.SetActive(false);
    }

    // Lang nghe phim tat khi dang mo shop
    void Update()
    {
        if (!dangMo || !UIManager.DangO(UIManager.TrangThaiUI.Shop)) return;
        if (Input.GetKeyDown(KeyCode.B))      DongShop();
        if (Input.GetKeyDown(KeyCode.Alpha1)) MuaDa();
        if (Input.GetKeyDown(KeyCode.Alpha2)) MuaDongHo();
        if (Input.GetKeyDown(KeyCode.Alpha3)) MuaLaBan();
    }

    // Mo shop thong thuong
    public void MoShop()
    {
        dangNghiNgoiChuyenMap = false;
        MoiShopChung();
    }

    // Mo shop sau khi thang man (co the load lai scene)
    public void MoShopThangMan()
    {
        dangNghiNgoiChuyenMap = true;
        MoiShopChung();
    }

    // Mo shop va khoa thoi gian
    private void MoiShopChung()
    {
        dangMo = true;
        UIManager.Mo(UIManager.TrangThaiUI.Shop);
        AudioManager.PhatMoMenu();
        if (panelShop != null) panelShop.SetActive(true);
        CapNhatUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;
    }

    // Dong shop va khoi phuc trang thai truoc
    public void DongShop()
    {
        dangMo = false;
        if (panelShop != null) panelShop.SetActive(false);

        if (dangNghiNgoiChuyenMap)
        {
            UIManager.DongVeGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }

        UIManager.DongVePanel();
        if (VictoryScreen.Instance != null && UIManager.DangO(UIManager.TrangThaiUI.ChienThang))
            VictoryScreen.Instance.KhoiPhucHienThiHUB();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Time.timeScale   = 1f;
        }
    }

    // Cap nhat text thong tin trong shop
    void CapNhatUI()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (txtManhHon  != null) txtManhHon.text  = $"MH: {data.soManhHon} Mảnh Hồn";
        if (txtSoDa     != null) txtSoDa.text     = $"x{data.soDaPhatSang}";
        if (txtSoDongHo != null) txtSoDongHo.text = $"x{data.soDongHo}";
        if (txtSoLaBan  != null) txtSoLaBan.text  = $"x{data.soLaBan}";
    }

    // Mua da phat sang
    public void MuaDa()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soManhHon < giaDa) { AudioManager.PhatKhongDuTien(); return; }
        data.soManhHon   -= giaDa;
        data.soDaPhatSang += 1;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        AudioManager.PhatMuaDo();
        CapNhatUI();
        GameHUD.LamMoi();
    }

    // Mua dong ho
    public void MuaDongHo()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soManhHon < giaDongHo) { AudioManager.PhatKhongDuTien(); return; }
        data.soManhHon -= giaDongHo;
        data.soDongHo  += 1;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        AudioManager.PhatMuaDo();
        CapNhatUI();
        GameHUD.LamMoi();
    }

    // Mua la ban
    public void MuaLaBan()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soManhHon < giaLaBan) { AudioManager.PhatKhongDuTien(); return; }
        data.soManhHon -= giaLaBan;
        data.soLaBan   += 1;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        AudioManager.PhatMuaDo();
        CapNhatUI();
        GameHUD.LamMoi();
    }

    // Goi man hinh nang cap tu shop
    public void GoiUpgradeScreen()
    {
        if (panelShop != null) panelShop.SetActive(false);
        UpgradeScreen us = UpgradeScreen.Instance ?? FindFirstObjectByType<UpgradeScreen>();
        if (us != null) us.MoUpgrade();
    }
}

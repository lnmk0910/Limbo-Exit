// ItemShopUI.cs - Fix lỗi mua hàng
// - Tất cả đọc/ghi đều qua SaveSystem trực tiếp (không qua PlayerInventory)
// - Thêm nhiều Debug.Log để dễ kiểm tra
// - Bỏ AddListener (dùng hàm public gán từ Inspector)

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

    [Header("=== GIÁ VẬT PHẨM (có thể chỉnh trong Inspector) ===")]
    public int giaDa     = 3;
    public int giaDongHo = 5;
    public int giaLaBan  = 4;

    private bool dangMo = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (panelShop != null) panelShop.SetActive(false);
    }

    void Update()
    {
        if (dangMo && Input.GetKeyDown(KeyCode.Escape))
            DongShop();
    }

    // -----------------------------------------------
    // MỞ / ĐÓNG
    // -----------------------------------------------
    public void MoShop()
    {
        dangMo = true;
        if (panelShop != null) panelShop.SetActive(true);
        CapNhatUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 0f;

        Debug.Log("🏪 Shop mở! Mảnh Hồn hiện có: " + SaveSystem.LoadGame().soManhHon);
    }

    public void DongShop()
    {
        dangMo = false;
        if (panelShop != null) panelShop.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        Time.timeScale   = 1f;
    }

    // -----------------------------------------------
    // CẬP NHẬT UI
    // -----------------------------------------------
    void CapNhatUI()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (txtManhHon  != null) txtManhHon.text  = $"💎 {data.soManhHon} Mảnh Hồn";
        if (txtSoDa     != null) txtSoDa.text     = $"x{data.soDaPhatSang}";
        if (txtSoDongHo != null) txtSoDongHo.text = $"x{data.soDongHo}";
        if (txtSoLaBan  != null) txtSoLaBan.text  = $"x{data.soLaBan}";
    }

    // -----------------------------------------------
    // MUA VẬT PHẨM – gọi trực tiếp từ Button OnClick
    // -----------------------------------------------
    public void MuaDa()
    {
        PlayerData data = SaveSystem.LoadGame();
        Debug.Log($"🛒 Mua Đá: soManhHon={data.soManhHon}, giaDa={giaDa}");

        if (data.soManhHon < giaDa)
        {
            Debug.Log($"❌ Không đủ Mảnh Hồn! Cần {giaDa}, có {data.soManhHon}");
            return;
        }

        data.soManhHon   -= giaDa;
        data.soDaPhatSang += 1;
        SaveSystem.SaveGame(data);

        // Đồng bộ PlayerInventory nếu tồn tại
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        Debug.Log($"✅ Mua Đá Phát Sáng! Còn {data.soManhHon} Mảnh Hồn | Đá: {data.soDaPhatSang}");
        CapNhatUI();
    }

    public void MuaDongHo()
    {
        PlayerData data = SaveSystem.LoadGame();
        Debug.Log($"🛒 Mua Đồng Hồ: soManhHon={data.soManhHon}, giaDongHo={giaDongHo}");

        if (data.soManhHon < giaDongHo)
        {
            Debug.Log($"❌ Không đủ! Cần {giaDongHo}, có {data.soManhHon}");
            return;
        }

        data.soManhHon -= giaDongHo;
        data.soDongHo  += 1;
        SaveSystem.SaveGame(data);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        Debug.Log($"✅ Mua Đồng Hồ! Còn {data.soManhHon} Mảnh Hồn | Đồng hồ: {data.soDongHo}");
        CapNhatUI();
    }

    public void MuaLaBan()
    {
        PlayerData data = SaveSystem.LoadGame();
        Debug.Log($"🛒 Mua La Bàn: soManhHon={data.soManhHon}, giaLaBan={giaLaBan}");

        if (data.soManhHon < giaLaBan)
        {
            Debug.Log($"❌ Không đủ! Cần {giaLaBan}, có {data.soManhHon}");
            return;
        }

        data.soManhHon -= giaLaBan;
        data.soLaBan   += 1;
        SaveSystem.SaveGame(data);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        Debug.Log($"✅ Mua La Bàn! Còn {data.soManhHon} Mảnh Hồn | La bàn: {data.soLaBan}");
        CapNhatUI();
    }
}

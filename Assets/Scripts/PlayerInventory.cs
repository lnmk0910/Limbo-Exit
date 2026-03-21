// PlayerInventory.cs
// Singleton: Lưu vật phẩm hiện tại trong phiên chơi (runtime)
// Đọc từ Save khi vào game, ghi lại khi dùng đồ
// GẮN vào: Player GameObject

using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // Số lượng từng vật phẩm (runtime)
    public int daPhatSang  { get; private set; }
    public int dongHo      { get; private set; }
    public int laBan       { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Đọc từ save
        PlayerData data = SaveSystem.LoadGame();
        daPhatSang = data.soDaPhatSang;
        dongHo     = data.soDongHo;
        laBan      = data.soLaBan;
    }

    // ---- Thêm vật phẩm (NPC bán) ----
    public void ThemDa()    { daPhatSang++; LuuVaoFile(); }
    public void ThemDongHo(){ dongHo++;     LuuVaoFile(); }
    public void ThemLaBan() { laBan++;      LuuVaoFile(); }

    // ---- Dùng vật phẩm ----
    public bool DungDa()
    {
        if (daPhatSang <= 0) return false;
        daPhatSang--; LuuVaoFile(); return true;
    }
    public bool DungDongHo()
    {
        if (dongHo <= 0) return false;
        dongHo--; LuuVaoFile(); return true;
    }
    public bool DungLaBan()
    {
        if (laBan <= 0) return false;
        laBan--; LuuVaoFile(); return true;
    }

    void LuuVaoFile()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soDaPhatSang = daPhatSang;
        data.soDongHo     = dongHo;
        data.soLaBan      = laBan;
        SaveSystem.SaveGame(data);
    }
}

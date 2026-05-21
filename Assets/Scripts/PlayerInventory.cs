// PlayerInventory.cs — Singleton: vật phẩm runtime, đồng bộ với Save
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int daPhatSang { get; private set; }
    public int dongHo     { get; private set; }
    public int laBan      { get; private set; }

    // Khoi tao singleton va dong bo so luong tu save
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        PlayerData data = SaveSystem.LoadGame();
        daPhatSang = data.soDaPhatSang;
        dongHo     = data.soDongHo;
        laBan      = data.soLaBan;
    }

    public void ThemDa()     { daPhatSang++; LuuVaoFile(); }
    public void ThemDongHo() { dongHo++;     LuuVaoFile(); }
    public void ThemLaBan()  { laBan++;      LuuVaoFile(); }

    public bool DungDa()     { if (daPhatSang <= 0) return false; daPhatSang--; LuuVaoFile(); return true; }
    public bool DungDongHo() { if (dongHo <= 0) return false;     dongHo--;     LuuVaoFile(); return true; }
    public bool DungLaBan()  { if (laBan <= 0) return false;      laBan--;      LuuVaoFile(); return true; }

    // Ghi so luong item hien tai vao save
    void LuuVaoFile()
    {
        PlayerData data = SaveSystem.LoadGame();
        data.soDaPhatSang = daPhatSang;
        data.soDongHo     = dongHo;
        data.soLaBan      = laBan;
        SaveSystem.SaveGame(data);
    }

    // Dong bo so luong item tu save (khi co thay doi ben ngoai)
    public void SyncTuSave()
    {
        PlayerData data = SaveSystem.LoadGame();
        daPhatSang = data.soDaPhatSang;
        dongHo     = data.soDongHo;
        laBan      = data.soLaBan;
    }
}

// ItemSystem.cs — Lớp tĩnh quản lý vật phẩm, đọc/ghi trực tiếp từ SaveSystem
using UnityEngine;

public static class ItemSystem
{
    // Dung da phat sang, tru so luong va ghi save
    public static bool DungDa()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soDaPhatSang <= 0) return false;
        data.soDaPhatSang--;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        return true;
    }

    // Dung dong ho, tru so luong va ghi save
    public static bool DungDongHo()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soDongHo <= 0) return false;
        data.soDongHo--;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        return true;
    }

    // Dung la ban, tru so luong va ghi save
    public static bool DungLaBan()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soLaBan <= 0) return false;
        data.soLaBan--;
        SaveSystem.SaveGame(data);
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.SyncTuSave();
        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        return true;
    }

    // Lay so luong da tu inventory hoac save
    public static int SoDa()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.daPhatSang;
        return SaveSystem.LoadGame().soDaPhatSang;
    }

    // Lay so luong dong ho tu inventory hoac save
    public static int SoDongHo()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.dongHo;
        return SaveSystem.LoadGame().soDongHo;
    }

    // Lay so luong la ban tu inventory hoac save
    public static int SoLaBan()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.laBan;
        return SaveSystem.LoadGame().soLaBan;
    }
}

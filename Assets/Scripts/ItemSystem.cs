// ItemSystem.cs
// Lop tĩnh quan ly vat pham — doc/ghi truc tiep tu SaveSystem
// KHONG phu thuoc PlayerInventory.Instance
// Su dung: ItemSystem.DungDa(), ItemSystem.DungDongHo(), ItemSystem.DungLaBan()
// GAN vao: KHONG CAN GAN vao GameObject nao — goi truc tiep

using UnityEngine;

public static class ItemSystem
{
    // -----------------------------------------------
    // DUNG VAT PHAM — tra ve true neu thanh cong
    // -----------------------------------------------
    public static bool DungDa()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soDaPhatSang <= 0)
        {
            Debug.Log("[ITEM] Khong co Da Phat Sang! (so luong = 0)");
            return false;
        }
        data.soDaPhatSang--;
        SaveSystem.SaveGame(data);

        // Dong bo PlayerInventory runtime neu co
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        Debug.Log($"[ITEM] Dung Da Phat Sang. Con lai: {data.soDaPhatSang}");
        return true;
    }

    public static bool DungDongHo()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soDongHo <= 0)
        {
            Debug.Log("[ITEM] Khong co Dong Ho Thoi Gian! (so luong = 0)");
            return false;
        }
        data.soDongHo--;
        SaveSystem.SaveGame(data);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        Debug.Log($"[ITEM] Dung Dong Ho. Con lai: {data.soDongHo}");
        return true;
    }

    public static bool DungLaBan()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data.soLaBan <= 0)
        {
            Debug.Log("[ITEM] Khong co La Ban Co! (so luong = 0)");
            return false;
        }
        data.soLaBan--;
        SaveSystem.SaveGame(data);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.SyncTuSave();

        GameHUD.LamMoi();
        DDAManager.GhiNhanDungItem();
        Debug.Log($"[ITEM] Dung La Ban. Con lai: {data.soLaBan}");
        return true;
    }

    // -----------------------------------------------
    // KIEM TRA SO LUONG (khong ghi file)
    // -----------------------------------------------
    public static int SoDa()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.daPhatSang;
        return SaveSystem.LoadGame().soDaPhatSang;
    }

    public static int SoDongHo()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.dongHo;
        return SaveSystem.LoadGame().soDongHo;
    }

    public static int SoLaBan()
    {
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance.laBan;
        return SaveSystem.LoadGame().soLaBan;
    }
}

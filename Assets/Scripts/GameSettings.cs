// GameSettings.cs
// Static Class: luu thong so Map de truyen giua cac Scene
// Khong gan vao GameObject, goi truc tiep: GameSettings.rong

using UnityEngine;

public static class GameSettings
{
    // ---- Kich thuoc luoi me cung ----
    public static int rong = 50;
    public static int dai  = 50;

    // ---- Kich thuoc Tuong ----
    public static float chieuCaoTuong = 8f;
    public static float doDayTuong    = 1.5f;

    // ---- Kich thuoc O (san) ----
    public static float kichThuocO = 6f;

    // ---- AM THANH TONG ----
    // true = có tiếng (mặc định), false = tắt tiếng
    private static bool _coAmThanh = true;
    private static bool _daDoc = false;

    public static bool coAmThanh
    {
        get
        {
            if (!_daDoc)
            {
                _coAmThanh = PlayerPrefs.GetInt("MasterAudio", 1) == 1;
                _daDoc = true;
            }
            return _coAmThanh;
        }
        set
        {
            _coAmThanh = value;
            PlayerPrefs.SetInt("MasterAudio", value ? 1 : 0);
            PlayerPrefs.Save();
            // Ap dung ngay lap tuc
            AudioListener.volume = value ? 1f : 0f;
        }
    }

    // Goi 1 lan khi game bat dau de dong bo AudioListener
    public static void ApDungAmThanh()
    {
        AudioListener.volume = coAmThanh ? 1f : 0f;
    }

    // ---- Reset ve mac dinh ----
    public static void ResetMacDinh()
    {
        rong          = 50;
        dai           = 50;
        chieuCaoTuong = 8f;
        doDayTuong    = 1.5f;
        kichThuocO    = 6f;
    }
}


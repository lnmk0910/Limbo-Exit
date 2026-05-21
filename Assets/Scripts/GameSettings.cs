// GameSettings.cs — Static: lưu thông số Map truyền giữa các Scene
using UnityEngine;

public static class GameSettings
{
    public static int rong = 50;
    public static int dai  = 50;

    public static float chieuCaoTuong = 8f;
    public static float doDayTuong    = 1.5f;
    public static float kichThuocO    = 6f;

    // Âm thanh tổng
    private static bool _coAmThanh = true;
    private static bool _daDoc = false;

    // Luu/lay trang thai am thanh tong tu PlayerPrefs
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
            AudioListener.volume = value ? 1f : 0f;
        }
    }

    // Ap dung trang thai am thanh hien tai
    public static void ApDungAmThanh()
    {
        AudioListener.volume = coAmThanh ? 1f : 0f;
    }

    // Reset cac thong so ve mac dinh
    public static void ResetMacDinh()
    {
        rong          = 50;
        dai           = 50;
        chieuCaoTuong = 8f;
        doDayTuong    = 1.5f;
        kichThuocO    = 6f;
    }
}

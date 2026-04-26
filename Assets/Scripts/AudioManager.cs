// AudioManager.cs
// Hệ thống âm thanh trung tâm — Singleton, DontDestroyOnLoad
// Cung cấp static methods cho toàn bộ game gọi
// GẮN vào: Empty GameObject "AudioManager" trong mọi Scene

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // -----------------------------------------------
    // AUDIO SOURCES
    // -----------------------------------------------
    [Header("=== AUDIO SOURCES ===")]
    public AudioSource srcBGM;       // Nhạc nền (loop)
    public AudioSource srcAmbient;   // Ambient (loop)
    public AudioSource srcSFX;       // Hiệu ứng một lần
    public AudioSource srcBuocChan;  // Bước chân (loop khi di chuyển)

    // -----------------------------------------------
    // NHẠC NỀN (BGM)
    // -----------------------------------------------
    [Header("=== NHẠC NỀN ===")]
    public AudioClip bgmMenu;
    public AudioClip bgmDaCo;
    public AudioClip bgmThuVien;
    public AudioClip bgmDamLay;
    public AudioClip bgmTinhThe;
    public AudioClip bgmVictory;
    public AudioClip bgmGameOver;

    // -----------------------------------------------
    // AMBIENT
    // -----------------------------------------------
    [Header("=== AMBIENT ===")]
    public AudioClip ambGio;
    public AudioClip ambGiotNuoc;    // Dùng cho Biome Đầm Lầy

    // -----------------------------------------------
    // NHÂN VẬT
    // -----------------------------------------------
    [Header("=== NHÂN VẬT ===")]
    public AudioClip sfxBuocChanDa;
    public AudioClip sfxBuocChanBun;
    public AudioClip sfxBuocChanTinhThe;
    public AudioClip sfxThoMan;
    public AudioClip sfxChet;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxThoatMan;
    public AudioClip sfxManhHonRoi;
    public AudioClip sfxManhHonNhan;
    public AudioClip sfxCuaMo;

    // -----------------------------------------------
    // KẺ ĐỊCH
    // -----------------------------------------------
    [Header("=== KẺ ĐỊCH ===")]
    public AudioClip sfxQuaiPhatHien;
    public AudioClip sfxQuaiDuoi;
    public AudioClip sfxBunNoiLen;
    public AudioClip sfxThuthuNghe;

    // -----------------------------------------------
    // UI / SHOP
    // -----------------------------------------------
    [Header("=== UI / SHOP ===")]
    public AudioClip sfxMuaDo;
    public AudioClip sfxKhongDuTien;
    public AudioClip sfxNangCap;
    public AudioClip sfxMoMenu;
    public AudioClip sfxDongMenu;
    public AudioClip sfxHoiThoai;
    public AudioClip sfxPhaDaoFanfare;

    // -----------------------------------------------
    // ÂM LƯỢNG MẶC ĐỊNH
    // -----------------------------------------------
    [Header("=== ÂM LƯỢNG ===")]
    [Range(0f, 1f)] public float amLuongBGM     = 0.5f;
    [Range(0f, 1f)] public float amLuongAmbient = 0.3f;
    [Range(0f, 1f)] public float amLuongSFX     = 1.0f;
    [Range(0f, 1f)] public float amLuongBuocChan= 0.6f;

    // -----------------------------------------------
    // AWAKE — Singleton + DontDestroyOnLoad
    // -----------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApDungAmLuong();
    }

    void ApDungAmLuong()
    {
        if (srcBGM      != null) srcBGM.volume      = amLuongBGM;
        if (srcAmbient  != null) srcAmbient.volume  = amLuongAmbient;
        if (srcSFX      != null) srcSFX.volume      = amLuongSFX;
        if (srcBuocChan != null) srcBuocChan.volume = amLuongBuocChan;
    }

    // ===============================================
    // STATIC API — Gọi từ bất kỳ script nào
    // ===============================================

    // ---- NHẠC NỀN ----
    public static void PhatBGM(AudioClip clip)
    {
        if (Instance == null || Instance.srcBGM == null || clip == null) return;
        if (Instance.srcBGM.clip == clip && Instance.srcBGM.isPlaying) return;
        Instance.srcBGM.clip = clip;
        Instance.srcBGM.loop = true;
        Instance.srcBGM.Play();
    }

    public static void PhatBGMTheoScene(string tenScene)
    {
        if (Instance == null) return;
        switch (tenScene)
        {
            case "MenuScene":  PhatBGM(Instance.bgmMenu);    break;
        }
    }

    public static void PhatBGMTheoBiome(int biomeIndex)
    {
        if (Instance == null) return;
        switch (biomeIndex)
        {
            case 0: PhatBGM(Instance.bgmDaCo);    PhatAmbient(Instance.ambGio);      break;
            case 1: PhatBGM(Instance.bgmThuVien); PhatAmbient(null);                 break;
            case 2: PhatBGM(Instance.bgmDamLay);  PhatAmbient(Instance.ambGiotNuoc); break;
            case 3: PhatBGM(Instance.bgmTinhThe); PhatAmbient(Instance.ambGio);      break;
        }
    }

    public static void DungBGM()
    {
        if (Instance?.srcBGM != null) Instance.srcBGM.Stop();
    }

    // ---- AMBIENT ----
    public static void PhatAmbient(AudioClip clip)
    {
        if (Instance == null || Instance.srcAmbient == null) return;
        if (clip == null) { Instance.srcAmbient.Stop(); return; }
        if (Instance.srcAmbient.clip == clip && Instance.srcAmbient.isPlaying) return;
        Instance.srcAmbient.clip = clip;
        Instance.srcAmbient.loop = true;
        Instance.srcAmbient.Play();
    }

    // ---- SFX ----
    public static void Phat(AudioClip clip, float volume = 1f)
    {
        if (Instance == null || Instance.srcSFX == null || clip == null) return;
        Instance.srcSFX.PlayOneShot(clip, volume * Instance.amLuongSFX);
    }

    // ---- BƯỚC CHÂN ----
    public static void BatBuocChan(int biomeIndex = 0)
    {
        if (Instance == null || Instance.srcBuocChan == null) return;

        AudioClip clip;
        switch (biomeIndex)
        {
            case 2:  clip = Instance.sfxBuocChanBun;      break;
            case 3:  clip = Instance.sfxBuocChanTinhThe;  break;
            default: clip = Instance.sfxBuocChanDa;        break;
        }

        if (clip == null) return;
        if (Instance.srcBuocChan.clip != clip)
            Instance.srcBuocChan.clip = clip;
        if (!Instance.srcBuocChan.isPlaying)
            Instance.srcBuocChan.Play();
    }

    public static void TatBuocChan()
    {
        if (Instance?.srcBuocChan != null && Instance.srcBuocChan.isPlaying)
            Instance.srcBuocChan.Stop();
    }

    // ---- SHORTCUTS PHỔ BIẾN ----
    public static void PhatMuaDo()        => Phat(Instance?.sfxMuaDo);
    public static void PhatKhongDuTien()  => Phat(Instance?.sfxKhongDuTien);
    public static void PhatNangCap()      => Phat(Instance?.sfxNangCap);
    public static void PhatMoMenu()       => Phat(Instance?.sfxMoMenu);
    public static void PhatDongMenu()     => Phat(Instance?.sfxDongMenu);
    public static void PhatCheckpoint()   => Phat(Instance?.sfxCheckpoint);
    public static void PhatManhHonNhan()  => Phat(Instance?.sfxManhHonNhan);
    public static void PhatManhHonRoi()   => Phat(Instance?.sfxManhHonRoi);
    public static void PhatChet()         => Phat(Instance?.sfxChet);
    public static void PhatThoatMan()     => Phat(Instance?.sfxThoatMan);
    public static void PhatCuaMo()        => Phat(Instance?.sfxCuaMo);
    public static void PhatPhaDao()       => Phat(Instance?.sfxPhaDaoFanfare);
    public static void PhatHoiThoai()     => Phat(Instance?.sfxHoiThoai, 0.5f);

    // Enemy
    public static void PhatPhatHien()     => Phat(Instance?.sfxQuaiPhatHien);
    public static void PhatDuoi()         => Phat(Instance?.sfxQuaiDuoi);
    public static void PhatBunNoiLen()    => Phat(Instance?.sfxBunNoiLen);
    public static void PhatThuthuNghe()   => Phat(Instance?.sfxThuthuNghe);
}

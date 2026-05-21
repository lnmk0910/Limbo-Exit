// ExitGate.cs — Cổng thoát: kích hoạt VictoryScreen khi Player chạm
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGate : MonoBehaviour
{
    private bool daKichHoat = false;
    private Transform playerTransform;

    [Header("=== BACKUP: khoảng cách bắt Player ===")]
    public float khoangCachThang = 2f;

    // Dat trigger va tim player
    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // Backup: kiem tra khoang cach neu trigger khong hoat dong
    void Update()
    {
        if (daKichHoat || playerTransform == null) return;
        if (Vector3.Distance(transform.position, playerTransform.position) <= khoangCachThang)
            KichHoatThangMan();
    }

    // Trigger khi player cham gate
    void OnTriggerEnter(Collider other)
    {
        if (daKichHoat) return;
        if (!other.CompareTag("Player")) return;
        KichHoatThangMan();
    }

    // Xu ly thang man: ghi DDA, thuong manh hon, hien victory
    void KichHoatThangMan()
    {
        daKichHoat = true;
        AudioManager.PhatCuaMo();

        PlayerData data = SaveSystem.LoadGame();

        // DDA: ghi nhận thời gian vượt tầng
        float thoiGianVuot = Time.realtimeSinceStartup - data.thoiGianBatDauTang;
        DDAManager.GhiNhanVuotTang(thoiGianVuot);

        // Thưởng Mảnh Hồn
        data = SaveSystem.LoadGame();
        data.soManhHon += 10;
        SaveSystem.SaveGame(data);

        VictoryScreen vs = VictoryScreen.Instance ?? FindFirstObjectByType<VictoryScreen>();
        if (vs != null)
        {
            vs.HienManHinhThang();
        }
        else
        {
            data.mapHienTai++;
            data.seed = 0;
            SaveSystem.SaveGame(data);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

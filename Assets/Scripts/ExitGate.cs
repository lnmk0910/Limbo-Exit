// ExitGate.cs - Fix toàn diện
// Fix 1: Bỏ gameObject.tag = "ExitGate" (lỗi nếu tag chưa tạo trong TagManager)
// Fix 2: Dùng distance check SONG SONG với OnTriggerEnter (backup)
// Fix 3: Tìm VictoryScreen bằng FindFirstObjectByType nếu Instance null

using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGate : MonoBehaviour
{
    private bool daKichHoat = false;
    private Transform playerTransform;

    [Header("=== BACKUP: khoảng cách bắt Player ===")]
    public float khoangCachThang = 2f;  // Nếu trigger lỗi, dùng distance check

    void Start()
    {
        // Đặt collider là trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Tự tìm Player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // BACKUP: nếu OnTriggerEnter không hoạt động → dùng distance check
        if (daKichHoat) return;
        if (playerTransform == null) return;

        float kc = Vector3.Distance(transform.position, playerTransform.position);
        if (kc <= khoangCachThang)
            KichHoatThangMan();
    }

    void OnTriggerEnter(Collider other)
    {
        if (daKichHoat) return;
        if (!other.CompareTag("Player")) return;
        KichHoatThangMan();
    }

    void KichHoatThangMan()
    {
        daKichHoat = true;
        Debug.Log("🚨 ExitGate kích hoạt → Hiện VictoryScreen cho người chơi lựa chọn...");
        AudioManager.PhatCuaMo();

        PlayerData data = SaveSystem.LoadGame();

        // Thưởng Mảnh Hồn và đánh dấu sẵn để VictoryScreen biết
        data.soManhHon += 10;
        SaveSystem.SaveGame(data);

        // LUÔN gọi VictoryScreen trước - người chơi tự quyết định tiếp theo
        VictoryScreen vs = VictoryScreen.Instance ?? FindFirstObjectByType<VictoryScreen>();
        if (vs != null)
        {
            vs.HienManHinhThang();
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy VictoryScreen! Reload thẳng...");
            data.mapHienTai++;
            data.seed = 0;
            SaveSystem.SaveGame(data);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

// MinigameTrigger.cs
// Khi Player bước vào: bắt đầu Minigame đơn giản (nhấn phím đúng thứ tự)
// GẮN vào: Prefab_Minigame
// Giai đoạn sau có thể thay bằng Minigame phức tạp hơn

using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{
    private bool dangChoi = false;
    private bool daThang = false;

    // Chuỗi phím cần nhấn đúng thứ tự (Minigame đơn giản)
    private KeyCode[] chuoiPhim = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private int buocHienTai = 0;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (daThang) return;
        if (!other.CompareTag("Player")) return;
        dangChoi = true;
        buocHienTai = 0;
        Debug.Log("🎮 Minigame bắt đầu! Nhấn: W → A → S → D");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dangChoi = false;
        Debug.Log("🎮 Bỏ qua Minigame.");
    }

    void Update()
    {
        if (!dangChoi || daThang) return;

        // Kiểm tra phím đang nhấn có đúng thứ tự không
        if (Input.GetKeyDown(chuoiPhim[buocHienTai]))
        {
            buocHienTai++;
            Debug.Log($"✔️ Đúng! Bước {buocHienTai}/{chuoiPhim.Length}");

            if (buocHienTai >= chuoiPhim.Length)
            {
                // THẮNG MINIGAME
                daThang = true;
                dangChoi = false;

                PlayerData data = SaveSystem.LoadGame();
                data.soManhHon += 5;
                SaveSystem.SaveGame(data);

                Debug.Log($"🏆 Thắng Minigame! +5 Mảnh Hồn. Tổng: {data.soManhHon}");

                // Đổi màu thành xám = đã hoàn thành
                Renderer r = GetComponent<Renderer>();
                if (r != null) r.material.color = Color.gray;
            }
        }
        else if (Input.anyKeyDown)
        {
            // Sai phím → reset về đầu
            buocHienTai = 0;
            Debug.Log("❌ Sai! Thử lại từ đầu: W → A → S → D");
        }
    }
}

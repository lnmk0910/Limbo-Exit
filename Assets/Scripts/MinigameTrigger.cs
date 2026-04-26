using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameTrigger : MonoBehaviour
{
    [Header("Tên các scene mini game")]
    [Tooltip("Kéo thả tên scene vào đây (phải thêm vào Build Settings)")]
    public string[] miniGameScenes;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (miniGameScenes == null || miniGameScenes.Length == 0)
            {
                Debug.LogError("Chưa có scene minigame nào được thêm vào mảng miniGameScenes!");
                return;
            }

            // Random index
            int randomIndex = Random.Range(0, miniGameScenes.Length);
            string chosenScene = miniGameScenes[randomIndex];

            Debug.Log($"Đang load minigame random: <color=yellow>{chosenScene}</color>");

            // Load scene (thay thế hoàn toàn scene hiện tại)
            SceneManager.LoadScene(chosenScene);
        }
    }

    // Optional: Random ngay khi object này xuất hiện (dùng để test)
    // private void Start()
    // {
    //     // LoadRandomMinigame();   // Bỏ comment nếu muốn test
    // }
}
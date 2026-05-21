// MinigameTrigger.cs — Trigger load scene minigame ngẫu nhiên
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameTrigger : MonoBehaviour
{
    [Header("Tên các scene mini game")]
    [Tooltip("Kéo thả tên scene vào đây (phải thêm vào Build Settings)")]
    public string[] miniGameScenes;

    // Khi player cham, load ngau nhien 1 mini game
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (miniGameScenes == null || miniGameScenes.Length == 0) return;

        int randomIndex = Random.Range(0, miniGameScenes.Length);
        SceneManager.LoadScene(miniGameScenes[randomIndex]);
    }
}

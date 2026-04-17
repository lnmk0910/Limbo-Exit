using UnityEngine;

public class HoopDetector : MonoBehaviour
{
    private BasketballMinigame minigame;

    void Awake()        // Dùng Awake thay vì Start để chắc chắn hơn
    {
        minigame = FindObjectOfType<BasketballMinigame>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (minigame != null)
            {
                minigame.AddScore(1);
                Debug.Log("✓ Bóng vào rổ! +1 điểm");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy BasketballMinigame!");
            }
        }
    }
}
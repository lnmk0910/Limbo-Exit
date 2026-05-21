using UnityEngine;
public class TestDeathTrigger : MonoBehaviour
{
    // Test nhanh: cham vao la chet
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            DeathScreen.Instance?.HienManHinhChet();
    }
}

using UnityEngine;
public class TestDeathTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            DeathScreen.Instance?.HienManHinhChet();
    }
}

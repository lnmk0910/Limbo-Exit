using UnityEngine;

public class XuLyVaChamChoXe : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            Debug.Log("Cham CheckPoint");
            GameManager1.Instance.QuaCheckPoint();
        }

        if (other.CompareTag("WinPoint"))
        {
            Debug.Log("Cham Dich");
            GameManager1.Instance.QuaWinPoint();
        }
    }
}
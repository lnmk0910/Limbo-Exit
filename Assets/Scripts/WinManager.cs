using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance;

    public GameObject winPanel; // Panel chứa nút Exit

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Ẩn panel khi bắt đầu game
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void ShowWin()
    {
        Debug.Log("Hiện màn hình chiến thắng!");
        if (winPanel != null)
            winPanel.SetActive(true);
    }
}
